using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using BluePrinceArchipelago.Utils.Actions;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using LibCpp2IL;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace BluePrinceArchipelago
{
    public class UniqueItemManager
    {
        public List<UniqueItem> SpawnedItems = new List<UniqueItem>();

        public void OnItemSpawn(GameObject obj, string poolName, GameObject transformObj, GameObject spawnedObj)
        {
            if (Plugin.AssetBundle.Contains(obj.name) && !(Plugin.ModItemManager.GetUniqueItem(obj.name)?.IsUnlocked ?? true))
            {
                GameObject prefab = Plugin.AssetBundle.LoadAsset(obj.name).TryCast<GameObject>();

                // Instantiate our prefab and reparent the original object to ours
                GameObject apObject = GameObject.Instantiate(prefab, transformObj.transform.position, transformObj.transform.rotation);
                spawnedObj.transform.parent = apObject.transform;
                spawnedObj.GetComponentInChildren<Collider>().enabled = false;

                // Disable the Global Manager FSM states to not give this item in inventory
                string youFoundName = GetYouFoundName(obj.name);
                string pickupName = GetPickupName(obj.name);
                FsmState state = GetPickupState(pickupName);
                if (state != null)
                {
                    state.RemoveActionsOfType<ArrayListAdd>();
                    SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(obj.name));
                }
                else
                {
                    Logging.LogError($"No FSM state {obj.name.Trim().ToTitleCase() + " Pickup"} found for: {obj.name}");
                }

                // Make the necessary changes to the "You Found" UI
                Transform youFoundParent = ModInstance.YouFoundText.Find("You Found" + youFoundName);
                if (youFoundParent != null)
                {
                    // Add the AP Swirlie to the item that appears on the "You Found" UI
                    Transform youFoundModel = youFoundParent.FindRecursive(obj.name);
                    if (youFoundModel != null)
                    {
                        GameObject.Instantiate(prefab, youFoundModel.transform.position, youFoundModel.transform.rotation, youFoundModel);
                    }
                    else
                    {
                        Logging.LogError("No 'You Found' object model found for: " + obj.name);
                    }

                    // Add special text for what you found
                    Transform textGameObject = youFoundParent.Find("Text/GameObject");
                    if (textGameObject != null)
                    {
                        GameObject textPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("You Found Text Template");
                        GameObject textObject = GameObject.Instantiate(textPrefab, textGameObject.position, textGameObject.rotation, textGameObject.parent);

                        long locationid = Plugin.ArchipelagoClient.GetLocationFromName(obj.name + " First Pickup");
                        ScoutedItemInfo scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];
                        // Get the string of the item found
                        string playerName = scout.Player.Name;
                        string itemName = scout.ItemName;
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
                                if (child.name.ToLower().Contains("prescription"))
                                {
                                    prescFont = text.font;
                                }
                                else if (child.name.ToLower().Contains("first"))
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
                            if (i < 2)
                            {
                                itemWordList.Add(itemWords[i].ToUpper());
                            }
                            else
                            {
                                string lastWord = "";
                                while (i < itemWords.Length)
                                {
                                    lastWord += itemWords[i].ToUpper();
                                    i++;
                                    if (i < itemWords.Length)
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
                                    else if (child.name.StartsWith("Item Name"))
                                    {
                                        if (objectIndex >= itemWordList.Count)
                                        {
                                            GameObject.Destroy(child.gameObject);
                                            continue;
                                        }

                                        text.font = mainFont;
                                        text.text = itemWordList[objectIndex].Substring(1);
                                    }
                                    else if (child.name.StartsWith("Description"))
                                    {
                                        if (objectIndex == itemWordList.Count - 1)
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
                                        Logging.LogError("Something weird happened with the 'You Found Text' prefab (check its child objects?)");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Logging.LogError("No 'You Found' text found for: " + obj.name);
                    }
                }
                else
                {
                    Logging.LogError("No 'You Found' parent found for: " + obj.name);
                }
            }
            else
            {
                Logging.LogError("No AP Item found for: " + obj.name);
            }
        }
        public void OnDayEnd() {
            SpawnedItems = new List<UniqueItem>();
        }

        private string GetYouFoundName(string name) {
            switch (name)
            {
                case "SLEEPING MASK":
                    return " Sleep Mask";
                default:
                    string[] wordsInName = name.Split(" ");
                    string normalCapsName = "";
                    for (int i = 0; i < wordsInName.Length; i++)
                    {
                        normalCapsName += " " + wordsInName[i].Substring(0, 1).ToUpper() + wordsInName[i].Substring(1).ToLower();
                    }
                    return normalCapsName;
            }
        }
        public UniqueItem GetIfSpawned(string name) {
            foreach (UniqueItem item in SpawnedItems)
            {
                string[] nameparts = item.Name.Split(" ");
                foreach (string part in nameparts) { 
                    if (name.ToLower().Contains(part.ToLower()) && part.ToLower() != "pickup")
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private FsmState GetPickupState(string pickupName) {
            Logging.Log(pickupName);
            FsmState state = ModInstance.GlobalManager.GetState(pickupName);
            if (state != null) {
                return state;
            }
            return null;
        }
        private string GetPickupName(string name) {
            name = name.ToTitleCase();
            Logging.Log(name);
            switch (name) {
                case "Key Of Aries":
                    return "Key of Aries Pickup";
                default:
                    return name + " Pickup";
            }
        }
    }
}
