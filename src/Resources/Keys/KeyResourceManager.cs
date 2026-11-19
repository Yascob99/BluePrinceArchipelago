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
    internal class KeyResourceManager : MonoBehaviour
    {
        public static KeyResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject KeyIconObject = null;

        public GameObject KeyTextObject = null;

        public KeyResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }

        public void RunUpdate()
        {
            
        }

        public void RunUpdate2()
        {
            
        }

        public void RunFreeze()
        {
            
        }

        public void RunUnFreeze()
        {
           
        }

        public void SetKeys(int amount)
        {
            
        }

        public void AdjustKeys(int amount)
        {
            
        }

        public void StartOfDay()
        {
            KeyIconObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Keys/Keys Icon");
            KeyTextObject = KeyIconObject.FindDescendant("Key #");
          
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
    }
}