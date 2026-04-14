using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using LibCpp2IL;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Rewired.UI.ControlMapper.ControlMapper;


namespace BluePrinceArchipelago.Core
{
    public class UniqueItem(string name, GameObject gameObject, bool isUnlocked, bool isPreSpawn = true) : ModItem(name, gameObject, isUnlocked)
    {

        private bool _IsUnique = true;
        public new bool IsUnique
        {
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }
        private bool _IsPrespawn = isPreSpawn;

        public bool IsPrespawn { get; set; }

        private bool _HasBeenFound = false;

        public bool HasBeenFound
        {
            get { return _HasBeenFound; }
            set
            {
                // Send the item found event on the first time it is found.
                if (!_HasBeenFound && value)
                {
                    ModInstance.ModEventHandler.OnFirstFound(this);
                    _HasBeenFound = value;
                }
                // No changes to value once the item has been found once, or if someone is trying to set this to false some reason.
            }
        }

        public void RemoveFromPool()
        {
            //If item has been found and is not unlocked, remove it from the pool. Otherwise Vanilla behavior.
            if (HasBeenFound && !IsUnlocked)
            {
                if (IsPrespawn && ModItemManager.PreSpawn.Contains(GameObj))
                {
                    ModItemManager.PreSpawn.Remove(GameObj, "GameObject");
                }
                if (ModItemManager.EstateItems.Contains(GameObj))
                {
                    ModItemManager.EstateItems.Remove(GameObj, "GameObject");
                }
                ModItemManager.PickedUp.Add(GameObj, "GameObject");
            }
        }

