using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Showroom : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];

    private static PlayMakerFSM _ShowroomMenuFsm;

    public Showroom()
    {
        Logging.Log("Initializing Showroom.");
    }
    /// <summary>
    /// Attach an event to the showroom menu being opened. This is used to handle the items that are in the showroom on a given day.
    /// </summary>
    public static void SetupShowroomMenuEvent()
    {
        _ShowroomMenuFsm = GameObject.Find("UI OVERLAY CAM").transform.Find("Showroom Menu")?.gameObject?.GetFsm("FSM");
        _ShowroomMenuFsm?.GetState("State 8")?.AddFirstAction(CustomFsmMethodManager.GetCallMethod("ShowroomMenuOpened"));
    }
    /// <summary>
    /// A mapping of the names of items to the FSM state(s) corresponding to when that item is purchased and picked up
    /// </summary>
    private static readonly Dictionary<string, string[]> ItemPickupStates = new()
    {
        {ShowroomItems.EmeraldBracelet,["Em Purchase", "Em Purchase 2"] },
        {ShowroomItems.MoonPendant, ["Moon Purchase"]},
        {ShowroomItems.OrnateCompass, ["Compass Purchase"]},
        {ShowroomItems.MasterKey, ["Master Key Purchase"]},
        {ShowroomItems.Chronograph, ["Chronograph Purchase"]},
        {ShowroomItems.SilverSpoon, ["Silver Spoon Purchase"]},
    };

    /// <summary>
    /// Get the in-game showroom random numbers and map them to the corresponding list of items on sale that day
    /// </summary>
    /// <returns>The list of items on sale in the showroom.</returns>
    private static string[] OnSaleItems()
    {
        string[] onSaleItems = new string[4];
        int randomA = _ShowroomMenuFsm.GetIntVariable("showroom_items_int_A").Value;
        int randomB = _ShowroomMenuFsm.GetIntVariable("showroom_items_int_B").Value;
        switch (randomA)
        {
            case 1:
                onSaleItems[0] = ShowroomItems.EmeraldBracelet;
                onSaleItems[2] = ShowroomItems.Chronograph;
                break;
            case 2:
                onSaleItems[0] = ShowroomItems.MoonPendant ;
                onSaleItems[2] = ShowroomItems.EmeraldBracelet;
                break;
            case 3:
            default:
                onSaleItems[0] = ShowroomItems.MoonPendant;
                onSaleItems[2] = ShowroomItems.Chronograph;
                break;
        }

        switch (randomB)
        {
            case 1:
                onSaleItems[1] = ShowroomItems.OrnateCompass;
                onSaleItems[3] = ShowroomItems.SilverSpoon;
                break;
            case 2:
                onSaleItems[1] = ShowroomItems.MasterKey;
                onSaleItems[3] = ShowroomItems.OrnateCompass;
                break;
            case 3:
            default:
                onSaleItems[1] = ShowroomItems.MasterKey;
                onSaleItems[3] = ShowroomItems.SilverSpoon;
                break;
        }
        return onSaleItems;
    }
    /// <summary>
    /// Any required setup for once we know the items in the showroom
    /// </summary>
    public static void SetupShowroomItems()
    {
        GetShowroomHints();
        DisableUnfoundShowroomItems();
    }
    /// <summary>
    /// Send scout hints for all current showroom items
    /// </summary>
    private static void GetShowroomHints()
    {
        foreach (string item in OnSaleItems())
        {

            if (!LocationMap.ContainsKey(item))
            {
                LocationMap.Add(item, new Models.ShopItem
                {
                    Name = item,
                });
            }

            LocationMap[item].GetScoutHint();
        }
    }
    /// <summary>
    /// Loop through all showroom item pickup states and disable the ones for unfound items
    /// </summary>
    private static void DisableUnfoundShowroomItems()
    {
        foreach (var item in ItemPickupStates)
        {
            string itemName = item.Key;
            string[] stateNames = item.Value;
            UniqueItem Item = ModItemManager.GetUniqueItem(itemName);
            if (Item != null)
            {
                if (!Item.IsUnlocked)
                {
                    foreach (string stateName in stateNames)
                    {
                        FsmState state = _ShowroomMenuFsm.GetState(stateName);
                        if (stateName != "Chronograph Purchase")
                        {
                            state.DisableAction(2);
                        }
                        else
                        {
                            state.DisableAction(3);
                        }
                        state.DisableAction(4);
                    }
                    //TODO: Handle re-enabling this if they lose the item to the lost and found
                }
            }
        }
    }
}

public static class ShowroomItems
{
    public static readonly string EmeraldBracelet = "EMERALD BRACELET";
    public static readonly string MoonPendant = "MOON PENDANT";
    public static readonly string OrnateCompass = "ORNATE COMPASS";
    public static readonly string MasterKey = "MASTER KEY";
    public static readonly string Chronograph = "CHRONOGRAPH";
    public static readonly string SilverSpoon = "SILVER SPOON";


}