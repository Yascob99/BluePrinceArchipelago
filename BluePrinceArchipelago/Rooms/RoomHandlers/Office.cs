using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Office : RoomHandler
{
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/_Pickup Items/13")?.GetComponent<PlayMakerFSM>();
        if (ItemDropFSM != null)
        {
            bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("OFFICE");
            Logging.LogWarning(found);
            FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
            CanSpawnDisk.Value = found;
            ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        }
        else
        {
            Logging.LogWarning("Error changing Office Upgrade disk spawn logic.");
        }
    }
}

