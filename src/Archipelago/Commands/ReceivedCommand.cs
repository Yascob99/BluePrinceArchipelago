using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Rooms;
using System.Collections.Generic;
using System.Linq;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for listing items received from archipelago.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class ReceivedCommand(string name) : Command(name)
    {
        private readonly string _Description = "Lists items received from Archipelago";
        public override string Description
        {
            get { return _Description; }
        }
        private readonly string _Syntax = "Usage:\n\t/received - List all received items\n\t/received rooms - List only received rooms\n\t/received items - List only received non-room items\n\t/received count - Show counts by category";
        public override string Syntax
        {
            get { return _Syntax; }
        }

        public override void Run(List<string> Args)
        {
            if (!ArchipelagoClient.Authenticated)
            {
                ArchipelagoConsole.LogMessage("Not connected to Archipelago.");
                return;
            }

            var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
            if (receivedItems == null || receivedItems.Count == 0)
            {
                ArchipelagoConsole.LogMessage("No items received from Archipelago yet.");
                return;
            }

            string subcommand = Args.Count > 0 ? Args[1].ToLower() : "all";

            if (subcommand == "rooms")
            {
                ListReceivedRooms(receivedItems);
            }
            else if (subcommand == "items")
            {
                ListReceivedNonRooms(receivedItems);
            }
            else if (subcommand == "count")
            {
                ShowCounts(receivedItems);
            }
            else
            {
                ListAll(receivedItems);
            }
        }

        /// <summary>
        ///     Outputs the received items to the console.
        /// </summary>
        /// <param name="receivedItems">A list of items received from Archipelago.</param>
        private void ListReceivedRooms(List<string> receivedItems)
        {
            var rooms = receivedItems.Where(i => ModRoomManager.GetRoomByName(i.ToUpper()) != null).ToList();
            ArchipelagoConsole.LogMessage($"=== Received Rooms ({rooms.Count}) ===");
            foreach (var room in rooms)
            {
                ModRoom modRoom = ModRoomManager.GetRoomByName(room.ToUpper());
                string poolInfo = modRoom != null ? $" [Pool: {modRoom.RoomsLeftInPool}/{modRoom.RoomPoolCount}]" : "";
                ArchipelagoConsole.LogMessage($"  {room}{poolInfo}");
            }
        }

        /// <summary>
        ///     A list of items received from archipelago excluding rooms.
        /// </summary>
        /// <param name="receivedItems"></param>
        private void ListReceivedNonRooms(List<string> receivedItems)
        {
            var nonRooms = receivedItems.Where(i => ModRoomManager.GetRoomByName(i.ToUpper()) == null).ToList();
            ArchipelagoConsole.LogMessage($"=== Received Non-Room Items ({nonRooms.Count}) ===");
            foreach (var item in nonRooms)
            {
                string type = ModItemManager.GetItemType(item) ?? "Unknown";
                ArchipelagoConsole.LogMessage($"  [{type}] {item}");
            }
        }

        /// <summary>
        ///     Lists the counts of certain types of items.
        /// </summary>
        /// <param name="receivedItems">The list of received items.</param>
        private void ShowCounts(List<string> receivedItems)
        {
            int roomCount = 0;
            int permanentCount = 0;
            int junkCount = 0;
            int unknownCount = 0;

            foreach (var item in receivedItems)
            {
                if (ModRoomManager.GetRoomByName(item.ToUpper()) != null)
                {
                    roomCount++;
                }
                else
                {
                    string type = ModItemManager.GetItemType(item);
                    if (type == "Permanent") permanentCount++;
                    else if (type == "Junk") junkCount++;
                    else unknownCount++;
                }
            }

            ArchipelagoConsole.LogMessage($"=== Received Item Counts ===");
            ArchipelagoConsole.LogMessage($"  Rooms:     {roomCount}");
            ArchipelagoConsole.LogMessage($"  Permanent: {permanentCount}");
            ArchipelagoConsole.LogMessage($"  Junk:      {junkCount}");
            ArchipelagoConsole.LogMessage($"  Unknown:   {unknownCount}");
            ArchipelagoConsole.LogMessage($"  Total:     {receivedItems.Count}");
        }

        /// <summary>
        ///     Lists the data of all received items.
        /// </summary>
        /// <param name="receivedItems">The list of received items.</param>
        private void ListAll(List<string> receivedItems)
        {
            ArchipelagoConsole.LogMessage($"=== All Received Items ({receivedItems.Count}) ===");
            foreach (var item in receivedItems)
            {
                bool isRoom = ModRoomManager.GetRoomByName(item.ToUpper()) != null;
                string type = isRoom ? "Room" : (ModItemManager.GetItemType(item) ?? "Unknown");
                ArchipelagoConsole.LogMessage($"  [{type}] {item}");
            }
        }
    }
}
