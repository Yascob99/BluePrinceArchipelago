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
    
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (!ArchipelagoOptions.WorkshopSanity)
        {
            return;
        }
        // TODO : Make sure to handle multiple instances of workshop before releasing
        GameObject workshopMenu = GameObject.Find("__SYSTEM/Room Spawn Pools/Workshop(Clone)001/_GAMEPLAY/WorkShop Table");
        
        PlayMakerFSM workshopFSM = workshopMenu.transform.Find("ICONS TEXT").GetComponent<PlayMakerFSM>();
        workshopMenu.transform.Find("ICONS TEXT/TITLE TEXT/Workshop TExt").GetComponent<TextMeshPro>().text = "Archipelago";

        foreach (KeyValuePair<string, string> contraptionStateName in contraptionStateNames)
        {
            UniqueItem contraption = ModItemManager.GetUniqueItem(contraptionStateName.Key);
            CustomFsmMethodManager.AddItemOnCraftMethod(contraption);
            string customMethodName = CustomFsmMethodManager.GetItemOnCraftMethodName(contraption.Name);
            FsmState state = workshopFSM.GetState(contraptionStateName.Value);
            state.AddAction(CustomFsmMethodManager.GetCallMethod(customMethodName));
        }
    }
}
