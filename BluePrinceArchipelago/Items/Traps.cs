using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Items
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
                ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = -count;
                // Send the "Update" event and the step counter should update.
                ModInstance.StepManager.SendEvent("Update");
            }
            if (TrapType == "Gems")
            {
                // change the adjustment amount.
                ModInstance.GemManager.FindIntVariable("Adjustment Amount").Value = -count;
                // Send the "Update" event and the step counter should update.
                ModInstance.GemManager.SendEvent("Update");
            }
            else if (TrapType == "Gold")
            {
                // change the adjustment amount.
                ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
                // Send the "Update" event and the step counter should update.
                ModInstance.GoldManager.SendEvent("Update");
            }
            else if (TrapType == "Allowance")
            {
                GameObject.Find("DAY").GetFsm("FSM").FindIntVariable("allowance").Value -= count;
            }
            else if (TrapType == "Dice")
            {
                // change the adjustment amount.
                ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = -count;
                // Send the "Update" event and the step counter should update.
                ModInstance.DiceManager.SendEvent("Update");
            }
            else if (TrapType == "Keys")
            {
                // change the adjustment amount.
                ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = -count;
                // Send the "Update" event and the step counter should update.
                ModInstance.KeyManager.SendEvent("Update");
            }
            else if (TrapType == "Luck")
            {
                int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
                if (luck - count > 0)
                {
                    ModInstance.LuckManager.FindIntVariable("LUCK").Value -= count;
                }
                else
                {
                    ModInstance.LuckManager.FindIntVariable("Luck").Value = 0;
                }
            }
            else if (TrapType == "Stars")
            {
                int totalStars = ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value;
                if (totalStars + 1 > 0)
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value += 1;
                }
                else
                {
                    ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = 0;
                }
                ModInstance.StarManager.SendEvent("Update");
            }
        }
    }
    public class SetTrap(string name, string trapType, int count = -1) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            if (TrapType == "Steps")
            {
                // TODO: find how to get current step count
                var current = 50;
                
                var difference = current - count;
                // change the adjustment amount.
                ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = -difference;
                // Send the "Update" event and the step counter should update.
                ModInstance.StepManager.SendEvent("Update");
            }
        }
    }
}
