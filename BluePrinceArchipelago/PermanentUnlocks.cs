using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Core
{
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
            GameObject.Find("PERMANENT ADDITIONS").SetActive(true);
            // Activate Apple Orchard Icon
            GameObject.Find("Apple Orchard Icon").SetActive(true);
            // Set the Bool in the global persistent Manager to true.
            ModInstance.GlobalPersistentManager.GetBoolVariable("Apple Orchard Open").Value = true;
            // Unlocks the Gate (this one seems to do it without sounds).
            GameObject.Find("Letters Click Code (1)").GetComponent<PlayMakerFSM>().SendEvent("Event 0");
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
            // Invokes the regular Gemstone Cavern Add code. (This may cause glitches in it's current state, I will likely need to add a way of checking if there is a UI active and queue the unlock until the unlock is complete).
            GameObject.Find("Gemstone Cavern Add").SetActive(true);
        }

        public override void PreventDefault() {
            // This code may need to be run after the Utility closet has been spawned.
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

            //TODO actually toggle gate open.
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class BlackBridgeGrotto : PermanentUnlock
    {
        public new string Name = "Blackbridge Grotto";

        public override void Unlock()
        {
            throw new NotImplementedException();
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class SatelliteDish : PermanentUnlock
    {
        public new string Name = "Satellite Dish";

        public override void Unlock()
        {
            throw new NotImplementedException();
        }

        public override void PreventDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class BlueTents : PermanentUnlock
    {
        public new string Name = "Blue Tents";

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
