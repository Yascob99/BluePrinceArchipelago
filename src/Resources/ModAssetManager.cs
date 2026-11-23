using System.Collections.Generic;
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
            if (gameObject.TryGetComponent<MeshRenderer>(out MeshRenderer meshrenderer))
            {
                if (index == 0)
                {
                    meshrenderer.material = material;
                }
                else if (meshrenderer.materials.Length > index)
                {
                    Material[] materials = meshrenderer.materials;
                    materials[index] = material;
                    meshrenderer.materials = materials;
                    SetRemapMaterial(meshrenderer);
                }
                return;
            }
            Logging.LogWarning($"GameObject did not have a renderer component. Unable to set material.", "Assets");
        }

        private static void SetRemapMaterial(Renderer renderer) {
            SetRemappedTexture[] components = renderer.gameObject.GetComponents<SetRemappedTexture>();
            foreach (SetRemappedTexture setRemappedTexture in components)
            {
                if (!(setRemappedTexture == null))
                {
                    setRemappedTexture.RemapMaterial();
                }
            }
        }
        private static void SetRemapMaterial(MeshRenderer renderer)
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
    }
}