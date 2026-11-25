using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Resources;
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
    /// <param name="count">An amount the trap changes something by.</param>
    public abstract class Trap(string name, int count = 0) : ArchipelagoItem(name, false, count)
    {
        public override bool IsTrap()
        {
            return true;
        }
    }

    /// <summary>
    ///     Causes the player to lose an object at random. 
    /// </summary>
    /// <param name="name">The name of the trap</param>
    public class LoseItemTrap(string name) : Trap(name)
    {
        public override void AddToWorld()
        {
            GameItemManager.LoseRandomItem();
        }
    }

    /// <summary>
    ///     Simulates the effect of the freezer.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    public class FreezeTrap(string name) : Trap(name) 
    {
        public override void AddToWorld()
        {
            FsmBool isFrozen = ModInstance.GlobalPersistentManager.GetBoolVariable("YesterFreezer");
            // If not in run and not already frozen.
            if (ModInstance.IsInRun && (isFrozen != null) && !isFrozen.Value)
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
    public class EndOfDayTrap(string name) : Trap(name)
    {
        public override void AddToWorld()
        {
            //Sets the Zero Step Ending to on, regardless of steps. Seems to be the easiest Ending to trigger. May add a custom ending later.
            ResourceManager.AdjustSteps(-1000000, true);
        }
    }

    /// <summary>
    ///     Sets the current number of a given resource to a specific value.
    /// </summary>
    /// <param name="name">The name of the trap</param>
    /// <param name="resourceType">The type of the trap.</param>
    /// <param name="resourceAmount">The count to set the player's resource to.</param>
    public class SetTrap(string name, string resourceType, int resourceAmount = 0) : ArchipelagoItem(name, false, resourceAmount)
    {
        private readonly string _resourceType = resourceType;
        private readonly int _resourceAmount = resourceAmount;

        public override void AddToWorld()
        {
            int currentResourceAmount = ResourceManager.GetResourceCount(_resourceType);
            var difference = _resourceAmount - currentResourceAmount;
            ResourceManager.AdjustResource(_resourceType, difference);
        }
    }
}
