using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Models;
namespace BluePrinceArchipelago.Archipelago;

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
}