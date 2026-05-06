
using System;
using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;

namespace BluePrinceArchipelago.Models
{
    public class ShopItem
    {
        public string Name { get; set; }
        public string[] DescriptionLines { get; set; }
        public string ScoutHint { get; set; }
        private string[] _ScoutHintParts;

        public virtual string GetScoutHint()
        {
            try {
                if (!ArchipelagoClient.Authenticated || ArchipelagoClient.ServerData.ReceivedItems.Contains(Name))
                    return Name;

                if (ScoutHint != null)
                    return ScoutHint;

                string locationName = Name;
                if (!Name.Contains("Upgrade Disk"))
                    locationName = Name + " First Pickup";

                long locationid = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
                if (locationid == -1)
                {
                    Logging.LogWarning($"Location '{locationName}' not found in Archipelago data.");
                    return Name; // Fallback to the original name if location is not found
                }
                Plugin.ArchipelagoClient.ScoutLocationHint([locationid]);
                ScoutedItemInfo scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];

                string playerName = scout?.Player?.Name ?? "";
                string itemName = scout?.ItemName ?? "";
                string description = scout?.Flags.ItemFlagDescription();

                ScoutHint = $"{playerName}'s {itemName} {description}";
                return ScoutHint;
            } catch (Exception ex)
            {
                Logging.LogWarning($"Unable to get scout hint for '{Name}': {ex}");
                return Name; // Fallback to the original name in case of any errors
            }
        }

        protected string GetScoutHint(string prefix)
        {
            try {
                if (!ArchipelagoClient.Authenticated || ArchipelagoClient.ServerData.ReceivedItems.Contains(Name))
                    return Name;

                if (ScoutHint != null)
                    return ScoutHint;

                string locationName = prefix + Name;

                long locationid = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
                if (locationid == -1)
                {
                    Logging.LogWarning($"Location '{locationName}' not found in Archipelago data.");
                    return Name; // Fallback to the original name if location is not found
                }
                Plugin.ArchipelagoClient.ScoutLocationHint([locationid]);
                ScoutedItemInfo scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];

                string playerName = scout?.Player?.Name ?? "";
                string itemName = scout?.ItemName ?? "";
                string description = scout?.Flags.ItemFlagDescription();

                ScoutHint = $"{playerName}'s {itemName} {description}";
                return ScoutHint;
            } 
            catch (Exception ex)
            {
                Logging.LogWarning($"Unable to get scout hint for '{Name}': {ex}");
                return Name; // Fallback to the original name in case of any errors
            }
        }

        public string[] GetScoutHintParts(int maxDescriptionLines = 2)
        {
            try {
                if (!ArchipelagoClient.Authenticated || ArchipelagoClient.ServerData.ReceivedItems.Contains(Name))
                    return [Name, .. DescriptionLines];

                if (maxDescriptionLines <= 0)
                    return [GetScoutHint()];

                if (_ScoutHintParts != null)
                    return _ScoutHintParts;
                
                string locationName = Name;
                if (!Name.Contains("Upgrade Disk"))
                    locationName = Name + " First Pickup";

                long locationid = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
                if (locationid == -1)
                {
                    Logging.LogWarning($"Location '{locationName}' not found in Archipelago data.");
                    return [Name, .. DescriptionLines]; // Fallback to the original name and description if location is not found
                }
                Plugin.ArchipelagoClient.ScoutLocationHint([locationid]);
                ScoutedItemInfo scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];

                string playerName = scout?.Player?.Name ?? "";
                string itemName = scout?.ItemName ?? "";
                string description = scout?.Flags.ItemFlagDescription();

                if (maxDescriptionLines < 2)
                    _ScoutHintParts = [itemName, $"{playerName}'s {description}"];
                else
                    _ScoutHintParts = [itemName, $"{playerName}'s", description];
                return _ScoutHintParts;
            } 
            catch (Exception ex)
            {
                Logging.LogWarning($"Unable to get scout hint parts for '{Name}': {ex}");
                return [Name, .. DescriptionLines]; // Fallback to the original name and description in case of any errors
            }
        }
    }

    public class BookshopItem : ShopItem
    {
        public override string GetScoutHint()
        {
            return GetScoutHint("Bookshop - ");
        }
    }

    public class GiftShopItem : ShopItem
    {
        public override string GetScoutHint()
        {
            return GetScoutHint("Gift Shop - ");
        }
    }
}