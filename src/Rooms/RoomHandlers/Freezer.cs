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

class Freezer : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        roomGameObject = ModRoomManager.GetRoomInstance("Freezer");
        if (roomGameObject != null)
        {
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/ICE DOORS/Door 20/6")?.GetComponent<PlayMakerFSM>();
            if (ItemDropFSM != null)
            {
                bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("FREEZER");
                Logging.LogWarning(found);
                FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                CanSpawnDisk.Value = found;
                ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
            }
            else
            {
                Logging.LogWarning("Error changing Freezer Upgrade disk spawn logic.");
            }
        }
    }
}

