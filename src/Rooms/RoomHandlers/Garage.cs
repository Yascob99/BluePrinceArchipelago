using BluePrinceArchipelago.FsmMethods;
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

class Garage : RoomHandler
{
    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        roomGameObject = ModRoomManager.GetRoomInstance("Garage");
        //Yes, one of these has a (1) at the end and the other does not. Tonda my GOAT. 
        PlayMakerFSM GaragePierceFSM = roomGameObject.transform.Find("_NONSTATIC/PIERCE/Garage Door Button (1)/Button").GetComponent<PlayMakerFSM>();
        PlayMakerFSM GarageCreepFSM = roomGameObject.transform.Find("_NONSTATIC/CREEP/Garage Door Button/Button").GetComponent<PlayMakerFSM>();
        GarageCreepFSM.GetState("Button Press")?.AddFirstAction(CustomFsmMethodManager.GetCallMethod("GarageOpened"));
        GaragePierceFSM.GetState("Button Press")?.AddFirstAction(CustomFsmMethodManager.GetCallMethod("GarageOpened"));

        if (roomGameObject != null)
        {
            PlayMakerFSM ItemDropFSM = roomGameObject.transform.Find("_GAMEPLAY/TrunkSpawn/1 Spawn/7")?.GetComponent<PlayMakerFSM>();
            if (ItemDropFSM != null)
            {
                bool found = ModItemManager.UpgradeDisks.FoundLocations.Contains("GARAGE");
                FsmBool CanSpawnDisk = ItemDropFSM.AddBoolVariable("CanSpawnDisk");
                CanSpawnDisk.Value = found;
                ItemDropFSM.GetState("State 1").GetFirstActionOfType<BoolTest>().boolVariable = CanSpawnDisk;
                ArrayListContains CheckInInventory = ItemDropFSM.GetState("State 2").GetFirstActionOfType<ArrayListContains>();
                CheckInInventory.isContainedEvent = CheckInInventory.isNotContainedEvent;
            }
            else
            {
                Logging.LogWarning("Error changing Garage Upgrade disk spawn logic.");
            }
        }
    }
}

