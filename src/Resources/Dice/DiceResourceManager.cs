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
    internal class DiceResourceManager : MonoBehaviour
    {
        public static DiceResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject DiceIconObject = null;

        public GameObject DiceTextObject = null;

        public GameObject Player = null;

        public GameObject PlayerCore = null;

        public GameObject DareEngine = null;

        public bool Initialized = false;

        public DiceResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        public void RunUpdate()
        {
            DiceIconObject.SetActive(true);
            int total = GetDiceCount();
            total = Math.Clamp(total + GetAdjustmentAmount(), 0, 1000000);
            SetAdjustmentAmount(0);
            SetDiceCount(total);
            UpdateText(total);
        }

        public void SetDice(int amount)
        {
            DiceIconObject.SetActive(true);
            int total = amount;
            total = Math.Clamp(total + GetAdjustmentAmount(), 0, 1000000);
            SetDiceCount(total);
            UpdateText(total);
        }

        private void UpdateText(int total) {
            DiceTextObject.transform.localScale = new Vector3(1.3652f, 1.3652f, 1.3652f);
            TextMeshPro TMPDiceText = DiceTextObject.GetComponent<TextMeshPro>();
            TMPDiceText.color = new Color(1, 1, 1, 1);
            TMPDiceText.text = total.ToString();
            if (total <= 0)
            {
                DiceIconObject.SetActive(false);
            }
        }

        public void AdjustDice(int amount)
        {
            SetAdjustmentAmount(amount);
            RunUpdate();
        }

        public void StartOfDay()
        {
            DiceIconObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Bones/Bones Icon");
            DiceTextObject = DiceIconObject.FindDescendant("Bone #");
            Player = GameObjectExtensions.FindGameObject("__SYSTEM/FPS Home/FPSController - Prince");
            PlayerCore = GameObjectExtensions.FindGameObject("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
            DareEngine = GameObjectExtensions.FindGameObject("DAY/DARE ENGINE");
            Initialized = false;
        }

        private int GetIntVariable(string variable)
        {
            return ModInstance.DiceManager.GetIntVariable(variable).Value;
        }

        private int SetIntVariable(string variable, int amount)
        {
            return ModInstance.DiceManager.GetIntVariable(variable).Value = amount;
        }

        //Example variable access
        private int GetDiceCount()
        {
            return GetIntVariable("BONES");
        }

        private int SetDiceCount(int amount)
        {
            return SetIntVariable("Bones", amount);
        }

        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Adjustment Amount", amount);
        }
    }
}