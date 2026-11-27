using BluePrinceArchipelago.Utils;
using System;
using UnityEngine;
#if Bep
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using TMPro;
#endif
#if ML
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppHutongGames.PlayMaker;
using Il2CppTMPro;
#endif

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    ///     A MonoBehaviour for hooking and processing specific Step related updates.
    /// </summary>
    internal class DiceResourceManager : MonoBehaviour
    {
        public static DiceResourceManager _instance;

        public GameObject _diceIconObject = null;

        public GameObject _diceTextObject = null;

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Update2/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Initializing the instance. Required for IL2CPP registration.
        /// </summary>
        /// <param name="ptr">The ptr for IL2Cpp.</param>
        public DiceResourceManager(IntPtr ptr) : base(ptr)
        {
            _instance = this; //Set the modInstance for easy access.
        }

        public static DiceResourceManager GetInstance()
        {
            return _instance;
        }

        public void RunUpdate()
        {
            _diceIconObject.SetActive(true);
            int total = GetDiceCount();
            total = Math.Clamp(total + GetAdjustmentAmount(), 0, 1000000);
            SetAdjustmentAmount(0);
            SetDiceCount(total);
            UpdateText(total);
        }

        public static void SetDice(int amount)
        {
            _instance._diceIconObject.SetActive(true);
            int total = amount;
            total = Math.Clamp(total + _instance.GetAdjustmentAmount(), 0, 1000000);
            _instance.SetDiceCount(total);
            _instance.UpdateText(total);
        }

        private void UpdateText(int total) {
            _diceTextObject.transform.localScale = new Vector3(1.3652f, 1.3652f, 1.3652f);
            TextMeshPro TMPDiceText = _diceTextObject.GetComponent<TextMeshPro>();
            TMPDiceText.color = new Color(1, 1, 1, 1);
            TMPDiceText.text = total.ToString();
            if (total <= 0)
            {
                _diceIconObject.SetActive(false);
            }
        }

        public static void AdjustDice(int amount)
        {
            _instance.SetAdjustmentAmount(amount);
            _instance.RunUpdate();
        }

        public void StartOfDay()
        {
            _diceIconObject = GameObject.Find("__SYSTEM/HUD/Bones/Bones Icon");
            _diceTextObject = _diceIconObject.GetChild("Bone #");
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
            return SetIntVariable("BONES", amount);
        }

        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Adjustment Amount", amount);
        }

        private static CallMethod GetCallMethod(string methodname)
        {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _instance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}