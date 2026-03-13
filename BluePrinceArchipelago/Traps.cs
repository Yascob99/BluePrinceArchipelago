using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrinceArchipelago
{
    public class TrapManager
    {
        
    }

    public abstract class Trap (string name, string trapType)
    {
        public string Name = name;
        public string TrapType = trapType;
        public abstract void ActivateTrap();
    }
    public abstract class SetTrap(string name, string trapType, string itemType) : Trap(name, trapType) { 

        public string itemType = itemType;

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
            FsmBool isFrozen = ModInstance.GlobalPersistentManager.GetBoolVariable("YesterFreezer");
            //If not in run and not already frozen.
            if (ModInstance.IsInRun && !isFrozen.Value)
            {
                isFrozen.Value = true;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value = ModInstance.GemManager.GetIntVariable("Gems").Value;
                ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGold").Value = ModInstance.GoldManager.GetIntVariable("Gold").Value;
                ModInstance.GoldManager.SendEvent("Freeze");
                ModInstance.GemManager.SendEvent("QuickFreeze");
            }
        }
    }
}
