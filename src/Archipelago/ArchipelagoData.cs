using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Models;
namespace BluePrinceArchipelago.Archipelago;

/// <summary>
///     A data structure for storing core data about the current Archipelago session mostly provided by the server.
/// </summary>
/// <param name="uri">The domain and port to connect to. Defaults to localhost:38281</param>
/// <param name="slotName">The slot name to connect to. Defaults to Player1</param>
/// <param name="password">The slot password to use. Defaults to blank.</param>
public class ArchipelagoData (string uri = "localhost:38281", string slotName = "Player1", string password = "")
{
    public string Uri = uri;
    public string SlotName = slotName;
    public string Password = password;
    public int Index = 0;
    public SlotData Options = new();
    public string Seed = "";

    public List<long> CheckedLocations = new();
    public List<string> ReceivedItems = new();
    public Dictionary<long, string> LocationDict = new(); //Stores all locationids and what name that represents.
    public Dictionary<long, string> ItemDict = new(); //Stores all items that are in this game, and their name.
    public Dictionary<long, ScoutedItemInfo> LocationItemMap = new(); //Maps the location id to it's associated item reward.
    public Dictionary<string, Object> OptionsDict = new(); //Stores all options and their values. Key is the option name, value is the option value.
    public List<List<string>> RunHistory = new();
}