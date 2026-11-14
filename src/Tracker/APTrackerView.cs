#if Bep
using TMPro;
#endif
#if ML
using Il2CppTMPro;
#endif
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrince.Tracker
{
	public static class APTrackerView
	{
		public static string GetTitleName()
		{
			GameObject headline = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/HEADLINE");
			return headline.GetComponent<TextMeshPro>().text;
		}

		public static void DisableRoomDirectoryComponents()
		{
			ChangeRoomDirectoryComponentsVisibility(false);
		}

		public static void EnableRoomDirectoryComponents()
		{
			ChangeRoomDirectoryComponentsVisibility(true);
		}

		private static void ChangeRoomDirectoryComponentsVisibility(bool isVisible)
		{
			GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/TYPE").SetActive(isVisible);
			GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/Type Type").SetActive(isVisible);
			GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/RARITY").SetActive(isVisible);
			GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/Rarity Type").SetActive(isVisible);
		}

		public static void DisplayTrackerInformation(string text)
		{
			GameObject description = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/scalers/Description");
			description.GetComponent<TextMeshPro>().text = text;
		}
	}
}