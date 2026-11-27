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
    internal class GemResourceManager : MonoBehaviour
    {
        private static GemResourceManager _instance;

        private GameObject _gemIconObject = null;

        private GameObject _goldIconObject = null;

        private GameObject _gemTextObject = null;

        private GameObject _iceOverlayObject = null;

        private GameObject _playerCore = null;

        private bool _flashIsRunning = false;

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update/UpdateSubtract
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update2
        /// </summary>
        public static CallMethod GetUpdate2Method => GetCallMethod("RunUpdate2");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UpdateWithSound
        /// </summary>
        public static CallMethod GetUpdateWithSoundMethod => GetCallMethod("RunUpdateWithSound");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Freeze/QuickFreeze
        /// </summary>
        public static CallMethod GetFreezeMethod => GetCallMethod("Freeze");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UnFreeze/QuickUnfreeze
        /// </summary>
        public static CallMethod GetUnFreezeMethod => GetCallMethod("UnFreeze");

        /// <summary>
        ///     Initializing the instance. Required for IL2CPP registration.
        /// </summary>
        /// <param name="ptr">The ptr for IL2Cpp.</param>
        public GemResourceManager(IntPtr ptr) : base(ptr)
        {
            _instance = this; //Set the modInstance for easy access.
        }

        public static GemResourceManager GetInstance()
        {
            return _instance;
        }

        private System.Collections.IEnumerator FlashRed() {
            TextMeshPro GemTMP = _gemTextObject.GetComponent<TextMeshPro>();
            GemTMP.color = Color.red;
            ModAssetManager.SetMaterial(_gemIconObject, "Ui Atlas - RED");
            yield return new WaitForSeconds(0.1f);
            _gemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            _gemIconObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            _gemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            _gemIconObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            _gemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            _gemIconObject.SetActive(true);
            GemTMP.color = Color.white;
            ModAssetManager.SetMaterial(_gemIconObject, "Ui Atlas");
            _flashIsRunning = false;
        }

        public void RunUpdate()
        {
            Logging.LogWarning("Updating Gems");
            _gemIconObject.SetActive(true);
            if (GetGemLock()) {
                AdjustGems(Math.Clamp(GetAdjustmentAmount(), 0, -1000000));
                return;
            }
            AdjustGems(GetAdjustmentAmount());
            SetAdjustmentAmount(0);
        }

        public void RunUpdate2() {
            Logging.LogWarning("Updating Gems");
            _gemIconObject.SetActive(true);
            if (!_flashIsRunning) {
                _flashIsRunning = true;
                _instance.StartCoroutine(_instance.FlashRed());
            }
        }

        public void RunUpdateWithSound() {
            Logging.LogWarning("Updating Gems");
            // Play GemDown Sound
            ModAssetManager.PlaySound(_playerCore, "GemDown", 0.65294117f);
            RunUpdate();
        }

        public void RunFreeze() {
            SetGemCount(0);
            // Play frozen accounts sound
            _goldIconObject.SetActive(true);
            _gemIconObject.SetActive(true);
        }

        public void RunUnFreeze()
        {
            int YesterGems = ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value;
            _iceOverlayObject.SetActive(false);
            SetGems(YesterGems);
        }

        public static void SetGems(int amount)
        {
            _instance.SetGemCount(amount);
            _instance._gemTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
        }

        public static void AdjustGems(int amount)
        {
            int Gems = _instance.GetGemCount() + amount;
            SetGems(Gems);
        }

        public void StartOfDay()
        {
            _gemIconObject = GameObject.Find("__SYSTEM/HUD/Gems/Gems Icon");
            _goldIconObject = GameObject.Find("__SYSTEM/HUD/Gold/Gold Icon");
            _iceOverlayObject = GameObject.Find("__SYSTEM/HUD/ICE OVERLAY");
            _gemTextObject = _gemIconObject.GetChild("Gem #");
            _playerCore = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
        }

        private int GetIntVariable(string variable)
        {
            return ModInstance.GemManager.GetIntVariable(variable).Value;
        }

        private int SetIntVariable(string variable, int amount)
        {
            return ModInstance.GemManager.GetIntVariable(variable).Value = amount;
        }

        private int GetGemCount()
        {
            return GetIntVariable("GEMS");
        }

        private int SetGemCount(int amount)
        {
            return SetIntVariable("GEMS", amount);
        }

        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Gem Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Gem Adjustment Amount", amount);
        }

        private bool GetGemLock() {
            return ModInstance.GemManager.GetBoolVariable("GEM LOCK").Value;
        }
        private bool SetGemLock(bool value)
        {
            return ModInstance.GemManager.GetBoolVariable("GEM LOCK").Value = value;
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