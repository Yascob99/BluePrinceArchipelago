using BluePrince;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker.Ecosystem.Utils;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BluePrinceArchipelago
{
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
                    Plugin.UniqueItemManager.OnItemSpawn(obj, poolName, transformObj, spawnedObj);
                    //Can theoritically replace the game object spawned by replacing the __instance.gameObject.
                }
            }

            if (__state != null)
            {
                Logging.Log("PmtSpawn OnEnter Postfix calling OnAfterRoomSpawned.");
                ModInstance.OnAfterRoomSpawned(__state);
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
                ModInstance.OnRoomSpawned(obj, transformObj);
                __state = obj; // Store the room GameObject in __state to be used in the Postfix
            }
            else
            {
                ModInstance.OnOtherSpawn(obj, poolName, transformObj);
            }
        }
    }
    public class RoomPatches {
        [HarmonyPatch(typeof(RoomDraftHelper), nameof(RoomDraftHelper.StartDraft))]
        [HarmonyPostfix]
        static void PostFix(RoomDraftHelper __instance)
        {
            ModInstance.OnDraftInitialize(__instance);
        }
        [HarmonyPatch(typeof(RoomDraftHelper), nameof(RoomDraftHelper.StartDraft))]
        [HarmonyPrefix]
        static void Prefix(RoomDraftHelper __instance) {
            ModInstance.OnDraftBeforeInitialize(__instance);
        }
        [HarmonyPatch(typeof(OuterDraftManager), nameof(OuterDraftManager.StartDraft))]
        [HarmonyPostfix]
        static void PostFix(OuterDraftManager __instance)
        {
            ModInstance.OnOuterDraftStart(__instance);
        }


    }
    public class EventPatches {
        [HarmonyPatch(typeof(SendEvent), "OnEnter")]
        [HarmonyPrefix]
        static void PreFix(SendEvent __instance)
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
            ModInstance.OnEventSend(target, sendEvent, delay, delayedEvent, __instance.owner, isDelayed);
        }

        // Should be called after all the 
        [HarmonyPatch(typeof(StatsLogger), "BeginDay", [typeof(int)])]
        [HarmonyPostfix]
        static void PostFix(int dayNum) { 
            ModInstance.OnDayStart(dayNum);
        }
        [HarmonyPatch(typeof(StatsLogger), "Record_Event", [typeof(EventID), typeof(EventFilter)])]
        [HarmonyPostfix]
        static void RecordEventPostFix(EventID id)
        {
            ModInstance.OnRecordEvent(id);
        }


        [HarmonyPatch(typeof(StatsLogger), "EndDayGUI")]
        [HarmonyPostfix]
        static void Postfix()
        {
            ModInstance.OnDayEnd();
        }
        [HarmonyPatch(typeof(GameObject), "SetActive")]
        [HarmonyPostfix]
        static void Postfix(GameObject __instance, bool value)
        {
            string name = __instance.name;
            if (name == null) {
                if (name.Contains("LOADING") && value) { 
                    GameObject currSave = GameObject.Find(name);
                    if (currSave != null) {
                        int saveSlot = currSave.GetComponent<PlayMakerFSM>()?.GetIntVariable("current save")?.Value ?? 5;
                        ModInstance.SaveSlot = saveSlot; //Set the saveSlot to the correct slot.
                    }
                }
            }
        }
    }

    public class ManagerPatches() 
    {
        [HarmonyPatch(typeof(ActionPromptData), "TriggerCallback")]
        [HarmonyPrefix]
        static void Prefix(ActionPromptData __instance) {
            string callbackName = __instance.Callback_TargetFSM?.GameObject?.name;
            if (callbackName == null) {
                callbackName = __instance.Callback_TargetEvent?.Name;
            }
            if (callbackName == null)
            {
                callbackName = __instance.Callback_Method.Method.Name ?? "Not Found";
            }
            Logging.Log($"BP Action {__instance.Type.ToString()} was triggered with the callback: {callbackName}.");
        }
    }
}
