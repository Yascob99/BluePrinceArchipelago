using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Patches;
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
    ///     Triggers related to the start or end of day.
    /// </summary>
    public static class DayTriggers
    {
        /// <summary>
        ///     Triggers at the Start of a new Day.   
        /// </summary>
        /// <param name="dayNum">The current day as an int</param>
        public static void OnDayStart(int dayNum)
        {
            ModInstance.IsInRun = true;
            // Reload the inventories on day start (in case a scene transition happened).
            ModItemManager.LoadInventories();

            // Reset room in-house counts and reload arrays — game resets pools at the start of each day
            Plugin.ModRoomManager.ResetRoomInHouseCounts();
            ModRoomManager.ReloadArrays();

            // Sync room pools with Archipelago at the start of each day, regardless of when auth happened
            ModRoomManager.SyncRoomPoolsWithArchipelago();
            if (ModInstance.FirstLoad)
            {
                RegisterItems.Register();
            }
            else
            {
                RegisterItems.ReloadGameObjects();
            }

            // Initialize the Star HUD so it can be properly updated when needed.
            GameObject.Find("__SYSTEM/HUD/Stars").SetActiveRecursively(true);
            if (ArchipelagoClient.Authenticated)
            {
                ModInstance.RanStartOfDay = true;
                FSMPatches.AddedFloorPlanOverrides();
                if (ModInstance.FirstLoad)
                {
                    // Rebuild the state if it couldn't be done on the Reconnect from crash.
                    //State.FirstLoad();

                    if (!ArchipelagoClient.StateRebuilt && ArchipelagoClient.Reconnected)
                    {
                        Logging.LogWarning("Rebuilding State");
                        Plugin.ArchipelagoClient.RebuildState();
                    }
                }

                // Release items that were queued while offline/before the run started
                ModInstance.QueueManager.ReleaseAllQueuedItems();
                ModInstance.QueueManager.ReleaseAllQueuedLocations();

                //Reload the Picker Arrays, Resync the room pools with archipelago.
                ModRoomManager.ReloadArrays();
                ModRoomManager.SyncRoomPoolsWithArchipelago();

                // Handle Start of day code for Permanent items (and maybe curses later).
                Plugin.ModItemManager.StartOfDay();
                Plugin.ModItemManager.ReplaceItemsWithAP();
                FSMPatches.TradingPostOverrides();
                FSMPatches.SundialOverrides();
                if (ArchipelagoOptions.UpgradeDiskSanity)
                {
                    FSMPatches.UpgradeDiskOverride(ModInstance.GlobalManager);
                }
                if (ArchipelagoOptions.RoomDraftSanity)
                {
                    FSMPatches.OuterDraftOverrides();
                }
                Plugin.ModRoomManager.HLCFix();
                Unlocks.AttemptPrePatch(); //Apply patches to the FSMs
                Unlocks.AppleOrchard.PreventDefault();
                Unlocks.WestGatePath.PreventDefault();
                Unlocks.SatelliteDish.PreventDefault();
                Plugin.UniqueItemManager.StartOfDay();
                Plugin.ModRoomManager.StartOfDay();
                Plugin.ArchipelagoClient.DeathLinkHandler.KillPlayer(); // If we have any queued death links, kill the player at the start of the day.
            }
        }


        /// <summary>
        ///     A function called at the end of a day (when the player loses control of the player object.
        /// </summary>
        public static void OnDayEnd()
        {
            ModInstance.IsInRun = false;
            State.UpdateItems(ArchipelagoClient.ServerData.ReceivedItems); // Update Items once a day so it can automatically add any items that should have been added by the crash.
            Plugin.ArchipelagoClient?.DeathLinkHandler?.SendEndOfDayDeathLink();
            Plugin.UniqueItemManager.EndOfDay();
        }
    }
}
