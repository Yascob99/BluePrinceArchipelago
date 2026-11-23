using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using System;
using UnityEngine;
#if Bep
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
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
    internal sealed class StepResourceManager : MonoBehaviour
    {
        private static StepResourceManager _Instance;

        private int _targetSteps = 0;

        private int _oldSteps = 0;

        private GameObject _StepTextObject = null;

        private GameObject _Player = null;

        private GameObject _PlayerCore = null;

        private GameObject _BedUISlide = null;

        private GameObject _StepTimeDelete = null;

        private GameObject _DareEngine = null;

        private bool _IsRunning = false;

        private bool _WaitingForUnfreeze = false;

        private bool _Initialized = false;

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for UpdateStep
        /// </summary>
        public static CallMethod GetUpdateStepMethod => GetCallMethod("RunUpdateStep");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Update
        /// </summary>
        public static CallMethod GetUpdateMethod => GetCallMethod("RunUpdate");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Event0
        /// </summary>
        public static CallMethod GetEvent0 => GetCallMethod("Event0");

        /// <summary>
        ///     Gets the CallMethod FsmStateAction for Event10
        /// </summary>
        public static CallMethod GetEvent10 => GetCallMethod("Event10");

        public StepResourceManager(IntPtr ptr) : base(ptr)
        {
            _Instance = this; //Set the modInstance for easy access.
        }

        public static StepResourceManager GetInstance() {
            return _Instance;
        }

        /// <summary>
        ///     Handles updating steps incrementally.
        /// </summary>
        private System.Collections.IEnumerator UpdateSteps()
        {
            _IsRunning = true;
            _targetSteps = GetTargetSteps();
            _oldSteps = GetOldStepCount();
            int stepChange = _targetSteps - _oldSteps;
            float waitTime = StepUpdate(stepChange);
            yield return new WaitForSeconds(waitTime);
            // Repeat adjusting steps one by one until it matches
            while (stepChange != 0 && ModInstance.IsInRun)
            {
                yield return new WaitForSeconds(waitTime);
                stepChange = _targetSteps - _oldSteps;
                waitTime = StepUpdate(stepChange);
                _targetSteps = GetStepCount();
            }
            _IsRunning = false; 
        }


        /// <summary>
        ///     A IEnumerator for waiting until the player is not in a cutscene or pause screen,
        ///     then sends the zero step ending if they are not in a cutscene or pause menu
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator WaitUntilUnfrozen() {
            _WaitingForUnfreeze = true;
            FsmBool currentlyFrozen = _Player?.GetComponent<PlayMakerFSM>()?.GetBoolVariable("currently frozen");
            if (currentlyFrozen != null) {
                while (currentlyFrozen.Value && ModInstance.IsInRun) {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            _WaitingForUnfreeze = false;
            Logging.LogWarning($"Unfrozen, checking steps {GetTargetSteps()}");
            // If the target steps are still at 0;
            string textSteps = Math.Clamp(GetTargetSteps(), 0, int.MaxValue).ToString();
            _StepTextObject.GetComponent<TextMeshPro>().text = textSteps;
            if (ModInstance.IsInRun && GetTargetSteps() < 1)
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
            if (stepChange > 0)
            {
                if (stepChange < 11)
                {
                    waitTime = (0.2f / stepChange);
                }
                _oldSteps++;
                
            }
            else if (stepChange < 0)
            {
                if (-stepChange < 11)
                {
                    waitTime = (0.2f / -stepChange);
                }
                _oldSteps--;
            }
            string textSteps = Math.Clamp(_oldSteps, 0, int.MaxValue).ToString();
            _StepTextObject.GetComponent<TextMeshPro>().text = textSteps;

            return waitTime;
        }


        /// <summary>
        ///     Runs the IEnumerator if it meets the correct condition.
        /// </summary>
        private void RunUpdater()
        {
            // If the MB is initialized
            if (_Initialized)
            {
                if (!ModInstance.IsInRun) {
                    _Initialized = false;
                }
                else if (!_IsRunning)
                {
                    Logging.LogWarning("Starting Step Updater");
                    _IsRunning = true;
                    _Instance.StartCoroutine(_Instance.UpdateSteps());
                }
            }
        }


        /// <summary>
        ///     Runs the IEnumerator if it meets the correct conditions.
        /// </summary>
        private void RunWaitUntilUnfrozen() {
            if (_Initialized)
            {
                if (!ModInstance.IsInRun)
                {
                    _Initialized = false;
                }
                else if (!_WaitingForUnfreeze)
                {
                    Logging.LogWarning("Starting Wait Until Unfrozen");
                    _Instance.StartCoroutine(_Instance.WaitUntilUnfrozen());
                }
            }
        }

        /// <summary>
        ///     Runs the Update Event code for the StepsManager.
        ///     Needs to be public so it can be invoked directly by the FSM.
        /// </summary>
        public void RunUpdate()
        {
            Logging.LogWarning($"Attempting to run update for the resource Steps", "Resources");
            int targetSteps = GetStepCount() + GetAdjustmentAmount();
            SetTargetSteps(targetSteps);
            SetAdjustmentAmount(0);
            if (GetTargetSteps() < 1)
            {
                Fix();
                return;
            }
            SetOldStepCount(GetStepCount());
            SetOldStepsToCompare(GetOldStepCount());
            SetStepCount(GetTargetSteps());
            RunUpdater();
        }


        /// <summary>
        ///     Sets the player's steps to the provided amount
        /// </summary>
        /// <param name="amount">The number of steps to set the player's steps to.</param>
        public static void SetSteps(int amount) {
            if (_Instance)
            {
                _Instance.SetStepCount(amount);
                _Instance.RunUpdate();
            }
        }

        /// <summary>
        ///     Increases or decreases the players steps by the provided amount.
        /// </summary>
        /// <param name="amount">The integer in which to change the step count by.</param>
        public static void AdjustSteps(int amount) {
            if (_Instance)
            {
                _Instance.SetAdjustmentAmount(amount);
                _Instance.RunUpdate();
            }
        }


        /// <summary>
        ///     Runs the UpdateStep code for the StepManager
        ///     Has to be public so the FSM can invoke it.
        /// </summary>
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

        /// <summary>
        ///     Runs the Event "10" code for the StepManager.
        ///     Has to be public so the FSM can invoke it.
        /// </summary>
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
                    Vector3 SecondPostion = _PlayerCore.transform.position;
                    ModInstance.StepManager.GetVector3Variable("2nd Position").Value = SecondPostion;
                    Vector3 FirstPostion = ModInstance.StepManager.GetVector3Variable("First Position").Value;
                    float Distance = Vector3.Distance(SecondPostion, FirstPostion);
                    if (Distance > 22f)
                    {
                        PlayMakerFSM BedUISlideFSM = _BedUISlide?.GetComponent<PlayMakerFSM>();
                        if (_BedUISlide != null)
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
                    Vector3 FirstPostion = _PlayerCore.transform.position;
                    ModInstance.StepManager.GetVector3Variable("First Position").Value = FirstPostion;
                    ModInstance.StepManager.GetBoolVariable("Postion Locked").Value = true;
                    StepAdjust();
                }
            }
        }

        /// <summary>
        ///     Handles the Stop Time path of the code.
        /// </summary>
        private void StopTime()
        {
            SetIntVariable("Total Steps Taken Today", GetIntVariable("Total Steps Taken Today")+ 1);
        }

        /// <summary>
        ///     Handles the NoStep Path of the code.
        /// </summary>
        private void NoStep()
        {
            ModInstance.StepManager.GetBoolVariable("NoStep").Value = false;
        }

        /// <summary>
        ///     Handles the step adjustment section of the code.
        ///     This a helper function intended to break apart the logic
        ///     more bite sized sections of logic.
        /// </summary>
        private void StepAdjust()
        {
            ModInstance.GlobalPersistentManager.GetIntVariable("TotalStepsSpent").Value++;
            int targetSteps = GetStepCount() -1;
            SetTargetSteps(targetSteps);
            SetIntVariable("totalstepsalltime", ModInstance.GlobalPersistentManager.GetIntVariable("TotalStepsSpent").Value);
            SetIntVariable("Total Steps Taken Today", GetIntVariable("Total Steps Taken Today") + 1);
            _StepTimeDelete.GetComponent<PlayMakerFSM>().SendEvent("Update");
            if (targetSteps < 1) {
                Fix();
                return;
            }
            Logging.LogWarning(SetOldStepCount(GetStepCount()));
            Logging.LogWarning(SetOldStepsToCompare(GetOldStepCount()));
            Logging.LogWarning(SetStepCount(targetSteps));
            RunUpdater();
            int steps = GetStepCount();
            if (steps < 20) {
                _DareEngine.GetComponent<PlayMakerFSM>().GetIntVariable("dare break").Value = 5;
                _DareEngine.GetComponent<PlayMakerFSM>().SendEvent("Update");
            }
        }


        /// <summary>
        ///     Handles adjusting the StepCount when it is at 0 or below 0.
        /// </summary>
        private void Fix()
        {
            SetOldStepCount(GetStepCount());
            SetStepCount(GetTargetSteps());
            RunWaitUntilUnfrozen();
        }

        /// <summary>
        ///     Handles the Event "0" for the StepManager
        ///      Has to be public so the FSM can invoke it.
        /// </summary>
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

        /// <summary>
        ///     Handles the Start of Day initialization. 
        ///     Specifically grabbing the correct instances of the important game objects.
        /// </summary>
        public void StartOfDay()
        {
            _StepTextObject = GameObject.Find("__SYSTEM/HUD/Steps/Steps Icon/step rotator/Steps #");
            _Player = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince");
            _PlayerCore = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince/Player Core");
            _BedUISlide = GameObject.Find("__SYSTEM/HUD/Steps/Steps Icon/Bed UI Slide");
            _StepTimeDelete = GameObject.Find("__SYSTEM/HUD/Steps/Step Time delete (and gloabl variable)");
            _DareEngine = GameObjectExtensions.FindInHierarchy("DAY/DARE ENGINE");
            _Initialized = true;
        }

        /// <summary>
        ///     A helper method to get FsmInt Variables as ints from the StepManager's PlayMaker FSM.
        /// </summary>
        /// <param name="variable">The name of the variable to get</param>
        /// <returns>The Int value of the Int Variable</returns>
        private int GetIntVariable(string variable)
        {
            return ModInstance.StepManager.GetIntVariable(variable).Value;
        }

        /// <summary>
        ///     A helper function that sets FsmInt Variables in the StepManager's PlayMaker FSM to the provided int value.
        /// </summary>
        /// <param name="variable">The name of the variable to get</param>
        /// <param name="value">The int to set it to.</param>
        /// <returns>The int value of the FsmInt was set to.</returns>
        private int SetIntVariable(string variable, int value)
        {
            return ModInstance.StepManager.GetIntVariable(variable).Value = value;
        }

        /// <summary>
        ///     Gets the "STEPS" FsmInt from the StepManager as an int.
        /// </summary>
        /// <returns>Step Count as an int.</returns>
        private int GetStepCount()
        {
            return GetIntVariable("STEPS");
        }

        /// <summary>
        ///     Sets the "STEPS" FsmInt to the provided int value
        /// </summary>
        /// <param name="value">The value to set the "STEPS" FsmInt to.</param>
        /// <returns>The int value that the "STEPS" FsmInt was set to</returns>
        private int SetStepCount(int value)
        {
            return SetIntVariable("STEPS", value);
        }

        /// <summary>
        ///     Gets the "old step" FsmInt from the StepManager as an int.
        /// </summary>
        /// <returns>The int value of the "old step" FsmInt</returns>
        private int GetOldStepCount()
        {
            return GetIntVariable("old step");
        }

        /// <summary>
        ///     Sets the "old step" FsmInt in the StepManager to the provided int value.
        /// </summary>
        /// <param name="value">The value to set the "old step" FsmInt to.</param>
        /// <returns>The int value that the "old step" FsmInt was set to</returns>
        private int SetOldStepCount(int value)
        {
            return SetIntVariable("old step", value);
        }

        /// <summary>
        ///     Gets the "Adjustment Amount" FsmInt from the StepManager as an int.
        /// </summary>
        /// <returns>The int value of the "Adjustment Amount" FsmInt</returns>
        private int GetAdjustmentAmount()
        {
            return GetIntVariable("Adjustment Amount");
        }


        /// <summary>
        ///     Sets the "Adjustment Amount" FsmInt in the StepManager to the provided int value.
        /// </summary>
        /// <param name="value">The value to set the "Adjustment Amount" FsmInt to.</param>
        /// <returns>The int value that the "Adjustment Amount" FsmInt was set to</returns>
        private int SetAdjustmentAmount(int value)
        {
            return SetIntVariable("Adjustment Amount", value);
        }

        /// <summary>
        ///     Gets the "target steps" FsmInt from the StepManager as an int.
        /// </summary>
        /// <returns>The int value of the "target steps" FsmInt</returns>
        private int GetTargetSteps()
        {
            return GetIntVariable("target steps");
        }

        /// <summary>
        ///     Sets the "target steps" FsmInt in the StepManager to the provided int value.
        /// </summary>
        /// <param name="value">The value to set the "target steps" FsmInt to.</param>
        /// <returns>The int value that the "target steps" FsmInt was set to</returns>
        private int SetTargetSteps(int value)
        {
            return SetIntVariable("target steps", value);
        }

        /// <summary>
        ///     Gets the "old steps to compare" FsmInt from the StepManager as an int.
        /// </summary>
        /// <returns>The int value of the "old steps to compare" FsmInt</returns>
        private int GetOldStepsToCompare()
        {
            return GetIntVariable("old steps to compare");
        }

        /// <summary>
        ///     Sets the "old steps to compare" FsmInt in the StepManager to the provided int value.
        /// </summary>
        /// <param name="value">The value to set the "old steps to compare" FsmInt to.</param>
        /// <returns>The int value that the "old steps to compare" FsmInt was set to</returns>
        private int SetOldStepsToCompare(int value)
        {
            return SetIntVariable("old steps to compare", value);
        }

        /// <summary>
        ///     Gets a CallMethod with the given Method name for the MonoBehavior.
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        private static CallMethod GetCallMethod(string methodname)
        {
            FsmObject StepResourceManagerInstance = new FsmObject() { value = _Instance };
            return new CallMethod() { behaviour = StepResourceManagerInstance, methodName = methodname, parameters = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<FsmVar>([]), everyFrame = false };
        }
    }
}