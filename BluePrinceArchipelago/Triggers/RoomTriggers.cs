using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using UnityEngine;

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
            ModRoom room = Plugin.ModRoomManager.GetRoomByName(obj.name.ToUpper().Trim());
            room?.Handler?.OnAfterRoomDrafted(obj);
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
                if (roomname.ToUpper().Trim().Contains("LADYSHIPS"))
                {
                    roomname = "HER LADYSHIP\'S CHAMBER";
                }
                Logging.LogWarning($"Room Drafted: {roomname}", "Room");
                if (Plugin.ModRoomManager.ForcedRoom != null)
                {
                    if (roomname.ToUpper() == Plugin.ModRoomManager.ForcedRoom.Name.ToUpper())
                    {
                        ModInstance.MasterPicker.GetBoolVariable("ForceDraft").Value = false;
                        ModRoomManager.ForceRoomQueue.Remove(Plugin.ModRoomManager.ForcedRoom);
                        Plugin.ModRoomManager.ForcedRoom = null;
                    }
                }
                ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomname.ToUpper().Trim());
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

        /// <summary>
        ///     Triggers before the game adds the copies of the Additional Floorplans to the pool.
        /// </summary>
        public static void OnBeforeFloorPlanAdds() {
            if (ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated)
            {
                // Skip Archipelago room pool management if RoomDraftSanity is disabled
                if (!ArchipelagoOptions.RoomDraftSanity)
                {
                    // Still allow force room draft for other purposes if needed
                    Plugin.ModRoomManager.CheckForceRoomDraft();
                    return;
                }

                // Reload arrays to ensure we have fresh references (game may have reset them)
                ModRoomManager.ReloadArrays();

                // If connected to Archipelago, ensure room unlock states are correct
                if (ArchipelagoClient.Authenticated)
                {
                    // Only set unlock states, don't update pools yet (we'll do that below)
                    ModRoomManager.EnsureRoomUnlockStates();
                }

                Plugin.ModRoomManager.CheckForceRoomDraft();
                Logging.Log("Updating Rooms for draft");
                Plugin.ModRoomManager.UpdateRoomPools();
            }
            else
            {
                Logging.Log("Unable to update Room Pool because Rooms have not been initialized.");
            }
        }
    }
}
