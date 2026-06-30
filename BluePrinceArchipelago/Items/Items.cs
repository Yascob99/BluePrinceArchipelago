using Archipelago.MultiClient.Net.Helpers;
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
    public class ModItemManager
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

        public ModItemManager()
        {
        }
        public void Initialize()
        {

        }
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
        public void ReplaceItemsWithAP()
        {
            ReplaceUniqueItemsWithAP();
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                ReplaceUpgradeDisksWithAP();
            }
        }
        public void ReplaceUniqueItemsWithAP()
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

        public void ReplaceAPItemNotifications(string itemName, GameObject item, string scoutname = "")
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

        private string GetYou___Name(string name) {
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
                default:
                    return name;
            }
        }

        private string GetItemModelName(string name) {
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
        private bool CheckSimilar(string itemName, Transform child) {

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

        // Removes the AP Swirlies from Unique Items. Not needed for regular items.
        public void RemoveUniqueItemAPSwirly(UniqueItem item) {
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

        public void ReplaceUpgradeDisksWithAP() {
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
                // Replaces the You Found and You bought Message for the Upgrade disk with the Commissary Message. Will Replace the others programmatically. 
                ReplaceUpgradeDiskNotifications();
            }
        }
        public void ReplaceUpgradeDiskNotifications() {
            int j = 0;
            GameObject item = null;
            foreach (string location in UpgradeDisks.Locations) {
                j++;
                
                if (j < 10)
                {
                    item = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)00{j}")?.gameObject;
                }
                else {
                    item = GameObject.Find($"__SYSTEM/Pickup Spawn Pools/UPGRADE DISK(Clone)0{j}")?.gameObject;
                }
                string scoutname = $"Upgrade Disk - {location.Replace("LADYSHIPS", "LADYSHIP\'s").Replace("AND ", "& ").Replace("FOUNDATION", "THE FOUNDATION").ToTitleCase()}";
                GameObject You___Message = GameObject.Find("UI OVERLAY CAM/You Found Text/You Found Upgrade Disk - " + location.ToTitleCase());
                if (location.ToTitleCase() == "Commissary") {
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
                else {
                    Logging.LogWarning($"Unable to find You___ Notification for {scoutname.ToTitleCase()}");
                }
            }
        }

        private GameObject FindSpawnObject(string name) {
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
                Plugin.ModItemManager.GetInventoryItem(name);
            }
            return spawnObj;
        }

        // Adds a unique item if it doesn't already exist.
        public void AddItem(UniqueItem item)
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

        public string ListItems(string listType)
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

        public void AddTrap(Trap trap)
        {
            TrapList.Add(trap);
        }

        public void AddItem(JunkItem itemToAdd, int count = 1)
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
        public void AddItem(PermanentItem itemToAdd)
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
        public void AddItem(string name, GameObject gameObject, bool isUnlocked, bool isUnique = false, bool isJunk = false, bool isPermanent = false, int count = 1, string itemType = null)
        {
            if (isUnique)
            {
                if (isJunk || isPermanent || itemType != null || count > 1 || count < 1)
                {
                    Logging.Log($"{name} could not be added as a Unique item, invalid parameters");
                    return;
                }
                UniqueItem item = new UniqueItem(name, gameObject, isUnlocked);
                ItemDict[item.Name] = item;
                UniqueItemList.Add(item);
            }
            else if (isJunk)
            {
                if (itemType == null || count == 0 || isPermanent)
                {
                    Logging.Log($"{name} could not be added as a Junk/Trap item, invalid parameters.");
                    return;
                }
                JunkItem item = new JunkItem(name, gameObject, isUnlocked, itemType, count);
                ItemDict[item.Name] = item;
                JunkItemList.Add(item);
            }
            else if (isPermanent)
            {
                if (itemType == null || count < 1)
                {
                    Logging.Log($"{name} could not be added as a Permanent Item, invalid parameters.");
                    return;
                }
                PermanentItem item = new PermanentItem(name, gameObject, isUnlocked, itemType, count);
                ItemDict[item.Name] = item;
                PermanentItemList.Add(item);
            }
            else
            {
                Logging.LogWarning("Item could not be added, invalid parameters.");
            }
        }
        public UniqueItem GetUniqueItem(string name)
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

        public JunkItem GetJunkItem(string name)
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
        public PermanentItem GetPermanentItem(string name)
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

        public void StartOfDay()
        {
            AddAllPermanenentItems();
            // Run upgrade disk start of day code if Upgrade Disk Sanity is on.
            if (ArchipelagoOptions.UpgradeDiskSanity) {
                UpgradeDisks.StartOfDay();
            }
            
        }
        // returns true if item was released from queue, returns false if no item in queue to release or failed to release the item.

        // Adds all permanent items to inventory, meant to be run at start of day.
        public void AddAllPermanenentItems()
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
        public void OnTrapReceived(ItemInfo itemInfo)
        {
            // Get the first matching item.
            Trap trap = TrapList.FirstOrDefault(trap => trap.Name.ToLower() == itemInfo.ItemName.ToLower());
            if (trap != null)
            {
                trap.ActivateTrap();
            }
            else
            {
                Logging.LogError($"Error receiving {itemInfo.ItemName}: No Trap with that name could be found.");
            }
        }
        public string GetItemType(string itemName)
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

        // Handle the code for recieving an item check that results in receiving an item.
        public void OnItemCheckRecieved(ItemInfo itemInfo)
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

        // Checks if the item is currently spawnable.
        public bool IsItemSpawnable(GameObject item, bool isPrespawn = true)
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

        // Gets an item from the prespawn item list.
        public GameObject GetInventoryItem(string itemName)
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

        // Gets an item that the player has picked up.
        public GameObject GetPickedUpItem(string itemName)
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

        // Makes the player lose a random item if they have an item. 
        public void LoseRandomItem()
        {
            //We don't care if this fails, since it's a trap, and I'm too lazy to handle the edgecase where you are not in a run, and you spawn with an item.
            int count = PickedUp.arrayList.Count;
            if (count > 0 && ModInstance.IsInRun)
            {
                int index = UnityEngine.Random.Range(0, count);
                PickedUp.RemoveAt(index);
            }
        }
    }

    public class ModItem(string name, GameObject gameObject, bool isUnlocked, int count = 1)
    {
        private string _Name = name;
        public string Name { get { return _Name; } set { _Name = value; } }

        private GameObject _GameObj = gameObject;
        public GameObject GameObj { get { return _GameObj; } set { _GameObj = value; } }

        private bool _IsUnlocked = isUnlocked;
        public bool IsUnlocked
        {
            get { return _IsUnlocked; }
            set { _IsUnlocked = value; }
        }

        private int _Count = count;
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        private bool _IsUnique = false;
        public bool IsUnique
        {
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }

        public virtual void AddItemToInventory()
        {
            // Put out an error if this method was not properly overriden. There should be no base moditems.
            Logging.LogError("Error: The Base Moditem.AddItemToInventory method should be overriden.");
        }
    }

    // Handles junk items.
    public class JunkItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {

        private string _ItemType = itemType;
        public string Itemtype
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                if (value > 0)
                {
                    _IsTrap = true; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                else
                {
                    _IsTrap = false; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                _Count = value;
            }
        }

        private bool _IsTrap = count < 0;
        public bool IsTrap
        {
            get { return _IsTrap; } //No setter since this is connected to count
        }

        public override void AddItemToInventory()
        {
            if (_ItemType == "Gems")
            {
                AdjustGems(_Count);
            }
            else if (_ItemType == "Steps")
            {
                AdjustSteps(_Count);
            }
            else if (_ItemType == "Gold")
            {
                AdjustGold(_Count);
            }
            else if (_ItemType == "Dice")
            {
                AdjustDice(_Count);
            }
            else if (_ItemType == "Keys")
            {
                AdjustKeys(_Count);
            }
            else if (_ItemType == "Luck")
            {
                AdjustLuck(_Count);
            }
            else if (_ItemType == "Stars")
            {
                AdjustStars(_Count);
            }
            else if (_ItemType == "Allowance")
            {
                AdjustAllowance(_Count);
            }
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }
        private void AdjustGems(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            FsmInt AdjustmentAmount = ModInstance.StepManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }
        private void AdjustGold(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GoldManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }
        private void AdjustDice(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.DiceManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.DiceManager.SendEvent("Update");
        }
        private void AdjustKeys(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.KeyManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + count;
            ModInstance.KeyManager.SendEvent("Update");
        }
        private void AdjustLuck(int count = 1)
        {
            int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
            if (luck + count > 0)
            {
                ModInstance.LuckManager.FindIntVariable("LUCK").Value += count;
            }
            else
            {
                ModInstance.LuckManager.FindIntVariable("Luck").Value = 0;
            }
        }
        private void AdjustStars(int count = 1)
        {
            int totalStars = ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value;
            if (totalStars + 1 > 0)
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value += count;
            }
            else
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = 0;
            }
            ModInstance.StarManager.SendEvent("Update");
        }
        private void AdjustAllowance(int count = 1)
        {
            int totalAllowance = ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value;
            if (totalAllowance + count > 0)
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value += count;
            }
            else
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("allowance").Value = 0;
            }
        }
    }

    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;
        public int UnlockedCount = 0;

        public string ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }
        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
            }
        }

        public override void AddItemToInventory()
        {
            if (_ItemType == "Gems")
            {
                AdjustGems(_Count);
            }
            else if (_ItemType == "Steps")
            {
                AdjustSteps(_Count);
            }
            else if (_ItemType == "Gold")
            {
                AdjustGold(_Count);
            }
            else if (_ItemType == "Dice")
            {
                AdjustDice(_Count);
            }
            else if (_ItemType == "Keys")
            {
                AdjustKeys(_Count);
            }
            else if (_ItemType == "Luck")
            {
                AdjustLuck(_Count);
            }
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }
        private void AdjustGems(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            FsmInt AdjustmentAmount = ModInstance.StepManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }
        //Todo replace with allowance.
        private void AdjustGold(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.GoldManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }
        //Todo replace with allowance.
        private void AdjustDice(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.DiceManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.DiceManager.SendEvent("Update");
        }
        private void AdjustKeys(int count = 1)
        {
            FsmInt AdjustmentAmount = ModInstance.KeyManager.FindIntVariable("Adjustment Amount");
            AdjustmentAmount.Value = AdjustmentAmount.Value + (UnlockedCount * count);
            ModInstance.KeyManager.SendEvent("Update");
        }
        private void AdjustLuck(int count = 1)
        {
            int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
            if (luck + count > 0)
            {
                ModInstance.LuckManager.FindIntVariable("LUCK").Value = luck + (UnlockedCount * count);
            }
            else
            {
                ModInstance.LuckManager.FindIntVariable("Luck").Value = 0;
            }
        }
    }
    public class ProgressiveItems(string name, GameObject gameObject, bool isUnlocked, int count = 0, bool isPreSpawn = true) : ModItem(name, gameObject, isUnlocked)
    {
        private int _Count = count;
        public new int Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
            }
        }
        //If it's in the prespawn list
        public bool IsPreSpawn = isPreSpawn;
        // The names of the locations where it is found.
        public List<string> Locations = new List<string>();
        // The locations at which it has been found.
        public List<string> FoundLocations = new List<string>();
        // The locations to which the upgrade disk has been received for;
        public List<string> RecievedItems = new List<string>();
        // The locations to which the upgrade disk received has been used.
        public List<string> UsedLocations = new List<string>();

        public int totalFound
        {
            get
            {
                return Locations.Count - FoundLocations.Count;
            }
        }
    }
    // Controls the upgrade disks. Disks should persist accross days.
    public class UpgradeDisks(GameObject gameObject) : ProgressiveItems("UPGRADE DISK", gameObject, false, 16, true)
    {
        public new List<string> Locations = ["ARCHIVES", "TRADING POST DYNAMITE", "TOMB", "COMMISSARY", "FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST AND FOUND", "HER LADYSHIPS CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        public static List<GameObject> YouFoundObjects = new List<GameObject>();
        public List<EventID> EventNames = [EventID.Upgrade_Disk_Archives_found, EventID.Upgrade_Disk_BootLeg_found, EventID.Upgrade_Disk_Cloister_found, EventID.Upgrade_Disk_Commissary_found, EventID.Upgrade_Disk_Foundation_found, EventID.Upgrade_Disk_Freezer_found, EventID.Upgrade_Disk_Garage_found, EventID.Upgrade_Disk_GreatHall_found, EventID.Upgrade_Disk_LostFound_found, EventID.Upgrade_Disk_MasterBedroom_found, EventID.Upgrade_Disk_Mechanarium_found, EventID.Upgrade_Disk_MorningRoom_found, EventID.Upgrade_Disk_Office_found, EventID.Upgrade_Disk_TradingPost_found, EventID.Upgrade_Disk_Vault_found, EventID.Upgrade_Disk_TorchRoom_found];
        public List<string> UsedVariables = ["Upgrade Disc - Archives", "Upgrade Disc - Bootleg", "Upgrade Disc - Cloister", "Upgrade Disc - Commissary", "Upgrade Disc - Foundation", "Upgrade Disc - Freezer", "Upgrade Disc - Garage", "Upgrade Disc - Great Hall", "Upgrade Disc - LostFound", "Upgrade Disc - Master Bedroom", "Upgrade Disc - Mechanarium", "Upgrade Disc - Morning Room", "Upgrade Disc - Office", "Upgrade Disc - Shop", "Upgrade Disc - Tomb", "Upgrade Disc - Torch Room"];
        public new GameObject GameObj = gameObject;

        public bool UnlockLocationIfExists(string locationName) {
            foreach (string location in Locations) {
                string lowlocation = location.ToLower().Replace("ladyships", "ladyship\'s").Replace("and ", "& ");
                if (locationName.ToLower().Contains(lowlocation)) {
                    if (!FoundLocations.Contains(location))
                    {
                        FoundLocations.Add(location);
                    }
                }
            }
            return false;
        }
        // Handles adding unlocked upgrade disks to the the players inventory until they are used.
        public void StartOfDay() {
            int i = 0;
            // Skip this Start of Day if it's a reconnect from crash or quit.
            Logging.LogWarning("Updating Upgrade Disk Used States");
            int j = -1;
            foreach (string boolName in UsedVariables)
            {
                j++;
                string location = Locations[j];
                if (ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(boolName).Value)
                {
                    if (!UsedLocations.Contains(location))
                    {
                        UsedLocations.Add(location);
                    }
                }
            }
            Logging.LogWarning($"[{UsedLocations.Join(", ")}]");
            foreach (string location in RecievedItems)
            {
                Logging.LogWarning($"{location}");
                // Check if the item has been used, and if it has, 
                if (!UsedLocations.Contains(location.ToUpper()))
                {
                    AddItemToInventory(location);
                }
                i++;
            }
        }

        // Handles the pickup of the Upgrade Disk. The Vanilla code handles the rest.
        public void OnPickup() {
            string roomname = ModInstance.RoomText.GetStringVariable("Current Room").Value;
            roomname = roomname.ToUpper().Replace("'", "").Replace("POST", "POST DYNAMITE").Replace(" AND", " &"); // HLC, TP Dynamite, and Lost & Found name fix
            OnFind(roomname);
        }
        public void OnUsed(int upgradeid) {
            Logging.LogWarning($"Upgrade With ID {upgradeid} used.");
            if (RecievedItems.Count > UsedLocations.Count)
            {
                string location = Locations[upgradeid-1];
                if (!UsedLocations.Contains(location))
                {
                    UsedLocations.Add(location);
                }
                ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[upgradeid-1]).Value = true;
            }
            else {
                Logging.LogWarning("Unable to set Locaation as used, no received locations are currently unused.", "UpgradeDisks");
            }
        }

        // Sends the location for the found upgrade disk.
        private void OnFind(string location)
        {
            if (!FoundLocations.Contains(location.ToUpper()))
            {
                FoundLocations.Add(location.ToUpper());
                //Fix location name for pickup event.
                location = location.Replace("LADYSHIPS", "LADYSHIP's").Replace(" &", " AND");
                Logging.LogWarning(location);
                ModInstance.GlobalManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[Locations.IndexOf(location)]).Value = true;
                ModInstance.ModEventHandler.OnUgradeDiskFound(location);
            }
        }

        public void OnSpawn(string location, GameObject spawnedObj) {
            if (spawnedObj != null) {
                if (FoundLocations.Contains(location.ToUpper())) {
                    GameObject.Destroy(spawnedObj);
                    Logging.LogWarning($"Despawned Upgrade Disk in {location}, since it has been found before.");
                }
                Logging.LogWarning($"Unable to despawn Upgrade Disk in {location}, location is not a valid location.");
                return;
            }
            Logging.LogWarning($"Unable to despawn Upgrade Disk in {location}, spawnedObj does not exist.");
        }

        public void AddItemToInventory(string location)
        {
            Logging.LogWarning("Attempting To Add Upgrade Disk to Inventory.");
            if (!RecievedItems.Contains(location.ToUpper()))
            {
                RecievedItems.Add(location.ToUpper());
            }
            GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
            PlayMakerFSM Inventory = InventoryGO.GetFsm("Inventory Icons");
            PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
            GameObject icon = Plugin.UniqueItemManager.GetIconGameObject("UPGRADE DISK");
            PlayMakerArrayListProxy UpgradeDisks = GameObject.Find("__SYSTEM/Upgrade Disks").GetArrayListProxy("upgrade disk pickup");

            if (icon != null && InventoryIcons != null)
            {
                UpgradeDisks.Add(Locations.IndexOf(location) + 1, "Integer");
                ModItemManager.PickedUp.Add(Plugin.ModItemManager.GetInventoryItem("UPGRADE DISK"), "GameObject");
                InventoryIcons.Add(icon, "GameObject");


                if (Name == "RUNNING SHOES")
                {
                    ModInstance.RunningEngine.SendEvent("Update");
                }
                //Send Event 0 to the Global Manager.
            }


        }
    }

    //TODO Later for a later goal. The locations they are found at is different from where they can be used. Should not persist across days
    public class SanctumKeys(string name, GameObject gameObject, int count = 0) : ProgressiveItems(name, gameObject, false, 1, true)
    {

    }

    public static class RegisterItems
    {

        public static void Register()
        {
            //Unique Items
            //  Keys
            Plugin.ModItemManager.AddItem(new UniqueItem("CAR KEYS", Plugin.ModItemManager.GetInventoryItem("CAR KEYS"), false, ItemSanityType.Key, true, false, ["Locksmith"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("KEYCARD", Plugin.ModItemManager.GetInventoryItem("KEYCARD"), false, ItemSanityType.Key));
            Plugin.ModItemManager.AddItem(new UniqueItem("SILVER KEY", Plugin.ModItemManager.GetInventoryItem("SILVER KEY"), false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("KEY 8", Plugin.ModItemManager.GetInventoryItem("KEY 8"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("BASEMENT KEY", Plugin.ModItemManager.GetInventoryItem("BASEMENT KEY"), false, ItemSanityType.Key));
            Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 149", Plugin.ModItemManager.GetInventoryItem("VAULT KEY 149"), false, ItemSanityType.Key, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 233", Plugin.ModItemManager.GetInventoryItem("VAULT KEY 233"), false, ItemSanityType.Key, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 304", Plugin.ModItemManager.GetInventoryItem("VAULT KEY 304"), false, ItemSanityType.Key, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 370", Plugin.ModItemManager.GetInventoryItem("VAULT KEY 370"), false, ItemSanityType.Key, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("DIARY KEY", Plugin.ModItemManager.GetInventoryItem("DIARY KEY"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("PRISM KEY_0", Plugin.ModItemManager.GetInventoryItem("PRISM KEY"), false, ItemSanityType.Key, false, false, ["Locksmith"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("KEY of Aries", Plugin.ModItemManager.GetInventoryItem("KEY of Aries"), false, ItemSanityType.Key, false));
            Plugin.ModItemManager.AddItem(new UniqueItem("SECRET GARDEN KEY", Plugin.ModItemManager.GetInventoryItem("SECRET GARDEN KEY"), false, ItemSanityType.Key, true, false, ["Locksmith", "Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 1", Plugin.ModItemManager.GetInventoryItem("MICROCHIP 1"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 2", Plugin.ModItemManager.GetInventoryItem("MICROCHIP 2"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 3", Plugin.ModItemManager.GetInventoryItem("MICROCHIP 3"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 1", Plugin.ModItemManager.GetInventoryItem("CABINET KEY 1"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 2", Plugin.ModItemManager.GetInventoryItem("CABINET KEY 2"), false, ItemSanityType.Key, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 3", Plugin.ModItemManager.GetInventoryItem("CABINET KEY 2"), false, ItemSanityType.Key, true, true));

            //  Standard Items
            Plugin.ModItemManager.AddItem(new UniqueItem("BATTERY PACK", Plugin.ModItemManager.GetInventoryItem("BATTERY PACK"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("BROKEN LEVER", Plugin.ModItemManager.GetInventoryItem("BROKEN LEVER"), false, ItemSanityType.Standard, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("MAGNIFYING GLASS", Plugin.ModItemManager.GetInventoryItem("MAGNIFYING GLASS"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("METAL DETECTOR", Plugin.ModItemManager.GetInventoryItem("METAL DETECTOR"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("SHOVEL", Plugin.ModItemManager.GetInventoryItem("SHOVEL"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("SLEDGE HAMMER", Plugin.ModItemManager.GetInventoryItem("SLEDGE HAMMER"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("TELESCOPE", Plugin.ModItemManager.GetInventoryItem("TELESCOPE"), false, ItemSanityType.Standard, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("RUNNING SHOES", Plugin.ModItemManager.GetInventoryItem("RUNNING SHOES"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("SALT SHAKER", Plugin.ModItemManager.GetInventoryItem("SALT SHAKER"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("SLEEPING MASK", Plugin.ModItemManager.GetInventoryItem("SLEEPING MASK"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("COIN PURSE", Plugin.ModItemManager.GetInventoryItem("COIN PURSE"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("COUPON BOOK", Plugin.ModItemManager.GetInventoryItem("COUPON BOOK"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("LOCK PICK KIT", Plugin.ModItemManager.GetInventoryItem("LOCK PICK KIT"), false, ItemSanityType.Standard, true, false, ["Locksmith"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("LUCKY RABBIT'S FOOT", Plugin.ModItemManager.GetInventoryItem("LUCKY RABBIT'S FOOT"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("TREASURE MAP", Plugin.ModItemManager.GetInventoryItem("TREASURE MAP"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("STOPWATCH", Plugin.ModItemManager.GetInventoryItem("STOPWATCH"), false, ItemSanityType.Standard, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("REPELLENT", Plugin.ModItemManager.GetInventoryItem("REPELLENT"), false, ItemSanityType.Standard, false, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("WATERING CAN", Plugin.ModItemManager.GetInventoryItem("WATERING CAN"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("LUNCH BOX", Plugin.ModItemManager.GetInventoryItem("LUNCH BOX"), false, ItemSanityType.Standard, false, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("CURSED EFFIGY", Plugin.ModItemManager.GetInventoryItem("CURSED EFFIGY"), false, ItemSanityType.Standard, false, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("CROWN", Plugin.ModItemManager.GetInventoryItem("CROWN"), false, ItemSanityType.Standard));
            Plugin.ModItemManager.AddItem(new UniqueItem("PAPER CROWN", Plugin.ModItemManager.GetInventoryItem("PAPER CROWN"), false, ItemSanityType.Standard, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("GEAR WRENCH", Plugin.ModItemManager.GetInventoryItem("GEAR WRENCH"), false, ItemSanityType.Standard, true, true));
            Plugin.ModItemManager.AddItem(new UniqueItem("COMPASS", Plugin.ModItemManager.GetInventoryItem("COMPASS"), false, ItemSanityType.Standard, true, false, ["Commissary"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("HALL PASS", Plugin.ModItemManager.GetInventoryItem("HALL PASS"), false, ItemSanityType.Standard, true, true));

            // Workshop Items
            Plugin.ModItemManager.AddItem(new UniqueItem("ELECTROMAGNET", Plugin.ModItemManager.GetInventoryItem("POWERED ELECTROMAGNET"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("LUCKY PURSE", Plugin.ModItemManager.GetInventoryItem("LUCKY PURSE"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("PICK SOUND AMPLIFIER", Plugin.ModItemManager.GetInventoryItem("PICK SOUND AMPLIFIER"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("BURNING GLASS", Plugin.ModItemManager.GetInventoryItem("BURNING GLASS"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("DETECTOR SHOVEL", Plugin.ModItemManager.GetInventoryItem("DETECTOR SHOVEL"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("DOWSING ROD", Plugin.ModItemManager.GetInventoryItem("DOWSING ROD"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("JACK HAMMER", Plugin.ModItemManager.GetInventoryItem("JACK HAMMER"), false, ItemSanityType.Workshop));
            Plugin.ModItemManager.AddItem(new UniqueItem("POWER HAMMER", Plugin.ModItemManager.GetInventoryItem("POWER HAMMER"), false, ItemSanityType.Workshop));

            // Special Shop Items
            Plugin.ModItemManager.AddItem(new UniqueItem("CHRONOGRAPH", Plugin.ModItemManager.GetInventoryItem("CHRONOGRAPH"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("EMERALD BRACELET", Plugin.ModItemManager.GetInventoryItem("EMERALD BRACELET"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("MASTER KEY", Plugin.ModItemManager.GetInventoryItem("MASTER KEY"), false, ItemSanityType.SpecialShop, true, false, ["Locksmith"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("MOON PENDANT", Plugin.ModItemManager.GetInventoryItem("MOON PENDANT"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("ORNATE COMPASS", Plugin.ModItemManager.GetInventoryItem("ORNATE COMPASS"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("SILVER SPOON", Plugin.ModItemManager.GetInventoryItem("SLIVER SPOON"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("MORNING STAR", Plugin.ModItemManager.GetInventoryItem("MORNING STAR"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("THE AXE", Plugin.ModItemManager.GetInventoryItem("THE AXE"), false, ItemSanityType.SpecialShop));
            Plugin.ModItemManager.AddItem(new UniqueItem("KNIGHTS SHIELD", Plugin.ModItemManager.GetInventoryItem("KNIGHTS SHIELD"), false, ItemSanityType.SpecialShop, true, false, ["Dig"]));
            Plugin.ModItemManager.AddItem(new UniqueItem("TORCH", Plugin.ModItemManager.GetInventoryItem("KNIGHTS SHIELD"), false, ItemSanityType.SpecialShop));

            //Permanent Items
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Dice 1", null, false, "Dice", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Dice 2", null, false, "Dice", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Keys 1", null, false, "Keys", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Keys 2", null, false, "Keys", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 1", null, false, "Steps", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 2", null, false, "Steps", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 5", null, false, "Steps", 5));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 10", null, false, "Steps", 10));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Gems 1", null, false, "Gems", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Gems 2", null, false, "Gems", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Luck 1", null, false, "Luck", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Luck 2", null, false, "Luck", 2));
            


            //Junk Items
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Allowance 1", null, false, "Allowance", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Allowance 2", null, false, "Allowance", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Stars 1", null, false, "Stars", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Stars 2", null, false, "Stars", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Stars 5", null, false, "Stars", 5));
            Plugin.ModItemManager.AddItem(new JunkItem("Dug Up Nothing", null, true, "Nothing", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Gold 1", null, true, "Gold", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Gold 2", null, true, "Gold", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Gold 5", null, true, "Gold", 5));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Dice 1", null, true, "Dice", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Dice 2", null, true, "Dice", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Dice 4", null, true, "Dice", 4));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Gems 1", null, true, "Gems", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Gems 2", null, true, "Gems", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Keys 1", null, true, "Keys", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Keys 2", null, true, "Keys", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Steps 1", null, true, "Steps", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Steps 2", null, true, "Steps", 2));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Steps 5", null, true, "Steps", 5));

            //Traps
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 1", "Steps", -1));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 2", "Steps", -2));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 5", "Steps", -5));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Stars 1", "Stars", -1));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Stars 2", "Stars", -2));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Stars 5", "Stars", -5));
            Plugin.ModItemManager.AddTrap(new SetTrap("Trap Set Steps 1", "Steps", 1));
            Plugin.ModItemManager.AddTrap(new SetTrap("Trap Set Steps 10", "Steps", 10));
            Plugin.ModItemManager.AddTrap(new EndOfDayTrap("Trap End Day", "EOD"));
            Plugin.ModItemManager.AddTrap(new FreezeTrap("Trap Freeze Items", "Freeze"));
            Plugin.ModItemManager.AddTrap(new LoseItemTrap("Trap Lose Item", "Lose Item"));

            //TODO Add PermanentUnlocks (Eg. Orchard)
        }

        public static void  ReloadGameObjects() {
            foreach (UniqueItem item in ModItemManager.UniqueItemList) {
                string name = GetObjName(item.Name);
                GameObject gameObj = Plugin.ModItemManager.GetInventoryItem(name);
                if (gameObj == null) {
                    gameObj = GameObjectExtensions.FindGameObject(name);
                }
                item.GameObj = gameObj;
            }
        }
        private static string GetObjName(string name) {
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
