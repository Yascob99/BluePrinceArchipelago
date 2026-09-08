using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        //PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_CULLABLE/_GAMEPLAY/Sliding Wall A Anchor/Gold Pay Off/3")?.GetComponent<PlayMakerFSM>();
        //if (ItemDropFSM != null)
        //{
        //    bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("TOMB");
        //    Logging.LogWarning(found);
        //    FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
        //    CanSpawnDisk.Value = found;
        //    ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
        //}
        //else
        //{
        //    Logging.LogWarning("Error changing Tomb Upgrade disk spawn logic.");
        //}
    }
}