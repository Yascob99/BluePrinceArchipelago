using System.Collections.Generic;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Showroom : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];

    private PlayMakerFSM _ShowroomMenuFsm;

    public Showroom()
    {
        Logging.Log("Initializing Showroom.");
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        Logging.Log("Showroom drafted, setting up.");
        RoomGameObject = roomGameObject;

        if (RoomGameObject == null)
        {
            Logging.LogError("Failed to find Showroom room GameObject, aborting OnRoomDrafted.");
            return;
        }

        _ShowroomMenuFsm = GameObject.Find("UI OVERLAY CAM").transform.Find("Showroom Menu")?.gameObject?.GetFsm("FSM");

        SetupShowroomItems();
    }

    private static readonly string[] ItemStateNames = ["items A 1", "items A 2", "items A 3", "items A 4", "items A 5", "items A 6", "items B 1", "items B 2", "items B 3", "items B 4", "items B 5", "items B 6"];

    private static readonly Dictionary<string, string[]> ItemPickupStates = new()
    {
        {"EMERALD BRACELET",["Em Purchase", "Em Purchase 2"] },
        {"MOON PENDANT", ["Moon Purchase"]},
        {"ORNATE COMPASS", ["Compass Purchase"]},
        {"MASTER KEY", ["Master Key Purchase"]},
        {"CHRONOGRAPH", ["Chronograph Purchase"]},
        {"SILVER SPOON PURCHASE", ["Silver Spoon Purchase"]},
    };
    private void SetupShowroomItems()
    {
        Logging.LogWarning("Adjusting Showroom FSM");
        foreach (var stateName in ItemStateNames)
        {
            var state = _ShowroomMenuFsm.GetState(stateName);
            if (state == null)
            {
                Logging.LogError($"Failed to find state {stateName} in Showroom Menu FSM.");
                continue;
            }

            SetProperty propSetActions = state.GetFirstActionOfType<SetProperty>();
            var target = propSetActions.targetProperty.StringParameter.Value;

            if (!LocationMap.ContainsKey(target))
            {
                LocationMap.Add(target, new Models.ShopItem
                {
                    Name = target,
                });
            }

            var shopItem = LocationMap[target];

            propSetActions.targetProperty.StringParameter.Value = shopItem.GetScoutHint();
        }
        // Prevent not unlocked items from being added to inventory.
        foreach (var item in ItemPickupStates) {
            string itemName = item.Key;
            string[] stateNames = item.Value;
            UniqueItem Item = ModItemManager.GetUniqueItem(itemName);
            if (Item != null) {
                if (!Item.IsUnlocked) {
                    foreach (string stateName in stateNames) {
                        FsmState state = _ShowroomMenuFsm.GetState(stateName);
                        if (stateName != "Chronograph Purchase")
                        {
                            state.DisableAction(2);
                        }
                        else { 
                            state.DisableAction(3);
                        }
                        state.DisableAction(4);
                    }
                }
            }
        }
    }
}