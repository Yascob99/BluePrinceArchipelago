using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppTMPro;
#endif
using System;
using System.Collections.Generic;
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

    private bool SetupItemForSalesInstances()
    {
        RoomGameObject = ModRoomManager.GetRoomInstance("Commissary");
        _ItemsForSaleGameObject = RoomGameObject.transform.Find("_GAMEPLAY/ITEMS FOR SALE")?.gameObject;
        if (_ItemsForSaleGameObject == null)
        {
            Logging.LogError("_ItemsForSaleGameObject is null", COMMISSARY_LOG_TAG);
            return false;
        }
        _ItemsForSaleFsm = _ItemsForSaleGameObject.GetComponent<PlayMakerFSM>();
        if (_ItemsForSaleFsm == null)
        {
            Logging.LogError("_ItemsForSaleFsm is null", COMMISSARY_LOG_TAG);
            return false;
        }
        return true;
    }

    public override void OnItemCodeComplete()
    {
        bool flowControl = SetupItemForSalesInstances();
        if (!flowControl)
        {
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
        Transform itemPoolTransform = GetItemPoolTranform();
        if (itemPoolTransform == null)
        {
            Logging.LogError($"Error, could not find object corresponding to the item pool", COMMISSARY_LOG_TAG);
            return;
        }
        for (int i = 0; i < itemPoolTransform.childCount; ++i)
        {
            Transform itemType = itemPoolTransform.GetChild(i);
            if (!HasApItemReplacement(itemType))
            {
                continue;
            }
            for (int j = 0; j < itemType.childCount; j++)
            {
                GameObject model = itemType.GetChild(j).gameObject;
                string itemName = GetItemNameFromCommissaryModelName(model.name);
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

    private static string GetItemNameFromCommissaryModelName(string modelName)
    {
        string itemName = modelName.Trim();
        for (int i = 1; i <= 5; ++i)
        {
            itemName = itemName.Replace($" ({i})", "");
        }
        return itemName.ToUpper();
    }

    private static bool HasApItemReplacement(Transform itemType)
    {
        return ItemsWithAP.Contains(itemType.gameObject.name.Trim());
    }

    private Transform GetItemPoolTranform()
    {
        char poolLetter = _ItemsForSaleFsm.ActiveStateName[0];
        string poolName = "Items " + poolLetter;
        Logging.LogWarning($"Pool {_ItemsForSaleFsm.ActiveStateName} chosen, Items in pool : {poolName} registered", "DEBUG");
        Transform itemPoolTransform = _ItemsForSaleGameObject.transform.Find(poolName);
        if (itemPoolTransform == null)
        {
            Logging.LogError($"Error, could not find object corresponding to the item pool : '{poolName}'", COMMISSARY_LOG_TAG);
        }
        return itemPoolTransform;
    }

    private void SetupItemsForSale()
    {
        FsmState state = _ItemsForSaleFsm.GetState(_ItemsForSaleFsm.ActiveStateName);
        SetFsmString[] actions = state.GetActionsOfType<SetFsmString>();
        foreach (SetFsmString action in actions)
        {
            if (action is not SetFsmString setFsmString)
            {
                Logging.LogError($"'{action.name}' action is not SetFsmString", COMMISSARY_LOG_TAG);
                continue;
            }
            FsmString variableName = setFsmString.variableName;
            var itemName = setFsmString.setValue.Value;
            if (!VariableNameDesignatesAnItem(variableName.Value) || !IsCommissaryUniqueItem(itemName))
            {
                continue;
            }
            UniqueItem item = ModItemManager.GetUniqueItem(itemName);
            if (item == null)
            {
                Logging.LogError($"Error Getting item {itemName}. No such Unique Item", COMMISSARY_LOG_TAG);
            }
            if (IsCommissaryUpgradeDisk(itemName))
            {
                itemName += " - Commissary";
            }
            new ShopItem {Name = itemName}.GetScoutHint();
        }
    }

    private static bool RequiresShopTitleUpdate(UniqueItem item, string itemName)
    {
        return (!item.HasBeenFound) || (IsCommissaryUpgradeDisk(itemName) && !CommissaryUpgradeDiskHasBeenFound());
    }

    private static bool CommissaryUpgradeDiskHasBeenFound()
    {
        return ModItemManager.UpgradeDisks.FoundLocations.Contains("COMMISSARY");
    }

    private static bool IsCommissaryUpgradeDisk(string itemName)
    {
        return itemName.Contains("Upgrade Disk");
    }

    private static bool IsCommissaryUniqueItem(string name)
    {
        return CommissaryStates.ContainsKey(name) || name.Contains("Upgrade Disk");
    }

    private static bool VariableNameDesignatesAnItem(string name)
    {
        return name.StartsWith("ITEM") && name.EndsWith("NAME");
    }
}
