using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.6.7";
    private const string Game = "Blue Prince";

    public static bool Authenticated;
    private bool _AttemptingConnection;
    public static bool Reconnected = false;
    public static bool Disconnected = false; // Indicates whether the client was fully disconnected at any point during the session (important for crash handling).
    public static bool StateRebuilt = false;

    public static ArchipelagoData ServerData = new();
    public DeathLinkHandler DeathLinkHandler { get; set; }
    private ArchipelagoSession session;

    public ArchipelagoClient()
    {
    }
    /// <summary>
    ///     Returns the locationid from the name or -1 if It can't be found.
    ///     Not Case Sensitive.
    /// </summary>
    /// <param name="locationName">The name of the location to find.</param>
    /// <returns>The long location id of an item or -1 if not found.</returns>
    public long GetLocationFromName(string locationName)
    {
        int length = ServerData.LocationDict.Count;
        foreach (var data in ServerData.LocationDict) {
            string val = data.Value;
            if (val.ToLower() == locationName.ToLower()) {
                return data.Key;
            }
        }
        return -1;
    }

    /// <summary>
    ///     Displays information about the seed and settings data of the current seed.
    /// </summary>
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
        Logging.Log("Location Dict:", "APData");
        foreach (var entry in ServerData.LocationDict)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value}", "APData");
        }
        Logging.Log("Item Dict:");
        foreach (var entry in ServerData.ItemDict)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value}");
        }
        Logging.Log("Location Item Map:", "APData");
        foreach (var entry in ServerData.LocationItemMap)
        {
            Logging.Log($"\t{entry.Key}:{entry.Value.ItemName}", "APData");
        }
    }

    /// <summary>
    ///     Call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
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
        State.InitializeDeathLinkTotals();
        TryConnect();
    }

    /// <summary>
    ///     Add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString(), isServerMessage: true);
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
        session.Locations.CheckedLocationsUpdated += OnRemoteLocationChecked;
        
    }


    /// <summary>
    ///     Attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        // Attempt to Connect to the server. 
        LoginResult loginResult = session.TryConnectAndLogin(
                    Game,
                    ServerData.SlotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(APVersion),
                    tags: DeathLinkHandler._deathLinkEnabled ? ["AP", "DeathLink"] : ["AP"],
                    password: ServerData.Password,
                    requestSlotData: true
         );
        // If failed to login display why.
        if (loginResult is LoginFailure failure) {
            string errors = string.Join(", ", failure.Errors);
            HandleConnectResult(new LoginFailure(errors));
            _AttemptingConnection = false;
        }
        // Else handle login.
        else if (loginResult is LoginSuccessful success)
        {
            // Get the slot data
            SlotData slotData = session.DataStorage.GetSlotData<SlotData>();

            // Check if the Seed and options match the expected Seed and Options.
            if (ServerData.Seed == "" || ServerData.Seed == session.RoomState.Seed) {
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
                ArchipelagoConsole.LogMessage($"SlotData doesn't match expected slot. If you didn't finish the last run please run /ResetData and reconnect or connect to the correct server.");

                HandleConnectResult(new LoginFailure($"Unexpected LoginResult type when connecting to Archipelago: {loginResult}"));
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
    ///     Handle the connection result and start initializing the mod.
    /// </summary>
    /// <param name="result">The Login Result (Successful or Unsucessful)</param>
    private void HandleConnectResult(LoginResult result)
    {
        // Handle Successful connection to AP Server.
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;
            

            // Handles the reconnection to the Server.
            if (Reconnected)
            {
                if (Disconnected)
                {
                    // Regular Recconnect;
                    ServerData.Options = session.DataStorage.GetSlotData<SlotData>();
                    ArchipelagoOptions.LoadFromSlotData(ServerData.Options);
                    // Initialize DeathLinkHandler.
                    DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName, ArchipelagoOptions.DeathLinkType != DeathLinkType.option_none);
                    Reconnect();
                }
                else {
                    //Crash Disconnect;
                    State.InitializeReceivedItems();
                    ServerData.Options = session.DataStorage.GetSlotData<SlotData>();
                    ArchipelagoOptions.LoadFromSlotData(ServerData.Options);
                    // Initialize DeathLinkHandler.
                    DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName, ArchipelagoOptions.DeathLinkType != DeathLinkType.option_none);
                    GameRestart();
                }
                ArchipelagoConsole.LogMessage($"Successfully Recconnected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
            // Handles a new connection to the Server.
            else
            {
                // Gets the Initial data from the server.
                ServerData.Options = session.DataStorage.GetSlotData<SlotData>();
                ServerData.Seed = session.RoomState.Seed;
                ServerData.Index = 0;
                // Load options into the static ArchipelagoOptions class
                ArchipelagoOptions.LoadFromSlotData(ServerData.Options);
                // Initialize DeathLinkHandler.
                DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName, ArchipelagoOptions.DeathLinkType != DeathLinkType.option_none);

                session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
                // Creates the Locally Stored data for the locations. 
                CreateLocationDicts(session.Locations.AllLocations.ToArray());
                State.UpdateLocationDict();
                ArchipelagoConsole.LogMessage($"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
            session.Items.ItemReceived += OnItemReceived;
            Authenticated = true;
            // Receives any Queued Items
            DequeueItems();
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
            ArchipelagoConsole.LogMessage($"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.");

            Authenticated = false;
            Disconnect();
        }
        _AttemptingConnection = false;
    }

    /// <summary>
    ///     Internal. Receives all currently queued items from the Archipelago Server.
    /// </summary>
    private void DequeueItems() {
        // Handle intial connect to AP.
        if (!Reconnected)
        {
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                // If the item was a starting item
                if (item.LocationName == "Server")
                {
                    Logging.Log($"Attempting to receive Item: {item.ItemName}");
                    // Checks if the item recieved is a room.
                    if (Plugin.ModRoomManager.GetRoomByName(item.ItemName) != null)
                    {
                        // If rooms haven't been initialized, add it to the item queue
                        if (!ModInstance.HasInitializedRooms)
                        {
                            ModInstance.QueueManager.AddItemToQueue(item);
                            session.Items.DequeueItem();
                        }
                        else
                        {
                            ModInstance.QueueManager.ReceiveRoom(item);
                        }
                    }
                    // Not a Room.
                    else
                    {
                        session.Items.DequeueItem();
                        // Try to recieve item, on failure add it back to the queue.
                        if (!ModInstance.QueueManager.ReceiveServerItem(item))
                        {
                            ModInstance.QueueManager.AddItemToQueue(item);
                        }

                    }
                }
                else
                {
                    //Handle non-server items normally.
                    session.Items.DequeueItem();
                    if (!ModInstance.QueueManager.ReceiveItem(item))
                    {
                        ModInstance.QueueManager.AddItemToQueue(item);
                    }
                }
            }
        }
        else if (!Disconnected)
        {
            ModInstance.QueueManager.SetItemQueue(new List<ItemInfo>());
            List<string> Received = [.. ServerData.ReceivedItems];
            // Rebuilds as much of the gamestate as possible from the received items on a game restart.
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                session.Items.DequeueItem();
                // Handle any items that have not been received formally.
                if (Received.RemoveFirst(item.ItemName) == -1 && !item.ItemName.Contains(" Starting ")) {
                    Logging.LogWarning($"Requeueing {item.ItemName}");
                    ModInstance.QueueManager.AddItemToQueue(item);
                } 
            }
        }
        else {
            ModInstance.QueueManager.SetItemQueue(new List<ItemInfo>());
        }
    }

    /// <summary>
    ///     Handles everything that should be handled on a basic reconnect.
    /// </summary>
    private void Reconnect() {
        ArchipelagoConsole.LogMessage("Attemping to reconnect...");
        Reconnected = true;
        ArchipelagoConsole.LogMessage("Rebuilding Archipelago State...");
        RebuildCheckedLocations();
    }

    /// <summary>
    ///     Handles all reconnection steps that should occur on recconnecting after a game crash or shut down. (game/modstate needs to be rebuilt).
    /// </summary>
    private void GameRestart() {
        ArchipelagoConsole.LogMessage("Attemping to reconnect after game restart...");
        Reconnected = true;
        CreateLocationDicts(session.Locations.AllLocations.ToArray());
        State.UpdateLocationDict();
        if (ModInstance.IsInRun) {
            ArchipelagoConsole.LogMessage("Rebuilding Archipelago State...");
            
            RebuildState();
        }
        ArchipelagoConsole.LogMessage("Gathering Seed Data...");
    }

    /// <summary>
    ///     Rebuilds the state of the game and mod to match the archipelago and stored data.
    /// </summary>
    public void RebuildState() {
        long[] locationids = session.Locations.AllLocationsChecked.ToArray();
        for (int i = 0; i < locationids.Length; i++) {
            long locationid = locationids[i];
            try
            {
                string location = ServerData.LocationDict[locationid];
                if (location.EndsWith("First Pickup"))
                {
                    UniqueItem item = Plugin.ModItemManager.GetUniqueItem(location.Replace(" First Pickup", ""));
                    item.HasBeenFound = true;
                }
                else if (location.EndsWith("First Entering"))
                {
                    ModRoom room = Plugin.ModRoomManager.GetRoomByName(location.Replace(" First Entering", ""));
                    room.IsUnlocked = true;
                }
                // Try Upgrade Disks. If that fails, try Permanent Unlocks.
                else if (!ModItemManager.UpgradeDisks.UnlockLocationIfExists(location))
                {
                    PermanentUnlock permSolved = Unlocks.GetPermanentSolveByLocation(location);
                    if (permSolved != null)
                    {
                        permSolved.Solved = true;
                    }
                }
            }
            catch
            {
                Logging.LogWarning($"Unable to find location name for location with id {locationids[i]}");
            }
            
        }
        foreach (ItemInfo item in session.Items.AllItemsReceived)
        {
            if (item.ItemName.ToUpper().Contains("UPGRADE DISK")) {
                string location = item.ItemName.ToUpper().Replace("UPGRADE DISK ", "");
                if (!ModItemManager.UpgradeDisks.RecievedItems.Contains(location))
                {
                    ModItemManager.UpgradeDisks.RecievedItems.Add(location);
                }
            }
            UniqueItem uniqueItem = Plugin.ModItemManager.GetUniqueItem(item.ItemName);
            PermanentItem permanentItem = Plugin.ModItemManager.GetPermanentItem(item.ItemName);
            if (uniqueItem != null)
            {
                uniqueItem.IsUnlocked = true;
            }
            else if (permanentItem != null) { 
                permanentItem.IsUnlocked = true;
                permanentItem.UnlockedCount++;
            }
            else
            {
                PermanentUnlock permUnlock = Unlocks.GetPermanentUnlock(item.ItemName);
                if (permUnlock != null)
                {
                    permUnlock.Unlocked = true;
                }
            }
        }
        
        // Handle all the items that are not preserved by the game.
        StateRebuilt = true;
    }
    /// <summary>
    ///     Attempts to rebuild the checked location list based on local and server locations.
    /// </summary>
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
            if (found && i < localLocations.Count) {
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
                ModInstance.QueueManager.AddLocationsToQueue(localLocations);
            }
        }
    }

    /// <summary>
    ///     Populates the dictionaries used for looking up location information.  
    /// </summary>
    /// <param name="locationIds">A series of known location ids.</param>
    /// <param name="hint">Whether the location lookups should also be hinted.</param>
    private void CreateLocationDicts(long[] locationIds, bool hint = false)
    {
        long[] serverLocations = [.. locationIds];
        for (int i = 0; i < serverLocations.Length; i++)
        {
            long location = serverLocations[i];
            // Only add new data if the old one is not good.
            if (!ServerData.LocationDict.ContainsKey(location)) { 
            
                string locationName = session.Locations.GetLocationNameFromId(location);
                ServerData.LocationDict[location] = locationName;
            }
        }

        Task<Dictionary<long, ScoutedItemInfo>> scoutTask = null;

        if (hint)
        {
            scoutTask = session.Locations
                .ScoutLocationsAsync(hint, serverLocations);
        }
        else
        {
            //Asynchronously gather the data for all items stored in all the active locations, then wait for a response.
            scoutTask = session.Locations
                    .ScoutLocationsAsync(hint, [.. ServerData.LocationDict.Keys]);
        }
        scoutTask.Wait();
        Dictionary<long, ScoutedItemInfo> scoutResult = scoutTask.Result;
        foreach (KeyValuePair<long, ScoutedItemInfo> scout in scoutResult)
        {
            long locationId = scout.Key;
            long itemId = scout.Value.ItemId;
            string itemName = scout.Value.ItemName ?? $"?Item {itemId}";
            ServerData.ItemDict[itemId] = itemName;
            ServerData.LocationItemMap[locationId] = scout.Value;
        }
    }

    /// <summary>
    ///     Scouts a series of locations and hints them.
    /// </summary>
    /// <param name="locationIds"></param>
    public void ScoutLocationHint(long[] locationIds) => CreateLocationDicts(locationIds, true);

    /// <summary>
    ///     Something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect()
    {
        Reconnected = false;
        Logging.LogDebug("disconnecting from server...");
        session?.Socket.DisconnectAsync();
        session = null;
        Authenticated = false;
    }

    /// <summary>
    ///    Sends a message to the Archipelago Server. 
    /// </summary>
    /// <param name="message"></param>
    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }

    /// <summary>
    ///     We received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo receivedItem = helper.DequeueItem();

        if (helper.Index <= ServerData.Index) return;

        ServerData.Index++;
        Logging.Log($"Attempting to recieve item: {receivedItem.ItemName}", "Items");
        ModInstance.QueueManager.AddItemToQueue(receivedItem);
        
    }

    /// <summary>
    ///     Something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Logging.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    ///     Something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Disconnected = true;
        Logging.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }

    /// <summary>
    ///     Whenever a local location(s) are checked remotely (like via a server command)
    /// </summary>
    /// <param name="newCheckedLocations">the ids of the locations that were checked.</param>
    private void OnRemoteLocationChecked(ReadOnlyCollection<long> newCheckedLocations) { 
        //TODO: Add code for normalizing the gamestate for those location unlocks.
    }
    /// <summary>
    ///     Sends to the server that the location has been checked.
    /// </summary>
    /// <param name="locationName">the name of the location to complete</param>
    public void CheckLocation(string locationName) {
        long locationid = GetLocationFromName(locationName);
        if (locationid > 0)
        {
            CheckLocation(locationid);
        }
        else 
        {
            Logging.Log($"Location '{locationName}' not found in Archipelago data. Unable to send location check.");
        }
    }
    /// <summary>
    ///     Sends to the server that the location has been checked.
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
        else if (locationid > 1) {
            Logging.Log($"Unable to send location for {ServerData.LocationDict[locationid]}. Location has already been sent or is not being used for this seed.", "Locations");
        }
    }

    /// <summary>
    ///     Sends the goal completed notification to the server.
    /// </summary>
    public void GoalCompleted()
    {
        session.SetGoalAchieved();
        State.Reset(); //Resets the State. 
    }
}

