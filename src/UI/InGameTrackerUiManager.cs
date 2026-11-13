#if Bep
using TMPro;
#endif
#if ML
using Il2CppTMPro;
#endif
using System.Text;
using BluePrinceArchipelago.Utils;
using UnityEngine;
using BluePrinceArchipelago.Archipelago;
using System.Linq;
using System.Collections.Generic;
using System;

namespace BluePrinceArchipelago.UI
{
	public static class InGameTrackerUiManager {
		private const int MAX_TRUNK = 10;
		private static readonly string[] MORA_JAI_BOX_ROOMS = 
        {
			ROOM.LOST_AND_FOUND,
			ROOM.SOLARIUM,
			ROOM.TUNNEL,
			ROOM.TRADING_POST,
			ROOM.TOMB,
			ROOM.CLOSED_EXHIBIT,
			ROOM.MASTER_BEDROOM,
		};

		private static readonly string[] UPGRADE_DISK_ROOMS = 
        {
			ROOM.OFFICE,
			ROOM.MORNING_ROOM,
			ROOM.HER_LADYSHIPS_CHAMBER,
			ROOM.COMMISSARY,
			ROOM.GARAGE,
			ROOM.GREAT_HALL,
			ROOM.VAULT,
			ROOM.TRADING_POST,
			ROOM.FREEZER,
			ROOM.TOMB,
			ROOM.THE_FOUNDATION,
			ROOM.LOST_AND_FOUND,
			ROOM.MECHANARIUM,
			ROOM.ARCHIVES,
		};

		private static readonly Dictionary<string, List<string>> PUZZLES_IN_ROOMS = new() 
		{
            {ROOM.THE_FOUNDATION, new() {"Lower The Foundation Elevator"}},
			{ROOM.TOMB, new() {"Solve Tomb Puzzle 1", "Solve Tomb Puzzle 2"}},
			{ROOM.SECRET_GARDEN, new() {"Break Secret Garden Wall"}},
            {ROOM.GREENHOUSE, new() {"Break Greenhouse Wall"}},
            {ROOM.WEIGHT_ROOM, new() {"Break Weight Room Wall"}},
            {ROOM.LABORATORY, new() {"Laboratory Puzzle"}},
            {ROOM.CLOISTER, new() {"Allowance Token - Cloister Statue"}},
            {ROOM.UTILITY_CLOSET, new() {"VAC Controls"}},
            {ROOM.HOVEL, new() {"Gasline Valve - Hovel"}},
            {ROOM.SCHOOLHOUSE, new() {"Gasline Valve - Schoolhouse"}},
            {ROOM.ENTRANCE_HALL, new() {"Entrance Hall West Vase", "Entrance Hall East Vase"}},
            {ROOM.SHELTER, new() {"Red Letter VII"}},
            {ROOM.STUDY, new() {"Red Letter 2", "Floorplan - Closed Exhibit"}},
            {ROOM.OFFICE, new() {"Red Letter ∞"}},
            {ROOM.DRAWING_ROOM, new() {"Red Letter SIX"}},
            {ROOM.DRAFTING_STUDIO, new() {"Red Letter 5"}},
            {ROOM.BOUDOIR, new() {"Red Letter 4"}},
            {ROOM.OBSERVATORY, new() {"Floorplan - Planetarium"}}
        };

        private static readonly List<string> STUDIO_ADDITIONS_LOCATIONS_NAME =
        [
            ROOM.DOVECOTE,
            ROOM.THE_KENNEL, //Mismatch ???
            ROOM.CLOCK_TOWER,
            ROOM.CLASSROOM,
            ROOM.SOLARIUM,
            ROOM.DOVECOTE,
            ROOM.VESTIBULE,
            ROOM.CASINO,
        ];

