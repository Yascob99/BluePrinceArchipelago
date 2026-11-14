using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;

namespace BluePrinceArchipelago.Tracker
{
	/// <summary>
	///		An AP location for use in the AP Tracker.
	/// </summary>
	public class TrackerLocation
	{
		private const long LOCATION_NOT_FOUND = -1;

		private readonly long _id;
		private bool _isCompleted = false;
		private readonly string _displayName;

		/// <summary>
		///		Constructor for AP location.
		/// </summary>
		/// <param name="locationName">Location name, must match the server side data.</param>
		/// <param name="displayName">Alternative name to display in the in game tracker.</param>
		public TrackerLocation(string locationName, string displayName = "")
		{
			_id = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
			if (_displayName == "")
			{
				_displayName = locationName.ToTitleCase();
			}
			_displayName = displayName.ToTitleCase();

        }

		/// <summary>
		///		Get display name.
		/// </summary>
		/// <returns>Display name.</returns>
		public string GetName()
		{
			return _displayName;
		}
		
		/// <summary>
		///		Indicate whether the location has been completed or not yet.
		/// </summary>	
		/// <returns>A boolean.</returns>
		public bool IsCompleted()
		{
			if (!IsInPlay())
			{
				return false;
			}
			if (_isCompleted)
			{
				return true;
			}
			_isCompleted = CheckCompletion();
			return _isCompleted;
		}

		/// <summary>
		///		Indicate whether the location is in play in this AP world or not.
		/// </summary>
		/// <returns>A boolean.</returns>
		public bool IsInPlay()
		{
			return (_id != LOCATION_NOT_FOUND);
		}

		private bool CheckCompletion()
		{
			return ArchipelagoClient.ServerData.CheckedLocations.Contains(_id);
		}
	}
}