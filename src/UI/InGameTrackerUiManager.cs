using BluePrinceArchipelago.Utils;
using UnityEngine;
using BluePrinceArchipelago.Archipelago;
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

namespace BluePrinceArchipelago.UI
{
	public static class InGameTrackerUiManager {

		public static string getCurrentRoomName()
		{
			GameObject headline = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/HEADLINE");
			return headline.GetComponent<TextMeshPro>().text;
		}

		public static void ShowArchipelagoRoomInfo()
		{
			string roomName = getCurrentRoomName();
			Logging.LogWarning($"Room Selected : {roomName}", "TRACKER");

			GameObject description = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/scalers/Description");
			description.GetComponent<TextMeshPro>().text = "Archipelago";
		}
	}
}