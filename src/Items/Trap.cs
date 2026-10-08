using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;

#endif
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     The Template for traps.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    /// <param name="count">An amount the trap changes something by.</param>
    public abstract class Trap (string name, string trapType, int count = 0)
    {
        public string Name = name;
        public string TrapType = trapType;

        public int Count = count;

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
            ModItemManager.LoseRandomItem();
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
            FsmBool isFrozen = ModInstance.GlobalPersistentManager.GetBoolVariable("YesterFreezer");
            // If not in run and not already frozen.
            if (ModInstance.IsInRun && isFrozen != null && !isFrozen.Value)
            {
                
                ModInstance.GlobalPersistentManager.GetBoolVariable("YesterFreezer").Value = true;
                Logging.LogWarning(ModInstance.GemManager.GetIntVariable("GEMS").Value);
                Logging.LogWarning(ModInstance.GoldManager.GetIntVariable("GOLD").Value);
                int gems = ModInstance.GemManager.GetIntVariable("GEMS").Value;
                int gold = ModInstance.GoldManager.GetIntVariable("GOLD").Value;
                ModInstance.GoldManager.GetIntVariable("GEMS").Value = gems;
                ModInstance.GoldManager.SendEvent("Freeze");
                ModInstance.GemManager.SendEvent("Freeze");
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value = gems;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGold").Value = gold;
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
            ResourceManager.AdjustSteps(-1000000, true);
        }
    }

    /// <summary>
    ///     A trap that causes the player to lose an amount of a resource.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    /// <param name="count">The number of that resource to adjust by. Defaults to -1</param>
    public class LoseTrap(string name, string trapType, int count = 0) : Trap(name, trapType, count)
    {
        public override void ActivateTrap()
        {
            ResourceManager.AdjustResource(TrapType, Count);
        }
    }

    /// <summary>
    ///     Sets the current number of a given resource to a specific value.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="trapType">The type of the trap.</param>
    /// <param name="count">The count to set the player's resource to.</param>
    public class SetTrap(string name, string trapType, int count = 0) : Trap(name, trapType, count)
    {
        public override void ActivateTrap()
        {
            if (TrapType == "Steps")
            {
                var current = ModInstance.StepManager.FindIntVariable("STEPS").value;
                
                var difference = current - Count;
                // change the adjustment amount.
                ResourceManager.AdjustSteps(-difference, true);
            }
        }
    }
}
