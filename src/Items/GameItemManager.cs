using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppTMPro;
#endif
using System.Collections.Generic;
using UnityEngine;
using Il2CppSystem.Collections;


namespace BluePrinceArchipelago.Items
{ 
    /// <summary>
    ///     A manager for common operations associated with Unique Items.
    /// </summary>
    public sealed class GameItemManager
    {

        private List<GameUniqueItem> _gameUniqueItemList = [];

        private readonly List<GameUniqueItem> _spawnedItems = [];
        public static PlayMakerArrayListProxy PreSpawn = new();
        public static PlayMakerArrayListProxy EstateItems = new();
        public static PlayMakerArrayListProxy PickedUp = new();
        public static PlayMakerArrayListProxy CoatCheck = new();
        public static PlayMakerArrayListProxy UsedItems = new();
        public static PlayMakerArrayListProxy InventoryItems = new();
        public static UpgradeDisks UpgradeDisks = new UpgradeDisks(null);


        private GameItemManager() {}

        private static GameItemManager _instance;

        public static GameItemManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameItemManager();
            }
            return _instance;
        }
        /// <summary>
        ///     Code to be run on end of day.
        /// </summary>
        public void EndOfDay()
        {
            foreach (GameUniqueItem item in _spawnedItems)
            {
                item.EndOfDay();
            }
            _spawnedItems.Clear();
        }

        /// <summary>
        ///     Code to be run at the start of day.
        /// </summary>
        public void StartOfDay()
        {
            foreach (GameUniqueItem item in _gameUniqueItemList)
            {
                item.StartOfDay();   
            }
            // Run upgrade disk start of day code if Upgrade Disk Sanity is on.
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                UpgradeDisks.StartOfDay();
            }
        }

        public GameUniqueItem GetGameUniqueItem(string itemName)
        {
            foreach (GameUniqueItem uniqueItem in _gameUniqueItemList)
            {
                if (uniqueItem.Name == itemName)
                {
                    return uniqueItem;
                }
            }
            throw new KeyNotFoundException($"Could not find a game item corresponding to {itemName}");
        }


        /// <summary>
        ///     Loads all the ArrayLists which the game stores Unique Items in.
        /// </summary>
        public static void LoadInventories()
        {

            PreSpawn = GameObject.Find("__SYSTEM/Inventory/Inventory (PreSpawn)")?.GetArrayListProxy("PreSpawn");
            EstateItems = GameObject.Find("__SYSTEM/Inventory/Inventory (EstateItems)")?.GetArrayListProxy("EstateItems");
            PickedUp = GameObject.Find("__SYSTEM/Inventory/Inventory (PickedUp)")?.GetArrayListProxy("PickedUp");
            CoatCheck = GameObject.Find("__SYSTEM/Inventory/Inventory (CoatCheck)")?.GetArrayListProxy("CoatCheck");
            UsedItems = GameObject.Find("__SYSTEM/Inventory/Inventory (UsedItems)")?.GetArrayListProxy("UsedItems");
            InventoryItems = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/InventoryGameobjects")?.GetArrayListProxy("InventoryGameobjects");
            UpgradeDisks.GameObj = GameObject.Find("__SYSTEM/Upgrade Disks");
        }

        /// <summary>
        ///     Replaces Unique items (excluding some special cases) with their AP Version if applicable.
        /// </summary>
        public void ReplaceUniqueItemsWithAP()
        {
            foreach (GameUniqueItem item in _gameUniqueItemList)
            {
                item.ReplaceUniqueItemWithArchipelagoItem();
            }
        }

        /// <summary>
        ///     Replaces all the Unique Item with their AP versions if applicable.
        /// </summary>
        public void ReplaceItemsWithAP()
        {
            ReplaceUniqueItemsWithAP();
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                ReplaceUpgradeDisksWithAP();
            }
        }


        /// <summary>
        ///     Reloads the GameObjects of all registered items (usually on scene transitions/reloads).
        /// </summary>
        public void ReloadGameObjects()
        {
            foreach (GameUniqueItem item in _gameUniqueItemList)
            {
                string name = ITEM.GetGameObjectName(item.Name);
                GameObject gameObj = GetInventoryItem(name);
                if (gameObj == null)
                {
                    gameObj = GameObjectExtensions.FindGameObject(name);
                }
                item.UpdateGameObject(gameObj);
            }
        }


        /// <summary>
        ///     Returns the Inventory Icon name for various items that differ from their usual name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The name of the icon</returns>
        public static string GetIconName(string name) {
            name = name.ToTitleCase();
            switch (name){
                case "Cabinet Key 1":
                    return "Cabinet Key Icon";
                case "Cabinet Key 2":
                    return "Cabinet Key Icon";
                case "Cabinet Key 3":
                    return "Cabinet Key Icon";
                case "Prism Key_0":
                    return "Prism Key";
                case "Electromagnet":
                    return "Powered Electro Magnet Icon";
                case "Key 8":
                    return "Key 8";
                case "Lucky Rabbit's Foot":
                    return "Lucky rabbit's foot Icon";
                case "Salt Shaker":
                    return "Salt Icon";
                case "Vault Key 149":
                    return "Vault Key 149 icon";
                case "Vault Key 233":
                    return "Vault Key 233 icon";
                case "Vault Key 304":
                    return "Vault Key 304 icon";
                case "Vault Key 370":
                    return "Vault Key 370 icon";
                default:
                    return name + " Icon";
            }
        }

        /// <summary>
        ///     Lists which items exist in a given pool.
        /// </summary>
        /// <param name="listType">The pool to list items from.</param>
        /// <returns>The list of items in the pool as a string.</returns>
        public static string ListItems(string listType)
        {
            if (listType == null)
                return "";
            ArrayList itemList;
            if (listType.ToLower() == "prespawn")
            {
                itemList = PreSpawn.arrayList;
            }
            else if (listType.ToLower() == "estateitems")
            {
                itemList = EstateItems.arrayList;
            }
            else if (listType.ToLower() == "pickedup")
            {
                itemList = PickedUp.arrayList;
            }
            else if (listType.ToLower() == "coatcheck")
            {
                itemList = CoatCheck.arrayList;
            }
            else if (listType.ToLower() == "useditems")
            {
                itemList = UsedItems.arrayList;

            }
            else
            {
                return "";
            }
            string output = "";
            foreach (var pickedupItem in itemList)
            {
                GameObject itemAsGO = pickedupItem.TryCast<GameObject>();
                if (pickedupItem != null)
                {
                    output += itemAsGO.name;
                    output += "\n";
                }
            }
            return output;
        }

        public void RegisterItems()
        {
            _gameUniqueItemList =
            [
                //Keys
                new GameUniqueItem(ITEM.CAR_KEYS, true, false, ["Locksmith"]),
                new GameUniqueItem(ITEM.KEYCARD),
                new GameUniqueItem(ITEM.SILVER_KEY, true, false, ["Locksmith", "Dig"]),
                new GameUniqueItem(ITEM.KEY_8, true, true),
                new GameUniqueItem(ITEM.BASEMENT_KEY),
                new GameUniqueItem(ITEM.VAULT_KEY_149, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.VAULT_KEY_233, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.VAULT_KEY_304, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.VAULT_KEY_370, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.DIARY_KEY, true, true),
                new GameUniqueItem(ITEM.PRISM_KEY, false, false, ["Locksmith"]),
                new GameUniqueItem(ITEM.KEY_OF_ARIES, false),
                new GameUniqueItem(ITEM.SECRET_GARDEN_KEY, true, false, ["Locksmith", "Dig"]),
                new GameUniqueItem(ITEM.MICROCHIP_SMALL, true, true),
                new GameUniqueItem(ITEM.MICROCHIP_MIDDLE_SIZED, true, true),
                new GameUniqueItem(ITEM.MICROCHIP_LARGE, true, true),
                new GameUniqueItem(ITEM.CABINET_KEY_PATIO, true, true),
                new GameUniqueItem(ITEM.CABINET_KEY_LAUNDRY, true, true),
                new GameUniqueItem(ITEM.CABINET_KEY_TUNNEL, true, true),
                
                //Basic
                new GameUniqueItem(ITEM.BATTERY_PACK),
                new GameUniqueItem(ITEM.BROKEN_LEVER, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.MAGNIFYING_GLASS, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.METAL_DETECTOR, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.SHOVEL, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.SLEDGE_HAMMER, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.TELESCOPE, true, true),
                new GameUniqueItem(ITEM.RUNNING_SHOES, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.SALT_SHAKER, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.SLEEPING_MASK, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.COIN_PURSE),
                new GameUniqueItem(ITEM.COUPON_BOOK),
                new GameUniqueItem(ITEM.LOCK_PICK_KIT, true, false, ["Locksmith"]),
                new GameUniqueItem(ITEM.LUCKY_RABBITS_FOOT),
                new GameUniqueItem(ITEM.TREASURE_MAP),
                new GameUniqueItem(ITEM.STOPWATCH, true, false, ["Dig"]),
                new GameUniqueItem(ITEM.REPELLENT, false, true),
                new GameUniqueItem(ITEM.WATERING_CAN, false, true),
                new GameUniqueItem(ITEM.LUNCH_BOX, false, true),
                new GameUniqueItem(ITEM.CURSED_EFFIGY, false, true),
                new GameUniqueItem(ITEM.CROWN),
                new GameUniqueItem(ITEM.PAPER_CROWN, true, true),
                new GameUniqueItem(ITEM.GEAR_WRENCH, true, true),
                new GameUniqueItem(ITEM.COMPASS, true, false, ["Commissary"]),
                new GameUniqueItem(ITEM.HALL_PASS, true, true),

                //Workshop
                new GameUniqueItem(ITEM.ELECTROMAGNET, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.LUCKY_PURSE, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.PICK_SOUND_AMPLIFIER, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.BURNING_GLASS, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.DETECTOR_SHOVEL, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.DOWSING_ROD, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.JACK_HAMMER, true, false, ["Workshop"]),
                new GameUniqueItem(ITEM.POWER_HAMMER, true, false, ["Workshop"]),
                
                //Showroom
                new GameUniqueItem(ITEM.CHRONOGRAPH, true, false),
                new GameUniqueItem(ITEM.EMERALD_BRACELET, true, false),
                new GameUniqueItem(ITEM.MASTER_KEY, true, false, ["Locksmith"]),
                new GameUniqueItem(ITEM.MOON_PENDANT, true, false),
                new GameUniqueItem(ITEM.ORNATE_COMPASS, true, false),
                new GameUniqueItem(ITEM.SILVER_SPOON, true, false),
                
                //Armory
                new GameUniqueItem(ITEM.MORNING_STAR, true, false, ["Armory"]),
                new GameUniqueItem(ITEM.THE_AXE, true, false, ["Armory"]),
                new GameUniqueItem(ITEM.KNIGHTS_SHIELD, true, false, ["Dig", "Armory"]),
                new GameUniqueItem(ITEM.TORCH, true, false, ["Armory"]),
            ];
        }

        /// <summary>
        ///     Gets the game object for the Inventory Icon of of a given Unique Item.
        /// </summary>
        /// <param name="name">The name of the Unique Item</param>
        /// <returns>The GameObject of the icon</returns>
        public static GameObject GetIconGameObject(string name) {
            name = GetIconName(name);
            PlayMakerArrayListProxy Inventory = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/InventoryIconMeshes").GetComponent<PlayMakerArrayListProxy>();
            for (int i=0; i < Inventory.arrayList.Count; i++)
            {
                GameObject child = Inventory.arrayList[i].TryCast<GameObject>();
                if (child.name.Trim() == name.Trim()) {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        ///     Finds the Parent Transform of the you found event based on the provided state.
        /// </summary>
        /// <param name="pickupState">The pickup state of the item in the GlobalFsm</param>
        /// <returns>The Parent Transform</returns>
        public Transform GetYouFoundParent(FsmState pickupState)
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

        /// <summary>
        ///     Checks if the item has been picked up before.
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <returns>True if item is spawned, false if not.</returns>
        public GameUniqueItem GetIfSpawned(string name)
        {
            foreach (GameUniqueItem item in _spawnedItems)
            {
                string[] nameparts = item.Name.Split(" ");
                foreach (string part in nameparts)
                {
                    
                    if (name.ToLower().Contains(part.ToLower()) && part.ToLower() != "pickup")
                    {
                        Logging.Log(item.Name, "Events");
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Finds the state in the Global Manager associated with the given item's pickup. Returns null if not found.    
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <returns>The FsmState of the item's pickup handling.</returns>
        public static FsmState GetPickupState(string name)
        {
            // Fixes a name differences
            name = name.ToLower().Replace("vault", "saftey deposit").Replace("rabbit's", "rabbbit's").Replace(" kit", "").Replace("_0", "").Replace("lunch", "luch");
            if (name.Contains("cabinet key")) {
                if (name.Contains("1"))
                {
                    name = "cabinet key";
                }
                else if (name.Contains("3")) {
                    name = "cabinet key 5";
                }
            }
            // Check each Global Transition in the Global Manager.
            FsmTransition[] GlobalTransitions = ModInstance.GlobalManager?.FsmGlobalTransitions;
            if (GlobalTransitions != null) {
                FsmTransition[] transitions = new FsmTransition[ModInstance.GlobalManager?.FsmGlobalTransitions?.Count ?? 0];
                GlobalTransitions.CopyTo(transitions, 0);
            foreach (FsmTransition transition in transitions)
                {
                    // If the transition's event name contains the item name it's the transition we want.
                    if (transition.EventName.ToLower().Contains(name))
                    {
                        // Treasure Map requires going 1 state deeper
                        if (name == ITEM.TREASURE_MAP.ToLower()) {
                            ModInstance.GlobalManager.GetBoolVariable("Treasure Already").Value = true;
                            return ModInstance.GlobalManager.GetState("Treasure Map Pickup");
                        } 
                        //Return the state the transition found goes to.
                        return transition.ToFsmState;
                    }
                }
            }
            Logging.LogWarning($"Failed to Get pickup state for: {name}");
            return null;
        }

        /// <summary>
        ///     Checks if the item is currently spawnable based on the state of inventories.
        /// </summary>
        /// <param name="item">The GameObject of the item check.</param>
        /// <param name="isPrespawn">If the item is normally in the prespawn list.</param>
        /// <returns>True if the item should be spawnable.</returns>
        public static bool IsItemSpawnable(GameObject item, bool isPrespawn = true)
        {
            if (CoatCheck.Contains(item))
            {
                return false;
            }
            else if (EstateItems.Contains(item))
            {
                return false;
            }
            else if (UsedItems.Contains(item))
            {
                return false;
            }
            else if (PickedUp.Contains(item))
            {
                return false;
            }
            else if (PreSpawn.Contains(item))
            {
                return true;
            }
            else if (!isPrespawn)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets an item from the inventory item list.   
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The GameObject of the item.</returns>
        public static GameObject GetInventoryItem(string itemName)
        {
            for (int i = 0; i < InventoryItems.GetCount(); i++)
            {
                GameObject invItem = InventoryItems.arrayList[i].TryCast<GameObject>();
                if (invItem != null)
                {
                    if (invItem.name.Trim().ToLower() == itemName.ToLower().Trim())
                    {
                        return invItem;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets an item that the player has picked up.
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The GameObject of the item.</returns>
        public static GameObject GetPickedUpItem(string itemName)
        {
            for (int i = 0; i < PickedUp.GetCount(); i++)
            {
                GameObject pickedupItem = PickedUp.arrayList[i].TryCast<GameObject>();
                if (pickedupItem != null)
                {
                    if (pickedupItem.name.Trim().ToLower() == itemName.ToLower())
                    {
                        return pickedupItem;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Makes the player lose a random item if they have an item.    
        /// </summary>
        public static void LoseRandomItem()
        {
            //We don't care if this fails, since it's a trap, and I'm too lazy to handle the edgecase where you are not in a run, and you spawn with an item.
            int count = PickedUp.arrayList.Count;
            if (count > 0 && ModInstance.IsInRun)
            {
                int index = UnityEngine.Random.Range(0, count);
                PickedUp.RemoveAt(index);
            }
        }


        /// <summary>
        ///     Replaces upgrade disks with their AP version (if applicable).
        /// </summary>
        public static void ReplaceUpgradeDisksWithAP()
        {
            GameObject prefab = ModInstance.Prefabs.GetChild("UPGRADE DISK");
            if (prefab != null)
            {
                for (int i = 1; i < 17; i++)
                {
                    GameObject spawnObj = null;
                    // Get the APswirly Component of the Prefab and reparent it to the spawn prefab.
                    if (i < 10)
                    {
                        spawnObj = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)00{i}")?.gameObject;
                    }
                    else
                    {
                        spawnObj = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)0{i}")?.gameObject;
                    }
                    if (spawnObj != null)
                    {
                        GameObject APGO = GameObject.Instantiate(prefab, spawnObj.transform.position, spawnObj.transform.rotation);
                        // Get the APswirly Component of the prefab
                        GameObject APswirly = APGO?.transform?.GetChild(0)?.gameObject;
                        if (APswirly != null)
                        {
                            APswirly.transform.parent = spawnObj.transform.GetChild(0);
                            GameObject.Destroy(APGO);
                        }
                    }
                }

                ReplaceUpgradeDiskNotifications();
            }
        }

        /// <summary>
        ///     Replaces the You Found and You bought Message for the Upgrade disk with the Commissary Message. Will Replace the others programmatically. 
        /// </summary>
        public static void ReplaceUpgradeDiskNotifications()
        {
            int j = 0;
            GameObject item = null;
            foreach (string location in UpgradeDisks.ItemNames)
            {
                j++;
                // Skips Trading Post Trade Disk, which has no normal pickup location.
                if (j != 14)
                {
                    if (j < 10)
                    {
                        item = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)00{j}")?.gameObject;
                    }
                    else
                    {
                        item = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)0{j}")?.gameObject;
                    }
                    string scoutname = $"Upgrade Disk - {location.Replace("LADYSHIPS", "LADYSHIP\'s").Replace("AND ", "& ").Replace("FOUNDATION", "THE FOUNDATION").ToTitleCase()}";
                    GameObject You___Message = GameObject.Find("UI OVERLAY CAM/You Found Text/You Found Upgrade Disk - " + location.ToTitleCase());
                    if (location.ToTitleCase() == "Commissary")
                    {
                        You___Message = GameObject.Find("UI OVERLAY CAM/You Found Text/You Bought Upgrade Disk - " + location.ToTitleCase());
                    }
                    if (You___Message != null)
                    {
                        Transform itemModel = You___Message.transform.FindRecursive("Floppy Disk", true);
                        if (itemModel != null)
                        {
                            //Instantiate a the AP Object at the original's position
                            GameObject APGO = GameObject.Instantiate(item.transform.GetChild(0).gameObject, itemModel.position, itemModel.rotation, itemModel.parent);
                            APGO.transform.localScale = itemModel.localScale;
                            APGO.name = itemModel.name;
                            itemModel.gameObject.DestroyAllChildren();
                            APGO.MoveChildrenTo(itemModel.gameObject);

                            GameObject.Destroy(APGO);

                            //Import the template Text Prefab.  
                            GameObject textPrefab = ModInstance.Prefabs.GetChild("You Found Text Template");

                            // Get the location ID of our first pickup.
                            long locationid = Plugin.ArchipelagoClient.GetLocationFromName(scoutname);
                            // Find the the details of the item that will be sent on pickup.
                            ScoutedItemInfo scout = null;

                            if (locationid != -1)
                            {
                                if (ArchipelagoClient.ServerData.LocationItemMap.ContainsKey(locationid))
                                {
                                    scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];
                                }
                            }

                            // Get the variables for creating our custom pickup message.
                            string playerName = scout?.Player?.Name ?? "";
                            //Check if item is being used.
                            if (playerName != "")
                            {
                                string scoutItemName = scout?.ItemName ?? "";
                                //TODO add logic for the descriptions to be different based on item importance.
                                Logging.Log($"scoutItemName");
                                string description = "";

                                string[] itemWords = scoutItemName.Split(" ");
                                if (itemWords.Length < 4)
                                {
                                    scoutItemName = string.Join("\n", itemWords);
                                }
                                else
                                {
                                    scoutItemName = scoutItemName.Minragged();
                                }
                                int FirstLetterCount = 0;
                                int ItemNameCount = 0;
                                int DescriptionCount = 0;
                                // Update all the fonts and words to be correct

                                Transform textObjects = You___Message.transform.Find("Text/GameObject");

                                GameObject textObject = GameObject.Instantiate(textPrefab, textObjects.position, textObjects.rotation);
                                Transform FirstFirstLetter = null;
                                Transform FirstItemName = null;
                                //Get rid of the original Text.
                                Transform Prescription = textObject.transform.FindChild("Prescription");
                                Transform Description = textObject.transform.FindChild($"Description (2)");

                                TextMeshPro text = null;
                                List<GameObject> toDestroy = new List<GameObject>();
                                for (int i = 0; i < textObjects.transform.childCount; i++)
                                {

                                    Transform textChild = textObjects.gameObject.transform.GetChild(i);
                                    if (textChild.TryGetComponent<TextMeshPro>(out text))
                                    {
                                        // Add the name of the player who owns the item being spawned.
                                        if (textChild.name.Contains("Prescription"))
                                        {
                                            textChild.name = "Prescription";
                                            textChild.SetLocalPositionAndRotation(Prescription.localPosition, Prescription.localRotation);
                                            textChild.transform.localScale = Prescription.transform.localScale;

                                            // Handle names ending in s with proper apostrophe convention
                                            if (playerName.ToLower().EndsWith('s'))
                                            {
                                                text.text = $"{playerName}'";
                                            }
                                            else
                                            {
                                                text.text = $"{playerName}'s";
                                            }
                                        }
                                        else
                                        {

                                            // The first letter of each word in the item name is handled differently.
                                            if (textChild.name.StartsWith("First Letter"))
                                            {

                                                if (FirstLetterCount != 0)
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                                else
                                                {
                                                    textChild.name = $"First Letter ({FirstLetterCount + 1})";
                                                    Transform FirstLetter = textObject.transform.FindChild($"First Letter ({FirstLetterCount + 1})");
                                                    textChild.SetLocalPositionAndRotation(FirstLetter.localPosition, FirstLetter.localRotation);
                                                    textChild.transform.localScale = FirstLetter.localScale;
                                                    FirstFirstLetter = textChild;
                                                    text.text = scoutItemName.Substring(0, 1);
                                                    FirstLetterCount++;
                                                }

                                            }
                                            // The rest of the word in the item name.
                                            else if (textChild.name.StartsWith("Item Name"))
                                            {
                                                if (ItemNameCount != 0)
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                                else
                                                {

                                                    FirstItemName = textChild;
                                                    textChild.name = $"Item Name ({ItemNameCount + 1})";

                                                    Transform ItemName = textObject.transform.FindChild($"Item Name ({ItemNameCount + 1})");
                                                    textChild.SetLocalPositionAndRotation(ItemName.localPosition, ItemName.localRotation);
                                                    textChild.transform.localScale = ItemName.localScale;
                                                    text.text = scoutItemName.ToUpper().Substring(1);
                                                    text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                                                    ItemNameCount++;
                                                }

                                            }
                                            // Handle the item description.
                                            else if (textChild.name.StartsWith("Description"))
                                            {

                                                textChild.name = "Description";
                                                textChild.SetLocalPositionAndRotation(Description.transform.localPosition, Description.transform.localRotation);
                                                textChild.transform.localScale = Description.transform.localScale;
                                                if (DescriptionCount == 0)
                                                {
                                                    text.text = description;
                                                    text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                                                }
                                                else
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                            }
                                            //Something else got mixed into the item prefab.
                                            else
                                            {
                                                Logging.Log($"An extra game object was found, removing object \"{textChild.name}\"");
                                                toDestroy.Add(textChild.gameObject);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        toDestroy.Add(textChild.gameObject);
                                    }
                                }
                                //Destroy all the game objects slated to be destroyed.
                                foreach (GameObject desObj in toDestroy)
                                {
                                    GameObject.Destroy(desObj);
                                }
                                GameObject.Destroy(textObject);
                            }
                            else
                            {
                                Logging.LogWarning($"Unable to scout location for Upgrade Disk - {location.ToTitleCase()}");
                            }
                        }
                        else
                        {
                            Logging.LogWarning($"Unable to find the item model for Upgrade Disk - {location.ToTitleCase()}");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"Unable to find You___ Notification for {scoutname.ToTitleCase()}");
                    }
                }
            }
        }
    }
}
