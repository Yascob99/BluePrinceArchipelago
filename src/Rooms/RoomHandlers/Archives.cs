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
        UpgradeDisks.ChangeItemDropFSM("ARCHIVES", "State", "skip", "Spawn Sanctum Key");
    }
}