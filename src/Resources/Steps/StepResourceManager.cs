using BluePrinceArchipelago;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;

using System;
using System.Collections.Generic;
#if Bep
using HutongGames.PlayMaker;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppTMPro;
using Il2CppHutongGames.PlayMaker;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Resources
{
    /// <summary>
    ///     A MonoBehaviour for hooking and processing specific Step related updates.
    /// </summary>
    internal class StepResourceManager : MonoBehaviour
    {
        public static StepResourceManager Instance;

        public int targetSteps = 0;

        public int oldSteps = 0;

        public GameObject StepTextObject = null;

        public GameObject Player = null;

        public GameObject PlayerCore = null;

        public GameObject BedUISlide = null;

        public GameObject StepTimeDelete = null;

        public GameObject DareEngine = null;

        public object Routine = null;

        public bool IsRunning = false;

        public bool WaitingForUnfreeze = false;

        public bool Initialized = false;

        public StepResourceManager(IntPtr ptr) : base(ptr)
        {
            Instance = this; //Set the modInstance for easy access.
        }


        /// <summary>
        ///     Handles updating steps incrementally.
        /// </summary>
        System.Collections.IEnumerator UpdateSteps()
        {
            IsRunning = true;
            targetSteps = ModInstance.StepManager.GetIntVariable("STEPS").Value;
            oldSteps = ModInstance.StepManager.GetIntVariable("old steps").Value;
            int stepChange = targetSteps - oldSteps;
            float waitTime = StepUpdate(stepChange);
            yield return new WaitForSeconds(waitTime);
            // Repeat adjusting steps one by one until it matches
            while (stepChange != 0 && ModInstance.IsInRun)
            {
                yield return new WaitForSeconds(waitTime);
                waitTime = StepUpdate(stepChange);
                targetSteps = ModInstance.StepManager.GetIntVariable("STEPS").Value;
                stepChange = targetSteps - oldSteps;
            }
            IsRunning = false;
        }


        System.Collections.IEnumerator WaitUntilUnfrozen() {
            WaitingForUnfreeze = true;
            FsmBool currentlyFrozen = Player?.GetComponent<PlayMakerFSM>()?.GetBoolVariable("currently frozen");
            if (currentlyFrozen != null) {
                while (currentlyFrozen.Value) {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            WaitingForUnfreeze = false;
            // If the target steps are still at 0;
            if (ModInstance.IsInRun && ModInstance.StepManager.GetIntVariable("target steps").Value < 1)
            {
                GameObject.Find("UI OVERLAY CAM").GetChild("ZERO STEP ENDING").SetActive(true);
            }
        }

        /// <summary>
        ///     Handles a step update, then returns the WaitTime
        /// </summary>
        /// <param name="stepChange">The difference between steps.</param>
        /// <returns>the wait time as a float</returns>
        private float StepUpdate(int stepChange) {
            
            float waitTime = 0.2f;
            if (oldSteps < targetSteps)
            {
                if (stepChange > 0)
                {
                    if (stepChange < 11)
                    {
                        waitTime = (0.2f / stepChange);
                    }
                    oldSteps++;
                }
                else
                {
                    if (-stepChange < 11)
                    {
                        waitTime = (0.2f / -stepChange);
                    }
                    oldSteps--;
                }
                string textSteps = Math.Clamp(oldSteps, 0, int.MaxValue).ToString();
                StepTextObject.GetComponent<TextMeshPro>().text = textSteps;
            }
            return waitTime;
        }

        public void RunUpdater()
        {
            // If the MB is initialized
            if (Initialized)
            {
                if (!ModInstance.IsInRun) {
                    Initialized = false;
                }
                else if (!IsRunning)
                {
                    IsRunning = true;
                    Instance.StartCoroutine(Instance.UpdateSteps());
                }
            }
        }

        public void RunUpdate()
        {
            Logging.Log($"Attempting to run update for the resource Steps", "Resources");
            ModInstance.StepManager.GetIntVariable("target steps").Value = ModInstance.StepManager.GetIntVariable("STEPS").Value + ModInstance.StepManager.GetIntVariable("Adjustment Amount").Value;
            ModInstance.StepManager.GetIntVariable("Adjustment Amount").Value = 0;
            if (ModInstance.StepManager.GetIntVariable("target steps").Value < 1) {
                Fix();
                return;
            }
            ModInstance.StepManager.GetIntVariable("old steps").Value = ModInstance.StepManager.GetIntVariable("old steps to compare").Value;
            ModInstance.StepManager.GetIntVariable("old steps").Value = ModInstance.StepManager.GetIntVariable("STEPS").Value;
            ModInstance.StepManager.GetIntVariable("STEPS").Value = ModInstance.StepManager.GetIntVariable("target steps").Value;
            RunUpdater();
        }

        public void SetSteps(int amount) {
            ModInstance.StepManager.GetIntVariable("STEPS").Value = amount;
            RunUpdate();
        }

        public void AdjustSteps(int amount) {
            ModInstance.StepManager.GetIntVariable("Adjustment Amount").Value = amount;
            RunUpdate();
        }

        public void RunUpdateStep()
        {
            if (ModItemManager.PickedUp.Contains("RUNNING SHOES"))
            {
                Event10();
            }
            else
            {
                Event0();
                
            }
        }
        public void Event10()
        {
            Logging.Log($"Attempting to run Update Step for Steps", "Resources");
            ModInstance.RunningEngine.SendEvent("Update");
            if (!ModInstance.StepManager.GetBoolVariable("StepFreeze").Value)
            {
                if (ModInstance.StepManager.GetBoolVariable("STOP time").Value)
                {
                    StopTime();
                }
                else if (ModInstance.StepManager.GetBoolVariable("NoStep").Value)
                {
                    NoStep();
                }
                else if (ModInstance.StepManager.GetBoolVariable("Position Locked").Value)
                {
                    ModInstance.StepManager.GetBoolVariable("Position Locked").Value = false;
                    Vector3 SecondPostion = PlayerCore.transform.position;
                    ModInstance.StepManager.GetVector3Variable("2nd Position").Value = SecondPostion;
                    Vector3 FirstPostion = ModInstance.StepManager.GetVector3Variable("First Position").Value;
                    float Distance = Vector3.Distance(SecondPostion, FirstPostion);
                    if (Distance > 22f)
                    {
                        PlayMakerFSM BedUISlideFSM = BedUISlide?.GetComponent<PlayMakerFSM>();
                        if (BedUISlide != null)
                        {
                            BedUISlideFSM.GetBoolVariable("Running Shoes").Value = true;
                            BedUISlideFSM.SendEvent("Begin");
                        }
                    }
                    else
                    {
                        StepAdjust();
                    }
                }
                else
                {
                    Vector3 FirstPostion = PlayerCore.transform.position;
                    ModInstance.StepManager.GetVector3Variable("First Position").Value = FirstPostion;
                    ModInstance.StepManager.GetBoolVariable("Postion Locked").Value = true;
                    StepAdjust();
                }
            }
        }

        private void StopTime()
        {
            ModInstance.StepManager.GetIntVariable("Total Steps Taken Today").Value++;
        }
        private void NoStep()
        {
            ModInstance.StepManager.GetBoolVariable("NoStep").Value = false;
        }

        private void StepAdjust()
        {
            ModInstance.GlobalPersistentManager.GetIntVariable("TotalStepsSpent").Value++;
            ModInstance.StepManager.GetIntVariable("totalstepslltime").Value = ModInstance.GlobalPersistentManager.GetIntVariable("TotalStepsSpent").Value;
            ModInstance.StepManager.GetIntVariable("Total Steps Taken Today").Value++;
            StepTimeDelete.GetComponent<PlayMakerFSM>().SendEvent("Update");
            ModInstance.StepManager.GetIntVariable("target steps").Value = ModInstance.StepManager.GetIntVariable("STEPS").Value - 1;
            if (ModInstance.StepManager.GetIntVariable("target steps").Value < 1) {
                Fix();
                return;
            }
            RunUpdater();
            int steps = ModInstance.StepManager.GetIntVariable("STEPS").Value;
            if (steps < 20) {
                DareEngine.GetComponent<PlayMakerFSM>().GetIntVariable("dare break").Value = 5;
                DareEngine.GetComponent<PlayMakerFSM>().SendEvent("Update");
            }
        }

        private void Fix()
        {
            ModInstance.StepManager.GetIntVariable("old steps").Value = ModInstance.StepManager.GetIntVariable("STEPS").Value;
            ModInstance.StepManager.GetIntVariable("STEPS").Value = ModInstance.StepManager.GetIntVariable("target steps").Value;
            FsmBool currentlyFrozen = Player?.GetComponent<PlayMakerFSM>()?.GetBoolVariable("currently frozen");
            if (currentlyFrozen.Value) {
                // Waits until the game is not paused/in a cutscene, then kills the player;
                WaitUntilUnfrozen();
            }
        }

        public void Event0()
        {
            if (!ModInstance.StepManager.GetBoolVariable("StepFreeze").Value)
            {
                if (ModInstance.StepManager.GetBoolVariable("STOP time").Value)
                {
                    StopTime();
                }
                else if (ModInstance.StepManager.GetBoolVariable("NoStep").Value)
                {
                    NoStep();
                }
                else
                {
                    StepAdjust();
                }
            }
        }

        public void StartOfDay()
        {

            // Prevent coroutine from being run multiple times.
            if (Routine != null)
            {
                MonoBehaviorExtensions.StopCoroutine(Instance, Routine);
            }
            StepTextObject = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Steps/Steps Icon/step rotator/Steps #");
            Player = GameObjectExtensions.FindGameObject("__SYSTEM/FPS Home/FPSController - Prince");
            PlayerCore = Player.GetChild("Player Core");
            BedUISlide = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Steps/Steps Icon/Bed UI Slide");
            StepTimeDelete = GameObjectExtensions.FindGameObject("__SYSTEM/HUD/Steps/Step Time delete (and gloabl variable)");
            DareEngine = GameObjectExtensions.FindGameObject("DAY/DARE ENGINE");
            Initialized = false;
        }

        private void Start()
        {
            CustomFsmMethodManager.RegisterMethods();
        }
    }
}