#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppTMPro;
#endif
using System.Text;
using BluePrinceArchipelago.Utils;
using UnityEngine;
using BluePrinceArchipelago.Archipelago;

namespace BluePrinceArchipelago.UI
{
	public static class InGameTrackerUiManager {

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

			//Logging.LogWarning($"amount of locations found : {ArchipelagoClient.ServerData.CheckedLocations.Count}", "TRACKER");

			long locationId = Plugin.ArchipelagoClient.GetLocationFromName($"First Draft - {roomName}");
			//Logging.LogWarning($"location found : {locationId}", "TRACKER");

			if (ArchipelagoClient.ServerData.CheckedLocations.Contains(locationId))
			{
				archipelagoStats.Append(ColorText("Drafted", "green"));
			}
			else
			{
				archipelagoStats.Append(ColorText("Drafted", "red"));
			}
			GameObject description = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/scalers/Description");
			description.GetComponent<TextMeshPro>().text = archipelagoStats.ToString();
		}

		public static string ColorText(string text, string color) 
		{
			return $"<color={color}>{text}</color>";
		}
	}
}