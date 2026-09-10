using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Patches;
using BluePrinceArchipelago.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to Archipelago
    /// </summary>
    public static class ArchipelagoTriggers
    {
        /// <summary>
        ///     Whenever a location from the game is sent.
        /// </summary>
        /// <param name="sender">The Object that sent the Location check.</param>
        /// <param name="e">The event arguements.</param>
        public static void OnLocalLocationSent(System.Object sender, LocationEventArgs e)
        {
            Logging.Log($"Location sent: {e.LocationName} of the location type: {e.LocationType}", "Locations");
            if (ArchipelagoClient.Authenticated)
            {
                Plugin.ArchipelagoClient.CheckLocation(e.LocationName);
            }
        }
        /// <summary>
        ///     Triggered when a connection to the Archipelago server has been established.
        /// </summary>
        public static void OnConnectToArchipelago()
        {

            // Only sync if rooms are already initialized (connected mid-run, not from main menu)
            if (ModInstance.HasInitializedRooms)
            {
                ModRoomManager.ReloadArrays();
                ModRoomManager.SyncRoomPoolsWithArchipelago();
            }
            if (ModInstance.IsInRun && !ModInstance.RanStartOfDay)
            {
                ModItemManager.LoadInventories();
                GameObject.Find("__SYSTEM/HUD/Stars").SetActiveRecursively(true);
                // Handle Start of day code for Permanent items (and maybe curses later).
                ModItemManager.StartOfDay();
                ModItemManager.ReplaceItemsWithAP();
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
                Unlocks.AttemptPrePatch(); //Apply patches to the FSMs
                Unlocks.AppleOrchard.PreventDefault();
                Unlocks.WestGatePath.PreventDefault();
                Unlocks.SatelliteDish.PreventDefault();
                Plugin.UniqueItemManager.StartOfDay();
                ModRoomManager.StartOfDay();
                Plugin.ArchipelagoClient.DeathLinkHandler.KillPlayer();
                ModRoomManager.HLCFix();
            }
        }
    }
}
