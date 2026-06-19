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
                }
                if (BlackBridgeGrotto.Solved && BlackBridgeGrotto.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Grotto Open").Value)
                {
                    BlackBridgeGrotto.UnlockItem();
                }
                if (AppleOrchard.Solved && AppleOrchard.Unlocked && !ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value)
                {
                    AppleOrchard.UnlockItem();
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
        public string Name = "";

        public string LocationName = "";

        public bool Unlocked = false;
        public bool Solved = false;

        public abstract void UnlockItem();

        public abstract void FoundLocation();

        public abstract void PreventDefault();
    }

    public class AppleOrchard:PermanentUnlock
    {
        // Override the Name
        public new string Name = "Apple Orchard";
        public new string LocationName = "Orchard Gate";
        public new bool Unlocked = false;
        public new bool Solved = false;
        // Run the unlock code.
        public override void UnlockItem() {
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

    }
    public class GemstoneCaverns : PermanentUnlock
    {
        // Override the Name
        public new string Name = "Gemstone Caverns";
        public new string LocationName = "VAC Controls";
        public new bool Unlocked = true;
        public new bool Solved = false;
        public GameObject RoomObject = new();

        // Run the unlock code.
        public override void UnlockItem()
        {
            FsmState State2 = GameObject.Find("Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2");
            State2.EnableFirstActionOfType<ActivateGameObject>();
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
            FsmState State2 = RoomObject.transform.Find("_GAMEPLAY").transform.Find("Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2");
            State2.AddAction(FSMEventHandler.RegisteredEvents["Gemstone Caverns Unlock"].Event);
            if (!Unlocked)
            {
                // This code may needs to be run after the utility closet has been spawned.
                State2.DisableFirstActionOfType<ActivateGameObject>();
            }
           
        }

        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnVACControlsSolved();
        }
    }
    public class WestGatePath : PermanentUnlock
    {
        public new string Name = "West Gate Path";
        public new string LocationName = "West Gate";
        public new bool Solved = false;
        public new bool Unlocked = false;

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
        public new string Name = "Blackbridge Grotto";
        public new string LocationName = "Laboratory Puzzle - Blackbridge";
        public new bool Solved = false;
        public new bool Unlocked = false;
        public override void UnlockItem()
        {
            PlayMakerFSM LabMachine = GameObjectExtensions.FindGameObject("Lab Machine")?.GetComponent<PlayMakerFSM>();
            LabMachine?.GetState("Chek if Grotto Is Open")?.EnableActionsOfType<GetFsmBool>();
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
            Unlocked = true;
        }

        public override void PreventDefault()
        {
            PlayMakerFSM GrottoTrigger = GameObject.Find("Grotto Trigger")?.GetComponent<PlayMakerFSM>();
            FsmState GrottoState = GameObject.Find("Grotto Trigger")?.GetComponent<PlayMakerFSM>()?.GetState("State 2");
            GrottoState?.DisableActionsOfType<SendEvent>();
            GrottoState?.InsertAction(5, FSMEventHandler.RegisteredEvents["Blackbridge Grotto Unlock"].Event);
            PlayMakerFSM LabMachine = GameObjectExtensions.FindGameObject("Lab Machine")?.GetComponent<PlayMakerFSM>();
            if (!Solved)
            {
                LabMachine?.GetState("Chek if Grotto Is Open")?.DisableActionsOfType<GetFsmBool>();
                FsmBool GrottoOpen = LabMachine.GetBoolVariable("Grotto Open");
                GrottoOpen.Value = Solved;
            }

        }
        public override void FoundLocation()
        {
            Solved = true;
            ModInstance.ModEventHandler.OnGateOpened(LocationName);
        }
    }

    public class SatelliteDish : PermanentUnlock
    {
        public new string Name = "Satellite Dish";

        public new string LocationName = "Raise Satellite";

        public new bool Solved = false;
        public new bool Unlocked = false;

        public override void UnlockItem()
        {
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
            Unlocked = true;
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
        public new string Name = "Blue Tents";
        public new string LocationName = "Blue Tents Pickup";

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
