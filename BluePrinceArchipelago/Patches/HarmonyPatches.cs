using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Patches
{
    /// <summary>
    ///     Patches related to item/rooms being spawned by the game.
    /// </summary>
    public class ItemPatches
    {
        [HarmonyPatch(typeof(PmtSpawn), "OnEnter")]
        [HarmonyPostfix]
        static void PostFix(PmtSpawn __instance, ref GameObject __state)
        {
            if (__instance != null)
            {
                Logging.Log("PmtSpawn OnEnter Postfix called.");
                GameObject obj = __instance.gameObject?.value;
                string poolName = __instance.poolName?.value;
                GameObject transformObj = __instance.spawnTransform?.value;
                GameObject spawnedObj = __instance.spawnedGameObject?.value;
                // Unsure why this results in a null object in some instances.
                if (poolName == "Pickup" && obj != null)
                {
                    ItemTriggers.OnAfterItemSpawned(obj, poolName, transformObj, spawnedObj);
                    //Can theoritically replace the game object spawned by replacing the __instance.gameObject.
                }
            }

            if (__state != null)
            {
                Logging.Log("PmtSpawn OnEnter Postfix calling OnAfterRoomSpawned.");
                RoomTriggers.OnAfterRoomSpawned(__state);
            }
        }
        [HarmonyPatch(typeof(PmtSpawn), "OnEnter")]
        [HarmonyPrefix]
        static void PreFix(PmtSpawn __instance, ref GameObject __state) {
            GameObject obj = __instance.gameObject?.value;
            string poolName = __instance.poolName?.value;
            GameObject transformObj = __instance.spawnTransform?.value;
            GameObject spawnedObj = __instance.spawnedGameObject?.value;
            if (poolName == "Rooms")
            {
                RoomTriggers.OnBeforeRoomSpawned(obj, transformObj);
                __state = obj; // Store the room GameObject in __state to be used in the Postfix
            }
            else
            {
                OtherGameObjectTriggers.OnBeforeOtherSpawn(obj, poolName, transformObj);
            }
        }
    }

    /// <summary>
    ///     Patches related to Rooms and drafting.
    /// </summary>
    public class RoomPatches {
        [HarmonyPatch(typeof(RoomDraftHelper), nameof(RoomDraftHelper.StartDraft))]
        [HarmonyPostfix]
        static void PostFix()
        {
            DraftTriggers.OnAfterDraftInitialize();
        }
        [HarmonyPatch(typeof(RoomDraftHelper), nameof(RoomDraftHelper.StartDraft))]
        [HarmonyPrefix]
        static void Prefix() {
            DraftTriggers.OnDraftBeforeInitialize();
        }
        [HarmonyPatch(typeof(OuterDraftManager), nameof(OuterDraftManager.StartDraft))]
        [HarmonyPrefix]
        static void OuterDraftPrefix()
        {
            DraftTriggers.OnBeforeOuterDraftStart();
        }

    }

    /// <summary>
    ///     Patches related to events.
    /// </summary>
    public class EventPatches {

        public static int depth = 0;

        private static readonly Dictionary<string, string> _LastStates = [];
        private static readonly Dictionary<string, (HashSet<string>, Action<Fsm, string, string>)> _ObservedFSMs = new(){
        {"ZERO STEP ENDING", (["State 3"], OnZeroStepsEnding)},
    };

        [HarmonyPatch(typeof(Fsm), nameof(Fsm.UpdateStateChanges))]
        [HarmonyPostfix]
        public static void UpdateStateChangesPostfix(Fsm __instance)
        {
            try
            {
                if (__instance == null) return;
                var gameObjectName = __instance.GameObjectName;

                foreach (var roomHandler in RoomHandler.RoomHandlers.Values)
                {
                    if (roomHandler.ObservedFSMStates.ContainsKey(gameObjectName))
                    {
                        var lastState = _LastStates.GetValueOrDefault(gameObjectName);
                        var currentState = __instance?.ActiveStateName;
                        if ((lastState == null || lastState != currentState) && roomHandler.ObservedFSMStates[gameObjectName].Contains(currentState))
                        {
                            _LastStates[gameObjectName] = currentState;
                            roomHandler.OnFSMStateChanged(__instance, gameObjectName, currentState);
                        }
                    }
                }

                foreach (var (fsmIdentifier, (observedStates, callback)) in _ObservedFSMs)
                {
                    if (fsmIdentifier == gameObjectName)
                    {
                        var lastState = _LastStates.GetValueOrDefault(gameObjectName);
                        var currentState = __instance?.ActiveStateName;
                        if ((lastState == null || lastState != currentState) && observedStates.Contains(currentState))
                        {
                            _LastStates[gameObjectName] = currentState;
                            callback(__instance, gameObjectName, currentState);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Logging.Log($"Error in FSM state change postfix: {ex}", "RoomHandler");
            }
        }

        // TODO: Find a hook that works for Mora Jai Boxes
        // [HarmonyPatch(typeof(MorajaiController), nameof(MorajaiController.CheckCorners))]
        // [HarmonyPostfix]
        // static void MorajaiPostfix(MorajaiController __instance)
        // {
        //     if (__instance == null) return;
        //     var gameObject = __instance.gameObject?.transform?.parent?.gameObject;
        //     if (gameObject == null) return;

        //     var value = __instance.hasSolved;

        //     Logging.Log($"Morajai Puzzle {gameObject.name} solved: {value}");

        //     foreach (var roomHandler in RoomHandler.RoomHandlers.Values)
        //     {
        //         if (roomHandler.MorajaiPuzzles.Contains(gameObject.name) && value)
        //         {
        //             roomHandler.OnMorajaiPuzzleSolved(gameObject.name);
        //         }
        //     }
        // }

        private static void OnZeroStepsEnding(Fsm fsm, string gameObjectName, string newState)
        {
            if (newState == "State 3")
            {
                Logging.Log("Zero Steps Ending reached, sending death link...", "DeathLink");
                Plugin.ArchipelagoClient.DeathLinkHandler.SendStepsDeathLink();
            }
        }

        [HarmonyPatch(typeof(SendEvent), "OnEnter")]
        [HarmonyPrefix]
        static void PreFix(SendEvent __instance)
        {
            try
            {
                FsmEventTarget target = __instance.eventTarget;
                FsmEvent sendEvent = __instance.sendEvent;
                string targetType = target == null ? "" : target.target.ToString();
                DelayedEvent delayedEvent = __instance.delayedEvent;
                FsmFloat delay = __instance.delay;
                bool isDelayed = false;
                if (delay.value > 0) {
                    isDelayed = true;
                }
                string eventName = sendEvent?.name;
                targetType = target?.target.ToString() ?? "";
                string targetName = target?.gameObject?.gameObject?.name ?? "";
                string SenderName = __instance.owner != null ? __instance.owner.name ?? __instance.owner.gameObject.name : "Unknown";
                Logging.Log($"{SenderName} Sending {eventName} to {targetType}: {targetName}", "Events");
                // Attempt to find the name of the GameObject being targeted.
                if (targetName.Trim() == "")
                {
                    GameObject targetObj = target?.gameObject?.gameObject?.value;
                    if (targetObj != null && !isDelayed)
                    {
                        targetName = targetObj.name;
                    }
                    else if (isDelayed)
                    {
                        targetName = delayedEvent?.eventTarget?.gameObject?.gameObject?.name ?? "";
                        if (targetName.Trim() == "")
                        {
                            targetName = delayedEvent?.eventTarget?.gameObject?.gameObject?.value?.name ?? "";
                        }
                    }
                }
                // Triggers whenever a custom Archipelago Event is sent to Archipelago FSM.
                if (targetName == "Archipelago")
                {
                    // If the Event is registered, trigger the event.
                    
                    if (eventName != null)
                    {
                        EventTriggers.ModEventTrigger(eventName);
                    }
                }
                else if (eventName.Contains("Allowance Token Pickup"))
                {
                    EventTriggers.AllowanceTokenPickup(__instance.owner);
                }
                else if (targetName == "Trunk Counter" && eventName == "Update Subtract")
                {
                    TrunkTriggers.OnTrunkOpened();
                }
                else if (targetName == "Upgrade Disks" && eventName == "Go")
                {
                    // Queues the Upgrade Disk Upgrade so operations involving PlayMakerArrayListProxies can be performed on the main thread.
                    PlayMakerArrayListProxy UpgradeIDs = ModInstance.UpgradeDisksObj.GetComponent<PlayMakerArrayListProxy>();
                    int length = UpgradeIDs.arrayList.Count;
                    int i = 0;
                    int id = -1;
                    bool exit = false;
                    while (i < length && !exit)
                    {
                        try
                        {
                            id = UpgradeIDs.GetItemAt(i).Unbox<int>();
                        }
                        catch
                        {
                            id = -1;
                            Logging.LogWarning("Error While attempting to convert Array item to integer");
                        }
                        if (id > -1)
                        {
                            if (ModInstance.QueueManager.AddUpgradeUsedToQueue(i))
                            {
                                exit = true;
                                ModInstance.QueueManager.AddUpgradeUsedToQueue(id);
                            }
                        }
                        i++;
                    }

                }
                else if (targetName == "Global Manager" && eventName.Contains("Pickup"))
                {
                    Logging.Log(eventName, "Events");
                    UniqueItem item = Plugin.UniqueItemManager.GetIfSpawned(eventName);
                    if (item != null)
                    {
                        // Handle the rare case of the item being spawned and the unlock for that item arriving before it has been picked up.
                        if (item.IsUnlocked)
                        {
                            // Re-enable the logic that adds the item to inventory. (Will not cause issues if already enabled).
                            FsmState state = Plugin.UniqueItemManager.GetPickupState(item.Name);
                            if (state != null)
                            {
                                state.EnableActionsOfType<ArrayListAdd>();
                            }
                        }
                        item.HasBeenFound = true;
                    }
                    else if (eventName.Contains("Upgrade"))
                    {
                        UpgradeDiskTriggers.OnUpgradeDiskPickedUp();
                    }
                }
                else if (eventName == "Go" && target?.gameObject?.gameObject?.Value?.transform?.parent?.name == "PLAN PICKER")
                {
                    DraftTriggers.OnBeforeFloorPlanAdds();
                }
            }
            catch (Exception e)
            {
                Logging.LogError(e, "EventPatches");
            }
        }

        [HarmonyPatch(typeof(StatsLogger), "BeginDay", [typeof(int)])]
        [HarmonyPostfix]
        static void PostFix(int dayNum) { 
            DayTriggers.OnDayStart(dayNum);
        }
        [HarmonyPatch(typeof(StatsLogger), "Record_Event", [typeof(EventID), typeof(EventFilter)])]
        [HarmonyPostfix]
        static void RecordEventPostFix(EventID id)
        {
            EventTriggers.OnStatsLoggerRecordEvent(id);
        }

        [HarmonyPatch(typeof(StatsLogger), nameof(StatsLogger.EndDay))]
        [HarmonyPostfix]
        static void EndDayPostfix(StatsLogger __instance)
        {
            Logging.Log("StatsLogger EndDay Postfix called.", "DeathLink");
            DayTriggers.OnDayEnd();
        }

        // The game will throw an error when falling back to closet. This prevents the error from filling up the log.
        [HarmonyPatch(typeof(RoomDraftHelper), nameof(RoomDraftHelper.PerformValidation))]
        [HarmonyPrefix]
        static bool PerformValidationPreFix(RoomDraftHelper __instance) {
            // Run the PerformValidation function once, but run it in a try catch so it's error doesn't break stuff too much.
            if (EventPatches.depth == 0) {
                EventPatches.depth++;
                try
                {
                    __instance.PerformValidation();
                    EventPatches.depth = 0;
                }
                catch
                {
                    EventPatches.depth = 0;
                    DraftTriggers.OnDraftValidationFailed();
                }
                return false;
            }
            return true;
        }
    }
}
