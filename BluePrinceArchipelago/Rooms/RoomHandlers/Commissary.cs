using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Commissary : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];
    
    private GameObject _ItemsForSaleGameObject;
    private PlayMakerFSM _ItemsForSaleFsm;
    private GameObject _CommissaryMenuGameObject;
    private PlayMakerFSM _CommissaryMenuFsm;
    private GameObject _ColliderGameObject;
    private static readonly string[] ItemStateNames = ["A Items", "B Items", "C Items", "D Items", "E Items", "F Items", "G Items", "C Items 2", "K ITEMS", "D Items 2", "A Items 2", "J ITEMS", "E Items 2", "G Items 2", "F Items 2", "M Items", "I ITEMS 6", "N Items ", "I ITEMS 7", "I ITEMS", "I ITEMS 2", "I ITEMS 5", "I ITEMS 3", "I ITEMS 4", "H Items"];
    private static readonly string[] ItemsWithAP = ["Shovel", "Magnifying Glasses", "Saltshakers", "Hammers", "Compasses", "Shoes", "Metal Detectors", "Sleeping Mask", "Upgrade Disk"];
    public static Dictionary<string, string> CommissaryStates = new Dictionary<string, string>(){
            {"MAGNIFYING GLASS", "Mag Glass" },
            {"SHOVEL", "Shovel Purchase"},
            {"SALT SHAKER", "Salt Shaker Purchase"},
            {"COMPASS", "Compass Purchase"},
            {"SLEDGE HAMMER", "Sledge Hammer Purchase"},
            {"SLEEPING MASK", "Sleep Mask Purchase"},
            {"RUNNING SHOES", "Running Shoes Purchase"},
            {"METAL DETECTOR", "MEtal Detector Purchase"}
         };
    public static List<string> CanStock = new();

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
        _ItemsForSaleGameObject = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR SALE")?.gameObject;
        _ItemsForSaleFsm = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR SALE")?.GetComponent<PlayMakerFSM>();
        _CommissaryMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Commissary Menu")?.gameObject;
        _CommissaryMenuFsm = _CommissaryMenuGameObject?.GetComponent<PlayMakerFSM>();
        _ColliderGameObject = RoomGameObject.transform.Find("_GAMEPLAY/Click Commissary Collider")?.gameObject;

        SetupItemsForSale();
    }
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        ReplaceModelsWithAP();
    }

    private void ReplaceModelsWithAP() {
        for (int i = 0; i < _ItemsForSaleGameObject.transform.childCount; i++)
        {
            Transform child = _ItemsForSaleGameObject.transform.GetChild(i);
            // Check if the item list is currently active.
            if (child.gameObject.active == true)
            {
 
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform type = child.GetChild(j);
                    // Check if this is an item with an AP replacement
                    if (ItemsWithAP.Contains(type.gameObject.name.Trim()))
                    {
                        for (int k = 0; k < type.childCount; k++)
                        {
                            GameObject model = type.GetChild(k).gameObject;
                            string itemName = model.name.Trim().Replace(" (1)", "").Replace(" (2)", "").Replace(" (3)", "").Replace(" (4)", "").ToUpper();
                            UniqueItem item = Plugin.ModItemManager.GetUniqueItem(itemName);
                            if (item != null)
                            {
                                // if the item has note been found yet.
                                if (!item.HasBeenFound)
                                {
                                    ReplaceWithAPModel(itemName, model);
                                }
                            }
                            else
                            {
                                Logging.Log($"Error Getting item {itemName}. No such Unique Item");
                            }
                        }
                    }
                }
            }
           
        }
    }
    public void EnableCommissaryPurchase(UniqueItem item, string stateName)
    {

        FsmState state = ModInstance.CommissaryMenu.GetState(stateName);
        if (state != null)
        {
            //If the item is not unlocked, prevent it from being added to inventory.
            if (!item.IsUnlocked && item.ApplySanity())
            {
                //Disable the actions that add the item to inventory.
                state.DisableActionsOfType<ArrayListAdd>();
                //Disables the You found Text (for now).
                state.DisableFirstActionOfType<ActivateGameObject>();
            }
            Plugin.UniqueItemManager.SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(item.Name));
            return;
        }
    }

    private void ReplaceWithAPModel(string itemName, GameObject gameObject) {
        GameObject prefab = ModInstance.Prefabs.GetChild(itemName);
        if (prefab != null)
            {
                //Instantiate a copy of the game object at the location of the spawn pool game object.
                GameObject APGO = GameObject.Instantiate(prefab, gameObject.transform.position, gameObject.transform.rotation);
                // Get the APswirly Component of the prefab
                GameObject APswirly = APGO.transform.GetChild(0)?.gameObject;
                if (APswirly != null)
                {
                    //Reparent the AP swirly to the spawn pool object.
                    APswirly.transform.parent = gameObject.transform;
                }
                else
                {
                    Logging.LogWarning($"Unable to find APSwirly for {itemName}.");
                }
                // Get rid of our temporarily instantiated game object.
                GameObject.Destroy(APGO);
            }
            else
            {
                Logging.LogWarning($"Unable to find prefab for {itemName}. Item is either unimplemented or not present in the assets.");
            }
    }

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
                        if (CommissaryStates.ContainsKey(setFsmString.setValue.Value) || setFsmString.setValue.Value.Contains("Upgrade Disk"))
                        {
                            var itemName = setFsmString.setValue.Value;

                            if (itemName.Contains("Upgrade Disk"))
                                itemName += " - Commissary";

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
