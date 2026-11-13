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

namespace BluePrinceArchipelago.UI
{
	public static class InGameTrackerUiManager {
		private const int MAX_TRUNK = 10;
		private static readonly string[] MORA_JAI_BOX_ROOMS = {
			ROOM.LOST_AND_FOUND,
			ROOM.SOLARIUM,
			ROOM.TUNNEL,
			ROOM.TRADING_POST,
			ROOM.TOMB,
			ROOM.CLOSED_EXHIBIT,
			ROOM.MASTER_BEDROOM,
		};

		private static readonly string[] UPGRADE_DISK_ROOMS = {
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
			{ROOM.SECRET_GARDEN, new() {"Break Secret Garden Wall" }},
            {ROOM.GREENHOUSE, new() {"Break Greenhouse Wall"}},
            {ROOM.WEIGHT_ROOM, new() {"Break Weight Room Wall"}},
            {ROOM.LABORATORY, new() {"Laboratory Puzzle"}},
            {ROOM.CLOISTER, new() {"Allowance Token - Cloister Statue"}},
            {ROOM.UTILITY_CLOSET, new() {"VAC Controls"}},
            {ROOM.HOVEL, new() {"Gasline Valve - Hovel"}},
            {ROOM.SCHOOLHOUSE, new() {"Gasline Valve - Schoolhouse"}},
            {ROOM.ENTRANCE_HALL, new() { "Entrance Hall West Vase", "Entrance Hall East Vase"}},
            {ROOM.SHELTER, new() {"Red Letter VII"}},
            {ROOM.STUDY, new() {"Red Letter 2"}},
            {ROOM.OFFICE, new() {"Red Letter ∞"}},
            {ROOM.DRAWING_ROOM, new() {"Red Letter SIX"}},
            {ROOM.DRAFTING_STUDIO, new() {"Red Letter 5"}},
            {ROOM.BOUDOIR, new() {"Red Letter 4"}},
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
            Logging.LogWarning($"Puzzles location", "TRACKER");
            archipelagoStats.Append(PuzzleLocations(roomName));

			//TODO : Drafting Room Floorplans
			//TODO : Items

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
		private static string PuzzleLocations(string roomName)
		{
            if (!PUZZLES_IN_ROOMS.ContainsKey(roomName))
            {
                Logging.LogWarning($"Not found {roomName} in puzzle list", "TRACKER");
                return "";
            }
			StringBuilder puzzlesTracker = new();
			foreach (string location in PUZZLES_IN_ROOMS[roomName])
			{
				string color = IsCompleted(location) ? "green" : "red";
				puzzlesTracker.Append(ColorText(location, color));
				puzzlesTracker.Append(", ");
            }
			puzzlesTracker.Remove(puzzlesTracker.Length - 2, 2);
			return "<br>Puzzle : " + puzzlesTracker.ToString();
        }


        private static string ColorText(string text, string color) 
		{
			return $"<color={color}>{text}</color>";
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