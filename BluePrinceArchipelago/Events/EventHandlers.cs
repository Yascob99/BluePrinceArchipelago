using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using System;

namespace BluePrinceArchipelago.Events
{
    public class LocationEventArgs : EventArgs
    {
        public string LocationName { get; set; }
        public string LocationType { get; set; }

        public LocationEventArgs(string locationName, string locationType)
        {
            LocationName = locationName;
            LocationType = locationType;
        }
    }

    public class ModEventHandler
    {
        public delegate void LocationHandler(System.Object sender, LocationEventArgs args);

        public event LocationHandler LocationFound;

        //Triggers the OnFirstDrafted Event
        public void OnFirstDrafted(ModRoom room)
        {
            LocationFound.Invoke(this, new LocationEventArgs($"{room.Name.ToTitleCase()} First Entering", "First Draft Room"));
        }
        public void OnFirstFound(ModItem item) {
            LocationFound.Invoke(this, new LocationEventArgs($"{item.Name.ToTitleCase()} First Pickup", "Item First Pickup"));
        }
        public void OnTrunkOpened(string roomName, int trunkCount) {
            LocationFound.Invoke(this, new LocationEventArgs($"{roomName.ToTitleCase()} Locked Trunk {trunkCount}", "Locked Trunk Unlocked"));
        }
    }


}
