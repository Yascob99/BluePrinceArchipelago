using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using System;
using UnityEngine;
#if Bep
using HutongGames.PlayMaker;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppTMPro;
using Il2CppHutongGames.PlayMaker;
#endif

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    ///     A MonoBehaviour for hooking and processing specific Step related updates.
    /// </summary>
    internal class GoldResourceManager : MonoBehaviour
    {
        public static GoldResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject GoldIconObject = null;

        public GameObject GemIconObject = null;

        public GameObject GoldTextObject = null;


        public GameObject LabExperiments = null;

        public GameObject CoinPurseTally = null;

        public GoldResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }


        public void RunUpdate()
        {
            GoldIconObject.SetActive(true);
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
            GoldIconObject.SetActive(true);
            int adjustment = GetAdjustmentAmount();
            PlayMakerFSM CoinPurseTallyFSM = CoinPurseTally.GetComponent<PlayMakerFSM>();
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

        public void SetGold(int amount, bool lab = true)
        {
            int currentGold = GetGoldCount();
            if (amount < currentGold && amount - currentGold < -3 && lab) {
                LabExperiments.GetComponent<PlayMakerFSM>().SendEvent("expiriment check 3");
            }
            SetGoldCount(amount);
            GoldTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            SetAdjustmentAmount(0);
        }

        public void AdjustGold(int amount)
        {
            int Gold = Math.Clamp(GetGoldCount() + GetAdjustmentAmount(), 0, 1000000);
            if (amount < -3)
            {
                LabExperiments.GetComponent<PlayMakerFSM>().SendEvent("expiriment check 3");
            }
            SetGoldCount(Gold);
            GoldTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            SetAdjustmentAmount(0);
        }

        public void StartOfDay()
        {
            GemIconObject = GameObject.Find("__SYSTEM/HUD/Gems/Gems Icon");
            GoldIconObject = GameObject.Find("__SYSTEM/HUD/Gold/Gold Icon");
            GoldTextObject = GoldIconObject.GetChild("Gold #");
            LabExperiments = GameObject.Find("__SYSTEM/LAB Experiments");
            CoinPurseTally = GameObject.Find("__SYSTEM/HUD/Gold/Coin Purse Gold/Coin Purse Tally");
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
    }
}