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
            //PlayMakerFSM FoundationSpawn = GameObject.Find("UNDERGROUND").transform.Find("Below Foundation (Cullable)").Find("Below Foundation - Prefab").Find("_GAMEPLAY").Find("5")?.GetComponent<PlayMakerFSM>();
            //if (FoundationSpawn != null)
            //{
            //    bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("FOUNDATION");
            //    FsmBool CanSpawnDisk = FoundationSpawn.AddBoolVariable("CanSpawnDisk");
            //    CanSpawnDisk.Value = found;
            //    FoundationSpawn.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
            //    ArrayListContains CheckInInventory = FoundationSpawn.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
            //    CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
            //}
            //else
            //{
            //    Logging.LogWarning("Error changing Foundation Upgrade disk spawn logic.");
            //}
        }
    }
}
