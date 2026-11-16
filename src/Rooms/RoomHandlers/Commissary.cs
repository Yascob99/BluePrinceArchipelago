using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Commissary : RoomHandler
{
    private const string COMMISSARY_LOG_TAG = "Commissary";

    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];
    
    private GameObject _ItemsForSaleGameObject;
    private PlayMakerFSM _ItemsForSaleFsm;
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
        Logging.Log("Initializing Commissary.", COMMISSARY_LOG_TAG);
    }

    public override void OnItemCodeComplete()
    {
        RoomGameObject = ModRoomManager.GetRoomInstance("Commissary");
        _ItemsForSaleGameObject = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR SALE")?.gameObject;
        if (_ItemsForSaleGameObject == null)
        {
            Logging.LogError("_ItemsForSaleGameObject is null", COMMISSARY_LOG_TAG);
            return;
        }
        _ItemsForSaleFsm = _ItemsForSaleGameObject.GetComponent<PlayMakerFSM>();
        if (_ItemsForSaleFsm == null)
        {
            Logging.LogError("_ItemsForSaleFsm is null", COMMISSARY_LOG_TAG);
            return;
        }
        SetupItemsForSale();
        CommissaryDiskAdjustment();
        ReplaceModelsWithAP();
    }

    private void CommissaryDiskAdjustment()
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            // Changes makes it check if the location has been found instead of a different check.
            FsmBool CanSpawnDisk = _ItemsForSaleFsm.GetBoolVariable("CanSpawnDisk");
            CanSpawnDisk.Value = ModItemManager.UpgradeDisks.FoundLocations.Contains("COMMISSARY");
            _ItemsForSaleFsm.GetState("State 5").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        }
    }

    private void ReplaceModelsWithAP() {
        char poolLetter = _ItemsForSaleFsm.ActiveStateName[0];
        string poolName = "Items " + poolLetter;
        Logging.LogWarning($"Pool {_ItemsForSaleFsm.ActiveStateName} chosen, Items in pool : {poolName} registered", "DEBUG");

        Transform itemPoolObject = _ItemsForSaleGameObject.transform.Find(poolName);
        if (itemPoolObject == null)
        {
            Logging.LogError($"Error, could not find object corresponding to {poolName}", COMMISSARY_LOG_TAG);
            return;
        }
        for (int i = 0; i < itemPoolObject.childCount; ++i)
        {
            Transform itemType = itemPoolObject.GetChild(i);
            // Check if this is an item with an AP replacement
            if (!ItemsWithAP.Contains(itemType.gameObject.name.Trim()))
            {
                continue;
            }
            for (int k = 0; k < itemType.childCount; k++)
            {
                GameObject model = itemType.GetChild(k).gameObject;
                string itemName = model.name.Trim().Replace(" (1)", "").Replace(" (2)", "").Replace(" (3)", "").Replace(" (4)", "").ToUpper();
                UniqueItem item = ModItemManager.GetUniqueItem(itemName);
                if (item == null)
                {
                    Logging.LogError($"Error Getting item {itemName}. No such Unique Item", COMMISSARY_LOG_TAG);
                    continue;
                }
                if (!item.HasBeenFound)
                {
                    ModItemManager.AddSwirly(item, model);
                }
            }
        }
    }

    private void SetupItemsForSale()
    {
        FsmState state = _ItemsForSaleFsm.GetState(_ItemsForSaleFsm.ActiveStateName);
        var actions = state.GetActionsOfType<SetFsmString>();
        foreach (var action in actions)
        {
            if (action is not SetFsmString setFsmString)
            {
                Logging.LogError($"checked action is not SetFsmString, this should not happen...", COMMISSARY_LOG_TAG);
                continue;
            }
            var varName = setFsmString.variableName;
            if (!varName.Value.StartsWith("ITEM") || !varName.Value.EndsWith("NAME"))
            {
                continue;
            }
            if (!CommissaryStates.ContainsKey(setFsmString.setValue.Value) && !setFsmString.setValue.Value.Contains("Upgrade Disk"))
            {
                continue;
            }
            var itemName = setFsmString.setValue.Value;
            Logging.LogWarning($"Item found: '{itemName}' in Items For Sale FSM.", "DEBUG");
            UniqueItem item = ModItemManager.GetUniqueItem(itemName);
            if (item == null)
            {
                Logging.LogError($"Item: '{itemName}' not found in unique items.", "DEBUG");
            }
            if (itemName.Contains("Upgrade Disk"))
            {
                itemName += " - Commissary";
            }
            if (!LocationMap.ContainsKey(itemName))
            {
                LocationMap.Add(itemName, new Models.ShopItem
                {
                    Name = itemName
                });
                Logging.LogWarning($"Item: '{itemName}' addded to LocationMap.", "DEBUG");
            }
            string apItem = LocationMap[itemName].GetScoutHint();
            if (!item.HasBeenFound)
            {
                setFsmString.setValue.Value = apItem;
            }
            if (itemName.Contains("Upgrade Disk") && !ModItemManager.UpgradeDisks.FoundLocations.Contains("COMMISSARY"))
            {
                setFsmString.setValue.Value = apItem;
            }
        }
    }
    
}
