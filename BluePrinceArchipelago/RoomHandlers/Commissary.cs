
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Il2CppSystem.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public class Commissary : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];

    private PlayMakerFSM _ItemsForSaleFsm;
    private GameObject _CommissaryMenuGameObject;
    private PlayMakerFSM _CommissaryMenuFsm;
    private GameObject _ColliderGameObject;

    public Commissary()
    {
        Logging.Log("Initializing Commissary.");
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        Logging.Log("Commissary drafted, setting up shop items.");
        RoomGameObject = roomGameObject;

        if (RoomGameObject == null)
        {
            Logging.LogError("Failed to find Commissary room GameObject, aborting OnRoomDrafted.");
            return;
        }
    
        _ItemsForSaleFsm = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR SALE")?.gameObject?.GetFsm("FSM");
        _CommissaryMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Commissary Menu")?.gameObject;
        _CommissaryMenuFsm = _CommissaryMenuGameObject?.GetFsm("FSM");
        _ColliderGameObject = RoomGameObject.transform.Find("_GAMEPLAY/Click Commissary Collider")?.gameObject;

        SetupItemsForSale();
    }

    // TODO: figure out why "N Items" can't be found
    private static readonly string[] ItemStateNames = ["A Items", "B Items", "C Items", "D Items", "E Items", "F Items", "G Items", "C Items 2", "K ITEMS", "D Items 2", "A Items 2", "J ITEMS", "E Items 2", "G Items 2", "F Items 2", "M Items", "I ITEMS 6", "N Items", "I ITEMS 7", "I ITEMS", "I ITEMS 2", "I ITEMS 5", "I ITEMS 3", "I ITEMS 4", "H Items"];

    private void SetupItemsForSale()
    {
        if (_ItemsForSaleFsm == null)
        {
            Logging.LogError("Items For Sale FSM not found, cannot set up shop items.");
            return;
        }

        foreach (var stateName in ItemStateNames)
        {
            var state = _ItemsForSaleFsm.GetState(stateName);
            if (state == null)
            {
                Logging.LogWarning($"State '{stateName}' not found in Items For Sale FSM.");
                continue;
            }

            var actions = state.GetActionsOfType<SetFsmString>();

            foreach (var action in actions)
            {
                if (action is SetFsmString setFsmString)
                {
                    var varName = setFsmString.variableName;
                    if (varName.Value.StartsWith("ITEM") && varName.Value.EndsWith("NAME"))
                    {
                        if (UniqueItemManager.ComissaryStates.ContainsKey(setFsmString.setValue.Value) || setFsmString.setValue.Value.Contains("Upgrade Disk"))
                        {
                            var itemName = setFsmString.setValue.Value;

                            if (!LocationMap.ContainsKey(itemName))
                            {
                                LocationMap.Add(itemName, new Models.ShopItem
                                {
                                    Name = itemName
                                });
                            }

                            string apItem = LocationMap[itemName].GetScoutHint();

                            setFsmString.setValue.Value = apItem;
                        }
                    }
                }
            }
        }
    }
}
