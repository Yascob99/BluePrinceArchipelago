using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class MorningRoom : RoomHandler
{
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/_Pickup Items/12")?.GetComponent<PlayMakerFSM>();
        if (ItemDropFSM != null)
        {
            bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("MORNING ROOM");
            Logging.LogWarning(found);
            FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
            CanSpawnDisk.Value = found;
            ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        }
        else
        {
            Logging.LogWarning("Error changing Morning Room Upgrade disk spawn logic.");
        }
    }
}

