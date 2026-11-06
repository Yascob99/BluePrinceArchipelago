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

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Workshop : RoomHandler
{
    private readonly string[] stateNames = ["Shovel Check", "State 6", "State 2", "Shovel", "Item 1", "No Combombs"];
    
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (!ArchipelagoOptions.WorkshopSanity)
        {
            return;
        }
        Logging.LogWarning("Successfully hooked Workshop", "DEBUG");
        //item.HasBeenFound && !item.IsUnlock
        // TODO : Make sure to handle multiple instances of workshop before releasing
        GameObject workshopMenu = GameObject.Find("__SYSTEM/Room Spawn Pools/Workshop(Clone)001/_GAMEPLAY/WorkShop Table");
        
        PlayMakerFSM workshopFSM = workshopMenu.transform.Find("ICONS TEXT").GetComponent<PlayMakerFSM>();
        workshopMenu.transform.Find("ICONS TEXT/TITLE TEXT/Workshop TExt").GetComponent<TextMeshPro>().text = "Archipelago";

        FsmState detectorShovel = workshopFSM.GetState("DETECTOR SHOVEL");
        detectorShovel.AddAction(CustomFsmMethodManager.GetCallMethod("LockContraption"));

        FsmState BurningGlass = workshopFSM.GetState("Burning Glass");
        BurningGlass.AddAction(CustomFsmMethodManager.GetCallMethod("UnlockContraption"));
    }
}
