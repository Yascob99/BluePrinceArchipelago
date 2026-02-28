using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using PathologicalGames;
using System;
using System.Reflection;
using UnityEngine;
using static UnityEngine.RectTransform;

namespace BluePrinceArchipelago
{
    public class ItemPatches
    {
        //[HarmonyPatch(typeof(SpawnPool), "Awake")]
        //[HarmonyPrefix]
        //static void PreFix(SpawnPool __instance)
        //{
        //    if (__instance != null && __instance.name == "Pickup Spawn Pools")
        //    {
        //        //Plugin.BepinLogger.LogMessage($"{__instance.name}");
        //        //for (int i = 0; i < __instance._perPrefabPoolOptions.Count; i++)
        //        //{
        //        //    Plugin.BepinLogger.LogMessage($"{i}: {__instance._perPrefabPoolOptions[i]?.prefab?.name}");
        //        //}
                
        //        ModInstance.sledge = Plugin.AssetBundle.LoadAsset<GameObject>("SLEDGE HAMMER");
        //        __instance._perPrefabPoolOptions[41].prefab = ModInstance.sledge.transform;
        //        Plugin.BepinLogger.LogMessage(__instance.prefabsFoldOutStates.Count);
        //        Plugin.BepinLogger.LogMessage(__instance._editorListItemStates.Count);

        //        PlayMakerArrayListProxy itemList = GameObject.Find("/__SYSTEM/Inventory/Inventory (PreSpawn)").GetComponent<PlayMakerArrayListProxy>();
        //        itemList.preFillGameObjectList[20] = ModInstance.sledge;
        //    }
        //}
        [HarmonyPatch(typeof(PmtSpawn), "OnEnter")]
        [HarmonyPostfix]
        static void PostFix(PmtSpawn __instance)
        {
            if (__instance != null)
            {
                GameObject obj = __instance.gameObject?.value;
                string poolName = __instance.poolName?.value;
                GameObject transformObj = __instance.spawnTransform?.value;
                GameObject spawnedObj = __instance.spawnedGameObject?.value;
                if (poolName == "Pickup")
                {
                    ModInstance.OnItemSpawn(obj, poolName, transformObj, spawnedObj);
                    //Can theoritically replace the game object spawned by replacing the __instance.gameObject.
                    if (obj)
                    {
                        if(Plugin.AssetBundle.Contains(obj.name))
                        {
                            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>(obj.name);
                            GameObject apObject = GameObject.Instantiate(prefab, transformObj.transform.position, transformObj.transform.rotation);
                            spawnedObj.transform.parent = apObject.transform;
                            spawnedObj.GetComponentInChildren<Collider>().enabled = false;
                        }
                        else
                        {
                            Plugin.BepinLogger.LogError("No AP Item found for: " + obj.name);
                        }
                    }
                }
                else if (poolName == "Rooms") {
                    ModInstance.OnRoomSpawned(obj, transformObj);
                }
                else
                {
                    ModInstance.OnOtherSpawn(obj, poolName, transformObj);
                }
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
    }
    public class EventPatches {
        [HarmonyPatch(typeof(SendEvent), "OnEnter")]
        [HarmonyPrefix]
        static void PreFix(SendEvent __instance)
        {
            FsmEventTarget target = __instance.eventTarget;
            FsmEvent sendEvent = __instance.sendEvent;
            string targetType = target.target.ToString();
            DelayedEvent delayedEvent = __instance.delayedEvent;
            FsmFloat delay = __instance.delay;
            bool isDelayed = false;
            if (delay.value > 0) {
                isDelayed = true;
            }
            ModInstance.OnEventSend(target, sendEvent, delay, delayedEvent, __instance.owner, isDelayed);
        }
        [HarmonyPatch(typeof(StatsLogger), "BeginDay")]
        [HarmonyPrefix]
        static void Prefix(int dayNum) { 
            ModInstance.OnDayStart(dayNum);
        }
    }
}
