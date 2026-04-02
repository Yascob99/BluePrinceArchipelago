using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Utils;
using Il2CppSystem.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static BluePrinceArchipelago.Archipelago.ItemQueue;

namespace BluePrinceArchipelago.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.6.6";
    private const string Game = "Blue Prince";

    public static bool Authenticated;
    private bool _AttemptingConnection;
    public static bool Reconnected = false;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    public ArchipelagoClient()
    {
    }
    //Returns the locationid from the name or -1 if It can't be found.
    public long GetLocationFromName(string locationName)
    {
        return ServerData.LocationDict?.FirstOrDefault(x => x.Value.ToLower() == locationName.ToLower()).Key ?? -1;
    }
    public void DisplayServerData()
    {
        Logging.Log("Options");
        foreach (var option in ServerData.Options.AsDictionary()) {
            if (option.Key != null && option.Value != null)
            {
                Logging.Log($"\t{option.Key}: {option.Value.ToString()}");
            }
        }
        Logging.Log("Checked Locations:");
        foreach (long locationid in ServerData.CheckedLocations)
        {
            Logging.Log($"\t{locationid}");
        }
        Logging.Log("Location Dict:");
        foreach (var entry in ServerData.LocationDict)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value}");
        }
        Logging.Log("Item Dict:");
        foreach (var entry in ServerData.ItemDict)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value}");
        }
        Logging.Log("Location Item Map:");
        foreach (var entry in ServerData.LocationItemMap)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value.ItemName}");
        }
    }

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (Authenticated || _AttemptingConnection) return;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Logging.LogError(e);
        }

        TryConnect();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
        session.Locations.CheckedLocationsUpdated += OnRemoteLocationChecked;
    }


    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        // Attempt to Connect to the server. 
        LoginResult loginResult = session.TryConnectAndLogin(
                    Game,
                    ServerData.SlotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(APVersion),
                    password: ServerData.Password,
                    requestSlotData: true
         );
        // If failed to login display why.
        if (loginResult is LoginFailure failure) {
            string errors = string.Join(", ", failure.Errors);
            Logging.LogError($"Unable to connect to Archipelago because: {errors}");
            HandleConnectResult(new LoginFailure(errors));
            _AttemptingConnection = false;
        }
        // Else handle login.
        else if (loginResult is LoginSuccessful success)
        {
            // Get the slot data
            SlotData slotData = session.DataStorage.GetSlotData<SlotData>();

            // Check if the Seed and options match the expected Seed and Options.
            if (ServerData.Options.Equals(slotData) && (ServerData.Seed == "" || ServerData.Seed == session.RoomState.Seed)) {
                //If the Seed data was already stored this is a recconnect.
                if (ServerData.Seed == session.RoomState.Seed) {
                    Reconnected = true;
                }
                HandleConnectResult(loginResult);
                _AttemptingConnection = false;
            }
            // Player Connected to wrong slot (Probably)
            else
            {
                //TODO once proper Archipelago login has been setup correct to actually disconnect instead. For now this can't be validated and so this will be handled this way for testing purposes.
                Logging.LogWarning($"SlotData doesn't match expected slot, assuming a new game was started without previous goal finishing. If you have Connected to the wrong slot, please disconnect now.");
                State.Reset();
                State.Initialize();
                HandleConnectResult(loginResult);
                _AttemptingConnection = false;
            }
        }
        else
        {
            HandleConnectResult(new LoginFailure($"Unexpected LoginResult type when connecting to Archipelago: {loginResult}"));
            _AttemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        // Handle Successful connection to AP Server.
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;
            Authenticated = true;
            // Initialize DeathLinkHandler.
            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            
            // Handles the reconnection to the Server.
            if (Reconnected)
            {
                Reconnect();
                ArchipelagoConsole.LogMessage($"Successfully Recconnected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
            // Handles a new connection to the Server.
            else
            {
                // Gets the Initial data from the server.
                ServerData.Options = session.DataStorage.GetSlotData<SlotData>();
                ServerData.Seed = session.RoomState.Seed;
                session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
                // Creates the Locally Stored data for the locations. 
                CreateLocationDicts(session.Locations.AllLocations.ToArray());
                ArchipelagoConsole.LogMessage($"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
            // Receives any Queued Items
            DequeueItems(Reconnected);
            // Debug: Displaying the data from the server.
            DisplayServerData();
            // Update the locally stored data to match the current state.
            State.UpdateAll();
            // Run any additional code that should be run on a successful connection.
            ModInstance.OnConnectToArchipelago();
        }
        // Output an Error Message and Disconnect.
        else
        {
            string outText;
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText += "\n" + failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Logging.LogError(outText);

            Authenticated = false;
            Disconnect();
        }
        _AttemptingConnection = false;
    }

    // Attempts to release any Queued items.
    private void DequeueItems(bool isReconnect = false) {
        // Handle Queue as normal if reconnect.
        if (isReconnect)
        {
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                if (!ModInstance.Instance.QueueManager.RecieveItem(item))
                {
                    ModInstance.Instance.QueueManager.AddItemToQueue(item);
                }
                session.Items.DequeueItem();
            }
        }
        // Handle Initial connection.
        else
        {
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                // If the item was a starting item
                if (item.LocationName == "Server")
                {
                    Logging.Log($"Attempting to receive Item: {item.ItemName}");
                    // Checks if the item recieved is an item.
                    if (ModRoomManager.VanillaRooms.Contains(item.ItemName.ToUpper()))
                    {
                        if (!ModInstance.HasInitializedRooms)
                        {
                            ModInstance.Instance.QueueManager.AddItemToQueue(item);
                            session.Items.DequeueItem();
                        }
                        ModRoom room = Plugin.ModRoomManager.GetRoomByName(item.ItemName.ToUpper());
                        room.IsUnlocked = true;
                        if (room.RoomPoolCount == 0) room.RoomPoolCount++;
                        session.Items.DequeueItem();
                    }
                    else
                    {
                        //TODO add item unlock code (Should not grant the unique items).
                        session.Items.DequeueItem();
                    }
                }
                else
                {
                    //Handle non-server items normally.
                    session.Items.DequeueItem();
                    if (!ModInstance.Instance.QueueManager.RecieveItem(item))
                    {
                        ModInstance.Instance.QueueManager.AddItemToQueue(item);
                    }
                }
            }
        }

    }

    private void Reconnect() {
        Reconnected = true;
        RebuildCheckedLocations();
        CreateLocationDicts(session.Locations.AllLocations.ToArray());
    }
    private void RebuildCheckedLocations()
    {
        // Make copies of the lists for editing purposes.
        List<long> serverLocations = [.. session.Locations.AllLocationsChecked];
        List<long> localLocations = [.. ServerData.CheckedLocations];
        bool found = false;
        int i = 0;

        // Check each server location.
        foreach (long location in serverLocations) {
            found = false;
            i = 0;
            // See if the location has been found locally
            while (i < localLocations.Count && !found) {
                if (localLocations[i] == location) { 
                    found = true;
                }
                i++;
            }
            if (!found) {
                // If the server has locations checked that the local game didn't send while disconnected, add them to the checked locationlist.
                ServerData.CheckedLocations.Add(location);
            }
            if (found) {
                // Remove the location from the local list.
                localLocations.RemoveAt(i); 
            }

        }
        // Any remaining local locations will not have been sent to the server, so send them to the server.
        if (localLocations.Count > 0) {

            // If the scene has been loaded and the client is connected, send the locations
            if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated)
            {
                // Update the session with any local locations that weren't yet sent due to a disconnection.
                session.Locations.CompleteLocationChecksAsync(localLocations.ToArray());
            }
            // Otherwise add it to the Queue to be sent later.
            else {
                ModInstance.Instance.QueueManager.AddLocationsToQueue(localLocations);
            }
        }
    }

    private void CreateLocationDicts(long[] locationIds)
    {
        for (int i = 0; i < locationIds.Count(); i++)
        {
            long location = locationIds[i];
            string locationName = session.Locations.GetLocationNameFromId(location);
            ServerData.LocationDict[location] = locationName; 
        }
        //Asynchronously gather the data for all items stored in all the active locations, then wait for a response.
        Task<Dictionary<long, ScoutedItemInfo>> scoutTask = session.Locations
                .ScoutLocationsAsync(locationIds);
        scoutTask.Wait();
        Dictionary<long, ScoutedItemInfo> scoutResult = scoutTask.Result;
        foreach (KeyValuePair<long, ScoutedItemInfo> scout in scoutResult)
        {
            long locationId = scout.Key;
            long itemId = scout.Value.ItemId;
            string itemName = scout.Value.ItemName ?? $"?Item {itemId}";
            ServerData.ItemDict[itemId] = itemName; //Might need to change this since ids
            ServerData.LocationItemMap[locationId] = scout.Value;
        }
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect()
    {
        Reconnected = false;
        Logging.LogDebug("disconnecting from server...");
        session?.Socket.DisconnectAsync();
        session = null;
        Authenticated = false;
    }

    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo receivedItem = helper.DequeueItem();

        if (helper.Index <= ServerData.Index) return;

        ServerData.Index++;

        //Attempt to receive item, if it fails, add to queue to be added later.
        if (!ModInstance.Instance.QueueManager.RecieveItem(receivedItem))
        {
            ModInstance.Instance.QueueManager.AddItemToQueue(receivedItem);
        }
        
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Logging.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Logging.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }

    /// <summary>
    /// Whenever a local location(s) are checked remotely (like via a server command)
    /// </summary>
    /// <param name="newCheckedLocations">the ids of the locations that were checked.</param>
    private void OnRemoteLocationChecked(ReadOnlyCollection<long> newCheckedLocations) { 
    }
    /// <summary>
    /// Sends to the server that the location has been checked.
    /// </summary>
    /// <param name="locationName">the name of the location to complete</param>
    public void CheckLocation(string locationName) {
        long locationid = GetLocationFromName(locationName);
        CheckLocation(locationid);
    }
    /// <summary>
    /// Sends to the server that the location has been checked.
    /// </summary>
    /// <param name="locationName">the name of the location to complete</param>
    public void CheckLocation(long locationid)
    {
        if (!ServerData.CheckedLocations.Contains(locationid))
        {
            session.Locations.CompleteLocationChecks([locationid]);
            ServerData.CheckedLocations.Add(locationid);
            State.UpdateLocations(ServerData.CheckedLocations);
        }
        else {
            Logging.Log($"Unable to send location for {ServerData.LocationDict[locationid]}. Location has already been sent.");
        }
    }
    public void GoalCompleted()
    {
        session.SetGoalAchieved();
        State.Reset();
    }
}
public class ArchipelagoQueueManager {
    private ItemQueue _ReceivedItemQueue = new("Received Item Queue");
    private LocationQueue _LocationQueue = new("Location Queue");
    public void AddItemToQueue(ItemInfo item) { 
        _ReceivedItemQueue.Enqueue(item);
    }
    public void AddLocationsToQueue(List<long> locations) {
        List<string> locationNames = new List<string>();
        foreach (int location in locations) {
            string locationName = ArchipelagoClient.ServerData.LocationDict[location];
            locationNames.Add(locationName);
        }
        _LocationQueue.Enqueue(locationNames.ToArray());
    }
    public void ReleaseAllQueuedLocations() {
        if (_LocationQueue.Count > 0) {
            for (int i = 0; i < _LocationQueue.Count; i++)
            {
                string item = _LocationQueue.Dequeue();
                if (!SendLocationCheck())
                {
                    _LocationQueue.Enqueue(item);
                }
                else {
                    Plugin.ArchipelagoClient.CheckLocation(item);
                }
            }
        }
    }

