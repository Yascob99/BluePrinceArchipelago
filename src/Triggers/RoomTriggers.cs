using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using UnityEngine;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using BluePrinceArchipelago.Items;
#endif

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to Rooms
    /// </summary>
    public static class RoomTriggers
    {
        /// <summary>
        ///     Runs after a room has been fully spawned.
        /// </summary>
        /// <param name="obj">The spawned room object.</param>
        public static void OnAfterRoomSpawned(GameObject obj) {
            ModRoom room = ModRoomManager.GetRoomByName(obj.name.ToUpper().Trim());
            room?.Handler?.OnAfterRoomDrafted(obj);
            // Turn off the forced draft.
            ModInstance.MasterPicker.GetBoolVariable("ForceDraft").Value = false;

            // Add ItemCodeCompleteHook
            AddItemCodeCompleteHook(room);
        }

        /// <summary>
        ///     Triggers whenever a room is spawned, before it's regular code.
        /// </summary>
        /// <param name="obj">The object prefab to be spawned.</param>
        /// <param name="transformObj">The object with the spawn location data.</param>
        public static void OnBeforeRoomSpawned(GameObject obj, GameObject transformObj) {
            if (obj != null)
            {
                string roomname = obj.name;
                if (roomname.ToUpper().Trim() == "MAIDS CHAMBER")
                {
                    roomname = "MAID\'S CHAMBER";
                }
                else if (roomname.ToUpper().Trim().Contains("LADYSHIPS"))
                {
                    roomname = "HER LADYSHIP\'S CHAMBER";
                }
                Logging.Log($"Room Drafted: {roomname}", "Rooms");
                if (ModRoomManager.ForcedRoom != null)
                {
                    if (roomname.ToUpper() == ModRoomManager.ForcedRoom.Name.ToUpper())
                    {
                        ModInstance.MasterPicker.GetBoolVariable("ForceDraft").Value = false;
                        ModRoomManager.ForceRoomQueue.Remove(ModRoomManager.ForcedRoom);
                        ModRoomManager.ForcedRoom = null;
                    }
                }
                ModRoom room = ModRoomManager.GetRoomByName(roomname.ToUpper().Trim());
                if (room != null)
                {
                    room.RoomInHouseCount++;
                    room.Handler?.OnRoomDrafted(obj);
                    if (!room.HasBeenDrafted)
                    {
                        room.HasBeenDrafted = true; //This triggers the Location found Event.
                    }
                }
            }
        }

        public static void OnItemCodeComplete(ModRoom room) {
            room?.Handler?.OnItemCodeComplete();

            // Attempt the extra trunk spawn.
            ModInstance.TrunkManager.TrySpawnTrunk(room);
        }


        private static void AddItemCodeCompleteHook(ModRoom room) {
            GameObject roomGameObject = ModRoomManager.GetRoomInstance(room.Name.ToTitleCase());
            List<GameObject> ItemDropGOs = roomGameObject.FindChildGameObjectsRecursive("_Pickup Items", true);
            bool success = false;
            foreach (GameObject go in ItemDropGOs) {
                PlayMakerFSM FSM;
                if (go.TryGetComponent<PlayMakerFSM>(out FSM))
                {
                    FsmState completed = FSM.GetState("Complete!");
                    if (completed != null)
                    {
                        completed.AddAction(room.RoomItemCodeComplete);
                        success = true;
                    }
                }
            }
            ModInstance.TrunkManager.TrunkHooked[room.Name] = success;
            if (success)
            {
               ModInstance.TrunkManager.CheckingForTrunkSpawn = true;
               Logging.LogWarning($"Successfully Added Item Code complete Hook for {room.Name}", "Rooms");
            }
            else {
                Logging.LogWarning($"Item Code Complete Hook could not be added for {room.Name}", "Rooms");
            }
        }
    }
}
