
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
    public List<Models.ShopItem> LocationPool { get; set; } = new List<Models.ShopItem>();
    public Models.ShopItem[] ShopItems { get; set; } = new Models.ShopItem[4];
    private bool[] ShopItemForSale { get; set; } = new bool[4];
    private string[] VanillaShopItems = new string[4];

    private PlayMakerFSM _ItemsForSaleFsm;
    private GameObject _CommissaryMenuGameObject;
    private PlayMakerFSM _CommissaryMenuFsm;
    private GameObject _ColliderGameObject;

    private readonly int _seed;

    public Commissary(int locationCount, int minPrice, int maxPrice, int seed)
    {
        Logging.Log($"Initializing Commissary with {locationCount} items, price range {minPrice}-{maxPrice}, seed {seed}.");
        _seed = seed;
        for (int i = 0; i < locationCount; i++)
        {
            LocationPool.Add(new Models.ShopItem
            {
                Name = $"Commissary Purchase {i + 1}",
                Price = new System.Random(seed + i).Next(minPrice, maxPrice + 1)
            });
        }
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
        _CommissaryMenuGameObject = GameObject.Find("UI OVERLAY CAM/Commissary Menu");
        _CommissaryMenuFsm = _CommissaryMenuGameObject?.GetFsm("FSM");
        _ColliderGameObject = RoomGameObject.transform.Find("_GAMEPLAY/Click Commissary Collider")?.gameObject;

        var rand = new System.Random(_seed);
        for (int i = 0; i < ShopItems.Length; i++)
        {
            if (LocationPool.Count == 0)
            {
                Logging.LogWarning("Not enough items in the location pool to fill all shop slots.");
                break;
            }

            var item = LocationPool[rand.Next(LocationPool.Count)];
            LocationPool.Remove(item);
            ShopItems[i] = item;
        }

        SetupItemsForSale();
        SetupCommissaryMenu();
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

        var rand = new System.Random(_seed);

        foreach (var stateName in ItemStateNames)
        {
            var state = _ItemsForSaleFsm.GetState(stateName);
            if (state == null)
            {
                Logging.LogWarning($"State '{stateName}' not found in Items For Sale FSM.");
                continue;
            }

            var actions = state.GetActionsOfType<SetFsmString>();

            bool hasChangedName = false;
            bool hasAddedItem = false;
            int i = rand.Next(ShopItems.Length);

            foreach (var action in actions)
            {
                if (action is SetFsmString setFsmString)
                {
                    var varName = setFsmString.variableName;
                    if (varName.Value.StartsWith("ITEM") && varName.Value.EndsWith("NAME"))
                    {
                        if (UniqueItemManager.ComissaryStates.ContainsKey(setFsmString.setValue.Value) || ShopItems.Any(item => item.Name == setFsmString.setValue.Value))
                        {
                            VanillaShopItems[i] = setFsmString.setValue.Value;

                            setFsmString.setValue.Value = $"{ShopItems[i]?.Name}: {ShopItems[i]?.GetScoutHint()}" ?? "Empty Archipelago Item Slot";

                            hasChangedName = true;
                        }
                    }
                    else if (varName.Value.StartsWith("ITEM") && varName.Value.EndsWith("PRICE") && hasChangedName)
                    {
                        setFsmString.setValue.Value = ShopItems[i]?.Price.ToString() ?? "0";
                        hasAddedItem = true;
                        hasChangedName = false;
                    }
                    else
                    {
                        Logging.Log($"Skipped SetFsmString action with variable '{varName.Value}' in state '{stateName}' because it does not match expected item name or price variable patterns.");
                    }
                }

                if (hasAddedItem)
                {
                    ShopItemForSale[i] = true;
                    i = (i + 1) % ShopItems.Length;
                    hasAddedItem = false;
                }
            }
        }
    }

    private static readonly Dictionary<string, string> ItemToCommisaryMenuState = new Dictionary<string, string>
    {
        { "Magnifying Glass", "Manifying Glass" },
        { "Running Shoes", "Running SHoes" }
    };
    private static readonly Dictionary<string, string> ItemToCommisarySaleState = new Dictionary<string, string>
    {
        
    };

    private void SetupCommissaryMenu()
    {
        if (_CommissaryMenuFsm == null)
        {
            Logging.LogError("Commissary Menu FSM not found, cannot set up menu.");
            return;
        }

        for (int i = 0; i < ShopItems.Length; i++)
        {
            if (!ShopItemForSale[i])
            {
                continue;
            }

            var vanillaItemName = ItemToCommisaryMenuState.ContainsKey(VanillaShopItems[i]) ? ItemToCommisaryMenuState[VanillaShopItems[i]] : VanillaShopItems[i];

            var state = _CommissaryMenuFsm.GetState(vanillaItemName);
            if (state == null)
            {
                Logging.LogWarning($"State '{vanillaItemName}' not found in Commissary Menu FSM.");
                continue;
            }


        }
    }
}
