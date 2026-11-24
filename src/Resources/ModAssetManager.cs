using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if ML
using Il2Cpp;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    /// Manage Simon's daily resources. 
    /// This includes gems, keys, gold, steps, dice, stars, allowance, and luck. 
    /// </summary>
    public static class ModAssetManager
    {
        private static Dictionary<string, AudioClip> _AudioLookup = new Dictionary<string, AudioClip>();

        private static Dictionary<string, Material> _MaterialLookup = new Dictionary<string, Material>();

        private static AssetBundle _AssetBundle = null;

        /// <summary>
        ///     Finds all currently loaded AudioClips in the scene and adds them to a dictionary, indexed by name.
        /// </summary>
        public static void InitializeResourceDicts() {
            _AudioLookup = new Dictionary<string, AudioClip>();
            AudioClip[] loadedAudio = UnityEngine.Resources.FindObjectsOfTypeAll<AudioClip>();
            foreach (AudioClip clip in loadedAudio) { 
                _AudioLookup[clip.name] = clip;
            }
            Logging.Log($"Asset Manager Loaded {_AudioLookup.Count} AudioClips.", "Assets");
            _MaterialLookup = new Dictionary<string, Material>();
            Material[] loadedMaterials = UnityEngine.Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material mat in loadedMaterials)
            {
                _MaterialLookup[mat.name] = mat;
            }
            Logging.Log($"Asset Manager Loaded {_MaterialLookup.Count} Materials.", "Assets");
        }

        /// <summary>
        ///     Plays a sound at the position of a Game Object.
        /// </summary>
        /// <param name="go">The Game Object to use as a relative postion.</param>
        /// <param name="clipname">The Name of the AudioClip to play</param>
        /// <param name="volume">The Volume to play the audio at</param>
        public static void PlaySound(GameObject go, string clipname, float volume) {
            PlaySound(go, clipname, volume, new Vector3(0, 0, 0));
        }

        /// <summary>
        ///     Plays a sound at the position of an object or a position relative to it.
        /// </summary>
        /// <param name="go">The Game Object to use as a relative postion.</param>
        /// <param name="clipname">The Name of the AudioClip to play</param>
        /// <param name="volume">The Volume to play the audio at</param>
        /// <param name="position">The position relative to the GameObject to play the audio at.</param>
        public static void PlaySound(GameObject go, string clipname, float volume, Vector3 position)
        {
            if (go == null)
            {
                Logging.LogWarning($"Unable to play clip. Provided game object was null.", "Assets");
                return;
            }
            PlaySoundAt(clipname, volume, go.transform.position + position);
        }

        /// <summary>
        ///     Plays a sound at an absolute position.
        /// </summary>
        /// <param name="clipname">The Name of the AudioClip to play</param>
        /// <param name="volume">The Volume to play the audio at</param>
        /// <param name="position">The position relative to the GameObject to play the audio at.</param>
        public static void PlaySoundAt(string clipname, float volume, Vector3 position) {
            if (!_AudioLookup.ContainsKey(clipname))
            {
                Logging.LogWarning($"Unable to play clip. '{clipname}' is not a valid clip.", "Assets");
                return;
            }
            AudioClip clip = _AudioLookup[clipname];
            if (clip == null)
            {
                Logging.LogWarning($"Unable to play clip. '{clipname}' was null.", "Assets");
                return;
            }
            
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>
        ///     Sets a GameObject with a renderer component to a material with the given name.
        /// </summary>
        /// <param name="gameObject">The GameObject with a renderer component.</param>
        /// <param name="index">The index of the material to set.</param>
        /// <param name="materialname">The name of the material to set.</param>
        public static void SetMaterial(GameObject gameObject, string materialname, int index = 0) {
            if (!_MaterialLookup.ContainsKey(materialname)) {
                Logging.LogWarning($"Unable to set material. '{materialname}' is not a valid material.", "Assets");
                return;
            }
            SetMaterial (gameObject, _MaterialLookup[materialname], index);
        }
        /// <summary>
        ///     Sets a GameObject with a renderer component to a material with the given name.
        /// </summary>
        /// <param name="gameObject">The GameObject with a renderer component.</param>
        /// <param name="index">The index of the material to set.</param>
        /// <param name="material">The Material to set.</param>
        public static void SetMaterial(GameObject gameObject, Material material, int index = 0) {
            if (gameObject == null)
            {
                Logging.LogWarning($"Unable to set material. Provided game object was null.", "Assets");
                return;
            }
            if (gameObject.TryGetComponent<Renderer>(out Renderer renderer)) {
                if (index == 0)
                {
                    renderer.material = material;
                }
                else if (renderer.materials.Length > index)
                {
                    Material[] materials = renderer.materials;
                    materials[index] = material;
                    renderer.materials = materials;
                    SetRemapMaterial(renderer);
                }
                return;
            }
            Logging.LogWarning($"GameObject did not have a renderer component. Unable to set material.", "Assets");
        }

        private static void SetRemapMaterial(Renderer renderer)
        {
            SetRemappedTexture[] components = renderer.gameObject.GetComponents<SetRemappedTexture>();
            foreach (SetRemappedTexture setRemappedTexture in components)
            {
                if (!(setRemappedTexture == null))
                {
                    setRemappedTexture.RemapMaterial();
                }
            }
        }

        /// <summary>
        ///     Gets the full asset paths of an asset with a given name from a given AssetBundle.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="bundle">The AssetBundle to search.</param>
        /// <param name="name">The name of the asset to find. Not case sensitive.</param>
        /// <returns>A list of assets paths of assets that include the given asset name.</returns>
        public static string GetAssetPath(this AssetBundle bundle, string name)
        {
            string[] names = bundle.GetAllAssetNames();
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].ToLower().Contains("/" + name.ToLower()))
                {
                    return names[i];
                }
            }
            return "";
        }
        /// <summary>
        ///     Loads an AssetBundle from a given resource path.
        /// </summary>
        /// <param name="resourceName">The Resource name of the resource to be loaded as an asset.</param>
        /// <returns>An AssetBundle.</returns>
        public static AssetBundle LoadAssetBundleFromAssembly(string resourceName)
        {
            // Creates a memory stream.
            using (MemoryStream ms = new MemoryStream())
            {
                // Gets the currently executing mod assembly, loads the resource, and copies it to the memory stream.
                Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName).CopyTo(ms);
                // Converts the memory stream into an IL2Cpp memorystream.
                Il2CppSystem.IO.MemoryStream memoryStream = new Il2CppSystem.IO.MemoryStream(ms.ToArray());
                // Loads the asset bundle from the stream (since regular loading is mostly stripped).
                AssetBundle bundle = AssetBundle.LoadFromStream(memoryStream);
                return bundle;
            }
        }
        /// <summary>
        ///     Returns the resource path name of any resources that have been added to the executing assembly.
        /// </summary>
        /// <param name="filePath">The file path of the given resource.</param>
        /// <returns>The resource name for the resource.</returns>
        public static string GetResourceNameFromPath(string filePath)
        {
            return "BluePrinceArchipelago." + filePath.Replace("\\", "/").Replace("/", ".");
        }

        /// <summary>
        ///     Loads the AssetBundle and stores it for later access.
        /// </summary>
        public static void LoadModAssetBundle() {
            if (_AssetBundle == null)
            {
                _AssetBundle = ModAssetManager.LoadAssetBundleFromAssembly(ModAssetManager.GetResourceNameFromPath("assets/apprefabs"));
            }
        }
    }
}