/// <summary>
///     A manager that handles certain lists as queues as queues were running into thread safety issues.
/// </summary>
public class ArchipelagoQueueManager {
    private ItemQueue _ReceivedItemQueue = new("Received Item Queue");
    private LocationQueue _LocationQueue = new("Location Queue");
    private UpgradeDiskUsedQueue _UpgradeUsedQueue = new("Upgrade Disk Used Queue");

    /// <summary>
    ///     Adds an item to the Item Queue.
    /// </summary>
    /// <param name="item">The item to add to queue.</param>
    public void AddItemToQueue(ItemInfo item) { 
        _ReceivedItemQueue.Enqueue(item);
    }
    /// <summary>
    ///     Removes an item from the queue.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void RemoveItemFromQueue(ItemInfo item) { 
        _ReceivedItemQueue.RemoveItemFromQueue(item);
    }

    /// <summary>
    ///     Replaces the item queue with the provided List.
    /// </summary>
    /// <param name="queueList">The List with which to replace the queue.</param>
    public void SetItemQueue(List<ItemInfo> queueList) {
        _ReceivedItemQueue.SetQueueList(queueList);
    }

    /// <summary>
    ///     Adds an upgrade usage to the Queue.
    /// </summary>
    /// <param name="value">The internal upgrade disk id</param>
    /// <returns>Returns if the value was successfully enqueued.</returns>
    public bool AddUpgradeUsedToQueue(int value) {
        return _UpgradeUsedQueue.Enqueue(value);
    }
    /// <summary>
    ///     Replaces the location queue with the provided List.
    /// </summary>
    /// <param name="queueList">The List with which to replace the queue.</param>
    public void SetLocationQueue(List<string> queueList) {
       _LocationQueue.SetQueueList(queueList);
    }

    /// <summary>
    ///     Returns the Item Queue as a list.
    /// </summary>
    /// <returns>The List of items in the queue.</returns>
    public List<ItemInfo> GetItemQueue() {
        return _ReceivedItemQueue.GetItemQueue();
    }
    /// <summary>
    ///     Returns the location Queue as a list.
    /// </summary>
    /// <returns>The list of locations in the queue.</returns>
    public List<string> GetLocationQueue()
    {
        return _LocationQueue.GetLocationQueue();
    }
    /// <summary>
    ///     Adds multiple locations to the Location Queue.
    /// </summary>
    /// <param name="locations"></param>
    public void AddLocationsToQueue(List<long> locations) {
        List<string> locationNames = new List<string>();
        foreach (int location in locations) {
            try
            {
                string locationName = ArchipelagoClient.ServerData.LocationDict[location];
                locationNames.Add(locationName);
            }
            catch {
                Logging.LogWarning($"Error Loading location {location}");
            }
        }
        _LocationQueue.Enqueue(locationNames.ToArray());
    }

    /// <summary>
    ///     Releases all the currently queued locations.
    /// </summary>
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

    /// <summary>
    ///     Releases all the currently queued items.
    /// </summary>
    public void ReleaseAllQueuedItems()
    {
        if (_ReceivedItemQueue.Count > 0)
        {
            for (int i = 0; i < _ReceivedItemQueue.Count; i++)
            {
                // Dequeues the item.
                ItemInfo item = _ReceivedItemQueue.Dequeue();
                // Tries to receive the item.
                if (!ReceiveItem(item))
                {
                    // On failure requeue the item.
                    _ReceivedItemQueue.Enqueue(item);
                }
            }
        }
    }

    /// <summary>
    ///     Tries to receive an item.
    /// </summary>
    /// <param name="item">The item to attempt to receive.</param>
    /// <param name="ignoreState">Whether this attempt should trigger a state update.</param>
    /// <returns>On sucess returns true. On failure returns false.</returns>
    public bool ReceiveItem(ItemInfo item, bool ignoreState = true)
    {
        if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated)
        {
            ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
            PermanentUnlock unlock = Unlocks.GetPermanentUnlock(item.ItemName);
            if (unlock != null)
            {

                Logging.Log($"Attempting to receive Unlock: {item.ItemName}", "Items");
                unlock.UnlockItem();
                return true;
            }
            // Checks if the item recieved is a Room (includes special mappings like classroom variants)
            if (Plugin.ModRoomManager.IsRoomItem(item.ItemName))
            {
                ReceiveRoom(item);
                return true;
            }
            if (item.Flags.HasFlag(ItemFlags.Trap))
            {
                // If a trap is received while in run receive it.
                if (ModInstance.IsInRun)
                {
                    ReceiveTrap(item, ignoreState);
                    return true;
                }
                return false;
            }
            // If the item is an upgrade disk.
            if (item.ItemName.ToUpper().Contains("UPGRADE DISK"))
            {
                if (ModInstance.IsInRun)
                {
                    // Trim the name of the item to remove the upgrade disk part.
                    ModItemManager.UpgradeDisks.AddItemToInventory(item.ItemName.ToUpper().Replace("UPGRADE DISK ", ""));
                    return true;
                }
                return false;
            }
            // if not handle it as an Item.
            string itemType = Plugin.ModItemManager.GetItemType(item.ItemName);
            if (itemType == null) {
                Logging.LogWarning($"Error receiving item {item.ItemName}: Item does not exist or is not currently handled by the mod.");
                return true;
            }
            else {

                if (ModInstance.IsInRun)
                {
                    ReceiveLocalItem(item, ignoreState);
                    return true;
                }
            }
           
        }
        return false;
    }

    /// <summary>
    ///     Handles the receiving of an item from the server. (Usually for starting inventory).
    /// </summary>
    /// <param name="item">The item to attempt to receive.</param>
    /// <param name="ignoreState">Whether this attempt should trigger a state update.</param>
    /// <returns>On sucess returns true. On failure returns false.</returns>
    public bool ReceiveServerItem(ItemInfo item, bool ignoreState = false) {
        if (ModInstance.IsInRun)
        {
            ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
            PermanentUnlock unlock = Unlocks.GetPermanentUnlock(item.ItemName);
            if (unlock != null)
            {
                unlock.UnlockItem();
            }
        }

        if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms)
        {
            ArchipelagoClient.ServerData.ReceivedItems.Add(item.ItemName);
            // Checks if the item recieved is a Room (includes special mappings like classroom variants)
            if (Plugin.ModRoomManager.IsRoomItem(item.ItemName))
            {
                ReceiveRoom(item);
                return true;
            }
            if (item.Flags.HasFlag(ItemFlags.Trap))
            {
                // If a trap is received while in run receive it.
                if (ModInstance.IsInRun)
                {
                    ReceiveTrap(item);
                    return true;
                }
                return false;
            }
            // If the item is an upgrade disk.
            if (item.ItemName.ToUpper().Contains("UPGRADE DISK")) {
                
                // Trim the name of the item to remove the upgrade disk part.
                string location = item.ItemName.ToUpper().Replace("UPGRADE DISK ", "");
                if (!ModItemManager.UpgradeDisks.RecievedItems.Contains(location))
                {
                    ModItemManager.UpgradeDisks.RecievedItems.Add(location);
                }
                return false;
            }
            // if not handle it as an Item.
            string itemType = Plugin.ModItemManager.GetItemType(item.ItemName);
            if (itemType == null)
            {
                Logging.LogWarning($"Error receiving item {item.ItemName}: Item does not exist or is not currently handled by the mod.");
                return true;
            }
            if (itemType == "Permanent")
            {
                ReceiveLocalItem(item);
                return true;
            }
            else if (itemType == "Unique") {
               
                UniqueItem uItem = Plugin.ModItemManager.GetUniqueItem(item.ItemName);
                if (uItem != null)
                {
                    uItem.IsUnlocked = true;
                    return true;
                }
                else {
                    Logging.LogWarning($"Error receiving item {item.ItemName}: Item does not exist or is not currently handled by the mod.");
                }
            }
            else
            {

                if (ModInstance.IsInRun)
                {
                    ReceiveLocalItem(item);
                    return true;
                }
            }

        }
        return false;
    }

    /// <summary>
    ///     Dequeues the next item in the Queue.
    /// </summary>
    public void DequeueItem() {
        if (_ReceivedItemQueue.Count > 0)
        {
            ItemInfo item = _ReceivedItemQueue.Dequeue();
            ReceiveItem(item);
        }
    }

    /// <summary>
    ///     Dequeues the next location in the Queue.
    /// </summary>
    public void DequeueLocation() {
        if (_LocationQueue.Count > 0)
        {
            string location = _LocationQueue.Dequeue();
            Plugin.ArchipelagoClient.CheckLocation(location);
        }
    }

    /// <summary>
    ///     Dequeues the next upgrade disk use in the queue.
    /// </summary>
    public void DequeueUsedUpgrade() {
        if (_UpgradeUsedQueue.Count > 0) {
            int upgradeId = _UpgradeUsedQueue.Dequeue() ?? -1;
            if (upgradeId > 0)
            {
                ModItemManager.UpgradeDisks.OnUsed(upgradeId);
            }
        }
    }

    /// <summary>
    ///     Adds a location to the queue.
    /// </summary>
    /// <param name="name">The name of the location to Dequeue.</param>
    public void AddLocationToQueue(string name) { 
        _LocationQueue.Enqueue(name);
    }

    /// <summary>
    ///    Handles receiving an Room. (doesn't check if it was successfully recieved.).  
    /// </summary>
    /// <param name="item">The archipelago ItemInfo of the room that is being received.</param>
    public void ReceiveRoom(ItemInfo item) {
        // Try to find the room, using mapping for special cases
        ModRoom room = Plugin.ModRoomManager.GetRoomByName(item.ItemName);

        bool isMappedRoom = false;
        string mappedName = null;

        // If not found with exact name, try the mapped name
        if (room == null)
        {
            mappedName = Plugin.ModRoomManager.GetMappedRoomName(item.ItemName);
            if (mappedName != null)
            {
                room = Plugin.ModRoomManager.GetRoomByName(mappedName);
                isMappedRoom = true;
            }
        }

        if (room == null)
        {
            Logging.LogWarning($"ReceiveRoom: Could not find room '{item.ItemName}'");
            return;
        }

        room.IsUnlocked = true;

        // Update the RoomRecords to simulate the room as having been drafted once. This makes it so the directory properly displays the unlocked room pool.
        if (!room.AddedToDirectory)
        {
            Il2CppSystem.Collections.Hashtable RoomRecords = GameObject.Find("Global Persitent Manager").GetHashTableProxy("RoomRecords").hashTable;
            if (RoomRecords.ContainsKey(room.Name))
            {
                int value = RoomRecords[room.Name].Unbox<int>();
                if (value == 0)
                {
                    RoomRecords[room.Name] = 1;
                }
                room.AddedToDirectory = true;
            }
        }
        // Special handling for CLASSROOM: always increment pool count
        // This allows receiving multiple "Classroom" items to add multiple copies to the pool
        // The base game will randomly pick which grade appears when drafted
        string roomNameUpper = room.Name.ToUpper().Trim();
        if (roomNameUpper == "CLASSROOM")
        {
            room.RoomPoolCount++;
            Logging.Log($"Received '{item.ItemName}': Pool count now {room.RoomPoolCount}");
        }
        // For mapped rooms, always increment pool count
        else if (isMappedRoom)
        {
            room.RoomPoolCount++;
            Logging.Log($"Received '{item.ItemName}' (maps to '{mappedName}'): Pool count now {room.RoomPoolCount}");
        }
        // For other rooms, only increment if pool is already full
        else
        {
            if (room.RoomsLeftInPool == 0)
            {
                room.RoomPoolCount++;
            }
        }
        // Update the pools immediately if we're in a run
        room.Handler?.OnRoomUnlocked(room);
        Logging.Log($"Room '{room.Name}' unlocked and added to pool.");
    }
    /// <summary>
    ///     Handles receiving a trap. (Doesn't check if it was successfully received).  
    /// </summary>
    /// <param name="item">The ItemInfo of the received trap.</param>
    /// <param name="ignoreState">Whether the State should be ignored on receiving the trap.</param>
    public void ReceiveTrap(ItemInfo item, bool ignoreState = false) {
        Plugin.ModItemManager.OnTrapReceived(item);
    }
    /// <summary>
    ///     Handles receiving a local item. (Doesn't check if it was successfully received).
    /// </summary>
    /// <param name="item">The ItemInfo of the received trap.</param>
    /// <param name="ignoreState">Whether the State should be ignored on receiving the trap.</param>
    public void ReceiveLocalItem(ItemInfo item, bool ignoreState = false) {
        Plugin.ModItemManager.OnItemCheckRecieved(item);
        //This may need to be moved to a better place once the item code is better implemented.
    }
    /// <summary>
    ///     Checks if a location check can be sent.
    /// </summary>
    /// <returns>True if a location can be sent, false if it cannot.</returns>
    private bool SendLocationCheck() {
        return ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated;
    }
}

