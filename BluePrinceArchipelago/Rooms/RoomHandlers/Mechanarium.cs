using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Mechanarium : RoomHandler
{
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/11")?.GetComponent<PlayMakerFSM>();
        if (ItemDropFSM != null)
        {
            bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("MECHANARIUM");
            Logging.LogWarning(found);
            FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
            CanSpawnDisk.Value = found;
            ItemDropFSM.GetState("State 7").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
            ItemDropFSM.GetState("State 5").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        }
        else
        {
            Logging.LogWarning("Error changing Mechanarium Upgrade disk spawn logic.");
        }
    }
}

