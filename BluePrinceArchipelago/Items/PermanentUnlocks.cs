using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    public static class Unlocks
    {
        public static AppleOrchard AppleOrchard = new();
        public static GemstoneCaverns GemstoneCaverns = new();
        public static WestGatePath WestGatePath = new();
        public static BlackBridgeGrotto BlackBridgeGrotto = new();
        public static SatelliteDish SatelliteDish = new();
        public static Dictionary<string, PermanentUnlock> UnlockedDict = new(){
            {"Apple Orchard", AppleOrchard},
            {"Gemstone Caverns", GemstoneCaverns},
            {"West Gate Path", WestGatePath},
            {"Blackbridge Grotto", BlackBridgeGrotto},
            {"Satellite Dish", SatelliteDish}
        };
        public static Dictionary<string, PermanentUnlock> SolvedDict = new(){
            {"Orchard Gate", AppleOrchard},
            {"VAC Controls", GemstoneCaverns},
            {"West Gate", WestGatePath},
            {"Laboratory Puzzle - Blackbridge", BlackBridgeGrotto},
            {"Raise Satellite", SatelliteDish}
        };


        public static bool HasPrepatched = false;

        public static PermanentUnlock GetPermanentUnlock(string name)
        {
            if (UnlockedDict.ContainsKey(name))
            {
                Logging.LogWarning(name);
                return UnlockedDict[name];
            }
            return null;
        }
        public static PermanentUnlock GetPermanentSolveByLocation(string locationName) {
            if (SolvedDict.ContainsKey(locationName)) { 
                return SolvedDict[locationName];
            }
            return null;
        }
        // Trying to patch the Rooms by modifying the prefabs.
        public static void AttemptPrePatch()
        {
            if (!HasPrepatched)
            {
                if (GemstoneCaverns.Solved && GemstoneCaverns.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Gemstone Cavern Open").Value)
                {
                    GemstoneCaverns.UnlockItem();
                    // Retroactively apply the Gemstone Cavern's Effect
                    GemstoneCaverns.ApplyEffects();
                }
                if (BlackBridgeGrotto.Solved && BlackBridgeGrotto.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Grotto Open").Value)
                {
                    BlackBridgeGrotto.UnlockItem();
                }
                if (AppleOrchard.Solved && AppleOrchard.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value)
                {
                    AppleOrchard.UnlockItem();
                    // Retroactively apply the Gemstone Cavern's Effect
                    AppleOrchard.ApplyEffects();
                }
                if (WestGatePath.Solved && WestGatePath.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("West Gate Open").Value)
                {
                    WestGatePath.UnlockItem();
                }
                if (SatelliteDish.Solved && SatelliteDish.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Satellite").Value)
                {
                    SatelliteDish.UnlockItem();
                }
                HasPrepatched = true;
            }
        }
    }
    public abstract class PermanentUnlock
    {
        public abstract string Name { get; set; }

        public abstract string LocationName { get; set; }

        public bool Unlocked = false;
        public bool Solved = false;

        public abstract void UnlockItem();

        public abstract void FoundLocation();

        public abstract void PreventDefault();
    }

    public class AppleOrchard:PermanentUnlock
    {
        // Override the Name
        public override string Name { get; set; } = "Apple Orchard";
        public override string LocationName { get; set; } = "Orchard Gate";
        // Run the unlock code.
        public override void UnlockItem() {
            Unlocked = true;
            PlayMakerFSM appleOrchard = GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/Letters Click Code (1)")?.GetComponent<PlayMakerFSM>();
            appleOrchard.GetState("State 4")?.EnableActionsOfType<SendEvent>();
            PlayMakerFSM appleOrchardButton = GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/lock anchor (1)/Rotate Anchor/Orchard Lock/Lock/Button")?.GetComponent<PlayMakerFSM>();
            appleOrchardButton.GetState("Check Code").ChangeTransition("FINISHED", "Collider's Off");
            if (Solved)
            {
                // Log the Unlock of the Apple Orchard to Stats.
                ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Orchard_Unlocked);
                // Set the Bool in the global persistent Manager to true.
                ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value = true;
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
            }
        }
        // Prevents the default Unlock.
        public override void PreventDefault()
        {
            if (!Unlocked)
            {
                ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value = false;
                // Unlocks the Gate (this one seems to do it without sounds).
                PlayMakerFSM appleOrchard = GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/Letters Click Code (1)")?.GetComponent<PlayMakerFSM>();
                PlayMakerFSM appleOrchardButton = GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/lock anchor (1)/Rotate Anchor/Orchard Lock/Lock/Button")?.GetComponent<PlayMakerFSM>();
                appleOrchardButton.GetState("Check Code").RemoveTransition("FINISHED");
                appleOrchardButton.GetState("Check Code").AddTransition("FINISHED", "Won't Open");
                appleOrchard.GetState("State 4")?.DisableActionsOfType<SendEvent>();
                appleOrchard.GetState("State 4")?.AddAction(FSMEventHandler.RegisteredEvents["Apple Orchard Unlock"].Event);
            }
        }
        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnGateOpened(LocationName);
        }

        public void ApplyEffects() {
            FsmInt AdjustmentAmount = ModInstance.StepManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + 20;
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }

    }
    public class GemstoneCaverns : PermanentUnlock
    {
        // Override the Name
        public override string Name { get; set; } = "Gemstone Caverns";
        public override string LocationName { get; set; } = "VAC Controls";
        public GameObject RoomObject = null;

        // Run the unlock code.
        public override void UnlockItem()
        {
            Unlocked = true;
            if (RoomObject != null)
            {
                FsmState State2 = RoomObject.transform.Find("_GAMEPLAY/Giant Switch/Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2");
                State2.EnableAction(2);
                State2.RemoveLastActionOfType<SendEventByName>();
            }
            if (Solved)
            {
                GameObject.Find("CULL GRID - GROUNDS/UNDERGROUND/Cull - Gemstone Cavern (once revealed)")?.SetActive(true);
                // Activate Permanent Additions
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
                // Activate Gemstone Caverns Icon
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/Gemstone Cavern Icon")?.SetActive(true);
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/5/Gemstone Cavern Icon")?.SetActive(true);
                // Activate and deactivate the required game objects.
                GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/FAR CULL/_GAMEPLAY do not bake/Gemstone DOOR/Cave Door")?.SetActive(false);

                //This is set false by the FSM but needs to be set true. However if the player is too close this will not be rendered properly.
                //May add a proximity check later.
                GameObject.Find("TERRAIN/EAST SECTOR/_GEM CAVE")?.SetActive(true);
                // Set the Bool in the Global Persistent Manager to true.
                ModInstance.GlobalPersistentManager.GetBoolVariable("Gemstone Cavern Open").Value = true;
            }
        }

        public override void PreventDefault() {
            SendEventByName unfreeze = new SendEventByName()
            {
                eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = new FsmOwnerDefault()
                    {
                        gameObject = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince"),
                        ownerOption = OwnerDefaultOption.SpecifyGameObject
                    },
                    fsmName = "FSM",
                    sendToChildren = false,
                    excludeSelf = false
                },
                sendEvent = "UnFreeze",
                delay = 0f,
                everyFrame = false
            };

            GameObject RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools");
            for (int i = 0; i < RoomSpawnPools.transform.childCount; i++)
            {
                Transform child = RoomSpawnPools.transform.GetChild(i);
                if (child.name.Contains("Utility Closet"))
                {
                    RoomObject = child.gameObject;
                    FsmState State2 = RoomObject.transform.Find("_GAMEPLAY/Giant Switch/Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2");
                    if (!Unlocked)
                    {
                        // This code may needs to be run after the utility closet has been spawned.
                        State2.DisableAction(2);
                    }
                    State2.AddAction(FSMEventHandler.RegisteredEvents["Gemstone Caverns Unlock"].Event);
                    State2.AddAction(unfreeze);
                    
                }
            }

        }

        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnVACControlsSolved();
        }

        public void ApplyEffects()
        {
            FsmInt AdjustmentAmount = ModInstance.GemManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + 2;
            // Send the "Update" event and the step counter should update.
            ModInstance.GemManager.SendEvent("Update");
        }
    }
    public class WestGatePath : PermanentUnlock
    {
        public override string Name { get; set; } = "West Gate Path";
        public override string LocationName { get; set; } = "West Gate";

        public override void UnlockItem()
        {
            Unlocked = true;
            PlayMakerFSM GateOpened = GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened")?.GetComponent<PlayMakerFSM>();
            GateOpened?.GetState("Hover").ChangeTransition("click", "GATE IS OPENED");
            if (Solved)
            {
                ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.West_Path_Gate_Unlocked);

                ModInstance.GlobalPersistentManager.GetBoolVariable("West Gate Open").Value = true;

                //Run code to open gate.
                GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened")?.GetComponent<PlayMakerFSM>()?.SendEvent("Begin");
            }
        }

        public override void PreventDefault()
        {
            if (!Unlocked)
            {
                PlayMakerFSM GateOpened = GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened")?.GetComponent<PlayMakerFSM>();
                GateOpened?.GetState("Hover").ChangeTransition("click", "Off");
            }
        }
        public override void FoundLocation()
        {
            ModInstance.ModEventHandler.OnGateOpened(LocationName);
            Solved = true;
        }
    }

    //TODO Confirm working and improve the handling to be more comparable to the Gemstone Caverns.
    public class BlackBridgeGrotto : PermanentUnlock
    {
        public override string Name { get; set; } = "Blackbridge Grotto";
        public override string LocationName { get; set; } = "Laboratory Puzzle - Blackbridge";

        public GameObject RoomObject = null;
        public override void UnlockItem()
        {
            Unlocked = true;
            if (RoomObject != null)
            {
                PlayMakerFSM LabMachine = RoomObject.transform.Find("_GAMEPLAY/Lab Machine").GetComponent<PlayMakerFSM>();
                LabMachine?.GetState("Chek if Grotto Is Open")?.EnableActionsOfType<GetFsmBool>();
            }
            if (Solved)
            {
                // Only 90% sure this is the correct event.
                ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Blackbridge_Powered);
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
                // Activate Gemstone Caverns Icon
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/BlackBridge Grotto Icon")?.SetActive(true);
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/5/BlackBridge Grotto Icon")?.SetActive(true);

                GameObject.Find("CULL GRID - GROUNDS/UNDERGROUND/Cull - Grotto (once revealed)")?.SetActive(true);

                GameObject.Find("TERRAIN/DRIVE SECTOR/HIDE FROM HOUSE/_GAMEPLAY/")?.SetActive(true);

                GameObject.Find("TERRAIN/EAST SECTOR/_GROTTO").SetActive(true);

                ModInstance.GlobalPersistentManager.GetBoolVariable("Grotto Open").Value = true;
            }
        }

        public override void PreventDefault()
        {
            SendEventByName unfreeze = new SendEventByName()
            {
                eventTarget = new FsmEventTarget()
                {
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = new FsmOwnerDefault()
                    {
                        gameObject = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince"),
                        ownerOption = OwnerDefaultOption.SpecifyGameObject
                    },
                    fsmName = "FSM",
                    sendToChildren = false,
                    excludeSelf = false
                },
                sendEvent = "UnFreeze",
                delay = 0f,
                everyFrame = false
            };
            GameObject RoomSpawnPools = GameObject.Find("__SYSTEM/Room Spawn Pools");
            for (int i = 0; i < RoomSpawnPools.transform.childCount; i++)
            {
                Transform child = RoomSpawnPools.transform.GetChild(i);
                if (child.name.Contains("Utility Closet"))
                {
                    RoomObject = child.gameObject;
                    PlayMakerFSM LabMachine = RoomObject.transform.Find("_GAMEPLAY/Lab Machine").GetComponent<PlayMakerFSM>();
                    PlayMakerFSM GrottoTrigger = RoomObject.transform.Find("_GAMEPLAY/Lab Machine/Grotto Trigger").GetComponent<PlayMakerFSM>();
                    FsmState GrottoState = GrottoTrigger?.GetState("State 2");
                    GrottoState?.DisableActionsOfType<SendEvent>();
                    GrottoState?.InsertAction(5, unfreeze);
                    GrottoState?.InsertAction(6, FSMEventHandler.RegisteredEvents["Blackbridge Grotto Unlock"].Event);
                    FsmBool GrottoOpen = LabMachine.GetBoolVariable("Grotto Open");
                    GrottoOpen.Value = Solved;
                    if (!Solved)
                    {
                        LabMachine?.GetState("Chek if Grotto Is Open")?.DisableActionsOfType<GetFsmBool>();
                        
                    }
                }
            }
            

        }
        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnLaboratoryPuzzleSolved();
        }
    }

    public class SatelliteDish : PermanentUnlock
    {
        public override string Name { get; set; } = "Satellite Dish";

        public override string LocationName { get; set; } = "Raise Satellite";

        public override void UnlockItem()
        {
            Unlocked = true;
            PlayMakerFSM pt2 = GameObject.Find("TERRAIN/EAST SECTOR/_APPLE ORCHARD/Back Orchard (cull)/BAKE LAYERS/Water - Just cast/SUNDIAL CONTROL/pt 2").GetComponent<PlayMakerFSM>();
            FsmState boolCheck = pt2.GetState("State 7");
            boolCheck.EnableFirstActionOfType<BoolTest>();
            if (Solved)
            {
                boolCheck.DisableFirstActionOfType<SendEvent>();
                ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Satellite_Raised);
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
                // Activate Satellite Dish Icon
                GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/5/Satellite Dish Icon")?.SetActive(true);
                GameObject.Find("TERRAIN/_GAMEPLAY (terrain)/sdish/MOVE UP B").SetActive(true);
                GameObject.Find("TERRAIN/EAST SECTOR/_APPLE ORCHARD/Back Orchard (cull)/BAKE LAYERS/Water - Just cast/SUNDIAL CONTROL/pt 2");
                ModInstance.GlobalPersistentManager.GetBoolVariable("Satellite").Value = true;
            }  
        }

        public override void PreventDefault()
        {
            if (!Unlocked)
            {
                PlayMakerFSM pt2 = GameObject.Find("TERRAIN/EAST SECTOR/_APPLE ORCHARD/Back Orchard (cull)/BAKE LAYERS/Water - Just cast/SUNDIAL CONTROL/pt 2").GetComponent<PlayMakerFSM>();
                FsmState boolCheck = pt2.GetState("State 7");
                boolCheck.DisableFirstActionOfType<BoolTest>();
                boolCheck?.AddAction(FSMEventHandler.RegisteredEvents["Satellite Raised"].Event);
            }
        }
        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnSatelliteRaised();
        }
    }

    public class BlueTents : PermanentUnlock
    {
        public override string Name { get; set; } = "Blue Tents";
        public override string LocationName { get; set; } = "Blue Tents Pickup";

        public bool _Bought = false;

        public override void UnlockItem()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
        public override void FoundLocation()
        {
        }
    }
}