        public override void AddItemToInventory()
        {
            bool isSpawned = false;
            if (!IsUnlocked)
            {
                IsUnlocked = true;
            }
            if (GameObj == null)
            {
                if (_IsPrespawn)
                {
                    GameObj = Plugin.ModItemManager.GetPreSpawnItem(Name);
                }
            }
            if (Plugin.UniqueItemManager.SpawnedItems.Contains(this))
            {
                isSpawned = true;
            }

            // If the item is spawned or is not in the prespawn list.
            if (Plugin.ModItemManager.IsItemSpawnable(GameObj, isSpawned ? false : IsPrespawn))
            {
                string iconName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Name.ToLower()) + " Icon(Clone)001";
                GameObject icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                // Some icons are spelled with icon as lower case
                if (icon == null)
                {
                    iconName = iconName.ToTitleCase() + " icon(Clone)001";
                    icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                }
                // Some icons are spelled without the word icon.
                if (icon == null)
                {
                    iconName = iconName.ToTitleCase() + " (Clone)001";
                    icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                }
                PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                if (icon != null && InventoryIcons != null)
                {
                    if (IsPrespawn)
                    {
                        ModItemManager.PreSpawn.Remove(GameObj, "GameObject");
                    }
                    // Re-enable the logic that adds the item to inventory. (will not cause issues if already enabled).
                    FsmState state = Plugin.UniqueItemManager.GetPickupState(Name);
                    if (state != null)
                    {
                        state.EnableActionsOfType<ArrayListAdd>();
                        if (Plugin.UniqueItemManager.ComissaryStates.Keys.Contains(Name))
                        {
                            //Re-enable commisary purchases of the item.
                            Plugin.UniqueItemManager.EnableCommissaryPurchase(this, Plugin.UniqueItemManager.ComissaryStates[Name]);
                        }
                        ModItemManager.PickedUp.Add(GameObj, "GameObject");
                        InventoryIcons.Add(icon, "GameObject");
                        ArchipelagoConsole.LogMessage($"Added {Name} to inventory.");
                    }
                }
            }
        }
    }


        public class UniqueItemManager
        {
            public List<UniqueItem> SpawnedItems = new List<UniqueItem>();

            // A Map of item names to the states in the Comissary.
            public Dictionary<string, string> ComissaryStates = new Dictionary<string, string>{
            {"MAGNIFYING GLASS", "Mag Glass" },
            {"SHOVEL", "Shovel Purchase"},
            {"SALT SHAKER", "Salt Shaker Purchase"},
            {"COMPASS", "Compass Purchase"},
            {"SLEDGE HAMMER", "Sledge Hammer Purchase"},
            {"SLEEPING MASK", "Sleep Mask Purchase"},
            {"RUNNING SHOES", "Running Shoes Purchase"},
            {"METAL DETECTOR", "MEtal Detector Purchase"}
         };

            public void OnItemSpawn(GameObject obj, string poolName, GameObject transformObj, GameObject spawnedObj)
            {
                UniqueItem item = Plugin.ModItemManager.GetUniqueItem(obj.name);
                //Check if Connected in before replacing items.
                if (ArchipelagoClient.Authenticated)
                {
                    if (item != null)
                    {
                        // If the item has not been found before.
                        if (!item.HasBeenFound)
                        {
                            Logging.Log("Item is an AP object attempting to replace object.");
                            ReplaceWithAPItem(obj, transformObj, spawnedObj, item);
                        }
                    }
                }
                else
                {
                    if (item != null)
                    {
                        FsmState state = GetPickupState(obj.name);
                        if (state != null)
                        {
                            if (item.IsUnlocked)
                            {
                                //Re-enable the previously disabled actions.
                                state.EnableActionsOfType<ArrayListAdd>();
                            }
                        }
                    }
                }
            }
            public void ReplaceCommissaryItemsWithAP()
            {
                foreach (var item in ComissaryStates)
                {
                    UniqueItem uniqueItem = Plugin.ModItemManager.GetUniqueItem(item.Key);
                    if (!uniqueItem.HasBeenFound)
                    {
                        ReplaceCommissaryPurchase(uniqueItem, item.Value);
                        //This likely will not work cleanly without a massive rewrite. I plan on just stopping the "you found message."
                        //ReplaceComissaryItemWithAP(uniqueItem, item.Value);
                    }
                    else if (uniqueItem.IsUnlocked)
                    {
                        EnableCommissaryPurchase(uniqueItem, item.Value);
                    }
                }
            }
            private void ReplaceComissaryItemWithAP(UniqueItem item, string stateName)
            {
                FsmState state = ReplaceCommissaryPurchase(item, stateName);
                if (state != null)
                {
                    ReplaceYouFoundText(state, item);
                }
            }

            //Finds the associated Pickup State and replaces the item.
            private FsmState ReplacePickup(UniqueItem item)
            {
                FsmState state = GetPickupState(item.Name);
                if (state != null)
                {
                    //If the item is not unlocked, prevent it from being added to inventory.
                    if (!item.IsUnlocked)
                    {
                        //Disable the actions that add the item to inventory.
                        state.DisableActionsOfType<ArrayListAdd>();
                    }
                    SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(item.Name));
                    return state;
                }
                // If the item pickup state was not found output an error.
                Logging.LogError($"No FSM state {item.Name.Trim().ToTitleCase() + " Pickup"} found for: {item.Name}");
                return null;
            }

            private FsmState ReplaceCommissaryPurchase(UniqueItem item, string stateName)
            {
                FsmState state = ModInstance.CommissaryMenu.GetState(stateName);
                if (state != null)
                {
                    //If the item is not unlocked, prevent it from being added to inventory.
                    if (!item.IsUnlocked)
                    {
                        //Disable the actions that add the item to inventory.
                        state.DisableActionsOfType<ArrayListAdd>();
                        //Disables the You found Text (for now).
                        state.DisableFirstActionOfType<ActivateGameObject>();
                    }
                    SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(item.Name));
                    return state;
                }
                // If the item pickup state was not found output an error.
                Logging.LogError($"No FSM state {stateName} found for: {item.Name}");
                return null;
            }
            public void EnableCommissaryPurchase(UniqueItem item, string stateName)
            {
                FsmState state = ModInstance.CommissaryMenu.GetState(stateName);
                if (state != null)
                {
                    //If the item is not unlocked, prevent it from being added to inventory.
                    if (!item.IsUnlocked)
                    {
                        //Disable the actions that add the item to inventory.
                        state.EnableActionsOfType<ArrayListAdd>();
                        //Disables the You found Text (for now).
                        state.DisableFirstActionOfType<ActivateGameObject>();
                    }
                    SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(item.Name));
                    return;
                }
            }

            private void ReplaceYouFoundText(FsmState state, UniqueItem item, GameObject prefab = null)
            {
                if (prefab == null)
                {
                    if (Plugin.AssetBundle.Contains(item.Name))
                    {
                        prefab = Plugin.AssetBundle.LoadAsset(item.Name).TryCast<GameObject>();
                    }
                }
                Transform youFoundParent = GetYouFoundParent(state);
                // Make the necessary changes to the "You Found" UI
                if (youFoundParent != null)
                {
                    // Add the AP Swirlie to the item that appears on the "You Found" UI
                    //Find the model in the You Found UI (location differs item to item).
                    Transform youFoundModel = youFoundParent.FindRecursive(item.Name);
                    if (youFoundModel != null)
                    {
                        //If the custom model exists
                        if (prefab != null)
                        {
                            // Insantiate the AP item in the you found model's location.
                            GameObject.Instantiate(prefab, youFoundModel.transform.position, youFoundModel.transform.rotation, youFoundModel);
                        }
                    }
                    else
                    {
                        Logging.LogError("No 'You Found' object model found for: " + item.Name);
                    }

                    // Add special text for what you found
                    // Find the transform of the text.
                    Transform textGameObject = youFoundParent.Find("Text/GameObject");
                    if (textGameObject != null)
                    {
                        //Load and instantiate our custom "You Found" Text Template
                        GameObject textPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("You Found Text Template");
                        GameObject textObject = GameObject.Instantiate(textPrefab, textGameObject.position, textGameObject.rotation, textGameObject.parent);

                        // Get the location ID of our it's first pickup.
                        long locationid = Plugin.ArchipelagoClient.GetLocationFromName(item.Name + " First Pickup");
                        // Find the the details of the item that will be sent on pickup.
                        ScoutedItemInfo scout = null;
                        if (locationid != -1)
                        {
                            scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];
                        }
                        // Get the variables for creating our custom pickup message.
                        string playerName = scout?.Player?.Name ?? "";
                        string itemName = scout?.ItemName ?? "";
                        //TODO add logic for the descriptions to be different based on item importance.
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
                                // Get the font for the first part of the text.
                                if (child.name.ToLower().Contains("prescription"))
                                {
                                    prescFont = text.font;
                                }
                                // Get the font for the second part of the text.
                                else if (child.name.ToLower().Contains("first"))
                                {
                                    mainFont = text.font;
                                }
                                // Get the font for the third part of the text.
                                else if (child.name.ToLower().Contains("description"))
                                {
                                    descFont = text.font;
                                }
                            }
                        }
                        // Destroy the instantiated GameObject
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
                                // Add the name of the player who owns the item being spawned.
                                if (child.name == "Prescription")
                                {
                                    text.font = prescFont;
                                    // Handle names ending in s with proper apostrophe convention
                                    if (playerName.ToLower().EndsWith('s'))
                                    {
                                        text.text = playerName + "'";
                                    }
                                    else
                                    {
                                        text.text = playerName + "'s";
                                    }
                                }
                                else
                                {
                                    int objectIndex = child.name[child.name.IndexOf("(") + 1].ParseDigit() - 1;

                                    // The first letter of each word in the item name is handled differently.
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
                                    // The rest of the word in the item name.
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
                                    // Handle the item description.
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
                                    //Something else got mixed into the item prefab.
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
                        Logging.LogError("No 'You Found' text found for: " + item.Name);
                    }
                }
                else
                {
                    Logging.LogError("No 'You Found' parent found for: " + item.Name);
                }
            }


            private void ReplaceWithAPItem(GameObject obj, GameObject transformObj, GameObject spawnedObj, UniqueItem item)
            {
                GameObject prefab = null;
                //If we currently have a prefab for this item, instantiate and replace the spawned model with our object. 
                if (Plugin.AssetBundle.Contains(obj.name))
                {
                    prefab = Plugin.AssetBundle.LoadAsset(obj.name).TryCast<GameObject>();
                    if (prefab != null)
                    {
                        // Instantiate our prefab and reparent the original object to ours
                        GameObject apObject = GameObject.Instantiate(prefab, transformObj.transform.position, transformObj.transform.rotation);
                        spawnedObj.transform.parent = apObject.transform;
                        spawnedObj.GetComponentInChildren<Collider>().enabled = false;
                    }
                }

                // Disable the Global Manager FSM states to not give this item in inventory
                FsmState state = ReplacePickup(item);
                if (state != null)
                {
                    ReplaceYouFoundText(state, item, prefab);
                }
            }

            public void EndOfDay()
            {
                //Reset the list of spawned items.
                SpawnedItems = new List<UniqueItem>();
            }
            public void StartOfDay()
            {
                RemoveItemsFromPool();
                ReplaceCommissaryItemsWithAP();
            }

            private void RemoveItemsFromPool()
            {
                foreach (UniqueItem item in ModItemManager.UniqueItemList)
                {
                    //If the item has been found once, remove it from
                    item.RemoveFromPool();
                }
            }

            //Finds the "You Found" Event based on the what "You Found" is called in the GlobalManager Pickup FSM State. Returns null if not found.
            private Transform GetYouFoundParent(FsmState pickupState)
            {
                if (pickupState != null)
                {
                    // Find the First Action of the type "ActivateGameObject"
                    ActivateGameObject youFoundEvent = pickupState.GetFirstActionOfType<ActivateGameObject>();
                    if (youFoundEvent != null)
                    {
                        //Find the GameObject the event would activate and return it's transform.
                        return youFoundEvent?.gameObject?.gameObject?.Value?.transform;
                    }

                }
                return null;
            }
            // Checks if the item has been picked up before.
            public UniqueItem GetIfSpawned(string name)
            {
                foreach (UniqueItem item in SpawnedItems)
                {
                    string[] nameparts = item.Name.Split(" ");
                    foreach (string part in nameparts)
                    {
                        if (name.ToLower().Contains(part.ToLower()) && part.ToLower() != "pickup")
                        {
                            return item;
                        }
                    }
                }
                return null;
            }

            // Finds the state in the Global Manager associated with the given item's pickup. Returns null if not found.
            public FsmState GetPickupState(string name)
            {
                // Fixes a name difference for the vault keys and puts name into lower case.
                name = name.ToLower().Replace("vault", "safety deposit");
                // Check each Global Transition in the Global Manager.
                foreach (FsmTransition transition in ModInstance.GlobalManager.FsmGlobalTransitions)
                {
                    // If the transition's event name contains the item name it's the transition we want.
                    if (transition.EventName.ToLower().Contains(name))
                    {
                        //Return the state the transition found goes to.
                        return transition.ToFsmState;
                    }
                }
                return null;
            }
        }
    }
