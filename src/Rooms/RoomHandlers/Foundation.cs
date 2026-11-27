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

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    class Foundation : RoomHandler
    {
        public static bool StartedDayInHouse { set; get; } = false;

        public override void OnAfterRoomDrafted(GameObject roomGameObject)
        {
            UpgradeDisks.ChangeItemDropFSM("THE FOUNDATION", "State 1", "skip", "Spawn Sanctum Key");
        }
    }
}
