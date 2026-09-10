using BluePrinceArchipelago.Rooms;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for resyncing the room pool with the received item pool.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class SyncCommand(string name) : Command(name)
    {
        private readonly string _Description = "Syncs room pool with Archipelago received items";
        public override string Description
        {
            get { return _Description; }
        }
        private readonly string _Syntax = "Usage:\n\t/sync rooms - Sync room pool from Archipelago received items\n\t/sync status - Show sync status";
        public override string Syntax
        {
            get { return _Syntax; }
        }

        public override void Run(List<string> Args)
        {
            if (Args.Count < 1)
            {
                ArchipelagoConsole.LogMessage($"Error: No subcommand provided.\n{_Syntax}");
                return;
            }

            string subcommand = Args[0].ToLower();

            if (subcommand == "rooms")
            {
                SyncRoomsFromArchipelago();
            }
            else if (subcommand == "status")
            {
                ShowSyncStatus();
            }
            else
            {
                ArchipelagoConsole.LogMessage($"Error: Unknown subcommand '{subcommand}'.\n{_Syntax}");
            }
        }
        /// <summary>
        ///     A function to rebuild the roompool to match the received rooms from archipelago.
        /// </summary>
        private void SyncRoomsFromArchipelago()
        {
            if (!ArchipelagoClient.Authenticated)
            {
                ArchipelagoConsole.LogMessage("Error: Not connected to Archipelago. Please connect first.");
                return;
            }

            if (!ModInstance.HasInitializedRooms)
            {
                ArchipelagoConsole.LogMessage("Error: Rooms have not been initialized yet. Start a run first.");
                return;
            }

            // Check if RoomDraftSanity is enabled
            if (!ArchipelagoOptions.RoomDraftSanity)
            {
                ArchipelagoConsole.LogMessage("RoomDraftSanity is disabled in your Archipelago options.");
                ArchipelagoConsole.LogMessage("Room drafts will use vanilla behavior. No sync needed.");
                return;
            }

            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;

            // Re-load arrays first to ensure we have fresh references
            ModRoomManager.ReloadArrays();

            // First, clear ALL rooms for Archipelago mode (disables vanilla handling too)
            ModRoomManager.ClearAllRoomsForArchipelago();

            int syncedCount = 0;
            int skippedCount = 0;

            // Then unlock rooms that are in received items
            if (receivedItems != null && receivedItems.Count > 0)
            {
                foreach (string itemName in receivedItems)
                {
                    if (ModRoomManager.UnlockRoomForArchipelago(itemName))
                    {
                        syncedCount++;
                    }
                    else
                    {
                        // Item is not a room, skip it
                        skippedCount++;
                    }
                }
            }

            // Update the pools after sync
            ModRoomManager.UpdateRoomPools();

            ArchipelagoConsole.LogMessage($"Room sync complete: {syncedCount} rooms unlocked, {skippedCount} non-room items skipped.");
            ArchipelagoConsole.LogMessage("All rooms set to Archipelago mode (vanilla handling disabled).");
        }

        /// <summary>
        ///     Displays the current information on what data has been received from archipelago.
        /// </summary>
        private void ShowSyncStatus()
        {
            if (!ArchipelagoClient.Authenticated)
            {
                ArchipelagoConsole.LogMessage("Status: Not connected to Archipelago");
                return;
            }

            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            int receivedRoomCount = 0;
            int unlockedRoomCount = 0;

            // Count received rooms
            if (receivedItems != null)
            {
                foreach (string itemName in receivedItems)
                {
                    if (ModRoomManager.GetRoomByName(itemName.ToUpper()) != null)
                    {
                        receivedRoomCount++;
                    }
                }
            }

            // Count unlocked rooms
            foreach (var room in ModRoomManager.Rooms)
            {
                if (room.IsUnlocked && !room.UseVanilla)
                {
                    unlockedRoomCount++;
                }
            }

            ArchipelagoConsole.LogMessage($"=== Sync Status ===");
            ArchipelagoConsole.LogMessage($"Connected: Yes");
            ArchipelagoConsole.LogMessage($"Received room items: {receivedRoomCount}");
            ArchipelagoConsole.LogMessage($"Currently unlocked (non-vanilla): {unlockedRoomCount}");
            ArchipelagoConsole.LogMessage($"Total items received: {receivedItems?.Count ?? 0}");
        }
    }
}
