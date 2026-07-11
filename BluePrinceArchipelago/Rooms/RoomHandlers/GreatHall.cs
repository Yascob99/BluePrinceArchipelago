using BluePrinceArchipelago.Items;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BluePrinceArchipelago.Utils;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class GreatHall : RoomHandler
{
    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        GameObject RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools");
        for (int i = 0; i < RoomSpawnPools.transform.childCount; i++)
        {
            Transform child = RoomSpawnPools.transform.GetChild(i);
            if (child.name.Contains("Great Hall"))
            {
                roomGameObject = child.gameObject;
                PlayMakerFSM ItemDropFSM = GetActiveCorner(roomGameObject);

                if (ItemDropFSM != null)
                {
                    bool found = !ModItemManager.UpgradeDisks.FoundLocations.Contains("HER LADYSHIPS CHAMBER");
                    Logging.LogWarning(found);
                    FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                    CanSpawnDisk.Value = found;
                    ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                }
                else
                {
                    Logging.LogWarning("Error changing Great Hall Upgrade disk spawn logic.");
                }
            }
        }       
    }
    // Attempts to find the room where the Upgrade Disk will be.
    private PlayMakerFSM GetActiveCorner(GameObject roomObject) {
        
        foreach (Transform transform in roomObject.transform.FindAllRecursive("8")) {
            if (transform?.parent?.parent?.gameObject?.active ?? false) {
                return transform.gameObject.GetComponent<PlayMakerFSM>();
            }
        }
        return null;
    }
}

