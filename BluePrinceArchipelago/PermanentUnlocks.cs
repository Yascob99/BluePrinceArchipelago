using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HutongGames.PlayMaker.FsmEventTarget;

namespace BluePrinceArchipelago.PermanentUnlocks
{
    public static class Unlocks
    {
        public static AppleOrchard AppleOrchard = new();
        public static GemstoneCaverns GemstoneCaverns = new();
        public static WestGatePath WestGatePath = new();
        public static BlackBridgeGrotto BlackBridgeGrotto = new();
        public static SatelliteDish SatelliteDish = new();
        public static Dictionary<string, PermanentUnlock> UnlockDict = new(){
            {"Apple Orchard", AppleOrchard},
            {"Gemstone Caverns", GemstoneCaverns},
            {"West Gate Path", WestGatePath},
            {"BlackBridgeGrotto", BlackBridgeGrotto},
            {"Satellite Dish", SatelliteDish}
        };

        public static PermanentUnlock GetPermanentUnlock(string name) {
            if (UnlockDict.ContainsKey(name)) { 
                return UnlockDict[name];
            }
            return null;
        }
    }
    public abstract class PermanentUnlock
    {
        public string Name;

        public abstract void UnlockItem();

        public abstract void FoundLocation();

        public abstract void PreventDefault();
    }

    public class AppleOrchard:PermanentUnlock
    {
        // Override the Name
        public new string Name = "Apple Orchard";

        // Run the unlock code.
        public override void UnlockItem() {
            // Log the Unlock of the Apple Orchard to Stats.
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Orchard_Unlocked);

            // Activate Permanent Additions
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
            // Activate Apple Orchard Icon
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/Apple Orchard Icon")?.SetActive(true);
            // Set the Bool in the global persistent Manager to true.
            ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value = true;
            // Unlocks the Gate (this one seems to do it without sounds).
            GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/Letters Click Code (1)")?.GetComponent<PlayMakerFSM>()?.GetState("State 4")?.EnableActionsOfType<SendEvent>();
        }
        // Prevents the default Unlock.
        public override void PreventDefault()
        {
            PlayMakerFSM appleOrchard = GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/CAMPSITE SOUTH CULL/Orchard Gameplay/Orchard Gate/Letters Click Code (1)")?.GetComponent<PlayMakerFSM>();
            appleOrchard.GetState("State 4")?.DisableActionsOfType<SendEvent>();
            appleOrchard.GetState("State 4")?.AddAction(FSMEventHandler.RegisteredEvents["Apple Orchard Unlock"].Event);
        }
        public override void FoundLocation()
        {
            ModInstance.ModEventHandler.OnGateOpened("Orchard Gate");
        }

    }
    public class GemstoneCaverns : PermanentUnlock
    {
        // Override the Name
        public new string Name = "Gemstone Caverns";

        // Run the unlock code.
        public override void UnlockItem()
        {
            GameObject.Find("CULL GRID - GROUNDS/UNDERGROUND/Cull - Gemstone Cavern (once revealed)")?.SetActive(true);
            // Activate Permanent Additions
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS")?.SetActive(true);
            // Activate Gemstone Caverns Icon
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/Gemstone Cavern Icon")?.SetActive(true);
            // Activate and deactivate the required game objects.
            GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/FAR CULL/_GAMEPLAY do not bake/Gemstone DOOR/Cave Door")?.SetActive(false);
           
            //This is set false by the FSM but needs to be set true. However if the player is too close this will not be rendered properly.
            //May add a proximity check later.
            GameObject.Find("TERRAIN/EAST SECTOR/_GEM CAVE")?.SetActive(true); 
            // Set the Bool in the Global Persistent Manager to true.
            ModInstance.GlobalPersistentManager.GetBoolVariable("Gemstone Cavern Open").Value = true;
        }

        public override void PreventDefault() {
            // This code may needs to be run after the utility closet has been spawned.
            FsmState State2 = GameObject.Find("Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2");
            State2.DisableFirstActionOfType<ActivateGameObject>();
            State2.AddAction(FSMEventHandler.RegisteredEvents["Gemstone Caverns Unlock"].Event);
        }

        public override void FoundLocation()
        {
            ModInstance.ModEventHandler.OnVACControlsSolved();
        }
    }
    public class WestGatePath : PermanentUnlock
    {
        public new string Name = "West Gate Path";

        public override void UnlockItem()
        {
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.West_Path_Gate_Unlocked);

            ModInstance.GlobalPersistentManager.GetBoolVariable("West Gate Open").Value = true;
            
            //Run code to open gate.
            GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened")?.GetComponent<PlayMakerFSM>()?.SendEvent("Begin");
        }

        public override void PreventDefault()
        {
            PlayMakerFSM GateOpened = GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened")?.GetComponent<PlayMakerFSM>();
            GateOpened?.GetState("Hover").ChangeTransition("click", "Off");
            

        }
        public override void FoundLocation()
        {
            ModInstance.ModEventHandler.OnGateOpened("West Gate");
        }
    }

    public class BlackBridgeGrotto : PermanentUnlock
    {
        public new string Name = "Blackbridge Grotto";
        private bool _Solved = false;

        public override void UnlockItem()
        {
            // Only 90% sure this is the correct event.
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Blackbridge_Powered);
            
        }

        public override void PreventDefault()
        {
            // Needs to be invoked once the lab has loaded.
            // Grotto trigger still needs to be partially run to show the Lab Puzzle Animation. So only disabling the event that activates the adding of the Grotto.
            FsmState State2 = GameObject.Find("Grotto Trigger")?.GetComponent<PlayMakerFSM>()?.GetState("State 2");
            State2?.DisableActionsOfType<SendEvent>();
            State2?.InsertAction(5, FSMEventHandler.RegisteredEvents["Blackbridge Grotto Unlock"].Event);
            // Lab Machine needs to be modified to still play solve animation even if grotto is unlocked, but not found.
            PlayMakerFSM LabMachine = GameObject.Find("Lab Machine")?.GetComponent<PlayMakerFSM>();
            FsmBool GrottoOpen = LabMachine?.GetBoolVariable("Grotto Open");
            GrottoOpen.Value = _Solved;
            // Get rid of step that pulls the bool from elsewhere.
            LabMachine?.GetState("Chek if Grotto Is Open")?.DisableActionsOfType<GetFsmBool>();
        }
        public override void FoundLocation()
        {
            _Solved = true;
            ModInstance.ModEventHandler.OnGateOpened("");
        }
    }

    public class SatelliteDish : PermanentUnlock
    {
        public new string Name = "Satellite Dish";

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

    public class BlueTents : PermanentUnlock
    {
        public new string Name = "Blue Tents";

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
