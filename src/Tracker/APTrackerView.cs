#if Bep
using TMPro;
#endif
#if ML
using Il2CppTMPro;
#endif
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Tracker
{
    /// <summary>
    ///     Handles the display of the in game AP Tracker.
    /// </summary>
    public static class APTrackerView
	{
		/// <summary>
		///		Get the room info headline of the current entry of room index.
		/// </summary>
		/// <returns>Room info title headline</returns>
		public static string GetTitleName()
		{
			GameObject headline = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/HEADLINE");
			return headline.GetComponent<TextMeshPro>().text;
		}

		/// <summary>
		///		Disable room directory components that would overlap with the tracker info.
		/// </summary>
		public static void DisableRoomDirectoryComponents()
		{
			ChangeRoomDirectoryComponentsVisibility(false);
		}

        /// <summary>
        ///		Re-enable room directory components.
        /// </summary>
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

		/// <summary>
		///		Replace room index description with the tracker information.
		/// </summary>
		/// <param name="text">Tracker information</param>
		public static void DisplayTrackerInformation(string text)
		{
			GameObject description = GameObjectExtensions.FindInHierarchy("UI OVERLAY CAM/ROOM INFO/Info Window/scalers/Description");
			description.GetComponent<TextMeshPro>().text = text;
		}
	}
}