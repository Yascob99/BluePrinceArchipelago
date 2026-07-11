using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/9")?.gameObject?.GetComponent<PlayMakerFSM>();
        if (ItemDropFSM != null)
        {
            bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("LOST AND FOUND");
            Logging.LogWarning(found);
            FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
            CanSpawnDisk.Value = found;
            ItemDropFSM.GetState("State 4").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        }
        else {
            Logging.LogWarning("Error changing Lost and Found Upgrade disk spawn logic.");
        }
    }
}