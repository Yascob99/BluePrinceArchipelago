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
using BluePrinceArchipelago.Archipelago;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Vault : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                roomGameObject = ModRoomManager.GetRoomInstance("Vault");
                if (roomGameObject != null)
                {
                    PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/Lockers/Locker 304/304 spwn/15")?.GetComponent<PlayMakerFSM>();
                    if (ItemDropFSM != null)
                    {
                        bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("VAULT");
                        Logging.LogWarning(found);
                        FsmBool CanSpawnDisk = ItemDropFSM.GetBoolVariable("CanSpawnDisk");
                        CanSpawnDisk.Value = found;
                        ItemDropFSM.GetState("State 3").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                        ItemDropFSM.GetState("State 3").ChangeTransition("skip", "Spawn Sanctum Key 2");
                        //ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                        //CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
                        //FsmState CoatCheckCheck = ItemDropFSM.GetState("Coat Check Check 2");
                        //CoatCheckCheck.ChangeTransition("cont", "Spawn Sanctum Key 2");
                        //if (!found)
                        //{
                        //    IntCompare KeyIntCheck = CoatCheckCheck.GetFirstActionOfType<IntCompare>();
                        //    KeyIntCheck.equal = KeyIntCheck.lessThan;
                        //}
                    }
                    else
                    {
                        Logging.LogWarning("Error changing Vault Upgrade disk spawn logic.");
                    }
                }
            }
        }
    }
}
