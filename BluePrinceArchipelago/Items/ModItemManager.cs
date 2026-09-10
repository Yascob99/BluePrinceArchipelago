using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Il2CppSystem.Collections;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{

    /// <summary>
    ///     The Manager for the Items the mod manages. 
    /// </summary>
    public static class ModItemManager
    {
        public static List<PermanentItem> PermanentItemList = [];
        public static List<UniqueItem> UniqueItemList = new();
        public static List<JunkItem> JunkItemList = new();
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

        /// <summary>
        ///     Replaces Unique items (excluding some special cases) with their AP Version if applicable.
        /// </summary>
        public static void ReplaceUniqueItemsWithAP()
        {
            foreach (UniqueItem item in UniqueItemList)
            {
                // If the item has not been found yet.
                if (!item.HasBeenFound && item.ApplySanity())
                {
                    FsmState state = Plugin.UniqueItemManager.GetPickupState(item.Name);
                    if (state != null) {
                        state.DisableActionsOfType<ArrayListAdd>();
                        state.AddAction(FSMEventHandler.RegisteredEvents[item.Name].Event);
                    }
                    GameObject prefab = ModInstance.Prefabs.GetChild(item.Name);
                    if (prefab != null)
                    {

                        GameObject spawnObj = FindSpawnObject(item.Name);
                        if (spawnObj == null) {
                            spawnObj = item.GameObj;
                        }
                        if (spawnObj != null)
                        {
                            //If the Model is not currently already replaced.
                            if (spawnObj.transform.FindChild("AP Swirlie") == null)
                            { 
                                //Instantiate a copy of the game object at the location of the spawn pool game object.
                                GameObject APGO = GameObject.Instantiate(prefab, spawnObj.transform.position, spawnObj.transform.rotation);
                                // Get the APswirly Component of the prefab
                                GameObject APswirly = APGO?.transform?.GetChild(0)?.gameObject;
                                if (APswirly != null)
                                {
                                    // Reparent the the AP Swirly to the Archipelago Mod GameObject.
                                    APswirly.transform.parent = spawnObj.transform;
                                    item.ModelReplaced = true;
                                    ReplaceAPItemNotifications(item.Name, spawnObj);
                                }
                                else
                                {
                                    Logging.LogWarning($"Unable to find APSwirly for {item.Name}.");
                                }
                                GameObject.Destroy(APGO);
                            }
                            else {
                                // Make sure the notification is changed for persistent items.
                                if (item.IsPersistent)
                                {
                                    ReplaceAPItemNotifications(item.Name, spawnObj);
                                }
                                else
                                {
                                    Logging.LogWarning($"{item.Name} has already Been Replaced with an AP Item:");
                                }
                            }
                        }
                        else
                        {
                            Logging.LogWarning($"Unable to change spawn prefab for {item.Name}, error finding prefab.");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"Unable to find prefab for {item.Name}. Item is either unimplemented or not present in the assets.");
                    }
                }
            }
        }


        /// <summary>
        ///     Replaces an AP Item's notification with the appropriate item data and the UI model with the AP model.
        /// </summary>
        /// <param name="itemName">The name of the item</param>
        /// <param name="item">The GameObject of the item.</param>
        /// <param name="scoutname">The scoutname of the object if different than it's name.</param>
        public static void ReplaceAPItemNotifications(string itemName, GameObject item, string scoutname = "")
        {
            List<Transform> FoundModels = new();
            if (scoutname == "")
            {
                scoutname = itemName;
            }
            string youName = GetYou___Name(itemName);
            GameObject You___Text = GameObject.Find("UI OVERLAY CAM/You Found Text")?.gameObject;
            if (You___Text != null)
            {
                for (int t = 0; t < You___Text.transform.childCount; t++)
                {
                    Transform child = You___Text.transform.GetChild(t);
                    // Tries to find all related You___Messages"
                    if (child.gameObject.name.Contains(youName) && !CheckSimilar(itemName, child))
                    {
                        FoundModels.Add(child);
                        Transform itemModel = child.FindRecursive(GetItemModelName(itemName), true);
                        if (itemModel != null)
                        {

                            // Make a clone of the original gameObject parented under the ModObject for easy editing/retreival later.
                            GameObject clone = GameObject.Instantiate(child.gameObject, Plugin.ModObject.transform);
                            clone.SetActive(false); //Make sure the clone is not visible.
                            clone.name = child.name;//Make the Name match the original so it can be replaced later.


                            //Instantiate a the AP Object at the original's position
                            GameObject APGO = GameObject.Instantiate(item, itemModel.position, itemModel.rotation, itemModel.parent);
                            APGO.transform.localScale = itemModel.localScale;
                            APGO.name = itemModel.name;
                            itemModel.gameObject.DestroyAllChildren();
                            APGO.MoveChildrenTo(itemModel.gameObject);
                            GameObject.Destroy(APGO);

                            //Import the template Text Prefab.  
                            GameObject textPrefab = ModInstance.Prefabs.GetChild("You Found Text Template");

                            // Get the location ID of our first pickup.
                            long locationid = Plugin.ArchipelagoClient.GetLocationFromName(scoutname.ToTitleCase() + " First Pickup");
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
                                string description = "";
                                string[] itemWords = scoutItemName.Split(" ");
                                if (itemWords.Length < 4)
                                {
                                    scoutItemName = itemWords.Join("\n");
                                }
                                else
                                {
                                    scoutItemName = scoutItemName.Minragged();
                                }
                                int FirstLetterCount = 0;
                                int ItemNameCount = 0;
                                int DescriptionCount = 0;
                                // Update all the fonts and words to be correct
                                Transform textObjects = GameObject.Find($"UI OVERLAY CAM/You Found Text/{child.name}/Text/GameObject")?.transform;
                                // Fix for an error in the Prism Key You Bought (and potentially others);
                                if (textObjects == null) { 
                                    textObjects = GameObject.Find($"UI OVERLAY CAM/You Found Text/{child.name}/GameObject")?.transform;
                                }

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
                        }
                        else
                        {
                            Logging.LogWarning($"Unable to find the item model for {itemName.ToTitleCase()}");
                        }
                    }

                }
                if (FoundModels.Count == 0)
                {
                    Logging.LogWarning($"Unable to find a 'You ___' notification for {itemName}.");
                }
            }
            else
            {
                Logging.LogWarning($"Unable to find the 'You Found Text' GameObject.");
            }
        }


        /// <summary>
        ///     Replaces the name of an item with the internally used name specific to pickup, buying, and dig up notifications.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The item name for the notification.</returns>
        private static string GetYou___Name(string name) {
            name = name.ToTitleCase();
            switch (name)
            {
                case "Magnifying Glass":
                    return "Mag Glass";
                case "Lock Pick Kit":
                    return "Lock Pick";
                case "Sleeping Mask":
                    return "Sleep Mask";
                case "Prism Key_0":
                    return "Prism Key";
                case "Key Of Aries":
                    return "The Key of Aries";
                case "Vault Key 149":
                    return "Key 149";
                case "Vault Key 233":
                    return "Key 233";
                case "Vault Key 304":
                    return "Key 304";
                case "Vault Key 370":
                    return "Key 370";
                case "Cabinet Key 1":
                    return "Cabinet Key";
                case "Cabinet Key 2":
                    return "Cabinet Key 5";
                case "Lucky Purse":
                    return "lucky purse";
                case "Pick Sound Amplifier":
                    return "pick sound amplifier";
                case "Burning Glass":
                    return "burning glass";
                case "Detector Shovel":
                    return "detector shovel";
                case "Power Hammer":
                    return "power hammer";
                case "Jack Hammer":
                    return "jack hammer";
                default:
                    return name;
            }
        }

        /// <summary>
        ///     Gets the internally used name of the item model within the item notification.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <returns>The model name.</returns>
        private static string GetItemModelName(string name) {
            switch (name.ToUpper())
            {
                case "KEY 8":
                    return "SILVER KEY";
                case "WATERING CAN":
                    return "Watering Can pickup";
                case "LUNCH BOX":
                    return "Joya Lunch Box";
                case "CURSED EFFIGY":
                    return "cursed effigy- dagger";
                case "CROWN":
                    return "crow";
                case "HALL PASS":
                    return "hallpass";
                case "CABINET KEY 1":
                    return "cabinet key";
                case "CABINET KEY 2":
                    return "cabinet key";
                case "PRISM KEY_0":
                    return "Prism Key";
                case "KEY OF ARIES":
                    return "o Key";
                default:
                    return name;
            }
        }

        /// <summary>
        ///     Looks to see if the notification is for a similarly named item.
        /// </summary>
        /// <param name="itemName">Item name.</param>
        /// <param name="child">The item's child transform</param>
        /// <returns></returns>
        private static bool CheckSimilar(string itemName, Transform child) {

            if (itemName.ToLower() == "compass")
            {
                if (child.gameObject.name.Contains("Ornate Compass"))
                {
                    return true;
                }
            }
            else if (itemName.ToLower() == "crown") {
                if (child.gameObject.name.Contains("Paper Crown"))
                {
                    return true;
                }
            }
                return false;
        }

        /// <summary>
        ///     Removes the AP Swirlies from Unique items, for use when the swirlies need to be removed mid-day.
        /// </summary>
        /// <param name="item">The item to remove swirlies from.</param>
        public static void RemoveUniqueItemAPSwirly(UniqueItem item) {
            GameObject spawnObj = FindSpawnObject(item.Name);
            if (spawnObj == null)
            {
                Logging.LogWarning($"Unable to change spawn prefab for {item.Name}, error finding prefab with name: {item.Name}(Clone)001");
                return;
            }
            // Delete the AP Swirly SubObject.
            GameObject.Destroy(spawnObj.transform.FindChild("AP Swirlie").gameObject);
            item.ModelReplaced = false;
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
                            APswirly.transform.parent = spawnObj.transform;
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
                            GameObject APGO = GameObject.Instantiate(item, itemModel.position, itemModel.rotation, itemModel.parent);
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
                                    scoutItemName = itemWords.Join("\n");
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
        ///     Finds the spawned version of an item icon,
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The GameObject of the inventory Icon.</returns>
        private static GameObject FindSpawnObject(string name) {
            name = name.Replace("_0", "");
            string instanceName = name + "(Clone)001";
            GameObject spawnObj = ModInstance.PickupSpawnPool.transform.FindChild(instanceName)?.gameObject;
            if (spawnObj == null)
            {
                instanceName = name + " (Clone)001";
                spawnObj = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + instanceName);
            }
            if (spawnObj == null)
            {
                GetInventoryItem(name);
            }
            return spawnObj;
        }

        /// <summary>
        ///     Adds a Unique item to the Unique item list if it's not already tracked.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public static void AddItem(UniqueItem item)
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
        public static void AddItem(JunkItem itemToAdd, int count = 1)
        {
            foreach (ModItem item in JunkItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.Count += 1;
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
        public static UniqueItem GetUniqueItem(string name)
        {
           
            foreach (UniqueItem item in UniqueItemList)
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
        public static JunkItem GetJunkItem(string name)
        {
            foreach (JunkItem item in JunkItemList)
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
        ///     The code related to items that should be run on day start.
        /// </summary>
        public static void StartOfDay()
        {
            AddAllPermanenentItems();
            // Run upgrade disk start of day code if Upgrade Disk Sanity is on.
            if (ArchipelagoOptions.UpgradeDiskSanity) {
                UpgradeDisks.StartOfDay();
            }
            
        }

        /// <summary>
        ///     Adds all permanent items to inventory, meant to be run at start of day.
        /// </summary>
        public static void AddAllPermanenentItems()
        {
            Logging.LogWarning("Adding Permanent Items", "Items");
            if (PermanentItemList.Count > 0)
            {
                foreach (PermanentItem item in PermanentItemList)
                {
                    if (item.UnlockedCount > 0)
                    {
                        Logging.LogWarning($"Adding {item.UnlockedCount} x {item.Count} {item.ItemType}(s)", "Items");
                        item.AddItemToInventory();
                    }
                }

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
                item.AddItemToInventory();
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
            AddItem(new UniqueItem("CAR KEYS", GetInventoryItem("CAR KEYS"), false, ItemSanityType.Key, true, false, ["Locksmith"]));
            AddItem(new UniqueItem("KEYCARD", GetInventoryItem("KEYCARD"), false, ItemSanityType.Key));
            AddItem(new UniqueItem("SILVER KEY", GetInventoryItem("SILVER KEY"), false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            AddItem(new UniqueItem("KEY 8", GetInventoryItem("KEY 8"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("BASEMENT KEY", GetInventoryItem("BASEMENT KEY"), false, ItemSanityType.Key));
            AddItem(new UniqueItem("VAULT KEY 149", GetInventoryItem("VAULT KEY 149"), false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new UniqueItem("VAULT KEY 233", GetInventoryItem("VAULT KEY 233"), false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new UniqueItem("VAULT KEY 304", GetInventoryItem("VAULT KEY 304"), false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new UniqueItem("VAULT KEY 370", GetInventoryItem("VAULT KEY 370"), false, ItemSanityType.Key, true, false, ["Dig"]));
            AddItem(new UniqueItem("DIARY KEY", GetInventoryItem("DIARY KEY"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("PRISM KEY_0", GetInventoryItem("PRISM KEY"), false, ItemSanityType.Key, false, false, ["Locksmith"]));
            AddItem(new UniqueItem("KEY of Aries", GetInventoryItem("KEY of Aries"), false, ItemSanityType.Key, false));
            AddItem(new UniqueItem("SECRET GARDEN KEY", GetInventoryItem("SECRET GARDEN KEY"), false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            AddItem(new UniqueItem("MICROCHIP 1", GetInventoryItem("MICROCHIP 1"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("MICROCHIP 2", GetInventoryItem("MICROCHIP 2"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("MICROCHIP 3", GetInventoryItem("MICROCHIP 3"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("CABINET KEY 1", GetInventoryItem("CABINET KEY 1"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("CABINET KEY 2", GetInventoryItem("CABINET KEY 2"), false, ItemSanityType.Key, true, true));
            AddItem(new UniqueItem("CABINET KEY 3", GetInventoryItem("CABINET KEY 2"), false, ItemSanityType.Key, true, true));

            //  Standard Items
            AddItem(new UniqueItem("BATTERY PACK", GetInventoryItem("BATTERY PACK"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("BROKEN LEVER", GetInventoryItem("BROKEN LEVER"), false, ItemSanityType.Standard, true, false, ["Dig"]));
            AddItem(new UniqueItem("MAGNIFYING GLASS", GetInventoryItem("MAGNIFYING GLASS"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("METAL DETECTOR", GetInventoryItem("METAL DETECTOR"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("SHOVEL", GetInventoryItem("SHOVEL"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("SLEDGE HAMMER", GetInventoryItem("SLEDGE HAMMER"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("TELESCOPE", GetInventoryItem("TELESCOPE"), false, ItemSanityType.Standard, true, true));
            AddItem(new UniqueItem("RUNNING SHOES", GetInventoryItem("RUNNING SHOES"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("SALT SHAKER", GetInventoryItem("SALT SHAKER"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("SLEEPING MASK", GetInventoryItem("SLEEPING MASK"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("COIN PURSE", GetInventoryItem("COIN PURSE"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("COUPON BOOK", GetInventoryItem("COUPON BOOK"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("LOCK PICK KIT", GetInventoryItem("LOCK PICK KIT"), false, ItemSanityType.Standard, true, false, ["Locksmith"]));
            AddItem(new UniqueItem("LUCKY RABBIT'S FOOT", GetInventoryItem("LUCKY RABBIT'S FOOT"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("TREASURE MAP", GetInventoryItem("TREASURE MAP"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("STOPWATCH", GetInventoryItem("STOPWATCH"), false, ItemSanityType.Standard, true, false, ["Dig"]));
            AddItem(new UniqueItem("REPELLENT", GetInventoryItem("REPELLENT"), false, ItemSanityType.Standard, false, true));
            AddItem(new UniqueItem("WATERING CAN", GetInventoryItem("WATERING CAN"), false, ItemSanityType.Standard, false, true));
            AddItem(new UniqueItem("LUNCH BOX", GetInventoryItem("LUNCH BOX"), false, ItemSanityType.Standard, false, true));
            AddItem(new UniqueItem("CURSED EFFIGY", GetInventoryItem("CURSED EFFIGY"), false, ItemSanityType.Standard, false, true));
            AddItem(new UniqueItem("CROWN", GetInventoryItem("CROWN"), false, ItemSanityType.Standard));
            AddItem(new UniqueItem("PAPER CROWN", GetInventoryItem("PAPER CROWN"), false, ItemSanityType.Standard, true, true));
            AddItem(new UniqueItem("GEAR WRENCH", GetInventoryItem("GEAR WRENCH"), false, ItemSanityType.Standard, true, true));
            AddItem(new UniqueItem("COMPASS", GetInventoryItem("COMPASS"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            AddItem(new UniqueItem("HALL PASS", GetInventoryItem("HALL PASS"), false, ItemSanityType.Standard, true, true));

            // Workshop Items - TODO
            AddItem(new UniqueItem("ELECTROMAGNET", GetInventoryItem("POWERED ELECTROMAGNET"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("LUCKY PURSE", GetInventoryItem("LUCKY PURSE"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("PICK SOUND AMPLIFIER", GetInventoryItem("PICK SOUND AMPLIFIER"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("BURNING GLASS", GetInventoryItem("BURNING GLASS"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("DETECTOR SHOVEL", GetInventoryItem("DETECTOR SHOVEL"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("DOWSING ROD", GetInventoryItem("DOWSING ROD"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("JACK HAMMER", GetInventoryItem("JACK HAMMER"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));
            AddItem(new UniqueItem("POWER HAMMER", GetInventoryItem("POWER HAMMER"), false, ItemSanityType.Workshop, true, false, ["Workshop"]));

            // Showroom
            AddItem(new UniqueItem("CHRONOGRAPH", GetInventoryItem("CHRONOGRAPH"), false, ItemSanityType.SpecialShop, true, false));
            AddItem(new UniqueItem("EMERALD BRACELET", GetInventoryItem("EMERALD BRACELET"), false, ItemSanityType.SpecialShop, true, false));
            AddItem(new UniqueItem("MASTER KEY", GetInventoryItem("MASTER KEY"), false, ItemSanityType.SpecialShop, true, false, ["Locksmith"]));
            AddItem(new UniqueItem("MOON PENDANT", GetInventoryItem("MOON PENDANT"), false, ItemSanityType.SpecialShop, true, false));
            AddItem(new UniqueItem("ORNATE COMPASS", GetInventoryItem("ORNATE COMPASS"), false, ItemSanityType.SpecialShop, true, false));
            AddItem(new UniqueItem("SILVER SPOON", GetInventoryItem("SLIVER SPOON"), false, ItemSanityType.SpecialShop, true, false));

            // Armory - TODO
            AddItem(new UniqueItem("MORNING STAR", GetInventoryItem("MORNING STAR"), false, ItemSanityType.SpecialShop, true, false, ["Armory"]));
            AddItem(new UniqueItem("THE AXE", GetInventoryItem("THE AXE"), false, ItemSanityType.SpecialShop, true, false, ["Armory"]));
            AddItem(new UniqueItem("KNIGHTS SHIELD", GetInventoryItem("KNIGHTS SHIELD"), false, ItemSanityType.SpecialShop, true, false, ["Dig", "Armory"]));
            AddItem(new UniqueItem("TORCH", GetInventoryItem("TORCH"), false, ItemSanityType.SpecialShop, true, false, ["Armory"]));

            // Permanent Items
            AddItem(new PermanentItem("Extra Starting Dice 1", null, false, "Dice", 1));
            AddItem(new PermanentItem("Extra Starting Dice 2", null, false, "Dice", 2));
            AddItem(new PermanentItem("Extra Starting Keys 1", null, false, "Keys", 1));
            AddItem(new PermanentItem("Extra Starting Keys 2", null, false, "Keys", 2));
            AddItem(new PermanentItem("Extra Starting Steps 1", null, false, "Steps", 1));
            AddItem(new PermanentItem("Extra Starting Steps 2", null, false, "Steps", 2));
            AddItem(new PermanentItem("Extra Starting Steps 5", null, false, "Steps", 5));
            AddItem(new PermanentItem("Extra Starting Steps 10", null, false, "Steps", 10));
            AddItem(new PermanentItem("Extra Starting Gems 1", null, false, "Gems", 1));
            AddItem(new PermanentItem("Extra Starting Gems 2", null, false, "Gems", 2));
            AddItem(new PermanentItem("Extra Starting Luck 1", null, false, "Luck", 1));
            AddItem(new PermanentItem("Extra Starting Luck 2", null, false, "Luck", 2));

            // Junk Items
            AddItem(new JunkItem("Extra Allowance 1", null, false, "Allowance", 1));
            AddItem(new JunkItem("Extra Allowance 2", null, false, "Allowance", 2));
            AddItem(new JunkItem("Extra Stars 1", null, false, "Stars", 1));
            AddItem(new JunkItem("Extra Stars 2", null, false, "Stars", 2));
            AddItem(new JunkItem("Extra Stars 5", null, false, "Stars", 5));
            AddItem(new JunkItem("Dug Up Nothing", null, true, "Nothing", 1));
            AddItem(new JunkItem("Extra Gold 1", null, true, "Gold", 1));
            AddItem(new JunkItem("Extra Gold 2", null, true, "Gold", 2));
            AddItem(new JunkItem("Extra Gold 5", null, true, "Gold", 5));
            AddItem(new JunkItem("Extra Dice 1", null, true, "Dice", 1));
            AddItem(new JunkItem("Extra Dice 2", null, true, "Dice", 2));
            AddItem(new JunkItem("Extra Dice 4", null, true, "Dice", 4));
            AddItem(new JunkItem("Extra Gems 1", null, true, "Gems", 1));
            AddItem(new JunkItem("Extra Gems 2", null, true, "Gems", 2));
            AddItem(new JunkItem("Extra Keys 1", null, true, "Keys", 1));
            AddItem(new JunkItem("Extra Keys 2", null, true, "Keys", 2));
            AddItem(new JunkItem("Extra Steps 1", null, true, "Steps", 1));
            AddItem(new JunkItem("Extra Steps 2", null, true, "Steps", 2));
            AddItem(new JunkItem("Extra Steps 5", null, true, "Steps", 5));

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
                string name = GetObjName(item.Name);
                GameObject gameObj = GetInventoryItem(name);
                if (gameObj == null)
                {
                    gameObj = GameObjectExtensions.FindGameObject(name);
                }
                item.GameObj = gameObj;
            }
        }

        /// <summary>
        ///     Gets the objectname of items where it doesn't match their name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns></returns>
        private static string GetObjName(string name)
        {
            switch (name)
            {
                case "CABINET KEY 3":
                    return "CABINET KEY 2";
                case "ELECTROMAGNET":
                    return "POWERED ELECTROMAGNET";
                case "PRISM KEY_0":
                    return "PRISM KEY";

                default:
                    return name;
            }
        }
    }
}