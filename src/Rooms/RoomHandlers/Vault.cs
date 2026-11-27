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

class Vault : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                UpgradeDisks.ChangeItemDropFSM("VAULT", "State 3", "skip", "Spawn Sanctum Key 2");
            }
        }
    }
}
