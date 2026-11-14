using BluePrinceArchipelago;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;

namespace BluePrince.Tracker
{
	public class Location
	{
		private const long LOCATION_NOT_FOUND = -1;

		private readonly long _id;
		private bool _isCompleted = false;
		private string _displayName;

		public Location(string locationName, string diplayName = "")
		{
			_id = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
			if (_displayName == "")
			{
				_displayName = locationName.ToTitleCase();
			}
			_displayName = diplayName.ToTitleCase();

        }

		public string GetName()
		{
			return _displayName;
		}

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