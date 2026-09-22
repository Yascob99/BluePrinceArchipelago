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

class Vault : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        roomGameObject = ModRoomManager.GetRoomInstance("Vault");
        if (roomGameObject != null)
        {
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/Lockers/Locker 370/370 spwn/6")?.GetComponent<PlayMakerFSM>();
            if (ItemDropFSM != null)
            {
                bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("VAULT");
                Logging.LogWarning(found);
                FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                CanSpawnDisk.Value = found;
                ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
            }
            else
            {
                Logging.LogWarning("Error changing Vault Upgrade disk spawn logic.");
            }
        }
    }
}
