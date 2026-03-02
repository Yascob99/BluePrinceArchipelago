using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Runtime.InteropServices;
using PathologicalGames;
using System;
using System.Reflection;
using UnityEngine;
using TMPro;
using static UnityEngine.RectTransform;
using Il2CppSystem.Collections.Generic;
using LibCpp2IL;

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
                    if (obj)   // && object has not been found before
                    {
                        if(Plugin.AssetBundle.Contains(obj.name))
                        {
                            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>(obj.name);

                            // Instantiate our prefab and reparent the original object to ours
                            GameObject apObject = GameObject.Instantiate(prefab, transformObj.transform.position, transformObj.transform.rotation);
                            spawnedObj.transform.parent = apObject.transform;
                            spawnedObj.GetComponentInChildren<Collider>().enabled = false;

                            // Make the necessary changes to the "You Found" UI
                            string youFoundName = GetYouFoundName(obj.name);
                            Transform youFoundParent = ModInstance.YouFoundText.Find("You Found" + youFoundName);
                            if(youFoundParent != null)
                            {
                                // Add the AP Swirlie to the item that appears on the "You Found" UI
                                Transform youFoundModel = youFoundParent.FindRecursive(obj.name);
                                if (youFoundModel != null)
                                {
                                    GameObject.Instantiate(prefab, youFoundModel.transform.position, youFoundModel.transform.rotation, youFoundModel);
                                }
                                else
                                {
                                    Plugin.BepinLogger.LogError("No 'You Found' object model found for: " + obj.name);
                                }

                                // Add special text for what you found
                                Transform textGameObject = youFoundParent.Find("Text/GameObject");
                                if (textGameObject != null)
                                {
                                    GameObject textPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("You Found Text Template");
                                    GameObject textObject = GameObject.Instantiate(textPrefab, textGameObject.position, textGameObject.rotation, textGameObject.parent);

                                    // Get the string of the item found
                                    string playerName = "chaseoqueso";
                                    string itemName = "Mothwing Cloak";
                                    string description = "Hope it un-BK's them!";

                                    // Get correct font assets for our prefab
                                    TMP_FontAsset prescFont = null;
                                    TMP_FontAsset mainFont = null;
                                    TMP_FontAsset descFont = null;
                                    for (int i = 0; i < textGameObject.childCount; i++)
                                    {
                                        TextMeshPro text;
                                        Transform child = textGameObject.GetChild(i);
                                        if (child.TryGetComponent<TextMeshPro>(out text))
                                        {
                                            if(child.name.ToLower().Contains("prescription"))
                                            {
                                                prescFont = text.font;
                                            }
                                            else if(child.name.ToLower().Contains("first"))
                                            {
                                                mainFont = text.font;
                                            }
                                            else if (child.name.ToLower().Contains("description"))
                                            {
                                                descFont = text.font;
                                            }
                                        }
                                    }
                                    GameObject.Destroy(textGameObject.gameObject);

                                    // Break up the item name into exactly 3 strings
                                    List<String> itemWordList = new();
                                    string[] itemWords = itemName.Split(" ");
                                    for (int i = 0; i < Math.Min(3, itemWords.Length); i++)
                                    {
                                        if(i < 2)
                                        {
                                            itemWordList.Add(itemWords[i].ToUpper());
                                        }
                                        else
                                        {
                                            string lastWord = "";
                                            while(i < itemWords.Length)
                                            {
                                                lastWord += itemWords[i].ToUpper();
                                                i++;
                                                if(i < itemWords.Length)
                                                {
                                                    lastWord += " ";
                                                }
                                            }
                                            itemWordList.Add(lastWord);
                                        }
                                    }

                                    // Update all the fonts and words to be correct
                                    for (int i = 0; i < textObject.transform.childCount; i++)
                                    {
                                        TextMeshPro text;
                                        Transform child = textObject.transform.GetChild(i);
                                        if (child.TryGetComponent<TextMeshPro>(out text))
                                        {
                                            if (child.name == "Prescription")
                                            {
                                                text.font = prescFont;
                                                text.text = playerName + "'s";
                                            }
                                            else
                                            {
                                                int objectIndex = child.name[child.name.IndexOf("(") + 1].ParseDigit() - 1;
                                                Plugin.BepinLogger.LogMessage(objectIndex);
                                                Plugin.BepinLogger.LogMessage(itemWordList.Count);

                                                if (child.name.StartsWith("First Letter"))
                                                {
                                                    if (objectIndex >= itemWordList.Count)
                                                    {
                                                        GameObject.Destroy(child.gameObject);
                                                        continue;
                                                    }

                                                    text.font = mainFont;
                                                    text.text = itemWordList[objectIndex].Substring(0, 1);
                                                }
                                                else if(child.name.StartsWith("Item Name"))
                                                {
                                                    if (objectIndex >= itemWordList.Count)
                                                    {
                                                        GameObject.Destroy(child.gameObject);
                                                        continue;
                                                    }

                                                    text.font = mainFont;
                                                    text.text = itemWordList[objectIndex].Substring(1);
                                                }
                                                else if(child.name.StartsWith("Description"))
                                                {
                                                    if(objectIndex == itemWordList.Count - 1)
                                                    {
                                                        text.font = descFont;
                                                        text.text = description;
                                                    }
                                                    else
                                                    {
                                                        GameObject.Destroy(child.gameObject);
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    Plugin.BepinLogger.LogError("Something weird happened with the 'You Found Text' prefab (check its child objects?)");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Plugin.BepinLogger.LogError("No 'You Found' text found for: " + obj.name);
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
