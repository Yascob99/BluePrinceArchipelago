using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Core
{

    public abstract class Trap (string name, string trapType)
    {
        public string Name = name;
        public string TrapType = trapType;
        public abstract void ActivateTrap();
    }
    public class LoseItemTrap(string name, string trapType) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            Plugin.ModItemManager.LoseRandomItem();
        }
    }
    public class FreezeTrap(string name, string trapType) : Trap(name, trapType) 
    {
        public override void ActivateTrap()
        {
            FsmBool isFrozen = ModInstance.GlobalPersistentManager?.GetBoolVariable("YesterFreezer");
            //If not in run and not already frozen.
            if (ModInstance.IsInRun && isFrozen != null && !isFrozen.Value)
            {
                isFrozen.Value = true;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value = ModInstance.GemManager.GetIntVariable("Gems").Value;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGold").Value = ModInstance.GoldManager.GetIntVariable("Gold").Value;
                ModInstance.GoldManager.SendEvent("Freeze");
                ModInstance.GemManager.SendEvent("QuickFreeze");
            }
        }
    }
    public class EndOfDayTrap(string name, string trapType) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            //Sets the Zero Step Ending to on, regardless of steps. Seems to be the easiest Ending to trigger. May add a custom ending later.
            GameObject.Find("ZERO STEP ENDING").SetActive(true);
        }
    }
    public class LoseTrap(string name, string trapType, int count = -1) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            if (TrapType == "Steps")
            {
                // change the adjustment amount.
                ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
                // Send the "Update" event and the step counter should update.
                ModInstance.StepManager.SendEvent("Update");
            }
            else if (TrapType == "Stars")
            {
                if (!GameObject.Find("__SYSTEM/HUD/Stars").active)
                {
                    //Activate stars to ensure it can properly be updated.
                    GameObject.Find("__SYSTEM/HUD/Stars").SetActive(true);
                }
                int totalStars = ModInstance.StarManager.FindIntVariable("TotalStars").Value;
                if (totalStars + count > 0)
                {
                    ModInstance.StarManager.FindIntVariable("TotalStars").Value += count;
                }
                else
                {
                    ModInstance.StarManager.FindIntVariable("TotalStars").Value = 0;
                }
            }
        }
    }
}
