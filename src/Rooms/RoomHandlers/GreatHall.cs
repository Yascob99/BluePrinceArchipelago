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
                List<GameObject> itemDropGOs = GetItemDropObjects(roomGameObject);
                foreach (GameObject itemDropGO in itemDropGOs)
                {

                    if (itemDropGO != null)
                    {
                        UpgradeDisks.ChangeItemDropFSM("GREAT HALL", itemDropGO, "State 3", "skip", "Spawn Sanctum Key");
                    }
                    else
                    {
                        Logging.LogWarning("Error changing Great Hall Upgrade disk spawn logic.");
                    }
                }
            }

        }
    }
    private List<GameObject> GetItemDropObjects(GameObject roomGameObject)
    {
        List<GameObject> ItemDropGOs = new List<GameObject>();
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
                        GameObject ItemDropGO = Subwall.gameObject.FindChildGameObjectsRecursive("8")[0].gameObject;
                        if (ItemDropGO != null)
                        {
                            ItemDropGOs.Add(ItemDropGO);
                        }
                    }
                }
            }
        }
        return ItemDropGOs;
    }
}


