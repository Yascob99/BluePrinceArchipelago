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
    internal class GemResourceManager : MonoBehaviour
    {
        public static GemResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject GemIconObject = null;

        public GameObject GoldIconObject = null;

        public GameObject GemTextObject = null;

        public GameObject IceOverlayObject = null;

        public GameObject PlayerCore = null;

        private bool _FlashIsRunning = false;

        public GemResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        private System.Collections.IEnumerator FlashRed() {
            TextMeshPro GemTMP = GemTextObject.GetComponent<TextMeshPro>();
            GemTMP.color = Color.red;
            ModAssetManager.SetMaterial(GemIconObject, "Ui Atlas - RED");
            yield return new WaitForSeconds(0.1f);
            GemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            GemIconObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            GemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            GemIconObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            GemIconObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            GemIconObject.SetActive(true);
            GemTMP.color = Color.white;
            ModAssetManager.SetMaterial(GemIconObject, "Ui Atlas");
            _FlashIsRunning = false;
        }

        public void RunUpdate()
        {
            GemIconObject.SetActive(true);
            if (GetGemLock()) {
                SetGems(0);
                return;
            }
            int Gems = Math.Clamp(GetGemCount() + GetAdjustmentAmount(), 0, 1000000);
            SetGems(Gems);
        }

        public void RunUpdate2() {
            GemIconObject.SetActive(true);
            if (!_FlashIsRunning) {
                _FlashIsRunning = true;
                Instance.StartCoroutine(Instance.FlashRed());
            }
        }

        public void RunUpdateWithSound() {

            // Play GemDown Sound
            ModAssetManager.PlaySound(PlayerCore, "GemUp", 0.65294117f);
            RunUpdate();
        }

        public void RunFreeze() {
            SetGemCount(0);
            // Play frozen accounts sound
            GoldIconObject.SetActive(true);
            GemIconObject.SetActive(true);
        }

        public void RunUnFreeze()
        {
            int YesterGems = ModInstance.GlobalPersistentManager.GetIntVariable("YesterFreezerGems").Value;
            IceOverlayObject.SetActive(false);
            SetGems(YesterGems);
        }

        public void SetGems(int amount)
        {
            SetGemCount(amount);
            GemTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
        }

        public void AdjustGems(int amount)
        {
            SetAdjustmentAmount(amount);
            RunUpdate();
        }

        public void StartOfDay()
        {
            GemIconObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Gems/Gems Icon");
            GoldIconObject = GemIconObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Gold/Gold Icon");
            IceOverlayObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/ICE OVERLAY");
            GemTextObject = GemIconObject.FindDescendant("Gem #");
            PlayerCore = GameObjectExtensions.FindGameObject("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
        }

        private int GetIntVariable(string variable)
        {
            return ModInstance.DiceManager.GetIntVariable(variable).Value;
        }

        private int SetIntVariable(string variable, int amount)
        {
            return ModInstance.DiceManager.GetIntVariable(variable).Value = amount;
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
            return GetIntVariable("Adjustment Amount");
        }

        private int SetAdjustmentAmount(int amount)
        {
            return SetIntVariable("Adjustment Amount", amount);
        }

        private bool GetGemLock() {
            return ModInstance.DiceManager.GetBoolVariable("GEM LOCK").Value;
        }
        private bool SetGemLock(bool val)
        {
            return ModInstance.DiceManager.GetBoolVariable("GEM LOCK").Value = val;
        }
    }
}