using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Rewired.Integration.PlayMaker;

namespace BluePrinceArchipelago.Archipelago;

public class ArchipelagoData (string uri = "localhost", string slotName = "Player1", string password = "")
{
    public string Uri = uri;
    public string SlotName = slotName;
    public string Password = password;
    public int Index;
    public Dictionary<string, object> slotData = new();
    public string seed = "";

    public List<long> CheckedLocations = new();
    public Dictionary<long, string> LocationDict = new(); //Stores all locationids and what name that represents.
    public Dictionary<long, string> ItemDict = new(); //Stores all items that are in this game, and their name.
    public Dictionary<long, ScoutedItemInfo> LocationItemMap = new(); //Maps the location id to it's associated item reward.

    public bool NeedSlotData => slotData == null;
}