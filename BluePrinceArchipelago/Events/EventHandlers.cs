using BluePrinceArchipelago.Core;
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

    public class ItemQueueEventArgs : EventArgs
    {
        public string EventType;
        public object Data;
        public Type Type;

        public ItemQueueEventArgs(string eventType, object data) { 
            EventType = eventType;
            Data = data;
            Type = data.GetType();
        }
        public ItemQueueEventArgs(string eventType, object data, Type type)
        {
            EventType = eventType;
            Data = data;
            Type = type;
        }
    }

    public class ModEventHandler
    {
        public delegate void LocationHandler(System.Object sender, LocationEventArgs args);

        public event LocationHandler LocationFound;

        public delegate void QueueHanlder(string senderName, ItemQueueEventArgs args);

        public event QueueHanlder QueueEvent;

        //Triggers the OnFirstDrafted Event
        public void OnFirstDrafted(ModRoom room)
        {
            LocationFound.Invoke(this, new LocationEventArgs($"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(room.Name.ToLower())} First Entering ", "First Draft Room"));
        }
        public void OnFirstFound(ModItem item) {
            LocationFound.Invoke(this, new LocationEventArgs($"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Name.ToLower())} First Pickup", "Item First Pickup"));
        }
        public void OnQueueEvent(string senderName,string eventType, object data) {
            QueueEvent.Invoke(senderName, new ItemQueueEventArgs(eventType, data));
        }
        public void OnQueueEvent(string senderName, string eventType, object data, Type type)
        {
            QueueEvent.Invoke(senderName, new ItemQueueEventArgs(eventType, data, type));
        }
    }


}
