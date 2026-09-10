using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Plugin.ModRoomManager.RecheckRoomUnlockStatus();
            }
        }

        /// <summary>
        ///     Called just before the draft initializes on non-outer room drafts.
        /// </summary>
        public static void OnDraftBeforeInitialize()
        {
            if (ArchipelagoClient.Authenticated)
            {
                Plugin.ModRoomManager.RecheckRoomUnlockStatus();
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
                    Plugin.ModRoomManager.CheckForceRoomDraft();
                    return;
                }

                Plugin.ModRoomManager.UpdateRoomPools();

                ModRoomManager.OuterDraftRooms = Plugin.ModRoomManager.OuterDraftPrePickShuffling();
                ModInstance.MasterPicker.GetIntVariable("Reroll Count").Value = 0;
                Plugin.ModRoomManager.SetOuterDraftRooms(ModRoomManager.OuterDraftRooms, 0);
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
            Plugin.ModRoomManager.SetOuterDraftRooms(ModRoomManager.OuterDraftRooms, ModInstance.MasterPicker.GetIntVariable("Reroll Count").Value);
        }
    }
}
