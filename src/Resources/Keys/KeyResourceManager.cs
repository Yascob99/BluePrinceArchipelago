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
    internal class KeyResourceManager : MonoBehaviour
    {
        private static KeyResourceManager _instance;

        private GameObject _keyIconObject = null;

        private GameObject _keyTextObject = null;

        private GameObject _playerCore = null;

        private GameObject _monkArtKeys = null;

        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/Update2/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdateWithSoundMethod => GetCallMethod("RunUpdateWithSound");

        /// <summary>
        ///     Initializing the instance. Required for IL2CPP registration.
        /// </summary>
        /// <param name="ptr">The ptr for IL2Cpp.</param>
        public KeyResourceManager(IntPtr ptr) : base(ptr)
        {
            _instance = this; //Set the modInstance for easy access.
        }

        public static KeyResourceManager GetInstance()
        {
            return _instance;
        }

        public void RunUpdate()
        {
            Logging.LogWarning("Updating Keys");
            _keyIconObject.SetActive(true);
            int adjust = GetAdjustmentAmount();
            AdjustKeys(adjust);
            if (adjust < 0)
            {
                int totalspent = GetTotalKeysUsed();
                SetTotalKeysUsed((adjust / -1) + totalspent);
            }
            SetAdjustmentAmount(0);
        }

        public void RunEvent2() {
            SetAdjustmentAmount(2);
            _monkArtKeys.SetActive(true);
            RunUpdate();
        }

        public void RunUpdateWithSound()
        {
            ModAssetManager.PlaySound(_playerCore, "Key Up 2", 0.65294117f);
            RunUpdate();
        }

        public static void SetKeys(int amount)
        {
            _instance.SetKeyCount(amount);
            Logging.LogWarning(amount);
            _instance._keyTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            _instance.SetAdjustmentAmount(0);
        }

        public static void AdjustKeys(int amount)
        {
            int Keys = Math.Clamp(_instance.GetKeyCount() + amount, 0, 1000000);
            SetKeys(Keys);
        }

        public void StartOfDay()
        {
            _keyIconObject = GameObject.Find("__SYSTEM/HUD/Keys/Keys Icon");
            _keyTextObject = _keyIconObject.GetChild("Key #");
            _playerCore = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
            _monkArtKeys = GameObject.Find("__SYSTEM/HUD/Keys/Monk Art Keys");
        }

        private int GetIntVariable(string variable)
        {
            return ModInstance.KeyManager.GetIntVariable(variable).Value;
        }

        private int SetIntVariable(string variable, int amount)
        {
            return ModInstance.KeyManager.GetIntVariable(variable).Value = amount;
        }

        private int GetKeyCount()
        {
            return GetIntVariable("KEYS");
        }

        private int SetKeyCount(int amount)
        {
            return SetIntVariable("KEYS", amount);
        }

        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Adjustment Amount", amount);
        }

        private int GetTotalKeysUsed()
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalKeysUsed").Value;
        }

        private int SetTotalKeysUsed(int amount)
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalKeysUsed").Value = amount;
        }

        /// <summary>
        ///     Gets a CallMethod with the given Method name for the MonoBehavior.
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        private static CallMethod GetCallMethod(string methodname)
        {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _instance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}