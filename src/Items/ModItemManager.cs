using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.FsmMethods;
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
using Il2CppSystem.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BluePrinceArchipelago.Items
{

    /// <summary>
    ///     The Manager for the Items the mod manages. 
    /// </summary>
    public static class ModItemManager
    {
        public static List<PermanentItem> PermanentItemList = [];
        public static List<ArchipelagoUniqueItem> UniqueItemList = new();
        public static List<ResourceItem> JunkItemList = new();
        public static List<GameUniqueItem> GameUniqueItemList = [];
        public static Dictionary<string, ModItem> ItemDict = new(); //Item name is the key, the type of item is the value.
        public static PlayMakerArrayListProxy PreSpawn = new();
        public static PlayMakerArrayListProxy EstateItems = new();
        public static PlayMakerArrayListProxy PickedUp = new();
        public static PlayMakerArrayListProxy CoatCheck = new();
        public static PlayMakerArrayListProxy UsedItems = new();
        public static PlayMakerArrayListProxy InventoryItems = new();
        public static List<Trap> TrapList = new();

        public static UpgradeDisks UpgradeDisks = new UpgradeDisks(null);


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
        ///     Replaces all the Unique Item with their AP versions if applicable.
        /// </summary>
        public static void ReplaceItemsWithAP()
        {
            ReplaceUniqueItemsWithAP();
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                ReplaceUpgradeDisksWithAP();
            }
        }

        public static void InitializeGameItems()
        {
            GameUniqueItemList =
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
        ///     Replaces Unique items (excluding some special cases) with their AP Version if applicable.
        /// </summary>
        public static void ReplaceUniqueItemsWithAP()
        {
            foreach (GameUniqueItem item in GameUniqueItemList)
            {
                item.ReplaceUniqueItemWithArchipelagoItem();
            }
        }

        

        private static string GetPrefabName(GameUniqueItem APitem)
        {
            if (APitem.Name.ToUpper() == "ELECTROMAGNET")
            {
                return "POWERED ELECTROMAGNET";
            }
            return APitem.Name;
        }

        /// <summary>
        ///     Replaces upgrade disks with their AP version (if applicable).
        /// </summary>
        public static void ReplaceUpgradeDisksWithAP() {
            GameObject prefab = ModInstance.Prefabs.GetChild("UPGRADE DISK");
            if (prefab != null) {
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
        public static void ReplaceUpgradeDiskNotifications() {
            int j = 0;
            GameObject item = null;
            foreach (string location in UpgradeDisks.ItemNames) {
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

        /// <summary>
        ///     Adds a Unique item to the Unique item list if it's not already tracked.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public static void AddItem(ArchipelagoUniqueItem item)
        {
            bool found = false;
            int counter = -1;
            // check if item already exists in the pool
            while (!found && counter < UniqueItemList.Count - 1)
            {
                counter++;
                if (UniqueItemList[counter].Name == item.Name)
                {
                    found = true;
                }
            }
            if (!found)
            {
                ItemDict[item.Name] = item;
                UniqueItemList.Add(item);
            }
            else
            {
                Logging.Log($"Item {item.Name} already added, can't add multiple copies.");
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

        /// <summary>
        ///     Adds a trap to the list of traps..
        /// </summary>
        /// <param name="trap">The trap to add.</param>
        public static void AddTrap(Trap trap)
        {
            TrapList.Add(trap);
        }

        /// <summary>
        ///     Adds a Junk Item to the junk item list.
        /// </summary>
        /// <param name="itemToAdd">The junk item to add.</param>
        /// <param name="count">The number of that item to add to the pool.</param>
        public static void AddItem(ResourceItem itemToAdd, int count = 1)
        {
            foreach (ModItem item in JunkItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.StackItem();
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = itemToAdd;
            JunkItemList.Add(itemToAdd);
        }

        /// <summary>
        ///     Adds a permanent (persistent) item to the permanent item list..
        /// </summary>
        /// <param name="itemToAdd">The item to add.</param>
        public static void AddItem(PermanentItem itemToAdd)
        {
            foreach (ModItem item in PermanentItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = itemToAdd;
            PermanentItemList.Add(itemToAdd);
        }

        /// <summary>
        ///     Gets a UniqueItem.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="name">The name of the unique item to find.</param>
        /// <returns>The UniqueItem or null if not found.</returns>
        public static ArchipelagoUniqueItem GetUniqueItem(string name)
        {
           
            foreach (ArchipelagoUniqueItem item in UniqueItemList)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }
            return null;
        }

        public static GameUniqueItem GetGameUniqueItem(string name)
        {

            foreach (GameUniqueItem item in GameUniqueItemList)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets a JunkItem.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="name">The name of the junk item to find.</param>
        /// <returns>The JunkItem or null if not found.</returns>
        public static ResourceItem GetJunkItem(string name)
        {
            foreach (ResourceItem item in JunkItemList)
            {
                
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets a PermanentItem.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="name">The name of the permanent item to find.</param>
        /// <returns>The PermanentItem or null if not found.</returns>
        public static PermanentItem GetPermanentItem(string name)
        {
            foreach (PermanentItem item in PermanentItemList)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        ///     Gets a trap by name
        /// </summary>
        /// <param name="name">The name of the trap to find.</param>
        /// <returns>The trap if it exists, or null if it doesn't.</returns>
        public static Trap GetTrap(string name)
        {
            return TrapList.FirstOrDefault(trap => trap.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        ///     The code related to items that should be run on day start.
        /// </summary>
        public static void StartOfDay()
        {
            // Run upgrade disk start of day code if Upgrade Disk Sanity is on.
            if (ArchipelagoOptions.UpgradeDiskSanity) {
                UpgradeDisks.StartOfDay();
            }
            
        }

        /// <summary>
        ///     Gets the type of the item based on it's name.
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>The type of the item as a string.</returns>
        public static string GetItemType(string itemName)
        {
            ModItem item = GetPermanentItem(itemName);
            itemName = itemName.Trim();
            if (item != null)
            {
                return "Permanent";
            }
            item = GetJunkItem(itemName);
            if (item != null)
            {
                return "Junk";
            }
            item = GetUniqueItem(itemName);
            if (item != null)
            {
                return "Unique";
            }
            return null;
        }

        /// <summary>
        ///     Triggers recieving an item check.
        /// </summary>
        /// <param name="itemInfo">The ItemInfo of the received item.</param>
        public static void OnItemCheckRecieved(ItemInfo itemInfo)
        {
            Logging.LogWarning(itemInfo.ItemName);
            ModItem item = null;
            //If item exists, retreive it.
            PermanentItem permanentItem = GetPermanentItem(itemInfo.ItemName);
            if (permanentItem != null) { 
                permanentItem.IsUnlocked = true;
                permanentItem.UnlockedCount += 1;
            }
            else if (ItemDict.ContainsKey(itemInfo.ItemName))
            {
                item = ItemDict[itemInfo.ItemName];
                item.AddToWorld();
                return;
            }
            else
            {
                Logging.Log($"Unable to give {itemInfo.ItemName} to player. The item doesn't exist or isn't currently handled by the mod.");
            }
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
        public static void RegisterItems()
        {
            //Unique Items
            //  Keys
            AddItem(new ArchipelagoUniqueItem("CAR KEYS", false, ItemSanityType.Key, true, false, ["Locksmith"]));
            AddItem(new ArchipelagoUniqueItem("KEYCARD", false, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem("SILVER KEY", false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            AddItem(new ArchipelagoUniqueItem("KEY 8", false, ItemSanityType.Key, true, true));
            AddItem(new ArchipelagoUniqueItem("BASEMENT KEY", false, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem("VAULT KEY 149", false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("VAULT KEY 233", false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("VAULT KEY 304", false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("VAULT KEY 370", false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("DIARY KEY", false, ItemSanityType.Key, true, true));
            AddItem(new ArchipelagoUniqueItem("PRISM KEY_0", false, ItemSanityType.Key, false, false, ["Locksmith"]));
            AddItem(new ArchipelagoUniqueItem("KEY of Aries", false, ItemSanityType.Key, false));
            AddItem(new ArchipelagoUniqueItem("SECRET GARDEN KEY", false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            AddItem(new ArchipelagoUniqueItem("MICROCHIP 1", false, ItemSanityType.Key, true, true));
            AddItem(new ArchipelagoUniqueItem("MICROCHIP 2", false, ItemSanityType.Key, true, true));
            AddItem(new ArchipelagoUniqueItem("MICROCHIP 3", false, ItemSanityType.Key, true, true));
            AddItem(new ArchipelagoUniqueItem("CABINET KEY 1", false, ItemSanityType.Key, true, true)); //Patio
            AddItem(new ArchipelagoUniqueItem("CABINET KEY 2", false, ItemSanityType.Key, true, true)); //Laundry
            AddItem(new ArchipelagoUniqueItem("CABINET KEY 3", false, ItemSanityType.Key, true, true)); //
                        
            //  StandardArchipelago Items
            AddItem(new ArchipelagoUniqueItem("BATTERY PACK", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("BROKEN LEVER", false, ItemSanityType.Standard, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("MAGNIFYING GLASS", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("METAL DETECTOR", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("SHOVEL", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("SLEDGE HAMMER", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("TELESCOPE", false, ItemSanityType.Standard, true, true));
            AddItem(new ArchipelagoUniqueItem("RUNNING SHOES", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("SALT SHAKER", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("SLEEPING MASK", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("COIN PURSE", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("COUPON BOOK", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("LOCK PICK KIT", false, ItemSanityType.Standard, true, false, ["Locksmith"]));
            AddItem(new ArchipelagoUniqueItem("LUCKY RABBIT'S FOOT", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("TREASURE MAP", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("STOPWATCH", false, ItemSanityType.Standard, true, false, ["Dig"]));
            AddItem(new ArchipelagoUniqueItem("REPELLENT", false, ItemSanityType.Standard, false, true));
            AddItem(new ArchipelagoUniqueItem("WATERING CAN", false, ItemSanityType.Standard, false, true));
            AddItem(new ArchipelagoUniqueItem("LUNCH BOX", false, ItemSanityType.Standard, false, true));
            AddItem(new ArchipelagoUniqueItem("CURSED EFFIGY", false, ItemSanityType.Standard, false, true));
            AddItem(new ArchipelagoUniqueItem("CROWN", false, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem("PAPER CROWN", false, ItemSanityType.Standard, true, true));
            AddItem(new ArchipelagoUniqueItem("GEAR WRENCH", false, ItemSanityType.Standard, true, true));
            AddItem(new ArchipelagoUniqueItem("COMPASS", false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new ArchipelagoUniqueItem("HALL PASS", false, ItemSanityType.Standard, true, true));
                        
            // Workshop Items - TODO
            AddItem(new ArchipelagoUniqueItem("Electromagnet", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Lucky Purse", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Pick Sound Amplifier", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Burning Glass", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Detector Shovel", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Dowsing Rod", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Jack Hammer", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new ArchipelagoUniqueItem("Power Hammer", false, ItemSanityType.Workshop, true, false, ["Workshop"]));
                        
            // Showroom 
            AddItem(new ArchipelagoUniqueItem("CHRONOGRAPH", false, ItemSanityType.SpecialShop, true, false));
            AddItem(new ArchipelagoUniqueItem("EMERALD BRACELET", false, ItemSanityType.SpecialShop, true, false));
            AddItem(new ArchipelagoUniqueItem("MASTER KEY", false, ItemSanityType.SpecialShop, true, false, ["Locksmith"]));
            AddItem(new ArchipelagoUniqueItem("MOON PENDANT", false, ItemSanityType.SpecialShop, true, false));
            AddItem(new ArchipelagoUniqueItem("ORNATE COMPASS", false, ItemSanityType.SpecialShop, true, false));
            AddItem(new ArchipelagoUniqueItem("SILVER SPOON", false, ItemSanityType.SpecialShop, true, false));

            // Armory - TODO
            AddItem(new ArchipelagoUniqueItem("MORNING STAR", false, ItemSanityType.SpecialShop, true, false, ["Armory"]));
            AddItem(new ArchipelagoUniqueItem("THE AXE", false, ItemSanityType.SpecialShop, true, false, ["Armory"]));
            AddItem(new ArchipelagoUniqueItem("KNIGHTS SHIELD", false, ItemSanityType.SpecialShop, true, false, ["Dig", "Armory"]));
            AddItem(new ArchipelagoUniqueItem("TORCH", false, ItemSanityType.SpecialShop, true, false, ["Armory"]));

            // Permanent Items
            AddItem(new ResourceItem("1 Permanent Dice", false, "Dice", 1, true));
            AddItem(new ResourceItem("2 Permanent Dice", false, "Dice", 2, true));
            AddItem(new ResourceItem("1 Permanent Keys", false, "Keys", 1, true));
            AddItem(new ResourceItem("2 Permanent Keys", false, "Keys", 2, true));
            AddItem(new ResourceItem("1 Permanent Steps", false, "Steps", 1, true));
            AddItem(new ResourceItem("2 Permanent Steps", false, "Steps", 2, true));
            AddItem(new ResourceItem("5 Permanent Steps", false, "Steps", 5, true));
            AddItem(new ResourceItem("10 Permanent Steps", false, "Steps", 10, true));
            AddItem(new ResourceItem("1 Permanent Gems", false, "Gems", 1, true));
            AddItem(new ResourceItem("2 Permanent Gems", false, "Gems", 2, true));
            AddItem(new ResourceItem("1 Permanent Luck", false, "Luck", 1, true));
            AddItem(new ResourceItem("2 Permanent Luck", false, "Luck", 2, true));

            // Junk Items
            AddItem(new ResourceItem("1 Allowance", false, "Allowance", 1));
            AddItem(new ResourceItem("2 Allowance", false, "Allowance", 2));
            AddItem(new ResourceItem("1 Stars", false, "Stars", 1));
            AddItem(new ResourceItem("2 Stars", false, "Stars", 2));
            AddItem(new ResourceItem("5 Stars", false, "Stars", 5));
            AddItem(new ResourceItem("Dug Up Nothing", true, "Nothing", 1));
            AddItem(new ResourceItem("1 Gold", true, "Gold", 1));
            AddItem(new ResourceItem("2 Gold", true, "Gold", 2));
            AddItem(new ResourceItem("5 Gold", true, "Gold", 5));
            AddItem(new ResourceItem("1 Dice", true, "Dice", 1));
            AddItem(new ResourceItem("2 Dice", true, "Dice", 2));
            AddItem(new ResourceItem("4 Dice", true, "Dice", 4));
            AddItem(new ResourceItem("1 Gems", true, "Gems", 1));
            AddItem(new ResourceItem("2 Gems", true, "Gems", 2));
            AddItem(new ResourceItem("1 Keys", true, "Keys", 1));
            AddItem(new ResourceItem("2 Keys", true, "Keys", 2));
            AddItem(new ResourceItem("3 Keys", true, "Keys", 3));
            AddItem(new ResourceItem("1 Steps", true, "Steps", 1));
            AddItem(new ResourceItem("2 Steps", true, "Steps", 2));
            AddItem(new ResourceItem("5 Steps", true, "Steps", 5));

            // Traps
            AddTrap(new LoseTrap("Trap Take Steps 1", "Steps", -1));
            AddTrap(new LoseTrap("Trap Take Steps 2", "Steps", -2));
            AddTrap(new LoseTrap("Trap Take Steps 5", "Steps", -5));
            AddTrap(new LoseTrap("Trap Take Stars 1", "Stars", -1));
            AddTrap(new LoseTrap("Trap Take Stars 2", "Stars", -2));
            AddTrap(new LoseTrap("Trap Take Stars 5", "Stars", -5));
            AddTrap(new SetTrap("Trap Set Steps 1", "Steps", 1));
            AddTrap(new SetTrap("Trap Set Steps 10", "Steps", 10));
            AddTrap(new EndOfDayTrap("Trap End Day", "EOD"));
            AddTrap(new FreezeTrap("Trap Freeze Items", "Freeze"));
            AddTrap(new LoseItemTrap("Trap Lose Item", "Lose Item"));
        }

        /// <summary>
        ///     Reloads the GameObjects of all registered items (usually on scene transitions/reloads).
        /// </summary>
        public static void ReloadGameObjects()
        {
            foreach (UniqueItem item in ModItemManager.UniqueItemList)
            {
                string name = ITEM.GetGameObjectName(item.Name);
                GameObject gameObj = GetInventoryItem(name);
                if (gameObj == null)
                {
                    gameObj = GameObjectExtensions.FindGameObject(name);
                }
                item._gameObj = gameObj;
            }
        }
    }
}
