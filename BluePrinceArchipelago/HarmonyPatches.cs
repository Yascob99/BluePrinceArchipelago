using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Runtime.InteropServices;
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
                    if (obj)   // && object has not been found before
                    {
                        if(Plugin.AssetBundle.Contains(obj.name))
                        {
                            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>(obj.name);

                            // Instantiate our prefab and reparent the original object to ours
                            GameObject apObject = GameObject.Instantiate(prefab, transformObj.transform.position, transformObj.transform.rotation);
                            spawnedObj.transform.parent = apObject.transform;
                            spawnedObj.GetComponentInChildren<Collider>().enabled = false;

                            // Add the AP Swirlie to the item that appears on the "You Found" UI
                            string youFoundName = GetYouFoundName(obj.name);
                            Transform youFoundParent = ModInstance.YouFoundText.Find("You Found" + youFoundName);
                            if(youFoundParent != null)
                            {
                                Transform youFoundModel = youFoundParent.FindRecursive(obj.name);
                                if (youFoundModel != null)
                                {
                                    GameObject.Instantiate(prefab, youFoundModel.transform.position, youFoundModel.transform.rotation, youFoundModel);
                                }
                                else
                                {
                                    Plugin.BepinLogger.LogError("No 'You Found' object model found for: " + obj.name);
                                }
                            }
                            else
                            {
                                Plugin.BepinLogger.LogError("No 'You Found' parent found for: " + obj.name);
                            }
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

        private static string GetYouFoundName(string objectName)
        {
            switch(objectName)
            {
                case "SLEEPING MASK":
                    return " Sleep Mask";
                default:
                    string[] wordsInName = objectName.Split(" ");
                    string normalCapsName = "";
                    for (int i = 0; i < wordsInName.Length; i++)
                    {
                        normalCapsName += " " + wordsInName[i].Substring(0, 1).ToUpper() + wordsInName[i].Substring(1).ToLower();
                    }
                    return normalCapsName;
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
            string targetType = target.target.ToString();
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
}
