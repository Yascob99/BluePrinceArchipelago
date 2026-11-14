using BluePrinceArchipelago;
using BluePrinceArchipelago.Archipelago;

namespace BluePrince.Tracker
{
	public class Location(string locationName)
	{
		private const long LOCATION_NOT_FOUND = -1;

		private readonly long id = Plugin.ArchipelagoClient.GetLocationFromName(locationName);
		private bool isCompleted = false;

		public bool IsCompleted()
		{
			if (isCompleted)
			{
				return true;
			}
			isCompleted = CheckCompletion();
			return isCompleted;
		}

		public bool IsInPlay()
		{
			return (id != LOCATION_NOT_FOUND);
		}

		private bool CheckCompletion()
		{
			return ArchipelagoClient.ServerData.CheckedLocations.Contains(id);
		}
	}
}