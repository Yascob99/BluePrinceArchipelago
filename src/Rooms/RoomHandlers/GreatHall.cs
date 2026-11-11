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
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class GreatHall : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        if (ArchipelagoOptions.UpgradeDiskSanity)
        {
            roomGameObject = ModRoomManager.GetRoomInstance("Great Hall");
            if (roomGameObject != null)
            {
                List<PlayMakerFSM> ItemDropFSMs = GetItemDropFSMs(roomGameObject);
                foreach (PlayMakerFSM ItemDropFSM in ItemDropFSMs)
                {

                    if (ItemDropFSM != null)
                    {
                        bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("GREAT HALL");
                        FsmBool CanSpawnDisk = ItemDropFSM.GetBoolVariable("CanSpawnDisk");
                        CanSpawnDisk.Value = found;
                        ItemDropFSM.GetState("State 3").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                        FsmState CoatCheckCheck = ItemDropFSM.GetState("Coat Check Check");
                        CoatCheckCheck.ChangeTransition("cont", "Spawn Sanctum Key");
                        if (!found)
                        {
                            IntCompare KeyIntCheck = CoatCheckCheck.GetFirstActionOfType<IntCompare>();
                            KeyIntCheck.equal = KeyIntCheck.lessThan;
                            ItemDropFSM.GetState("State 2").DisableFirstActionOfType<ArrayListContains>();
                            ItemDropFSM.GetState("State 2").ChangeTransition("FINISHED", "Coat Check Check");
                        }
                    }
                    else
                    {
                        Logging.LogWarning("Error changing Great Hall Upgrade disk spawn logic.");
                    }
                }
            }

        }
    }
    private List<PlayMakerFSM> GetItemDropFSMs(GameObject roomGameObject)
    {
        List<PlayMakerFSM> ItemDropFSMs = new List<PlayMakerFSM>();
        Transform SubWalls = roomGameObject.FindDescendant("_GAMEPLAY/SubWalls").transform;
        if (SubWalls != null)
        {
            for (int i = 0; i < SubWalls.childCount; i++)
            {
                Transform Side = SubWalls.GetChild(i);
                if (Side != null)
                {
                    for (int j = 0; j < Side.childCount; j++)
                    {
                        Transform Subwall = Side.GetChild(j);
                        PlayMakerFSM ItemDropFSM = Subwall.gameObject.FindChildGameObjectsRecursive("8")[0].GetComponent<PlayMakerFSM>();
                        if (ItemDropFSM != null)
                        {
                            ItemDropFSMs.Add(ItemDropFSM);
                        }
                    }
                }
            }
        }
        return ItemDropFSMs;
    }
}


