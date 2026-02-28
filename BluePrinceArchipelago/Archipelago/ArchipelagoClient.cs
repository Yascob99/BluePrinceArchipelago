using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ES3;

namespace BluePrinceArchipelago.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.5.0";
    private const string Game = "My Game";

    public static bool Authenticated;
    private bool _AttemptingConnection;
    public static bool Reconnected;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    public ArchipelagoClient() {
    }

    public void LoadStateData()
    {
        ServerData = State.GetData<ArchipelagoData>("ServerData") ?? new(); // Restore ServerData from State Data, if it doesn't exist it will be set to the default.
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
            Plugin.BepinLogger.LogError(e);
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
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    session.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems, // TODO make sure to change this line
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: false // ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            _AttemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        string outText;
        if (result.Successful)
        {
            //TODO Add check to confirm the client is reconnecting.

            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
            Authenticated = true;
            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            if (CheckReconnect())
            {
                Reconnect();
                ArchipelagoConsole.LogMessage($"Successfully Recconnected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
            else
            {
                session.RoomState.Seed.Store<string>("Seed");
                session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
                CreateLocationDicts(session.Locations.AllLocations.ToArray());
                ServerData.Store<ArchipelagoData>("ServerData"); 
                ArchipelagoConsole.LogMessage($"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!");
            }
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText += "\n" + failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Plugin.BepinLogger.LogError(outText);

            Authenticated = false;
            Disconnect();
        }
        _AttemptingConnection = false;
    }
    //Checks the seed to see if the data matches.
    private bool CheckReconnect() {
        string seed = State.GetData<string>("Seed") ?? "";
        if (seed == session.RoomState.Seed) { 
            return true;
        }
        return false;
    }

    private void Reconnect() {
        Reconnected = true;
        ServerData = State.GetData<ArchipelagoData>("ServerData");
        RebuildLocations();
    }
    private void RebuildLocations()
    {
        // Make copies of the lists for editing purposes.
        List<long> serverLocations = [.. session.Locations.AllLocations];
        List<long> localLocations = [.. ServerData.CheckedLocations];
        bool found = false;
        bool updated = false;
        int i = 0;

        foreach (long location in serverLocations) {
            found = false;
            i = 0;
            while (i < localLocations.Count && !found) {
                if (localLocations[i] == location) { 
                    found = true;
                }
                i++;
            }
            if (!found) {
                //If the server has locations checked that the local game didn't send while disconnected, add them to the checked locationlist.
                ServerData.CheckedLocations.Add(location);
                updated = true;
            }
            if (found) { 
                localLocations.RemoveAt(i); //Remove the value from the list so we can send the checks that occurred while disconnected.
            }

        }
        if (updated) {
            // Update server data if there were queued locations from the server.
            ServerData.Store<ArchipelagoData>("ServerData");
        }
        if (localLocations.Count > 0) {
            if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated)
            {
                // Update the session with any local locations that weren't yet sent due to a disconnection.
                session.Locations.CompleteLocationChecksAsync(localLocations.ToArray());
            }
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
        State.Update("ServerData", ServerData);
        Plugin.BepinLogger.LogDebug("disconnecting from server...");
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
        Plugin.BepinLogger.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
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
        long locationid = session.Locations.GetLocationIdFromName("Blue Prince", locationName);
        // verify the location has not already been sent to prevent sending duplicate locations.
        if (!ServerData.CheckedLocations.Contains(locationid))
        {
            session.Locations.CompleteLocationChecks([locationid]);
            ServerData.CheckedLocations.Add(locationid);
        }
        else {
            Plugin.BepinLogger.LogMessage($"Unable to send location for {locationName}. Location has already been sent.");
        }
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
        }
        else {
            Plugin.BepinLogger.LogMessage($"Unable to send location for {ServerData.LocationDict[locationid]}. Location has already been sent.");
        }
    }
    public void GoalCompleted()
    {
        session.SetGoalAchieved();
        State.Reset();
    }
}
public class ArchipelagoQueueManager {
    private Queue<ItemInfo> _ReceivedItemQueue = new();
    private Queue<string> _LocationQueue = new();

    public void LoadFromState() {
        _ReceivedItemQueue = State.GetData<Queue<ItemInfo>>("ReceivedItemQueue");
        _LocationQueue = State.GetData<Queue<string>>("LocationQueue");
    }
    public void AddItemToQueue(ItemInfo item) { 
        _ReceivedItemQueue.Enqueue(item);
        _ReceivedItemQueue.OnQueueEvent("ReceivedItemQueue", "Add");
    }
    public void RemoveItemFromQueue() { 
       ItemInfo item = _ReceivedItemQueue.Dequeue();
        _ReceivedItemQueue.OnQueueEvent("ReceivedItemQueue", "Remove");
    }
    public void AddLocationsToQueue(List<long> locations) {
        foreach (int location in locations) {
            string locationName = ArchipelagoClient.ServerData.LocationDict[location];
            _LocationQueue.Enqueue(locationName);
        }
        _LocationQueue.OnQueueEvent("LocationQueue", "Add[]");
    }

    public void AddLocationToQueue(string location) { 
        _LocationQueue.Enqueue(location);
        _LocationQueue.OnQueueEvent("LocationQueue", "Add");
    }
    public void RemoveLocationFromQueue() { 
        string location = _LocationQueue.Dequeue();
        _LocationQueue.OnQueueEvent("LocationQueue", "Remove");
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
                else { 
                    //TODO handle releasing items. 
                }
            }
        }
    }
    public bool RecieveItem(ItemInfo item)
    {
        if (ModInstance.SceneLoaded && ModInstance.HasInitializedRooms)
        {
            // Checks if the item recieved is an item.
            if (ModRoomManager.VanillaRooms.Contains(item.ItemName.ToUpper()))
            {
                ModRoom room = Plugin.ModRoomManager.GetRoomByName(item.ItemName.ToUpper());
                room.IsUnlocked = true;
                if (room.RoomPoolCount == 0) room.RoomPoolCount++;
                return true;
            }
            // if not handle it as an Item.
            Plugin.ModItemManager.OnItemCheckRecieved(item);
            return true;
        }
        return false;
    }
    private bool SendLocationCheck() {
        return ModInstance.SceneLoaded && ModInstance.HasInitializedRooms && ArchipelagoClient.Authenticated;
    }
}
//Makes sending Queue Related events easier.
public static class QueueExtensions {

    public static void OnQueueEvent<T>(this Queue<T> queue, string senderName, string eventType)
    {
        ModInstance.Instance.ModEventHandler.OnQueueEvent("LocationQueue", "Remove", queue, queue.GetType());
    }
    public static void OnQueueEvent<T>(this Queue<T> queue, string senderName, string eventType, Type type)
    {
        ModInstance.Instance.ModEventHandler.OnQueueEvent("LocationQueue", "Remove", queue, type);
    }
}