/// <summary>
///     A class for treating an item list like a Queue.
/// </summary>
/// <param name="name">The name of the Queue.</param>
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
    /// <summary>
    ///     Adds an item to the Queue.
    /// </summary>
    /// <param name="item">The item to add to the queue.</param>
    public void Enqueue(ItemInfo item) {
        if (item != null)
        {
            _Queue.Add(item);
        }
    }

    /// <summary>
    ///     Adds multiple items to the queue.
    /// </summary>
    /// <param name="items">The collection of items to add to the queue.</param>
    public void Enqueue(ItemInfo[] items) {
        _Queue.AddRange(items);
    }

    /// <summary>
    ///     Removes an item from the queue.
    /// </summary>
    /// <param name="item">The item to find.</param>
    public void RemoveItemFromQueue(ItemInfo item) {
        int index = IndexOf(item.ItemName);
        if (index != -1) {
            _Queue.RemoveAt(index);
        }
    }
    /// <summary>
    ///     Finds the index of an item via the item name.
    /// </summary>
    /// <param name="itemName">The item name of the item to find.</param>
    /// <returns>The index of the item to find. -1 if not found.</returns>
    private int IndexOf(string itemName) {
        for (int i = 0; i < _Queue.Count; i++) {
            if (_Queue[i].ItemName == itemName) {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    ///     Dequeues an item from the queue.
    /// </summary>
    /// <returns>The item info of the dequeued Item.</returns>
    public ItemInfo Dequeue() {
        if (_Queue.Count == 0) {
            Logging.LogWarning("No Items in Queue, cannot Dequeue");
            return null;
        }
        ItemInfo temp = _Queue[0];
        _Queue.RemoveAt(0);
        return temp;
    }

    /// <summary>
    ///     Replaces the queue with the provided list.
    /// </summary>
    /// <param name="queueList">The list to replace with.</param>
    public void SetQueueList(List<ItemInfo> queueList)
    {
        _Queue = queueList;
    }

    /// <summary>
    ///     Returns the ItemQueue as a list.
    /// </summary>
    /// <returns>The ItemQueue as a list.</returns>
    public List<ItemInfo> GetItemQueue()
    {
        return _Queue;
    }
}

/// <summary>
///      A class for treating an item list like a Queue.
/// </summary>
/// <param name="name">The name of the location Queue.</param>
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
    /// <summary>
    ///     Adds a location to the queue.
    /// </summary>
    /// <param name="location">The location to add to the queue.</param>
    public void Enqueue(string location)
    {
        _Queue.Add(location);
    }
    /// <summary>
    ///     Adds multiple locations to the queue.
    /// </summary>
    /// <param name="locations">The collection of locations to add to the queue.</param>
    public void Enqueue(string[] locations)
    {
        _Queue.AddRange(locations);
    }

    /// <summary>
    ///     Dequeues the next location from the queue.
    /// </summary>
    /// <returns>The Dequeued location.</returns>
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

    /// <summary>
    ///     Replaces the queue with the given list.
    /// </summary>
    /// <param name="queueList">The list to replace the queue with.</param>
    public void SetQueueList(List<string> queueList)
    {
        _Queue = queueList;
    }

    /// <summary>
    ///     Returns the LocationQueue as a list.
    /// </summary>
    /// <returns>The LocationQueue as a list.</returns>
    public List<string> GetLocationQueue()
    {
        return _Queue;
    }

}

/// <summary>
///     A class for queueing Upgrade Disk Uses for the next Update().
/// </summary>
/// <param name="name">The Name of the Queue.</param>
public class UpgradeDiskUsedQueue(string name)
{
    private readonly string _Name = name;
    public string Name
    {
        get { return _Name; }
    }
    private List<int> _Queue = new List<int>();
    public int Count
    {
        get { return _Queue.Count; }
    }
   
    /// <summary>
    ///     Adds the upgrade disk id to the queue.
    /// </summary>
    /// <param name="value">The uppgrade disk id to be queued.</param>
    /// <returns>If the value was successfully Queued.</returns>
    public bool Enqueue(int value)
    {
        if (!_Queue.Contains(value)) {
            _Queue.Add(value);
            return true;
        }
        return false;
    }

    /// <summary>
    ///    Dequeues the next upgrade disk id.
    /// </summary>
    /// <returns>The dequeued upgrade disk id, or null if none to dequeue.</returns>
    public int? Dequeue()
    {
        if (_Queue.Count == 0)
        {
            Logging.LogWarning("No Locations in Queue, cannot Dequeue");
            return null;
        }
        int temp = _Queue[0];
        _Queue.RemoveAt(0);
        return temp;
    }

    /// <summary>
    ///     Replaces the UpgradeDiskQueue with the provided list.
    /// </summary>
    /// <param name="queueList">The list to replace the UpgradeDiskUsedQueue with.</param>
    public void SetQueueList(List<int> queueList)
    {
        _Queue = queueList;
    }
}