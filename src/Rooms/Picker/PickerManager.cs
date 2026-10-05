using System.Collections.Generic;
#if ML
using Il2Cpp;
#endif
#if Bep
using HutongGames;
#endif

namespace BluePrinceArchipelago.Rooms.Pickers;

 public static class PickerManager
{
        public static Dictionary<string, PlayMakerArrayListProxy> PickerDict { set; get; } = [];
        public static Dictionary<string, PlayMakerArrayListProxy> UntouchedPickers { set; get; } = [];

        public static List<string> CurrentPickerArrays = [];
        public static List<string> CurrentPickerLists = [];

        /// <summary>
        /// Re-loads the picker arrays. Call this when arrays may have been reset by the game.
        /// </summary>
        public static void ReloadArrays()
        {
            Logging.Log("Reloading picker arrays...");
            PickerManager.PickerDict.Clear();
            PickerManager.UntouchedPickers.Clear();
            LoadArrays();
            Logging.Log($"Reloaded {PickerManager.PickerDict.Count} picker arrays.");
        }

                //TODO update this to be less hacky.
        /// <summary>
        ///     loads the list of picker arrays the rooms can be added to. 
        ///     May rewrite to use names instead of the id of the child for better forward compatibility.
        /// </summary>
        public static void LoadArrays()
        {
            // Core picker arrays (indexes 2-32, 55-56, 58-61)
            PlayMakerArrayListProxy array = null;
            List<int> coreChildIDs = [2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32];
            for (int i = 0; i < coreChildIDs.Count; i++)
            {
                array = ModInstance.PlanPicker.transform.GetChild(coreChildIDs[i]).gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (array != null)
                {

                    PickerManager.PickerDict[array.name.Trim()] = array;
                }
            }

            // Standalone Array Full
            array = ModInstance.PlanPicker.transform.GetChild(56).gameObject.GetComponent<PlayMakerArrayListProxy>();
            if (array != null)
            {
                PickerManager.UntouchedPickers["STANDALONE ARRAY"] = array;
            }

            //// Additional arrays that may be needed for special drafts (like Entrance Hall, first draft, etc.)
            //List<int> additionalChildIDs = [0, 33, 34, 35, 36, 37, 38, 39, 40, 44, 45, 57];
            //for (int i = 0; i < additionalChildIDs.Count; i++) {
            //    PlayMakerArrayListProxy array = PlanPicker.transform.GetChild(additionalChildIDs[i]).gameObject?.GetComponent<PlayMakerArrayListProxy>();
            //    if (array != null) {
            //        PickerManager.PickerDict[array.name.Trim()] = array;
            //        Logging.Log($"Loaded additional array: {array.name} with {array.GetCount()} rooms");
            //    }
            //}
        }
}
