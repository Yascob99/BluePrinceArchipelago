using BluePrinceArchipelago.Items;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class LostAndFound : RoomHandler
{
    public LostAndFound()
    {
        AllowanceTokens.Add("Lost & Found");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Lost & Found");
    }
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        GameObject RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools");
        roomGameObject = ModRoomManager.GetRoomInstance("Lost & Found");
        if (roomGameObject != null)
        {
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/9")?.gameObject?.GetFsm("Go Items Random");

            if (ItemDropFSM != null)
            {
                bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("LOST & FOUND");
                Logging.LogWarning(found);
                FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                CanSpawnDisk.Value = found;
                ItemDropFSM.GetState("State 4").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                if (CheckInInventory != null)
                {
                    BoolTest CheckFound = new BoolTest() { boolVariable = CanSpawnDisk, isTrue = CheckInInventory.isContainedEvent, isFalse = CheckInInventory.isNotContainedEvent, everyFrame = false };
                    ItemDropFSM.GetState("State 2").ReplaceAction(CheckFound, 4);
                    return;
                }
                Logging.LogWarning("Error changing Lost and Found Upgrade disk spawn logic.");
                return;
            }
            Logging.LogWarning("Error changing Lost and Found Upgrade disk spawn logic. Couldn't get Item Drop FSM.S");
            return;
        }
        Logging.LogWarning("Error changing Lost and Found Upgrade disk spawn logic. Couldn't Find Room Instance.");
    }
}