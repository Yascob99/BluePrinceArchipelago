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

class Archives: RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        roomGameObject = ModRoomManager.GetRoomInstance("Archives");
        if (roomGameObject != null)
        {
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/Hero Filing Cabinet/Draw 04/Draw 04/1")?.GetComponent<PlayMakerFSM>();
            if (ItemDropFSM != null)
            {
                bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("ARCHIVES");
                Logging.LogWarning(found);
                FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                CanSpawnDisk.Value = found;
                ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
            }
            else
            {
                Logging.LogWarning("Error changing Archives Upgrade disk spawn logic.");
            }
        }
    }
}