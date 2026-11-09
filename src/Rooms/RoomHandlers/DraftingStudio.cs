using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    public class DraftingStudio : RoomHandler {

        public DraftingStudio()
        {
            Logging.Log("Initializing Drafting Studio.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            ModInstance.GlobalManager.GetBoolVariable("Clock Tower Unlocked").Value = ModRoomManager.GetRoomByName("CLOCK TOWER").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("The Kennel Unlocked").Value = ModRoomManager.GetRoomByName("THE KENNEL").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Vestibule Unlocked").Value = ModRoomManager.GetRoomByName("VESTIBULE").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Dovecote Unlocked").Value = ModRoomManager.GetRoomByName("DOVECOTE").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Solarium Unlocked").Value = ModRoomManager.GetRoomByName("SOLARIUM").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Dormitory Unlocked").Value = ModRoomManager.GetRoomByName("DORMITORY").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Casino Unlocked").Value = ModRoomManager.GetRoomByName("CASINO").IsUnlocked;
            GetScoutHints();
        }

        private string[] GetScoutHints() {
            string[] FloorplanLocations = [];
            List<long> locationids = [];
            foreach (string FloorplanLocation in FloorplanLocations) {
                long locationid = Plugin.ArchipelagoClient.GetLocationFromName(FloorplanLocation);
                if (locationid != -1)
                {
                    locationids.Add(locationid);
                }
            }
           
            Plugin.ArchipelagoClient.ScoutLocationHint(locationids.ToArray());
            List<string> ScoutHints = [];
            foreach (long locationid in locationids)
            {
                ScoutedItemInfo scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];

                string playerName = scout?.Player?.Name ?? "";
                string itemName = scout?.ItemName ?? "";
                string description = scout?.Flags.ItemFlagDescription();

                string ScoutHint = $"{playerName}'s {itemName} {description}";
                ScoutHints.Add(ScoutHint);
            }
            return ScoutHints.ToArray();
        }
    }
}
