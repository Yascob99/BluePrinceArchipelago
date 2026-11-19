using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    /// Manage Simon's daily resources. 
    /// This includes gems, keys, gold, steps, dice, stars, allowance, and luck. 
    /// </summary>
    public static class ModAudioManager
    {
        public static Dictionary<string, AudioClip> AudioLookup = new Dictionary<string, AudioClip>();

        public static bool Initialized = false;


        /// <summary>
        ///     Finds all currently loaded AudioClips in the scene and adds them to a dictionary, indexed by name.
        /// </summary>
        public static void InitializeClipDict() {
            AudioLookup = new Dictionary<string, AudioClip>();
            AudioClip[] loadedAudio = UnityEngine.Resources.FindObjectsOfTypeAll<AudioClip>();
            foreach (AudioClip clip in loadedAudio) { 
                AudioLookup[clip.name] = clip;
            }
            Initialized = true;
            Logging.Log($"Audio Manager Loaded {AudioLookup.Count} AudioClips.", "Audio");
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
            PlaySoundAt(clipname, volume, go.transform.position + position);
        }

        /// <summary>
        ///     Plays a sound at an absolute position.
        /// </summary>
        /// <param name="clipname">The Name of the AudioClip to play</param>
        /// <param name="volume">The Volume to play the audio at</param>
        /// <param name="position">The position relative to the GameObject to play the audio at.</param>
        public static void PlaySoundAt(string clipname, float volume, Vector3 position) {
            if (!AudioLookup.ContainsKey(clipname))
            {
                Logging.LogWarning($"Unable to play clip. '{clipname}' is not a valid clip.");
                return;
            }
            AudioClip clip = AudioLookup[clipname];
            if (clip == null)
            {
                Logging.LogWarning($"Unable to play clip. '{clipname}' was null.");
                return;
            }
            if (go == null)
            {
                Logging.LogWarning($"Unable to play clip. Provided game object was null.");
                return;
            }
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}