using System.Text;
using BluePrinceArchipelago.Utils;
using System.Linq;
using System.Collections.Generic;
using System;
using BluePrince.Tracker;

namespace BluePrinceArchipelago.Tracker
{
	public static class APTrackerPresenter {
		private const int MAX_TRUNK = 10;
        private const string LINE_BREAK = "<br>";
        private const string INDIVIDUAL_LOCATIONS_SEPARATOR = ", ";
        private const string SEPARATOR_BETWEEN_CATEGORY_AND_LOCATIONS = " : ";
        private const string COLOR_YELLOW = "yellow";
        private const string COLOR_RED = "red";
        private const string COLOR_GREEN = "green";
        private const string COLOR_BLUE = "#01A9FF";
        private const string COLOR_LIGHT_GRAY = "#D3D3D3";
        private const string DEFAULT_PLURAL_FORM = "s";
        private const string NO_PLURAL = "";
        private const string ES_PLURAL = "es";
        private const string EMPTY_STRING = "";
        private const string DRAFT_CATEGORY_TITLE = "Drafted";
        private const string UPGRADE_DISK_CATEGORY_TITLE = "Upgrade Disk";
        private const string TRUNK_CATEGORY_TITLE = "Trunk";
        private const string MORA_JAI_BOX_CATEGORY_TITLE = "Mora Jai Box";
        private const string STUDIO_ADDITION_CATEGORY_TITLE = "Studio Addition";
        private const string PUZZLE_CATEGORY_TITLE = "Puzzle";
        private const string ITEM_CATEGORY_TITLE = "Item";
        private static readonly string[] MORA_JAI_BOX_ROOMS = 
        [
			ROOM.LOST_AND_FOUND,
			ROOM.SOLARIUM,
			ROOM.TUNNEL,
			ROOM.TRADING_POST,
			ROOM.TOMB,
			ROOM.CLOSED_EXHIBIT,
			ROOM.MASTER_BEDROOM,
		];

		private static readonly string[] UPGRADE_DISK_ROOMS = 
        [
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
		];

		private static readonly Dictionary<string, List<string>> PUZZLES_IN_ROOMS = new() 
		{
            {ROOM.THE_FOUNDATION, new() {"Lower The Foundation Elevator"}},
			{ROOM.TOMB, new() {"Solve Tomb Puzzle 1"}},
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
            ROOM.THE_KENNEL, //TODO : Report Mismatch for kennel and cabinet keys
            ROOM.CLOCK_TOWER,
            ROOM.CLASSROOM,
            ROOM.SOLARIUM,
            ROOM.DOVECOTE,
            ROOM.VESTIBULE,
            ROOM.CASINO,
        ];

