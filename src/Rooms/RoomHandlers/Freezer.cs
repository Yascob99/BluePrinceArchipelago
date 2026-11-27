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

class Freezer : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            UpgradeDisks.ChangeItemDropFSM("FREEZER", "State 3", "skip", "Spawn Sanctum Key");
        }
    }
}

