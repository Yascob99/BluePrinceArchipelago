using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     The Template for traps.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    public abstract class Trap (string name, string trapType)
    {
        public string Name = name;
        public string TrapType = trapType;

        /// <summary>
        ///     Handles what happens on trap activation.
        /// </summary>
        public abstract void ActivateTrap();
    }
    /// <summary>
    ///     Causes the player to lose an object at random. 
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    public class LoseItemTrap(string name, string trapType) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            Plugin.ModItemManager.LoseRandomItem();
        }
    }
    /// <summary>
    ///     Simulates the effect of the freezer.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    public class FreezeTrap(string name, string trapType) : Trap(name, trapType) 
    {
        public override void ActivateTrap()
        {
            FsmBool isFrozen = ModInstance.GlobalPersistentManager?.GetBoolVariable("YesterFreezer");
            // If not in run and not already frozen.
            if (ModInstance.IsInRun && isFrozen != null && !isFrozen.Value)
            {
                isFrozen.Value = true;
                Logging.LogWarning(ModInstance.GemManager.GetIntVariable("Gems").Value);
                Logging.LogWarning(ModInstance.GoldManager.GetIntVariable("Gold").Value);
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value = ModInstance.GemManager.GetIntVariable("Gems").Value;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGold").Value = ModInstance.GoldManager.GetIntVariable("Gold").Value;
                ModInstance.GoldManager.SendEvent("Freeze");
                ModInstance.GemManager.SendEvent("QuickFreeze");
            }
        }
    }
    /// <summary>
    ///     Ends the day by invoking the ZeroStepEnding.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    public class EndOfDayTrap(string name, string trapType) : Trap(name, trapType)
    {
        public override void ActivateTrap()
        {
            //Sets the Zero Step Ending to on, regardless of steps. Seems to be the easiest Ending to trigger. May add a custom ending later.
            GameObject.Find("ZERO STEP ENDING").SetActive(true);
        }
    }

    /// <summary>
    ///     A trap that causes the player to lose an amount of a resource.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    /// <param name="count">The number of that resource to adjust by. Defaults to -1</param>
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

    /// <summary>
    ///     Sets the current number of a given resource to a specific value.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    /// <param name="count">The count to set the player's resource to.</param>
    public class SetTrap(string name, string trapType, int count = 0) : Trap(name, trapType)
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
