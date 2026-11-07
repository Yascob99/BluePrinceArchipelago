using BluePrinceArchipelago.Items;
#if Bep
using HutongGames.PlayMaker;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppTMPro;
#endif
using BluePrinceArchipelago.Utils;
using UnityEngine;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Archipelago;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Workshop : RoomHandler
{
    private const string ITEM_ELECTROMAGNET_NAME = "Electromagnet";
    private const string ITEM_DOWSING_ROD_NAME = "Dowsing Rod";
    private const string ITEM_DETECTOR_SHOVEL_NAME = "Detector Shovel";
    private const string ITEM_BURNING_GLASS_NAME = "Burning Glass";
    private const string ITEM_PICK_SOUND_AMPLIFIER_NAME = "Pick Sound Amplifier";
    private const string ITEM_LUCKY_PURSE_NAME = "Lucky Purse";
    private const string ITEM_POWER_HAMMER_NAME = "Power Hammer";
    private const string ITEM_JACK_HAMMER_NAME = "Jack Hammer";
    private readonly Dictionary<string, string> contraptionStateNames = new()
        {
            {ITEM_ELECTROMAGNET_NAME, "Electo Magnet"},
            {ITEM_DOWSING_ROD_NAME, "Dowsing Rod"},
            {ITEM_DETECTOR_SHOVEL_NAME, "DETECTOR SHOVEL"},
            {ITEM_BURNING_GLASS_NAME, "Burning Glass"},
            {ITEM_PICK_SOUND_AMPLIFIER_NAME, "Pick Sound Amplifier"},
            {ITEM_LUCKY_PURSE_NAME, "Lucky Purse"},
            {ITEM_POWER_HAMMER_NAME, "Power Hammer"},
            {ITEM_JACK_HAMMER_NAME, "Jack Hammer"}
        };
    private readonly Dictionary<string, string> contraptionObjectPaths = new()
        {
            {ITEM_ELECTROMAGNET_NAME, "ElectroMagnet Home/Powerd Electromagnet"},
            {ITEM_DOWSING_ROD_NAME, "Dowsing Rod Home/DOWSING ROD"},
            {ITEM_DETECTOR_SHOVEL_NAME, "Detector Shovel Home/DETECTOR SHOVEL"},
            {ITEM_BURNING_GLASS_NAME, "Burning Glass Home/BURNING GLASS"},
            {ITEM_PICK_SOUND_AMPLIFIER_NAME, "Pick Sound Amplifier Home/PICK SOUND AMPLIFIED"},
            {ITEM_LUCKY_PURSE_NAME, "Lucky Purse Home/LUCKY PURSE"},
            {ITEM_POWER_HAMMER_NAME, "Power Hammer Home/POWER HAMMER"},
            {ITEM_JACK_HAMMER_NAME, "Jack Hammer Home/JACK HAMMER"}
        };
    private const string WORKSHOP_FSM_SUBPATH = "ICONS TEXT";

    private static int currentInstance = 0;
    
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        ++currentInstance;
        if (!ArchipelagoOptions.WorkshopSanity)
        {
            return;
        }
        string workshopTablePath = $"__SYSTEM/Room Spawn Pools/Workshop(Clone)00{currentInstance}/_GAMEPLAY/WorkShop Table";
        GameObject workshopTable = GameObject.Find(workshopTablePath);
        PlayMakerFSM workshopFSM = workshopTable.transform.Find(WORKSHOP_FSM_SUBPATH).GetComponent<PlayMakerFSM>();
        HookItemIconsOnCraft(workshopFSM);
        AddSwirlies(workshopTable);
    }

    private void AddSwirlies(GameObject workshopTable)
    {
        foreach (KeyValuePair<string, string> contraptionObjectPath in contraptionObjectPaths)
        {
            AddSwirly(workshopTable, contraptionObjectPath.Key, contraptionObjectPath.Value);
        }
    }

    private static void AddSwirly(GameObject workshopTable, string contraptionName, string contraptionObjectPath)
    {
        UniqueItem contraption = ModItemManager.GetUniqueItem(contraptionName);
        // Assuming an item can not be crafted twice the same day (in 2 workshops)
        if (contraption.HasBeenFound)
        {
            return;
        }
        GameObject contraptionObject = workshopTable.transform.Find(contraptionObjectPath).gameObject;
        if (contraptionObject == null)
        {
            Logging.LogWarning($"Failed to reach object for {contraptionName}", "DEBUG");
            return;
        }
        ModItemManager.AddSwirly(contraption, contraptionObject);
    }


    private void HookItemIconsOnCraft(PlayMakerFSM workshopFSM)
    {
        foreach (KeyValuePair<string, string> contraptionStateName in contraptionStateNames)
        {
            HookItemIconOnCraft(workshopFSM, contraptionStateName.Key, contraptionStateName.Value);
        }
    }

    private static void HookItemIconOnCraft(PlayMakerFSM workshopFSM, string itemName, string stateName)
    {
        UniqueItem contraption = ModItemManager.GetUniqueItem(itemName);
        CustomFsmMethodManager.AddItemOnCraftMethod(contraption, currentInstance);
        string customMethodName = CustomFsmMethodManager.GetItemOnCraftMethodName(contraption.Name, currentInstance);
        FsmState state = workshopFSM.GetState(stateName);
        state.AddAction(CustomFsmMethodManager.GetCallMethod(customMethodName));
    }
}
