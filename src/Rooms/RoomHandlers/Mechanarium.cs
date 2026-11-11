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

class Mechanarium : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            roomGameObject = ModRoomManager.GetRoomInstance("Mechanarium");
            if (roomGameObject != null)
            {
                PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/11")?.GetComponent<PlayMakerFSM>();
                if (ItemDropFSM != null)
                {
                    bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("MECHANARIUM");
                    Logging.LogWarning(found);
                    FsmBool CanSpawnDisk = ItemDropFSM.GetBoolVariable("CanSpawnDisk");
                    CanSpawnDisk.Value = !found;
                    ItemDropFSM.GetState("Upgrae Disk").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                    FsmState CoatCheckCheck = ItemDropFSM.GetState("Coat Check Check 2");
                    CoatCheckCheck.ChangeTransition("cont", "Spawn Sanctum Key 2");
                    if (!found)
                    {
                        IntCompare KeyIntCheck = CoatCheckCheck.GetFirstActionOfType<IntCompare>();
                        KeyIntCheck.equal = KeyIntCheck.lessThan;
                        ItemDropFSM.GetState("State 6").DisableFirstActionOfType<ArrayListContains>();
                        ItemDropFSM.GetState("State 6").ChangeTransition("FINISHED", "Coat Check Check 2");
                    }
                }
                else
                {
                    Logging.LogWarning("Error changing Mechanarium Upgrade disk spawn logic.");
                }
            }
        }
    }
}

