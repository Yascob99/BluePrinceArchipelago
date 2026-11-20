using BluePrinceArchipelago.Utils;
using System;
using UnityEngine;
#if Bep
using TMPro;
#endif
#if ML
using Il2CppTMPro;
#endif

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    ///     A MonoBehaviour for hooking and processing specific Step related updates.
    /// </summary>
    internal class KeyResourceManager : MonoBehaviour
    {
        public static KeyResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject KeyIconObject = null;

        public GameObject KeyTextObject = null;

        public GameObject PlayerCore = null;

        public GameObject MonkArtKeys = null;

        public KeyResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        public void RunUpdate()
        {
            Logging.LogWarning("Updating Keys");
            KeyIconObject.SetActive(true);
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
            MonkArtKeys.SetActive(true);
            RunUpdate();
        }

        public void RunUpdateWithSound()
        {
            ModAssetManager.PlaySound(PlayerCore, "Key Up 2", 0.65294117f);
            RunUpdate();
        }

        public void SetKeys(int amount)
        {
            SetKeyCount(amount);
            Logging.LogWarning(amount);
            KeyTextObject.GetComponent<TextMeshPro>().text = amount.ToString();
            SetAdjustmentAmount(0);
        }

        public void AdjustKeys(int amount)
        {
            int Keys = Math.Clamp(GetKeyCount() + amount, 0, 1000000);
            SetKeys(Keys);
        }

        public void StartOfDay()
        {
            KeyIconObject = GameObject.Find("__SYSTEM/HUD/Keys/Keys Icon");
            KeyTextObject = KeyIconObject.GetChild("Key #");
            PlayerCore = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
            MonkArtKeys = GameObject.Find("__SYSTEM/HUD/Keys/Monk Art Keys");
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

        private bool GetKeyLock()
        {
            return ModInstance.KeyManager.GetBoolVariable("KEY LOCK").Value;
        }
        private bool SetKeyLock(bool val)
        {
            return ModInstance.KeyManager.GetBoolVariable("KEY LOCK").Value = val;
        }

        private int GetTotalKeysUsed()
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalKeysUsed").Value;
        }

        private int SetTotalKeysUsed(int amount)
        {
            return ModInstance.GlobalPersistentManager.GetIntVariable("TotalKeysUsed").Value = amount;
        }
    }
}