        private static readonly Dictionary<string, List<string>> ITEMS_IN_ROOMS = new()
        {
            {ROOM.ENTRANCE_HALL, new() {ITEM.MICROCHIP_MIDDLE_SIZED}},
            {ROOM.THE_ARMORY, new() {ITEM.MORNING_STAR, ITEM.TORCH, ITEM.KNIGHTS_SHIELD, ITEM.THE_AXE}},
            {ROOM.ANTECHAMBER, new() {ITEM.BASEMENT_KEY}},
            {ROOM.SHOWROOM, new() {ITEM.CHRONOGRAPH, ITEM.EMERALD_BRACELET, ITEM.MASTER_KEY, ITEM.MOON_PENDANT, ITEM.ORNATE_COMPASS, ITEM.SILVER_SPOON}},
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
            {ROOM.PATIO, new() {ITEM.CABINET_KEY_PATIO, ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.METAL_DETECTOR, ITEM.SHOVEL}},
            {ROOM.ROOT_CELLAR, new() {ITEM.BATTERY_PACK, ITEM.METAL_DETECTOR, ITEM.SLEDGE_HAMMER, ITEM.TREASURE_MAP}},
            {ROOM.STUDY, new() {ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.KEYCARD, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.TRADING_POST, new() {ITEM.BATTERY_PACK, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.GEAR_WRENCH, ITEM.HALL_PASS, ITEM.KEYCARD, ITEM.LOCK_PICK_KIT, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.METAL_DETECTOR, ITEM.PRISM_KEY, ITEM.RUNNING_SHOES, ITEM.SALT_SHAKER, ITEM.SECRET_GARDEN_KEY, ITEM.SHOVEL, ITEM.SILVER_KEY, ITEM.SLEDGE_HAMMER, ITEM.SLEEPING_MASK, ITEM.TELESCOPE, ITEM.VAULT_KEY_149, ITEM.VAULT_KEY_233, ITEM.VAULT_KEY_304, ITEM.VAULT_KEY_370}},
            {ROOM.BEDROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.SLEEPING_MASK}},
            {ROOM.DEN, new() {ITEM.CAR_KEYS, ITEM.COMPASS, ITEM.COUPON_BOOK, ITEM.LUCKY_RABBITS_FOOT, ITEM.MAGNIFYING_GLASS, ITEM.TREASURE_MAP}},
            {ROOM.DINING_ROOM, new() {ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.COUPON_BOOK, ITEM.SALT_SHAKER}},
            {ROOM.GALLERY, new() { ITEM.KEY_8, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.LUCKY_RABBITS_FOOT, ITEM.VAULT_KEY_233}},
            {ROOM.GYMNASIUM, new() {ITEM.CAR_KEYS, ITEM.RUNNING_SHOES}},
            {ROOM.LAUNDRY_ROOM, new() {ITEM.CABINET_KEY_LAUNDRY, ITEM.CAR_KEYS, ITEM.COIN_PURSE, ITEM.KEYCARD, ITEM.RUNNING_SHOES, ITEM.SLEEPING_MASK}},
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

        private static Dictionary<string, List<string>> ESTATE_LOCATIONS = new()
        {
            {"Campsite", new() {"Orchard Gate", "Gasline Valve - Orchard", "Gasline Valve - Gemstone Cavern", "Floorplan - Conservatory", "Scorch Sundial", "Raise Satellite"}},
            {"Blackbridge Grotto", new() {"Floorplan - Throne Room", "First Pickup - MICROCHIP 1"}},
            {"West Path", new() {"West Gate", "First Pickup - MICROCHIP 3"}},
            {"Underpass", new() {"Floorplan - Mechanarium", "Red Letter 1"}},
            {"Tunnel", new() {"Floorplan - Tunnel", "Break Tunnel Wall"}},
            {"Precipice", new() {"Break Precipice Wall"}},
            {"Basement", new() {"Floorplan - Lost And Found", "Unlock Basement Door The Foundation", "Break Grounds to Sealed Entrance Wall", "Open Basement to Reservoir Door"}},
            {"Abandoned Mine", new() {"Upgrade Disk - Abandoned Mine", "Unlock Basement Door The Well"}},
        };

        public static string GetCurrentRoomName()
		{
            string roomDisplayName = APTrackerView.GetTitleName();
            return APTrackerModel.StripRoomName(roomDisplayName);
		}

		public static void ShowArchipelagoRoomInfo()
        {
            string roomName = GetCurrentRoomName();
            StringBuilder archipelagoStats = new();
			archipelagoStats.Append(DraftedLocation(roomName));
			archipelagoStats.Append(TrunkLocations(roomName));
			archipelagoStats.Append(MoraJaiBoxLocation(roomName));
            archipelagoStats.Append(UpgradeDiskLocation(roomName));
            if (roomName == ROOM.DRAFTING_STUDIO)
            {
                archipelagoStats.Append(StudioAdditionsLocations());
            }
            archipelagoStats.Append(PuzzleLocations(roomName));
            archipelagoStats.Append(ItemPickupLocations(roomName));
            if (roomName == ROOM.ENTRANCE_HALL)
            {
                archipelagoStats.Append(EstateLocations());
            }
            
            if (roomName == ROOM.ENTRANCE_HALL || roomName == ROOM.TRADING_POST)
            {
                APTrackerView.DisableRoomDirectoryComponents();
            } 
            else
            {
                APTrackerView.EnableRoomDirectoryComponents();
            }
            APTrackerView.DisplayTrackerInformation(archipelagoStats.ToString());
		}

        private static string DraftedLocation(string roomName)
        {
            if (roomName == ROOM.ENTRANCE_HALL || roomName == ROOM.ROOM_46)
            {
                return EMPTY_STRING;
            }
            if (roomName == ROOM.BUNK_ROOM)
            {
                List<string> locationNames = [$"First Draft - {roomName}", $"First Draft - {roomName} 2"];
                return GetStackableLocationsDisplay(locationNames, DRAFT_CATEGORY_TITLE, NO_PLURAL);
			}
            return GetStackableLocationsDisplay([$"First Draft - {roomName}"], DRAFT_CATEGORY_TITLE, NO_PLURAL);
        }

        private static string TrunkLocations(string roomName)
        {
            List<string> trunkLocationNames = [];
            for (int i = 0; i < MAX_TRUNK; ++i)
            {
                trunkLocationNames.Add($"Locked Trunk - {roomName} - {i + 1}");
            }
            return GetStackableLocationsDisplay(trunkLocationNames, TRUNK_CATEGORY_TITLE);
        }

        private static string MoraJaiBoxLocation(string roomName)
        {
            if (!MORA_JAI_BOX_ROOMS.Contains(roomName))
            {
				return EMPTY_STRING;
            }
            return GetStackableLocationsDisplay([$"{roomName} Mora Jai Box"], MORA_JAI_BOX_CATEGORY_TITLE, ES_PLURAL);
		}

		private static string UpgradeDiskLocation(string roomName)
		{
			if (!UPGRADE_DISK_ROOMS.Contains(roomName))
			{
				return EMPTY_STRING;
			}
			if (roomName == ROOM.TRADING_POST)
			{
				List<string> locationNames = ["Upgrade Disk - Trading Post Dynamite", "Upgrade Disk - Trading Post Trade"];
                return GetStackableLocationsDisplay(locationNames, UPGRADE_DISK_CATEGORY_TITLE);
			}
            return GetStackableLocationsDisplay([$"Upgrade Disk - {roomName}"], UPGRADE_DISK_CATEGORY_TITLE);
		}

        private static string PuzzleLocations(string roomName)
		{
            return GetIndividualLocationsDisplayFromDict(roomName, PUZZLES_IN_ROOMS, x => x, PUZZLE_CATEGORY_TITLE);
        }

        private static string ItemPickupLocations(string roomName)
        {
            static string itemToLocation(string item)
            {
                if (item == ITEM.CABINET_KEY_PATIO)
                {
                    return "First Pickup - File Cabinet Key - Patio";
                }
                if (item == ITEM.CABINET_KEY_LAUNDRY)
                {
                    return "First Pickup - File Cabinet Key - Laundry Room";
                }
                return $"First Pickup - {item}";
            }
            return GetIndividualLocationsDisplayFromDict(roomName, ITEMS_IN_ROOMS, itemToLocation, ITEM_CATEGORY_TITLE);
        }

        private static string StudioAdditionsLocations()
        {
            static string studioAdditionToLocation(string room)
            {
                if (room == ROOM.THE_KENNEL)
                {
                    return "Floorplan - Kennel";
                }
                return $"Floorplan - {room}";
            }
            return GetIndividualLocationsDisplay(STUDIO_ADDITIONS_LOCATIONS_NAME, studioAdditionToLocation, STUDIO_ADDITION_CATEGORY_TITLE);
        }

        private static string EstateLocations()
        {
            StringBuilder estateTracker = new(LINE_BREAK);
            foreach (KeyValuePair<string, List<string>> placeAndLocations in ESTATE_LOCATIONS)
            {
                estateTracker.Append(GetIndividualLocationsDisplay(placeAndLocations.Value, x => x, placeAndLocations.Key, NO_PLURAL));  
            }
            return estateTracker.ToString();
        }

        private static string GetIndividualLocationsDisplayFromDict(string roomName, Dictionary<string, List<string>> locationsInRooms, Func<string, string> nameToLocation, string categoryName, string pluralForm = DEFAULT_PLURAL_FORM)
        {
            if (!locationsInRooms.ContainsKey(roomName))
            {
                return EMPTY_STRING;
            }
            return GetIndividualLocationsDisplay(locationsInRooms[roomName], nameToLocation, categoryName, pluralForm);
        }

        private static string GetIndividualLocationsDisplay(List<string> locationList, Func<string, string> nameToLocation, string categoryName, string pluralForm = DEFAULT_PLURAL_FORM)
        {
            List<Location> locations = CreateLocationList(locationList, nameToLocation);
            (List<Location> completedLocations, List<Location> uncompletedLocations) = APTrackerModel.GetCompletedAndUncompletedLocations(locations);
            int numberOfLocationsInPlay = completedLocations.Count + uncompletedLocations.Count;
            if (numberOfLocationsInPlay == 0)
            {
                return EMPTY_STRING;
            }
            StringBuilder trackerDisplay = new();
            foreach (Location uncompletedlocation in uncompletedLocations)
            {
                string locationDisplay = UncompletedLocationText(uncompletedlocation.GetName());
                trackerDisplay.Append(locationDisplay);
                trackerDisplay.Append(INDIVIDUAL_LOCATIONS_SEPARATOR);
            }
            foreach (Location completedlocation in completedLocations)
            {
                string locationDisplay = CompletedLocationText(completedlocation.GetName());
                trackerDisplay.Append(locationDisplay);
                trackerDisplay.Append(INDIVIDUAL_LOCATIONS_SEPARATOR);
            }
            RemoveLastSeparator(trackerDisplay);
            return HandlePlural(categoryName, pluralForm, numberOfLocationsInPlay > 1, trackerDisplay);
        }

        private static string GetStackableLocationsDisplay(List<string> locationNames, string categoryName, string pluralForm = DEFAULT_PLURAL_FORM)
        {
            List<Location> locations = CreateLocationList(locationNames, x => x);
            (List<Location> completedLocations, List<Location> uncompletedLocations) = APTrackerModel.GetCompletedAndUncompletedLocations(locations);
            int completedLocationCount = completedLocations.Count;
            int numberOfLocationsInPlay = completedLocationCount + uncompletedLocations.Count;
            if (numberOfLocationsInPlay == 0)
            {
                return EMPTY_STRING;
            }
            string prefix = GetPrefix(categoryName, numberOfLocationsInPlay > 1, pluralForm);
            return prefix + StackableLocationResultText(completedLocationCount, numberOfLocationsInPlay);
        }

        private static List<Location> CreateLocationList(List<string> locationList, Func<string, string> nameToLocation)
        {
            List<Location> locations = [];
            foreach (string locationName in locationList)
            {
                locations.Add(new Location(nameToLocation(locationName), locationName));
            }

            return locations;
        }

        private static string HandlePlural(string categoryName, string pluralForm, bool isPlural, StringBuilder trackerDisplay)
        {
            string prefix = GetPrefix(categoryName, isPlural, pluralForm);
            return prefix + trackerDisplay.ToString();
        }

        private static void RemoveLastSeparator(StringBuilder trackerDisplay)
        {
            int separatorLength = INDIVIDUAL_LOCATIONS_SEPARATOR.Length;
            trackerDisplay.Remove(trackerDisplay.Length - separatorLength, separatorLength);
        }

        private static string GetPrefix(string categoryName, bool plural = false, string pluralForm = DEFAULT_PLURAL_FORM)
        {
            if (plural)
            {
                return LINE_BREAK + categoryName + pluralForm + SEPARATOR_BETWEEN_CATEGORY_AND_LOCATIONS;
            }
            return LINE_BREAK + categoryName + SEPARATOR_BETWEEN_CATEGORY_AND_LOCATIONS;
        }

        private static string CompletedLocationText(string text)
        {
            return TMProTextUtils.ColorText(TMProTextUtils.StrikethroughText(text), COLOR_LIGHT_GRAY);
        }

        private static string UncompletedLocationText(string text)
        {
            return TMProTextUtils.ColorText(text, COLOR_BLUE);
        }

        private static string StackableLocationResultText(int completed, int total)
        {
			string color = COLOR_YELLOW;
			if (completed == 0)
			{
				color = COLOR_RED;
			}
			if (completed == total)
			{
				color = COLOR_GREEN;
			}
			return TMProTextUtils.ColorText($"{completed}/{total}", color);
		}
	}
}