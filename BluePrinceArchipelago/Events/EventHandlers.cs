using BluePrinceArchipelago.Core;
using CirrusPlay.PortalLibrary;
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
            LocationFound.Invoke(this, new LocationEventArgs($"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(room.Name.ToLower())} First Entering ", "First Draft Room"));
        }
        public void OnFirstFound(ModItem item) {
            LocationFound.Invoke(this, new LocationEventArgs($"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Name.ToLower())} First Pickup", "Item First Pickup"));
        }
    }
}
