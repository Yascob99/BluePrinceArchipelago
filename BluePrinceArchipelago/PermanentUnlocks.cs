using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using UnityEngine;

namespace BluePrinceArchipelago.PermanentUnlocks
{

    public static class PermanentUnlocks {
        public static AppleOrchard AppleOrchard = new AppleOrchard();
        public static GemstoneCavern GemstoneCavern = new GemstoneCavern();
        public static WestGatePath WestGatePath = new WestGatePath();
        public static BlackBridgeGrotto BlackBridgeGrotto = new BlackBridgeGrotto();
        public static SatelliteDish SatelliteDish = new SatelliteDish();
        //public static BlueTents BlueTents = new BlueTents(); Probably to be handled in gift shop code.
    }
    public abstract class PermanentUnlock
    {
        public string Name;

        public abstract void Unlock();

        public abstract void PreventDefault();
    }

    public class AppleOrchard :PermanentUnlock
    {
        // Override the Name
        public new string Name = "Apple Orchard";

        // Run the unlock code.
        public override void Unlock() {
            // Log the Unlock of the Apple Orchard to Stats.
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Orchard_Unlocked);

            // Activate Permanent Additions
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS").SetActive(true);
            // Activate Apple Orchard Icon
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/Apple Orchard Icon").SetActive(true);
            // Set the Bool in the global persistent Manager to true.
            ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value = true;
            // Unlocks the Gate (this one seems to do it without sounds).
            GameObject.Find("Letters Click Code (1)").GetComponent<PlayMakerFSM>().GetState("State 4").EnableActionsOfType<SendEvent>();
        }
        // Prevents the default Unlock.
        public override void PreventDefault()
        {
            // Disables the send event of the unlock. May need to update later so I can hook into this for pickups later.
            GameObject.Find("Letters Click Code (1)").GetComponent<PlayMakerFSM>().GetState("State 4").DisableFirstActionOfType<SendEvent>();
        }
    }
    public class GemstoneCavern : PermanentUnlock
    {
        // Override the Name
        public new string Name = "Gemstone Cavern";

        // Run the unlock code.
        public override void Unlock()
        {
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Gemstone_Cavern_Unlocked);
            // Activate Permanent Additions
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS").SetActive(true);
            // Activate Gemstone Caverns Icon
            GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /PERMANENT ADDITIONS/4/Gemstone Cavern Icon").SetActive(true);
            // Activate and deactivate the required game objects.
            GameObject.Find("TERRAIN/EAST SECTOR/_CAMPSITE/FAR CULL/_GAMEPLAY do not bake/Gemstone DOOR/Cave Door").SetActive(false);
            GameObject.Find("CULL GRID - GROUNDS/UNDERGROUND/Cull - Gemstone Cavern (once revealed)").SetActive(true);
            //This is set false by the FSM but needs to be set true. However if the player is too close this will not be rendered properly.
            //May add a proximity check later.
            GameObject.Find("TERRAIN/EAST SECTOR/_GEM CAVE").SetActive(true); 
            // Set the Bool in the global persistent Manager to true.
            ModInstance.GlobalPersistentManager.GetBoolVariable("Gemstone Cavern Open").Value = true;

            // Invokes the regular Gemstone Cavern Add code. (This may cause glitches in it's current state, I will likely need to add a way of checking if there is a UI active and queue the unlock until the unlock is complete).
            GameObject.Find("Gemstone Cavern Add").SetActive(true);
        }

        public override void PreventDefault() {
            // This code may needs to be run after the utility closet has been spawned.
            GameObject.Find("Giant Switch Lever").GetComponent<PlayMakerFSM>().GetState("State 2").DisableFirstActionOfType<ActivateGameObject>();
        }


    }
    public class WestGatePath : PermanentUnlock
    {
        public new string Name = "West Gate Path";

        public override void Unlock()
        {
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.West_Path_Gate_Unlocked);

            ModInstance.GlobalPersistentManager.GetBoolVariable("West Gate Open").Value = true;
            
            //Run code to open gate.
            GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened").GetComponent<PlayMakerFSM>().SendEvent("Begin");
        }

        public override void PreventDefault()
        {
            // Prevents the default from instead hooking the click back into the hover. Hopefully this doesn't break anything.
            GameObject.Find("TERRAIN/WEST SECTOR/_WEST SECTOR GAMEPLAY/West Gate/Gameplay Opened").GetComponent<PlayMakerFSM>().ChangeTransition("Off", "click", "Hover");
        }
    }

    public class BlackBridgeGrotto : PermanentUnlock
    {
        public new string Name = "Blackbridge Grotto";
        private bool _Solved = false;

        public override void Unlock()
        {
            // Only 90% sure this is the correct event.
            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(EventID.Blackbridge_Powered);
        }

        public override void PreventDefault()
        {
            // Needs to be invoked once the lab has loaded.
            // Grotto trigger still needs to be partially run to show the Lab Puzzle Animation. So only disabling the event that activates the adding of the Grotto.
            GameObject.Find("Grotto Trigger").GetComponent<PlayMakerFSM>().GetState("State 2").DisableActionsOfType<SendEvent>();
            // Lab Machine needs to be modified to still play solve animation even if grotto is unlocked, but not found.
            PlayMakerFSM LabMachine = GameObject.Find("Lab Machine").GetComponent<PlayMakerFSM>();
            FsmBool GrottoOpen = LabMachine.GetBoolVariable("Grotto Open");
            GrottoOpen.Value = _Solved;
            // Replace the 
            LabMachine.GetState("Chek if Grotto Is Open").DisableActionsOfType<GetFsmBool>();
        }
    }

    public class SatelliteDish : PermanentUnlock
    {
        public new string Name = "Satellite Dish";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }

    public class BlueTents : PermanentUnlock
    {
        public new string Name = "Blue Tents";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }

    public class ConservatoryPickup : PermanentUnlock
    {

        public new string Name = "Conservatory Pickup";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            
        }
    }

    public class LostAndFoundPickup : PermanentUnlock
    {
        public new string Name = "Lost&Found Pickup";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }
    public class MechnanariumPickup : PermanentUnlock
    {
        public new string Name = "Mechnarium Pickup";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }
    public class ThroneRoomPickup : PermanentUnlock
    {
        public new string Name = "ThroneRoom Pickup";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }

    public class PlanetariumPickup : PermanentUnlock
    {
        public new string Name = "Planetarium Pickup";

        public override void Unlock()
        {
            //TODO
        }

        public override void PreventDefault()
        {
            //TODO
        }
    }

    public class ClosedExhibitickup : PermanentUnlock
    {
        public new string Name = "Lost&Found Pickup";

        public override void Unlock()
        {
            throw new NotImplementedException();
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class TreasureTrovePickup : PermanentUnlock
    {
        public new string Name = "Lost&Found Pickup";

        public override void Unlock()
        {
            throw new NotImplementedException();
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class TunnelPickup : PermanentUnlock
    {
        public new string Name = "Lost&Found Pickup";

        public override void Unlock()
        {
            throw new NotImplementedException();
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

}
