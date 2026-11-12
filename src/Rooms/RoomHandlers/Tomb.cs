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
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Tomb : RoomHandler
{
    public Tomb()
    {
        AllowanceTokens.Add("Tomb");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Tomb");
    }
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            roomGameObject = ModRoomManager.GetRoomInstance("Tomb");
            if (roomGameObject != null)
            {
                PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_CULLABLE/_GAMEPLAY/Sliding Wall A Anchor/Gold Pay Off/3")?.GetComponent<PlayMakerFSM>();
                if (ItemDropFSM != null)
                {
                    bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("TOMB");
                    Logging.LogWarning(found);
                    FsmBool CanSpawnDisk = ItemDropFSM.GetBoolVariable("CanSpawnDisk");
                    CanSpawnDisk.Value = found;
                    ItemDropFSM.GetState("State 3").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                    ItemDropFSM.GetState("State 3").ChangeTransition("skip", "Spawn Sanctum Key");
                    //ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                    //CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
                    //FsmState CoatCheckCheck = ItemDropFSM.GetState("Coat Check Check");
                    //CoatCheckCheck.ChangeTransition("cont", "Spawn Sanctum Key");
                    //if (!found)
                    //{
                    //    IntCompare KeyIntCheck = CoatCheckCheck.GetFirstActionOfType<IntCompare>();
                    //    KeyIntCheck.equal = KeyIntCheck.lessThan;
                    //}
                }
                else
                {
                    Logging.LogWarning("Error changing Tomb Upgrade disk spawn logic.");
                }
            }
        }
    }
}