using System.Collections.Generic;
using System.Linq;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker.Actions;
using Il2CppSystem.Linq;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

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

    private void SetupShowroomItems()
    {
        foreach (var stateName in ItemStateNames)
        {
            var state = _ShowroomMenuFsm.GetState(stateName);
            if (state == null)
            {
                Logging.LogError($"Failed to find state {stateName} in Showroom Menu FSM.");
                continue;
            }

            var propSetActions = state.GetActionsOfType<SetProperty>();
            foreach (var action in propSetActions)
            {
                var target = action.targetProperty.StringParameter.Value;

                if (!LocationMap.ContainsKey(target))
                {
                    LocationMap.Add(target, new Models.ShopItem
                    {
                        Name = target,
                    });
                }

                var shopItem = LocationMap[target];

                action.targetProperty.StringParameter.Value = shopItem.GetScoutHint();
            }
        }
    }
}