    public void ReleaseAllQueuedItems()
    {
        if (_ReceivedItemQueue.Count > 0)
        {
            for (int i = 0; i < _ReceivedItemQueue.Count; i++)
            {
                ItemInfo item = _ReceivedItemQueue.Dequeue();
                if (!RecieveItem(item))
                {
                    _ReceivedItemQueue.Enqueue(item);
                }
            }
        }
    }
    public bool RecieveItem(ItemInfo item)
    {
        Logging.Log($"Attempting to receive Item: {item.ItemName}");
        if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms)
        {
            // Checks if the item recieved is a Room.
            if (Plugin.ModRoomManager.Rooms.Contains(Plugin.ModRoomManager.GetRoomByName(item.ItemName.ToUpper().Trim())))
            {
                ModRoom room = Plugin.ModRoomManager.GetRoomByName(item.ItemName.ToUpper());
                room.IsUnlocked = true;
                if (room.RoomsLeftInPool == 0)
                {
                    room.RoomPoolCount++; //Update the rooms in pool count if this item is received and already at it's max number.
                }
                ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
                State.UpdateItems(ArchipelagoClient.ServerData.ReceivedItems);
                return true;
            }
            if (item.Flags.HasFlag(ItemFlags.Trap))
            {
                Plugin.ModItemManager.OnTrapReceived(item);
                ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
                State.UpdateItems(ArchipelagoClient.ServerData.ReceivedItems);
                return true;
            }
            // if not handle it as an Item.
            Plugin.ModItemManager.OnItemCheckRecieved(item);
            //This may need to be moved to a better place once the item code is better implemented.
            ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
            State.UpdateItems(ArchipelagoClient.ServerData.ReceivedItems);
            return true;
        }
        return false;
    }
    private bool SendLocationCheck() {
        return ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated;
    }
}
public class ItemQueue(string name) {
    private readonly string _Name = name;
    public string Name { 
        get { return _Name; }
    }
    private List<ItemInfo> _Queue = new List<ItemInfo>();
    public int Count
    {
        get { return _Queue.Count; }
    }
    public void Enqueue(ItemInfo item) {
        _Queue.Add(item);
    }
    public void Enqueue(ItemInfo[] items) {
        _Queue.AddRange(items);
    }
    public ItemInfo Dequeue() {
        if (_Queue.Count == 0) {
            Logging.LogWarning("No Items in Queue, cannot Dequeue");
            return null;
        }
        ItemInfo temp = _Queue[0];
        _Queue.RemoveAt(0);
        return temp;
    }
    public class LocationQueue(string name) {
        private readonly string _Name = name;
        public string Name
        {
            get { return _Name; }
        }
        private List<string> _Queue = new List<string>();
        public int Count
        {
            get { return _Queue.Count; }
        }
        public void Enqueue(string location)
        {
            _Queue.Add(location);
        }
        public void Enqueue(string[] locations) {
            _Queue.AddRange(locations);
        }
        public string Dequeue()
        {
            if (_Queue.Count == 0)
            {
                Logging.LogWarning("No Locations in Queue, cannot Dequeue");
                return null;
            }
            string temp = _Queue[0];
            _Queue.RemoveAt(0);
            return temp;
        }
    }
}