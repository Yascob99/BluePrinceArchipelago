using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrinceArchipelago.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.5.0";
    private const string Game = "My Game";

    public static bool Authenticated;
    private bool attemptingConnection;
    private bool IsReconnect;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (Authenticated || attemptingConnection) return;

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
            attemptingConnection = false;
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
            if (IsReconnect)
            {
                Reconnect();
            }
            else
            {
                var success = (LoginSuccessful)result;

                ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
                Authenticated = true;

                DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
                session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
                CreateLocationDicts(session.Locations.AllLocations.ToArray());
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
        attemptingConnection = false;
    }
    private void Reconnect() { 
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
            ServerData.ItemDict[itemId] = itemName;
            ServerData.LocationItemMap[locationId] = scout.Value;
        }
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect()
    {
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

        // TODO reward the item here
        // if items can be received while in an invalid state for actually handling them, they can be placed in a local
        // queue/collection to be handled later
        Plugin.ModItemManager.OnItemCheckRecieved(receivedItem);
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
        session.Locations.CompleteLocationChecks([locationid]);
    }
    /// <summary>
    /// Sends to the server that the location has been checked.
    /// </summary>
    /// <param name="locationName">the name of the location to complete</param>
    public void CheckLocation(int locationid)
    {
        session.Locations.CompleteLocationChecks([locationid]);
    }
    public void GoalCompleted()
    {
        session.SetGoalAchieved();
    }
}