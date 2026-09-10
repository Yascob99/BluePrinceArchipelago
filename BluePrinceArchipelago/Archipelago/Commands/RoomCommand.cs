using BluePrinceArchipelago.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for manipulating the room pool.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class RoomCommand(string name) : Command(name)
    {
        private readonly string _Description = "Manages the room pool - add, remove, list, or clear rooms";
        public override string Description
        {
            get { return _Description; }
        }
        private readonly string _Syntax = "Usage:\n\t/room add <RoomName> - Add a room to the pool\n\t/room remove <RoomName> - Remove a room from the pool\n\t/room list - List all rooms and their pool status\n\t/room list unlocked - List only unlocked rooms\n\t/room clear - Remove all non-vanilla rooms from pool\n\t/room clearall - Clear ALL rooms (for Archipelago mode)";
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

            // List doesn't require being in a run
            if (subcommand == "list")
            {
                bool unlockedOnly = Args.Count > 1 && Args[1].ToLower() == "unlocked";
                ListRooms(unlockedOnly);
                return;
            }

            // Other commands require being in a run
            if (!ModInstance.IsInRun)
            {
                ArchipelagoConsole.LogMessage("You are not currently in a run. You can only modify the pool during a run.");
                return;
            }

            if (subcommand == "add")
            {
                if (Args.Count < 2)
                {
                    ArchipelagoConsole.LogMessage("Error: No room name provided.\nUsage: /room add <RoomName>");
                    return;
                }
                string roomName = string.Join(" ", Args.Skip(1));
                AddRoomToPool(roomName);
            }
            else if (subcommand == "remove")
            {
                if (Args.Count < 2)
                {
                    ArchipelagoConsole.LogMessage("Error: No room name provided.\nUsage: /room remove <RoomName>");
                    return;
                }
                string roomName = string.Join(" ", Args.Skip(1));
                RemoveRoomFromPool(roomName);
            }
            else if (subcommand == "clear")
            {
                ClearPool();
            }
            else if (subcommand == "clearall")
            {
                ClearAllForArchipelago();
            }
            else
            {
                ArchipelagoConsole.LogMessage($"Error: Unknown subcommand '{subcommand}'.\n{_Syntax}");
            }
        }

        /// <summary>
        ///     Prints out the mod details and counts of the current room pool.
        /// </summary>
        /// <param name="unlockedOnly">Whether to display only rooms that have been unlocked.</param>
        private static void ListRooms(bool unlockedOnly)
        {
            var rooms = ModRoomManager.Rooms;
            if (rooms == null || rooms.Count == 0)
            {
                ArchipelagoConsole.LogMessage("No rooms have been initialized yet.");
                return;
            }

            int unlockedCount = 0;
            int lockedCount = 0;
            int vanillaCount = 0;

            ArchipelagoConsole.LogMessage(unlockedOnly ? "=== Unlocked Rooms ===" : "=== All Rooms ===");
            foreach (var room in rooms)
            {
                if (room.IsUnlocked) unlockedCount++;
                else lockedCount++;
                if (room.UseVanilla) vanillaCount++;

                if (unlockedOnly && !room.IsUnlocked) continue;

                string status = room.IsUnlocked ? "[UNLOCKED]" : "[LOCKED]";
                string vanilla = room.UseVanilla ? " (Vanilla)" : " (AP Mode)";
                string poolInfo = $"Pool: {room.RoomsLeftInPool}/{room.RoomPoolCount}";
                ArchipelagoConsole.LogMessage($"  {status} {room.Name}{vanilla} - {poolInfo}");
            }
            ArchipelagoConsole.LogMessage($"Summary: {unlockedCount} unlocked, {lockedCount} locked, {vanillaCount} vanilla mode");
        }

        /// <summary>
        ///     Adds a Room to the room pool.
        /// </summary>
        /// <param name="roomName">The name of the room to add.</param>
        private static void AddRoomToPool(string roomName)
        {
            ModRoom room = ModRoomManager.GetRoomByName(roomName.ToUpper());
            if (room == null)
            {
                ArchipelagoConsole.LogMessage($"Error: '{roomName}' is not a valid room name.");
                return;
            }

            room.IsUnlocked = true;
            room.RoomPoolCount++;
            ModRoomManager.UpdateRoomPools();
            ArchipelagoConsole.LogMessage($"Added '{room.Name}' to the pool. Pool count: {room.RoomPoolCount}");
        }

        /// <summary>
        ///     Removes a room from the current room pool.
        /// </summary>
        /// <param name="roomName">The name of the room to remove.</param>
        private static void RemoveRoomFromPool(string roomName)
        {
            ModRoom room = ModRoomManager.GetRoomByName(roomName.ToUpper());
            if (room == null)
            {
                ArchipelagoConsole.LogMessage($"Error: '{roomName}' is not a valid room name.");
                return;
            }

            if (!room.IsUnlocked)
            {
                ArchipelagoConsole.LogMessage($"'{room.Name}' is already not in the pool.");
                return;
            }

            room.IsUnlocked = false;
            ModRoomManager.UpdateRoomPools();
            ArchipelagoConsole.LogMessage($"Removed '{room.Name}' from the pool.");
        }

        /// <summary>
        ///     Empties the current room pool.
        /// </summary>
        private static void ClearPool()
        {
            ModRoomManager.EmptyDraftPool();
            ModRoomManager.UpdateRoomPools();
            ArchipelagoConsole.LogMessage("Cleared all non-vanilla rooms from the pool.");
        }

        /// <summary>
        ///     Clears all the rooms for archipelago then rebuilds the room pool based on received items and settings.
        /// </summary>
        private static void ClearAllForArchipelago()
        {
            ModRoomManager.ClearAllRoomsForArchipelago();
            if (ModInstance.IsInRun)
            {
                ModRoomManager.UpdateRoomPools();
            }
            ArchipelagoConsole.LogMessage("Cleared ALL rooms and disabled vanilla mode for Archipelago.");
        }
    }
}
