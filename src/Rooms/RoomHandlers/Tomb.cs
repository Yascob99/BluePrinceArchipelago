using BluePrinceArchipelago.Archipelago;
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
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            UpgradeDisks.ChangeItemDropFSM("TOMB", "State 3", "skip", "Spawn Sanctum Key");
        }
    }
}