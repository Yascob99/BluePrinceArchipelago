using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to Drafts
    /// </summary>
    public static class DraftTriggers
    {
        /// <summary>
        ///     Handles initializing rooms. Called when a draft is about to start (e.g., player opens a door).
        /// </summary>
        public static void OnAfterDraftInitialize()
        {
            if (ArchipelagoClient.Authenticated)
            {
                ModRoomManager.RecheckRoomUnlockStatus();
            }
        }

        /// <summary>
        ///     Called just before the draft initializes on non-outer room drafts.
        /// </summary>
        public static void OnDraftBeforeInitialize()
        {
            if (ArchipelagoClient.Authenticated)
            {
                //ModRoomManager.RecheckRoomUnlockStatus();
            }
        }

        /// <summary>
        ///     Triggers before the game adds the copies of the Additional Floorplans to the pool and before a draft occurs.
        /// </summary>
        public static void OnBeforeFloorPlanAdds()
        {
            if (ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated)
            {
                // Skip Archipelago room pool management if RoomDraftSanity is disabled
                if (!ArchipelagoOptions.RoomDraftSanity)
                {
                    // Still allow force room draft for other purposes if needed
                    ModRoomManager.CheckForceRoomDraft();
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

                ModRoomManager.CheckForceRoomDraft();
                Logging.Log("Updating Rooms for draft");
                ModRoomManager.UpdateRoomPools();
            }
            else
            {
                Logging.Log("Unable to update Room Pool because Rooms have not been initialized.");
            }
        }

        /// <summary>
        ///     Called when the Outer Room Draft starts. Hook currently doesn't function properly.
        /// </summary>
        public static void OnBeforeOuterDraftStart()
        {
            Logging.LogWarning("Outer Draft Triggered");
            if (ModInstance.HasInitializedRooms)
            {
                // Skip Archipelago room pool management if RoomDraftSanity is disabled
                if (!ArchipelagoOptions.RoomDraftSanity)
                {
                    ModRoomManager.CheckForceRoomDraft();
                    return;
                }

                ModRoomManager.UpdateRoomPools();

                ModRoomManager.OuterDraftRooms = ModRoomManager.OuterDraftPrePickShuffling();
                ModInstance.MasterPicker.GetIntVariable("Reroll Count").Value = 0;
                ModRoomManager.SetOuterDraftRooms(ModRoomManager.OuterDraftRooms, 0);
                PlayMakerFSM StandaloneDoorCode = GameObject.Find("Standalone Rooms/Rustic Door/Rustic Door/Standalone Door Code").GetComponent<PlayMakerFSM>();
                PlayMakerFSM DraftUI = GameObject.Find("__SYSTEM/THE DRAFT/anchor/DRAFT UI").GetComponent<PlayMakerFSM>();
            }
            else
            {
                Logging.Log("Unable to update Room Pool because Rooms have not been initialized.");
            }
        }

        /// <summary>
        ///     Triggers on an outer draft being rerolled.
        /// </summary>
        public static void OnOuterDraftReroll() {
            ModRoomManager.SetOuterDraftRooms(ModRoomManager.OuterDraftRooms, ModInstance.MasterPicker.GetIntVariable("Reroll Count").Value);
        }

        /// <summary>
        ///     Triggers when the draft validation fails due to the game's own internal requirements.
        ///     This will result in some of the rooms being set to closets
        /// </summary>
        public static void OnDraftValidationFailed() {
            Logging.Log("DraftHelper Validation ran into an error and could not run to completion.", "Draft");
        }
    }
}