        private static readonly Dictionary<string, List<string>> ITEMS_IN_ROOMS = new()
        {
            //TODO : CABINET KEYS 1 2 3
            //TODO : Key of Aries
            //TODO : Microchips 1 3
            {ROOM.ENTRANCE_HALL, new() {ITEM.MICROCHIP_MIDDLE_SIZED}},
            {ROOM.THE_ARMORY, new() {ITEM.MORNING_STAR, ITEM.TORCH, ITEM.KNIGHTS_SHIELD, ITEM.THE_AXE}},
            {ROOM.ANTECHAMBER, new() {ITEM.BASEMENT_KEY}},
            {ROOM.SHOWROOM, new() {ITEM.CHRONOGRAPH, ITEM.EMERALD_BRACELET, ITEM.MASTER_KEY, ITEM.MOON_PENDANT, ITEM.ORNATE_COMPASS, ITEM.SILVER_SPOON}},
            {ROOM.ROOM_46, new() {ITEM.CROWN}},
            {ROOM.SHRINE, new() {ITEM.CURSED_EFFIGY}},
            {ROOM.AQUARIUM, new() {ITEM.BROKEN_LEVER, ITEM.COMPASS, ITEM.SHOVEL, ITEM.TREASURE_MAP}},
            {ROOM.ARCHIVES, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.TREASURE_MAP}},
            {ROOM.ATTIC, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.LOCK_PICK_KIT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.SECRET_GARDEN_KEY, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149}},
            {ROOM.BILLIARD_ROOM, new() {ITEM.BROKEN_LEVER, ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.SALT_SHAKER, ITEM.SECRET_GARDEN_KEY, ITEM.SILVER_KEY}},
            {ROOM.CLOCK_TOWER, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.COMPASS, ITEM.LOCK_PICK_KIT, ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.STOPWATCH, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_233}},
            {ROOM.CLOSED_EXHIBIT, new() {ITEM.PAPER_CROWN, ITEM.BROKEN_LEVER, ITEM.LOCK_PICK_KIT, ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.VAULT_KEY_370}},
            {ROOM.CLOSET, new() {ITEM.BROKEN_LEVER, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.LUCKY_RABBITS_FOOT, ITEM.METAL_DETECTOR, ITEM.RUNNING_SHOES, ITEM.SHOVEL, ITEM.SILVER_KEY, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK}},
            {ROOM.CONSERVATORY, new() {ITEM.BROKEN_LEVER, ITEM.MAGNIFYING_GLASS, ITEM.SHOVEL}},
            {ROOM.DRAFTING_STUDIO, new() {ITEM.BROKEN_LEVER, ITEM.COMPASS, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.FOYER, new() {ITEM.BROKEN_LEVER, ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT, ITEM.RUNNING_SHOES, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK, ITEM.TREASURE_MAP}},
            {ROOM.GARAGE, new() {ITEM.BROKEN_LEVER, ITEM.LOCK_PICK_KIT, ITEM.METAL_DETECTOR, ITEM.RUNNING_SHOES, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.HOVEL, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.COMPASS, ITEM.LUCKY_RABBITS_FOOT, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_304, ITEM.WATERING_CAN}},
            {ROOM.KITCHEN, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.SALT_SHAKER}},
            {ROOM.LABORATORY, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.KEYCARD, ITEM.MAGNIFYING_GLASS, ITEM.VAULT_KEY_149}},
            {ROOM.LOCKER_ROOM, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.RUNNING_SHOES, ITEM.SECRET_GARDEN_KEY, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149, ITEM.VAULT_KEY_233}},
            {ROOM.MUSIC_ROOM, new() {ITEM.BROKEN_LEVER, ITEM.CAR_KEYS, ITEM.PRISM_KEY, ITEM.SILVER_KEY, ITEM.SLEDGE_HAMMER, ITEM.KEYCARD, ITEM.SECRET_GARDEN_KEY, ITEM.VAULT_KEY_149, ITEM.VAULT_KEY_233, ITEM.VAULT_KEY_304, ITEM.VAULT_KEY_370, }},
            {ROOM.OBSERVATORY, new() {ITEM.BROKEN_LEVER, ITEM.COMPASS, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149}},
            {ROOM.SAUNA, new() {ITEM.BROKEN_LEVER, ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.RUNNING_SHOES, ITEM.SLEEPING_MASK, ITEM.VAULT_KEY_233}},
            {ROOM.SCHOOLHOUSE, new() {ITEM.BROKEN_LEVER, ITEM.SLEDGE_HAMMER}},
            {ROOM.SECRET_GARDEN, new() {ITEM.BROKEN_LEVER, ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.SECURITY, new() {ITEM.BROKEN_LEVER, ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.VAULT_KEY_149}},
            {ROOM.SERVANTS_QUARTERS, new() {ITEM.BROKEN_LEVER, ITEM.METAL_DETECTOR, ITEM.RUNNING_SHOES, ITEM.SHOVEL}},
            {ROOM.STOREROOM, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.TOOLSHED, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.GEAR_WRENCH, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.SECRET_GARDEN_KEY, ITEM.SHOVEL, ITEM.SILVER_KEY, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149, ITEM.WATERING_CAN}},
            {ROOM.UTILITY_CLOSET, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.WEIGHT_ROOM, new() {ITEM.BROKEN_LEVER, ITEM.RUNNING_SHOES}},
            {ROOM.WINE_CELLAR, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.LOCK_PICK_KIT, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_233}},
            {ROOM.WORKSHOP, new() {ITEM.BROKEN_LEVER, ITEM.BATTERY_PACK, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.BURNING_GLASS, ITEM.DETECTOR_SHOVEL, ITEM.DOWSING_ROD, ITEM.ELECTROMAGNET, ITEM.JACK_HAMMER, ITEM.LUCKY_PURSE, ITEM.PICK_SOUND_AMPLIFIER, ITEM.POWER_HAMMER}},
            {ROOM.MAIL_ROOM, new() {ITEM.BATTERY_PACK, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.TREASURE_MAP}},
            {ROOM.PATIO, new() {ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.METAL_DETECTOR, ITEM.SHOVEL}},
            {ROOM.ROOT_CELLAR, new() {ITEM.BATTERY_PACK, ITEM.METAL_DETECTOR, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP}},
            {ROOM.STUDY, new() {ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.TRADING_POST, new() {ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.GEAR_WRENCH, ITEM.HALL_PASS, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.PRISM_KEY, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.SECRET_GARDEN_KEY, ITEM.SHOVEL, ITEM.SILVER_KEY, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK, ITEM.TELESCOPE, ITEM.VAULT_KEY_149, ITEM.VAULT_KEY_233, ITEM.VAULT_KEY_304, ITEM.VAULT_KEY_370}},
            {ROOM.BEDROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.SLEEPING_MASK}},
            {ROOM.DEN, new() {ITEM.CAR_KEYS, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.DINING_ROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COUPON_BOOK, ITEM.SALT_SHAKER}},
            {ROOM.GALLERY, new() { ITEM.KEY_8, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT, ITEM.VAULT_KEY_233}},
            {ROOM.GYMNASIUM, new() {ITEM.CAR_KEYS, ITEM.RUNNING_SHOES}},
            {ROOM.LAUNDRY_ROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.KEYCARD, ITEM.RUNNING_SHOES, ITEM.SLEEPING_MASK}},
            {ROOM.LOCKSMITH, new() {ITEM.CAR_KEYS, ITEM.LOCK_PICK_KIT, ITEM.PRISM_KEY, ITEM.SECRET_GARDEN_KEY, ITEM.SILVER_KEY}},
            {ROOM.MORNING_ROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COUPON_BOOK, ITEM.LUCKY_RABBITS_FOOT, ITEM.SALT_SHAKER, ITEM.SHOVEL, ITEM.VAULT_KEY_233}},
            {ROOM.OFFICE, new() {ITEM.CAR_KEYS, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.MAGNIFYING_GLASS, ITEM.VAULT_KEY_233}},
            {ROOM.PARLOR, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.MAGNIFYING_GLASS, ITEM.VAULT_KEY_304}},
            {ROOM.RUMPUS_ROOM, new() {ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149}},
            {ROOM.THE_POOL, new() {ITEM.CAR_KEYS, ITEM.KEYCARD, ITEM.RUNNING_SHOES}},
            {ROOM.WALK_IN_CLOSET, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.SLEEPING_MASK, ITEM.TELESCOPE, ITEM.VAULT_KEY_304}},
            {ROOM.BALLROOM, new() {ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT}},
            {ROOM.BOUDOIR, new() {ITEM.COIN_PURSE, ITEM.MAGNIFYING_GLASS, ITEM.SLEEPING_MASK}},
            {ROOM.DRAWING_ROOM, new() {ITEM.COIN_PURSE, ITEM.MAGNIFYING_GLASS, ITEM.VAULT_KEY_304}},
            {ROOM.HER_LADYSHIPS_CHAMBER, new() {ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.SECRET_GARDEN_KEY, ITEM.SILVER_KEY, ITEM.SLEEPING_MASK, ITEM.TELESCOPE, ITEM.VAULT_KEY_304}},
            {ROOM.MASTER_BEDROOM, new() {ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.MAGNIFYING_GLASS, ITEM.SLEEPING_MASK, ITEM.VAULT_KEY_233}},
            {ROOM.SOLARIUM, new() {ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT, ITEM.SHOVEL}},
            {ROOM.THRONE_ROOM, new() {ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.LUCKY_RABBITS_FOOT, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149, ITEM.VAULT_KEY_233, ITEM.VAULT_KEY_370}},
            {ROOM.COMMISSARY, new() {ITEM.COMPASS, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK}},
            {ROOM.TERRACE, new() {ITEM.COMPASS, ITEM.SHOVEL}},
            {ROOM.TROPHY_ROOM, new() {ITEM.COMPASS, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP, ITEM.VAULT_KEY_149}},
            {ROOM.CONFERENCE_ROOM, new() {ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.VAULT_KEY_304}},
            {ROOM.LIBRARY, new() {ITEM.COUPON_BOOK, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.NOOK, new() {ITEM.COUPON_BOOK, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.PANTRY, new() {ITEM.COUPON_BOOK, ITEM.SALT_SHAKER}},
            {ROOM.SECRET_PASSAGE, new() {ITEM.COUPON_BOOK, ITEM.TREASURE_MAP}},
            {ROOM.VAULT, new() {ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT}},
            {ROOM.TOMB, new() {ITEM.DIARY_KEY}},
            {ROOM.LOST_AND_FOUND, new() {ITEM.GEAR_WRENCH, ITEM.HALL_PASS, ITEM.LUCKY_RABBITS_FOOT, ITEM.PRISM_KEY, ITEM.REPELLENT, ITEM.SECRET_GARDEN_KEY, ITEM.SILVER_KEY, ITEM.STOPWATCH, ITEM.TELESCOPE, ITEM.VAULT_KEY_370, ITEM.WATERING_CAN}},
            {ROOM.CLASSROOM, new() {ITEM.HALL_PASS, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS}},
            {ROOM.DORMITORY, new() {ITEM.HALL_PASS, ITEM.KEYCARD, ITEM.RUNNING_SHOES, ITEM.SLEEPING_MASK}},
            {ROOM.CASINO, new() {ITEM.LUCKY_RABBITS_FOOT}},
            {ROOM.THE_KENNEL, new() {ITEM.LUCKY_RABBITS_FOOT, ITEM.VAULT_KEY_233}},
            {ROOM.BOILER_ROOM, new() {ITEM.METAL_DETECTOR, ITEM.SHOVEL}},
            {ROOM.COURTYARD, new() {ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.DOVECOTE, new() {ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
            {ROOM.GREENHOUSE, new() {ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER, ITEM.WATERING_CAN}},
            {ROOM.VERANDA, new() {ITEM.METAL_DETECTOR, ITEM.SHOVEL, ITEM.SLEDGE_HAMMER} },
            {ROOM.TUNNEL, new() {ITEM.SHOVEL, ITEM.SLEDGE_HAMMER}},
        };


        public static string getCurrentRoomName()
		{
			GameObject headline = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/HEADLINE");
			string roomName = headline.GetComponent<TextMeshPro>().text;
			return RemoveColorCode(roomName);
		}

		private static string RemoveColorCode(string roomName) 
		{
			if (roomName[0] != '<')
			{
				return roomName;
			}
			int begining = roomName.IndexOf('>') + 1;
			int end = roomName.IndexOf('/') - 1;
			return roomName[begining..end];
		}

		public static void ShowArchipelagoRoomInfo()
        {
            //TODO : Beware of the upgraded floorplans
            string roomName = getCurrentRoomName();
            Logging.LogWarning($"Room Selected : {roomName}", "TRACKER");

            StringBuilder archipelagoStats = new();

			Logging.LogWarning($"Drafted location", "TRACKER");
			archipelagoStats.Append(DraftedLocation(roomName));
			Logging.LogWarning($"Trunk location", "TRACKER");
			archipelagoStats.Append(TrunkLocations(roomName));
			Logging.LogWarning($"Mora Jai location", "TRACKER");
			archipelagoStats.Append(MoraJaiBoxLocation(roomName));
			Logging.LogWarning($"Upgrade Disk location", "TRACKER");
            archipelagoStats.Append(UpgradeDiskLocation(roomName));
            if (roomName == ROOM.DRAFTING_STUDIO)
            {
                archipelagoStats.Append(StudioAdditionsLocations());
            }
            Logging.LogWarning($"Puzzle location", "TRACKER");
            archipelagoStats.Append(PuzzleLocations(roomName));
            Logging.LogWarning($"Item location", "TRACKER");
            archipelagoStats.Append(ItemPickupLocations(roomName));

            //TODO : Studio Additions
            //TODO : Found Floorplans
            //TODO : Archipelago options check (sanities)
            //TODO : Estate Locations

            GameObject description = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/scalers/Description");
			description.GetComponent<TextMeshPro>().text = archipelagoStats.ToString();
		}

        private static string DraftedLocation(string roomName)
        {
			string prefix = "Drafted : ";
            if (roomName == ROOM.ENTRANCE_HALL || roomName == ROOM.ROOM_46)
            {
                return "";
            }
            if (roomName == ROOM.BUNK_ROOM)
            {
                string[] locationNames = { $"First Draft - {roomName}", $"First Draft - {roomName} 2" };
                int completed = LocationCompletion(locationNames);
                return prefix + ResultText(completed, locationNames.Length);
			}
            int complete = LocationCompletion($"First Draft - {roomName}");
			return prefix + ResultText(complete, 1);
        }

		//TODO : Clean that ?
        private static string TrunkLocations(string roomName)
        {
            //Compute trunk amount for this room and store trunks locations
            int totalTrunkLocations = MAX_TRUNK;
            long[] trunkLocationIds = new long[MAX_TRUNK];

            for (int i = 0; i < MAX_TRUNK; ++i)
            {
                long id = Plugin.ArchipelagoClient.GetLocationFromName($"Locked Trunk - {roomName} - {i + 1}");
                if (id == -1)
                {
                    totalTrunkLocations = i;
                    break;
                }
                else
                {
                    trunkLocationIds[i] = id;
                }
            }

			if (totalTrunkLocations == 0)
			{
				return "";
			}

            int trunksFound = 0;
            for (int i = 0; i < totalTrunkLocations; ++i)
            {
                long locationId = trunkLocationIds[i];
                if (ArchipelagoClient.ServerData.CheckedLocations.Contains(locationId))
                {
                    ++trunksFound;
                }
                else
                {
                    break;
                }
            }
            return "<br>Trunks : " + ResultText(trunksFound, totalTrunkLocations);
        }

        private static string MoraJaiBoxLocation(string roomName)
        {
            if (!MORA_JAI_BOX_ROOMS.Contains(roomName))
            {
				return "";
            }
			int complete = LocationCompletion($"{roomName} Mora Jai Box");
			return "<br>Mora Jai Box : " + ResultText(complete, 1);
		}

		private static string UpgradeDiskLocation(string roomName)
		{
			if (!UPGRADE_DISK_ROOMS.Contains(roomName))
			{
				return "";
			}
			if (roomName == ROOM.TRADING_POST)
			{
				string[] locationNames = { "Upgrade Disk - Trading Post Dynamite", "Upgrade Disk - Trading Post Trade" };
				int completed = LocationCompletion(locationNames);
				return "<br>Upgrade Disks : " + ResultText(completed, locationNames.Length);
			}
			int complete = LocationCompletion($"Upgrade Disk - {roomName}");
			return "<br>Upgrade Disk : " + ResultText(complete, 1);
		}

        // TODO : RENAME
        private static string IndividualLocations(string roomName, Dictionary<string, List<string>> locationsInRooms, string categoryName, Func<string, string> nameToLocation)
        {
            if (!locationsInRooms.ContainsKey(roomName))
            {
                return "";
            }
            return IndividualLocationsFromArray(locationsInRooms[roomName], categoryName, nameToLocation);
        }
        
        //TODO : Rename
        private static string IndividualLocationsFromArray(List<string> locationList, string categoryName, Func<string, string> nameToLocation)
        {
            StringBuilder trackerDisplay = new();
            foreach (string locationName in locationList)
            {
                string location = nameToLocation(locationName);
                string displayLocationName = locationName.ToTitleCase();
                string locationText = IsCompleted(location) ? CompletedLocationText(displayLocationName) : UncompletedLocationText(displayLocationName);
                trackerDisplay.Append(locationText);
                trackerDisplay.Append(", ");
            }
            trackerDisplay.Remove(trackerDisplay.Length - 2, 2);
            return "<br>" + categoryName + " : " + trackerDisplay.ToString();
        }

        private static string PuzzleLocations(string roomName)
		{
            return IndividualLocations(roomName, PUZZLES_IN_ROOMS, "Puzzle", x => x);
        }

        private static string ItemPickupLocations(string roomName)
        {
            return IndividualLocations(roomName, ITEMS_IN_ROOMS, "Items", x => $"First Pickup - {x}");
        }

        private static string StudioAdditionsLocations()
        {
            return IndividualLocationsFromArray(STUDIO_ADDITIONS_LOCATIONS_NAME, "Studio Additions", x => $"Floorplan - {x}");
        }

        private static string ApplyRichTextTag(string text, string tag, string attribute = "")
        {
            if (attribute.Length == 0)
            {
                return $"<{tag}>{text}</{tag}>";
            }
            return $"<{tag}={attribute}>{text}</{tag}>";
        }

        private static string ColorText(string text, string color)
        {
            return ApplyRichTextTag(text, "color", color);
        }

        private static string StrikethroughText(string text)
        {
            return ApplyRichTextTag(text, "s");
        }

        private static string CompletedLocationText(string text)
        {
            return ColorText(StrikethroughText(text), "#D3D3D3");
        }

        private static string UncompletedLocationText(string text)
        {
            return ColorText(text, "#01A9FF");
        }

        private static string ResultText(int completed, int total)
        {
			string color = "yellow";
			if (completed == 0)
			{
				color = "red";
			}
			if (completed == total)
			{
				color = "green";
			}
			return ColorText($"{completed}/{total}", color);
		}

		private static int LocationCompletion(string[] locationNames)
		{
            int locationsCompleted = 0;
			int locationChecked = locationNames.Length;
			for (int i = 0; i < locationChecked; ++i)
            {
				if (IsCompleted(locationNames[i]))
				{
					locationsCompleted++;
				}
            }
            return locationsCompleted;
		}

        private static bool IsCompleted(string locationName)
        {
            long id = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
            if (id == -1)
            {
                Logging.LogError($"location : {locationName} not found", "TRACKER");
            }
            if (ArchipelagoClient.ServerData.CheckedLocations.Contains(id))
            {
                return true;
            }
            return false;
        }

        private static int LocationCompletion(string locationName)
        {
			if (IsCompleted(locationName))
			{
				return 1;
			}
			return 0;
        }
	}
}