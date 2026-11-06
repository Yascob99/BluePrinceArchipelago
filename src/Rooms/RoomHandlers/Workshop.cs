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

    private readonly Dictionary<string, string> contraptionStateNames = new()
        {
            {"Electromagnet", "Electo Magnet"},
            {"Dowsing Rod", "Dowsing Rod"},
            {"Detector Shovel", "DETECTOR SHOVEL"},
            {"Burning Glass", "Burning Glass"},
            {"Pick Sound Amplifier", "Pick Sound Amplifier"},
            {"Lucky Purse", "Lucky Purse"},
            {"Power Hammer", "Power Hammer"},
            {"Jack Hammer", "Jack Hammer"}
        };

    private static int currentInstance = 0;
    
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        ++currentInstance;
        if (!ArchipelagoOptions.WorkshopSanity)
        {
            return;
        }
        GameObject workshopMenu = GameObject.Find($"__SYSTEM/Room Spawn Pools/Workshop(Clone)00{currentInstance}/_GAMEPLAY/WorkShop Table");

        PlayMakerFSM workshopFSM = workshopMenu.transform.Find("ICONS TEXT").GetComponent<PlayMakerFSM>();
        // TODO : Remove
        workshopMenu.transform.Find("ICONS TEXT/TITLE TEXT/Workshop TExt").GetComponent<TextMeshPro>().text = "Archipelago";
        HookItemIconsOnCraft(workshopFSM);
    }

    private void HookItemIconsOnCraft(PlayMakerFSM workshopFSM)
    {
        foreach (KeyValuePair<string, string> contraptionStateName in contraptionStateNames)
        {
            HookItemIconOnCraft(workshopFSM, contraptionStateName.Key, contraptionStateName.Value);
            UniqueItem contraption = ModItemManager.GetUniqueItem(contraptionStateName.Key);
            //ModItemManager.AddSwirly(contraption);
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
