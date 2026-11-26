using BluePrinceArchipelago.Utils;
using System;
using UnityEngine;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppTMPro;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    ///     A MonoBehaviour for hooking and processing specific Step related updates.
    /// </summary>
    internal class GoldResourceManager : MonoBehaviour
    {
        private static GoldResourceManager _instance;
        private GameObject _goldIconObject = null;
        private GameObject _goldTextObject = null;
        private GameObject _labExperiments = null;
        private GameObject _coinPurseTally = null;

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Add Coins.
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Update2/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdate2Method => GetCallMethod("RunUpdate2");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Freeze/QuickFreeze
        /// </summary>
        public static CallMethod GetFreezeMethod => GetCallMethod("Freeze");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UnFreeze/QuickUnfreeze
        /// </summary
        public static CallMethod GetUnFreezeMethod => GetCallMethod("UnFreeze");

        /// <summary>
        ///     Initializing the instance. Required for IL2CPP registration.
        /// </summary>
        /// <param name="ptr">The ptr for IL2Cpp.</param>
        public GoldResourceManager(IntPtr ptr) : base(ptr)
        {
            _instance = this; //Set the modInstance for easy access.
        }

        public static GoldResourceManager GetInstance()
        {
            return _instance;
        }


        public void RunUpdate()
        {
            _goldIconObject.SetActive(true);
            int adjustment = GetAdjustmentAmount();
            if (adjustment < 0)
            {
                int totalspent = GetTotalCoinsSpent();
                SetTotalCoinsSpent((adjustment / -1) + totalspent);
            }
            else if (adjustment > 0) {
                if (GetGoldLock()) {
                    return;
                }
                RunUpdate2();
            }
            AdjustGold(adjustment);
        }

        public void RunUpdate2()
        {
            _goldIconObject.SetActive(true);
            int adjustment = GetAdjustmentAmount();
            PlayMakerFSM CoinPurseTallyFSM = _coinPurseTally.GetComponent<PlayMakerFSM>();
            int purseamount = CoinPurseTallyFSM.GetIntVariable("purse amount").Value;
            purseamount += adjustment;
            CoinPurseTallyFSM.GetIntVariable("purse amount").Value = purseamount;
            CoinPurseTallyFSM.SendEvent("Update 2");
        }

        public void RunFreeze()
        {
            SetGold(0, false);
        }

        public void RunUnFreeze()
        {
            int YesterGold = ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGold").Value;
            SetGold(YesterGold);
        }

        public static void SetGold(int amount, bool lab = true)
        {
            int currentGold = _instance.GetGoldCount();
            if (amount < currentGold && amount - currentGold < -3 && lab) {
                _instance._labExperiments.GetComponent<PlayMakerFSM>().SendEvent("expiriment check 3");
            }
            _instance.SetGoldCount(amount);
            _instance._goldTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            _instance.SetAdjustmentAmount(0);
        }

        public static void AdjustGold(int amount)
        {
            int Gold = Math.Clamp(_instance.GetGoldCount() + _instance.GetAdjustmentAmount(), 0, 1000000);
            if (amount < -3)
            {
                _instance._labExperiments.GetComponent<PlayMakerFSM>().SendEvent("expiriment check 3");
            }
            _instance.SetGoldCount(Gold);
            _instance._goldTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            _instance.SetAdjustmentAmount(0);
        }

        public void StartOfDay()
        {
            _goldIconObject = GameObject.Find("__SYSTEM/HUD/Gold/Gold Icon");
            _goldTextObject = _goldIconObject.GetChild("Gold #");
            _labExperiments = GameObject.Find("__SYSTEM/LAB Experiments");
            _coinPurseTally = GameObject.Find("__SYSTEM/HUD/Gold/Coin Purse Gold/Coin Purse Tally");
        }

        private int GetIntVariable(string variable)
        {
            return ModInstance.GoldManager.GetIntVariable(variable).Value;
        }

        private int SetIntVariable(string variable, int amount)
        {
            return ModInstance.GoldManager.GetIntVariable(variable).Value = amount;
        }

        private int GetGoldCount()
        {
            return GetIntVariable("GOLD");
        }

        private int SetGoldCount(int amount)
        {
            return SetIntVariable("GOLD", amount);
        }

        private int GetTotalCoinsSpent() {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalCoinsSpent").Value;
        }

        private int SetTotalCoinsSpent(int amount) {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalCoinsSpent").Value = amount;
        }

        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Adjustment Amount", amount);
        }

        private bool GetGoldLock()
        {
            return ModInstance.GoldManager.GetBoolVariable("GOLD LOCK").Value;
        }
        private bool SetGoldLock(bool val)
        {
            return ModInstance.GoldManager.GetBoolVariable("GOLD LOCK").Value = val;
        }
        private static CallMethod GetCallMethod(string methodname)
        {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _instance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}