using System;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Tracker
{
    /// <summary>
    ///     Handles the core logic of the in game AP Tracker.
    /// </summary>
    public static class APTrackerModel
	{
		private const string HER_LADYSHIPS_CHAMBER_DISPLAY_NAME = "<color=#9A6FCF><size=12.5>Her Ladyship's Chamber</size></color>";
		private const string AQUARIUM_DISPLAY_NAME = "<color=#C20F00>A</color><color=#52B74F>q</color><color=#E78F52>u</color><color=#9A6FCF>a</color><color=#D2BE60>r</color><color=#C20F00>i</color><color=#52B74F>u</color><color=#E78F52>m</color> ";

		/// <summary>
		///		Correct the room name from the headline to extract only the name of the room.
		///		The original room name may have a color tag.
		/// </summary>
		/// <param name="displayName">Raw name from the game title.</param>
		/// <returns>Cleaned room name.</returns>
		public static string StripRoomName(string displayName)
		{
			string stripedRoomName = RemoveColorCode(displayName);
			return ROOM.GetDowngradedRoomName(stripedRoomName);
		}

		private static string RemoveColorCode(string roomName)
		{
			if (roomName[0] != '<')
			{
				return roomName;
			}
			if (roomName == AQUARIUM_DISPLAY_NAME)
			{
				return ROOM.AQUARIUM;
			}
			if (roomName == HER_LADYSHIPS_CHAMBER_DISPLAY_NAME)
			{
				return ROOM.HER_LADYSHIPS_CHAMBER;
			}
			int begining = roomName.IndexOf('>') + 1;
			int end = roomName.IndexOf('/') - 1;
			return roomName[begining..end];
		}

		/// <summary>
		///		Separate the location list given into completed locations and uncompleted locations. 
		///		Ignore the location that are not in play.
		/// </summary>
		/// <param name="locations">Location list.</param>
		/// <returns>Completed location list and Uncompleted location list.</returns>
		public static Tuple<List<TrackerLocation>, List<TrackerLocation>> GetCompletedAndUncompletedLocations(List<TrackerLocation> locations)
		{
			List<TrackerLocation> completedLocations = [];
			List<TrackerLocation> uncompletedLocations = [];

			foreach (TrackerLocation location in locations)
			{
				if (!location.IsInPlay())
				{
					continue;
				}
				if (location.IsCompleted())
				{
					completedLocations.Add(location);
				}
				else
				{
					uncompletedLocations.Add(location);
				}
			}
			return new(completedLocations, uncompletedLocations);
		}
	}
}