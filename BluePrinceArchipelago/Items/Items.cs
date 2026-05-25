using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using HarmonyLib;
using HutongGames.PlayMaker;
using Il2CppSystem.Collections;
using Newtonsoft.Json.Linq;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            PreSpawn = GameObject.Find("__SYSTEM/Inventory/Inventory (PreSpawn)")?.GetArrayListProxy("Inventory (PreSpawn)");
            EstateItems = GameObject.Find("__SYSTEM/Inventory/Inventory (EstateItems)")?.GetArrayListProxy("Inventory (EstateItems)");
            PickedUp = GameObject.Find("__SYSTEM/Inventory/Inventory (PickedUp)")?.GetArrayListProxy("Inventory (PickedUp)");
            CoatCheck = GameObject.Find("__SYSTEM/Inventory/Inventory (CoatCheck)")?.GetArrayListProxy("Inventory (CoatCheck)");
            UsedItems = GameObject.Find("__SYSTEM/Inventory/Inventory (UsedItems)")?.GetArrayListProxy("Inventory (UsedItems)");
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
                if (!item.HasBeenFound)
                {
                    GameObject prefab = ArchipelagoPrefabs.GetPrefab(item.Name);
                    if (prefab != null)
                    {

                        GameObject spawnObj = FindSpawnObject(item.Name);
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
                        }
                        else
                        {
                            Logging.LogWarning($"Unable to change spawn prefab for {item.Name}, error finding prefab with name: {item.Name}(Clone)001");
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
            if (scoutname == "")
            {
                scoutname = itemName;
            }
            GameObject You___Text = GameObject.Find("UI OVERLAY CAM/You Found Text")?.gameObject;
            if (You___Text != null)
            {
                for (int t = 0; t < You___Text.transform.childCount; t++)
                {
                    Transform child = You___Text.transform.GetChild(t);
                    if (child.gameObject.name.Contains(GetYou___Name(itemName)))
                    {
                        //TODO handle special case names (EG Crown of blueprints)
                        Transform itemModel = child.FindRecursive(itemName, true);
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
                            GameObject textPrefab = ArchipelagoPrefabs.GetPrefab("You Found Text Template");

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
                                string description = "Hope it un-BK's them!";

                                string[] itemWords = scoutItemName.Split(" ");
                                if (itemWords.Length < 4)
                                {
                                    scoutItemName = itemWords.Join("\n");
                                }
                                else {
                                    scoutItemName = scoutItemName.Minragged();
                                }
                                int FirstLetterCount = 0;
                                int ItemNameCount = 0;
                                int DescriptionCount = 0;
                                // Update all the fonts and words to be correct

                                Transform textObjects = child.Find("Text/GameObject");
                                
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
                                            Vector3 localPos;
                                            Quaternion localRot;
                                            Prescription.GetLocalPositionAndRotation(out localPos, out localRot);
                                            textChild.SetLocalPositionAndRotation(localPos, localRot);
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
                                                    Vector3 localPos;
                                                    Quaternion localRot;
                                                    FirstLetter.GetLocalPositionAndRotation(out localPos, out localRot);
                                                    textChild.SetLocalPositionAndRotation(localPos, localRot);
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
                                                    Vector3 localPos;
                                                    Quaternion localRot;
                                                    ItemName.GetLocalPositionAndRotation(out localPos, out localRot);
                                                    textChild.SetLocalPositionAndRotation(localPos, localRot);
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

                                                Vector3 localPos;
                                                Quaternion localRot;
                                                Description.GetLocalPositionAndRotation(out localPos, out localRot);
                                                textChild.SetLocalPositionAndRotation(localPos, localRot);
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
                        else {
                            Logging.LogWarning($"Unable to find the item model for {itemName.ToTitleCase()}");
                        }
                    }
                }
            }
            else
            {
                Logging.LogWarning($"Unable to find the 'You Found Text' GameObject.");
            }
        }

        private string GetYou___Name(string name) {
            name = name.ToTitleCase();
            if (name == "Magnifying Glass") {
                name = "Mag Glass";
            }
            if (name == "Lock Pick Kit") {
                name = "Lock Pick";
            }
            if (name.Contains("Key")) {
                name = name.Replace("Vault ", "").Trim();
            }
            return name;
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
            GameObject prefab = ArchipelagoPrefabs.GetPrefab("UPGRADE DISK");
            if (prefab != null) {
                GameObject APswirly = prefab.transform.GetChild(0)?.gameObject;
                for (int i = 1; i < 17; i++)
                {
                
                    if (APswirly != null)
                    {
                        // Get the APswirly Component of the Prefab and reparent it to the spawn prefab.
                        GameObject spawnObj = ModInstance.PickupSpawnPool.transform.FindChild($"UPGRADEDISK(Clone)00{i}")?.gameObject;
                        if (spawnObj != null)
                        {
                            APswirly.transform.parent = spawnObj.transform;
                        }
                    }
                }
                // Replaces the You Found and You bought Message for the Upgrade disk with the Commissary Message. Will Replace the others programmatically. 
                ReplaceAPItemNotifications("UPGRADE DISK", ModInstance.PickupSpawnPool.transform.FindChild("UPGRADEDISK(Clone)001")?.gameObject, "UPGRADE DISK COMISSARY"); 
            }
        }

        private GameObject FindSpawnObject(string name) {
            string instanceName = name + "(Clone)001";
            Logging.Log(name);
            GameObject spawnObj = ModInstance.PickupSpawnPool.transform.FindChild(instanceName)?.gameObject;
            if (spawnObj == null) {
               spawnObj =  GameObjectExtensions.FindGameObject(name);
            }
            return spawnObj;
        }

        // Adds a unique item if it doesn't already exist.
        public void AddItem(UniqueItem item)
        {
            bool found = false;
            int counter = -1;
            // check if room already exists in the room pool
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
                PermanentItem item = new PermanentItem(name, gameObject, isUnlocked, itemType, 1, count);
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

        public void StartOfDay(int dayNum)
        {
            AddAllPermanenentItems();
        }
        // returns true if item was released from queue, returns false if no item in queue to release or failed to release the item.

        // Adds all permanent items to inventory, meant to be run at start of day.
        public void AddAllPermanenentItems()
        {
            if (PermanentItemList.Count > 0)
            {
                foreach (PermanentItem item in PermanentItemList)
                {
                    if (item.IsUnlocked)
                    {
                        Logging.Log($"Adding {item.Count} {item.Name}(s)");
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
                permanentItem.unlockedCount += 1;
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
        public GameObject GetPreSpawnItem(string itemName)
        {
            for (int i = 0; i < PreSpawn.GetCount(); i++)
            {
                GameObject prespawnItem = PreSpawn.arrayList[i].TryCast<GameObject>();
                if (prespawnItem != null)
                {
                    if (prespawnItem.name.Trim().ToLower() == itemName.ToLower())
                    {
                        return prespawnItem;
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
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }
        private void AdjustGems(int count = 1)
        {
            ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value = count;
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }
        private void AdjustGold(int count = 1)
        {
            ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }
        private void AdjustDice(int count = 1)
        {
            ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.DiceManager.SendEvent("Update");
        }
        private void AdjustKeys(int count = 1)
        {
            ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = count;
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
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value += 1;
            }
            else
            {
                ModInstance.GlobalPersistentManager.GetIntVariable("TotalStars").Value = 0;
            }
            ModInstance.StarManager.SendEvent("Update");
        }
    }

    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int poolcount, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;
        private PlayMakerFSM _DayFSM = GameObject.Find("DAY").GetFsm("FSM");
        public int unlockedCount = 0;
        public int poolcount = poolcount;

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
                AdjustGems(unlockedCount * _Count);
            }
            else if (_ItemType == "Steps")
            {
                AdjustSteps(unlockedCount * _Count);
            }
            else if (_ItemType == "Gold")
            {
                AdjustGold(unlockedCount * _Count);
            }
            else if (_ItemType == "Allowance")
            {
                AdjustAllowance(unlockedCount * _Count);
            }
            else if (_ItemType == "Dice")
            {
                AdjustDice(unlockedCount * _Count);
            }
            else if (_ItemType == "Keys")
            {
                AdjustKeys(unlockedCount * _Count);
            }
            else if (_ItemType == "Luck")
            {
                AdjustLuck(unlockedCount * _Count);
            }
            else
            {
                Logging.LogWarning($"{_ItemType} is an invalid type, or is not currently supported.");
            }
        }
        private void AdjustGems(int count = 1)
        {
            ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value = count;
            // I think sound would be neat since it's more noticeable.
            ModInstance.GemManager.SendEvent("Update with Sound");
        }
        private void AdjustSteps(int count = 1)
        {
            // change the adjustment amount.
            ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
            // Send the "Update" event and the step counter should update.
            ModInstance.StepManager.SendEvent("Update");
        }
        //Todo replace with allowance.
        private void AdjustGold(int count = 1)
        {
            ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
        }

        private void AdjustAllowance(int count = 1)
        {
            _DayFSM.FindIntVariable("allowance").Value += count;
        }

        //Todo replace with allowance.
        private void AdjustDice(int count = 1)
        {
            ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.DiceManager.SendEvent("Update");
        }
        private void AdjustKeys(int count = 1)
        {
            ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.KeyManager.SendEvent("Update");
        }
        private void AdjustLuck(int count = 1)
        {
            int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
            if (luck + count > 0)
            {
                ModInstance.LuckManager.FindIntVariable("LUCK").Value = luck + count;
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
        // The locations to which the upgrade disk received has been found at.
        public List<string> RecievedLocations = new List<string>();
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
        public List<EventID> EventNames = [EventID.Upgrade_Disk_Archives_found, EventID.Upgrade_Disk_BootLeg_found, EventID.Upgrade_Disk_Cloister_found, EventID.Upgrade_Disk_Commissary_found, EventID.Upgrade_Disk_Foundation_found, EventID.Upgrade_Disk_Freezer_found, EventID.Upgrade_Disk_Garage_found, EventID.Upgrade_Disk_GreatHall_found, EventID.Upgrade_Disk_LostFound_found, EventID.Upgrade_Disk_MasterBedroom_found, EventID.Upgrade_Disk_Mechanarium_found, EventID.Upgrade_Disk_MorningRoom_found, EventID.Upgrade_Disk_Office_found, EventID.Upgrade_Disk_TradingPost_found, EventID.Upgrade_Disk_Vault_found, EventID.Upgrade_Disk_TorchRoom_found];
        public List<string> usedVariables = ["Upgrade Disc - Archives", "Upgrade Disc - Bootleg", "Upgrade Disc - Cloister", "Upgrade Disc - Commissary", "Upgrade Disc - Foundation", "Upgrade Disc - Freezer", "Upgrade Disc - Garage", "Upgrade Disc - Great Hall", "Upgrade Disc - LostFound", "Upgrade Disc - Master Bedroom", "Upgrade Disc - Mechanarium", "Upgrade Disc - Morning Room", "Upgrade Disc - Office", "Upgrade Disc - Shop", "Upgrade Disc - Tomb", "Upgrade Disc - Torch Room"];
        public new GameObject GameObj = gameObject;

        // Updates the unlocked status of the upgrade disks.
        public void UpdateUnlocked() {
            // Updates the unlocked state of the upgrade disk. (A fallback for some edgecases.)
            foreach (string location in Locations) {
                FsmBool unlocked = ModInstance.GlobalManager.GetBoolVariable(location.ToTitleCase() + " Disk");
                if (unlocked != null)
                {
                    if (ArchipelagoClient.Authenticated)
                    {
                        if (RecievedLocations.Contains(location))
                        {
                            unlocked.Value = true;
                        }
                        else
                        {
                            unlocked.Value = false;
                        }
                    }
                    else {
                        unlocked.Value = true; // Forces default behaviour on not connected.
                    }
                }
            }
        }
        // Handles adding unlocked upgrade disks to the the players inventory until they are used.
        public void StartOfDay() {
            int i = 0;
            foreach (string location in RecievedLocations) {
                if (!ModInstance.GlobalManager.GetBoolVariable(usedVariables[i]).Value) { 
                    AddItemToInventory(location);
                }
                i++;
            }
        }

        // Handles the pickup of the Upgrade Disk. The Vanilla code handles the rest.
        public void OnPickup() {
            string roomname = ModInstance.RoomText.GetStringVariable("Current Room").Value;
            roomname = roomname.ToUpper().Replace("'", "").Replace("POST", "POST DYNAMITE").Replace(" AND", "&"); // HLC, TP Dynamite, and Lost & Found name fix
            OnFind(roomname);
        }

        // Sends the location for the found upgrade disk.
        private void OnFind(string location)
        {
            if (!FoundLocations.Contains(location.ToUpper()))
            {
                FoundLocations.Add(location.ToUpper());

                if (RecievedLocations.Contains(location.ToUpper()))
                {
                    AddItemToInventory(location);
                }
                //Fix location name for pickup event.
                location = location.Replace("LADYSHIPS", "LADYSHIP's");
                ModInstance.ModEventHandler.OnUgradeDiskFound(location);
            }
        }

        public void AddItemToInventory(string location)
        {
            if (!RecievedLocations.Contains(location.ToUpper()))
            {
                RecievedLocations.Add(location.ToUpper());

            }
            int locationIndex = Locations.IndexOf(location) + 1; //Blue Prince tends to use 1-indexing for some things.
            if (locationIndex != -1) {
                PlayMakerArrayListProxy pickedUp = GameObj?.GetArrayListProxy("upgrade disk pickup");
                if (pickedUp != null)
                {
                    if (!pickedUp.Contains(locationIndex)) {
                        PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                        if (InventoryIcons != null) {
                            string iconName = Name.ToTitleCase() + " Icon(Clone)001"; //Unsure if multiple clones will be needed when multiple disks are present.
                            GameObject icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                            if (icon != null)
                            {
                                // Set the pickup of the upgrade disk to true to prevent future spawns
                                ModInstance.GlobalManager.GetBoolVariable("DISK - Garage").Value = true;
                                ModInstance.GlobalPersistentManager.GetBoolVariable("?Upgrade").Value = true;
                                // Unsure why this is set, but probably important
                                ModInstance.GlobalManager.GetIntVariable("NewCursor").Value = 0;
                                // Add item to inventory icons
                                InventoryIcons.Add(icon, "GameObject");
                                // Add the index to the list of picked up upgrade disks.
                                pickedUp.Add(locationIndex, "Int");
                                // Add item to prespawn icons.
                                ModItemManager.PickedUp.Add(Plugin.ModItemManager.GetPreSpawnItem("UPGRADE DISK"), "GameObject");
                                // Record the Upgrade disk being picked up.
                                return;
                            }
                        }
                    }
                    Logging.Log($"Upgrade disk for {location} has already been picked up. Unable to add to inventory");
                }
            }
        }
    }

    //TODO Later for a later goal. The locations they are found at is different from where they can be used. Should not persist across days
    public class SanctumKeys(string name, GameObject gameObject, int count = 0) : ProgressiveItems(name, gameObject, false, 1, true)
    {

    }

    public static class RegisterItems
    {
        public static void ParseAndRegisterFillerItems(string itemName, int poolcount)
        {
            if (itemName.ToLower().Contains("trap"))
            {
                if (itemName.ToLower().Contains("eod"))
                {
                    Plugin.ModItemManager.AddTrap(new EndOfDayTrap("Trap End Day", "EOD"));
                }
                else if (itemName.ToLower().Contains("freeze"))
                {
                    Plugin.ModItemManager.AddTrap(new FreezeTrap("Trap Freeze Items", "Freeze"));
                }
                else if (itemName.ToLower().Contains("lose"))
                {
                    Plugin.ModItemManager.AddTrap(new LoseItemTrap("Trap Lose Item", "Lose Item"));
                }
                else
                {
                    if (itemName.ToLower().Contains("luck"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Luck", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else if (itemName.ToLower().Contains("dice"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Dice", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else if (itemName.ToLower().Contains("keys"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Keys", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else if (itemName.ToLower().Contains("steps"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Steps", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else if (itemName.ToLower().Contains("gems"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Gems", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else if (itemName.ToLower().Contains("gems"))
                    {
                        Plugin.ModItemManager.AddTrap(new LoseTrap(itemName, "Star", int.Parse(itemName[itemName.Length - 1].ToString())));
                    }
                    else
                    {
                        Logging.Log($"\"{itemName}\" could not be identified as a junk item or is not currently implemented in the mod.");
                    }
                }
            }
            else if (itemName.ToLower().Contains("allowance"))
            {
                Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Allowance", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
            }
            else if (itemName.ToLower().Contains("starting"))
            {
                if (itemName.ToLower().Contains("luck"))
                {
                    Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Luck", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("dice"))
                {
                    Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Dice", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("keys"))
                {
                    Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Keys", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("steps"))
                {
                    Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Steps", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("gems"))
                {
                    Plugin.ModItemManager.AddItem(new PermanentItem(itemName, null, false, "Gems", poolcount, int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else
                {
                    Logging.Log($"\"{itemName}\" could not be identified as a starting item or is not currently implemented in the mod");
                }
                // Stars will be handled as junk items as they don't need to be handled at start of day.
            }
            else if (itemName.ToLower().Contains("nothing"))
            {
                Plugin.ModItemManager.AddItem(new JunkItem("Dug Up Nothing", null, true, "Nothing", poolcount));
            }
            // Junk Items
            else {
                if (itemName.ToLower().Contains("luck"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Luck", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("dice"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Dice", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("keys"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Keys", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("steps"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Steps", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("gems"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Gems", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else if (itemName.ToLower().Contains("gems"))
                {
                    Plugin.ModItemManager.AddItem(new JunkItem(itemName, null, false, "Stars", int.Parse(itemName[itemName.Length - 1].ToString())));
                }
                else {
                    Logging.Log($"\"{itemName}\" could not be identified as a junk item or is not currently implemented in the mod.");
                }
            }
        }

        public static void Register()
        {
            //Unique Items
            if (ArchipelagoOptions.KeySanity)
            {
                Plugin.ModItemManager.AddItem(new UniqueItem("CAR KEYS", Plugin.ModItemManager.GetPreSpawnItem("CAR KEYS"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("KEYCARD", Plugin.ModItemManager.GetPreSpawnItem("KEYCARD"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("SILVER KEY", Plugin.ModItemManager.GetPreSpawnItem("SILVER KEY"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("KEY 8", Plugin.ModItemManager.GetPreSpawnItem("KEY 8"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("BASEMENT KEY", Plugin.ModItemManager.GetPreSpawnItem("BASEMENT KEY"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 149", Plugin.ModItemManager.GetPreSpawnItem("VAULT KEY 149"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 233", Plugin.ModItemManager.GetPreSpawnItem("VAULT KEY 233"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 304", Plugin.ModItemManager.GetPreSpawnItem("VAULT KEY 304"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("VAULT KEY 370", Plugin.ModItemManager.GetPreSpawnItem("VAULT KEY 370"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("DIARY KEY", Plugin.ModItemManager.GetPreSpawnItem("DIARY KEY"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("PRISM KEY_0", Plugin.ModItemManager.GetPreSpawnItem("PRISM KEY_0"), false, false));
                Plugin.ModItemManager.AddItem(new UniqueItem("KEY of Aries", null, false, false));
                Plugin.ModItemManager.AddItem(new UniqueItem("SECRET GARDEN KEY", Plugin.ModItemManager.GetPreSpawnItem("SECRET GARDEN KEY"), false));
            }


            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 1", Plugin.ModItemManager.GetPreSpawnItem("MICROCHIP 1"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 2", Plugin.ModItemManager.GetPreSpawnItem("MICROCHIP 2"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("MICROCHIP 3", Plugin.ModItemManager.GetPreSpawnItem("MICROCHIP 3"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 1", Plugin.ModItemManager.GetPreSpawnItem("CABINET KEY 1"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 2", Plugin.ModItemManager.GetPreSpawnItem("CABINET KEY 2"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("CABINET KEY 3", Plugin.ModItemManager.GetPreSpawnItem("CABINET KEY 3"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("BATTERY PACK", Plugin.ModItemManager.GetPreSpawnItem("BATTERY PACK"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("BROKEN LEVER", Plugin.ModItemManager.GetPreSpawnItem("BROKEN LEVER"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("MAGNIFYING GLASS", Plugin.ModItemManager.GetPreSpawnItem("MAGNIFYING GLASS"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("METAL DETECTOR", Plugin.ModItemManager.GetPreSpawnItem("METAL DETECTOR"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("SHOVEL", Plugin.ModItemManager.GetPreSpawnItem("SHOVEL"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("SLEDGE HAMMER", Plugin.ModItemManager.GetPreSpawnItem("SLEDGE HAMMER"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("TELESCOPE", Plugin.ModItemManager.GetPreSpawnItem("TELESCOPE"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("RUNNING SHOES", Plugin.ModItemManager.GetPreSpawnItem("RUNNING SHOES"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("SALT SHAKER", Plugin.ModItemManager.GetPreSpawnItem("SALT SHAKER"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("SLEEPING MASK", Plugin.ModItemManager.GetPreSpawnItem("SLEEPING MASK"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("COIN PURSE", Plugin.ModItemManager.GetPreSpawnItem("COIN PURSE"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("COUPON BOOK", Plugin.ModItemManager.GetPreSpawnItem("COUPON BOOK"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("LOCK PICK KIT", Plugin.ModItemManager.GetPreSpawnItem("LOCK PICK KIT"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("LUCKY RABBIT'S FOOT", Plugin.ModItemManager.GetPreSpawnItem("LUCKY RABBIT'S FOOT"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("TREASURE MAP", Plugin.ModItemManager.GetPreSpawnItem("TREASURE MAP"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("STOPWATCH", Plugin.ModItemManager.GetPreSpawnItem("STOPWATCH"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("REPELLENT", null, false, false));
            Plugin.ModItemManager.AddItem(new UniqueItem("WATERING CAN", Plugin.ModItemManager.GetPreSpawnItem("WATERING CAN"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("LUNCH BOX", null, false, false));
            Plugin.ModItemManager.AddItem(new UniqueItem("CURSED EFFIGY", null, false, false));
            Plugin.ModItemManager.AddItem(new UniqueItem("CROWN", Plugin.ModItemManager.GetPreSpawnItem("CROWN"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("PAPER CROWN", Plugin.ModItemManager.GetPreSpawnItem("PAPER CROWN"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("GEAR WRENCH", Plugin.ModItemManager.GetPreSpawnItem("GEAR WRENCH"), false));
            Plugin.ModItemManager.AddItem(new UniqueItem("COMPASS", Plugin.ModItemManager.GetPreSpawnItem("COMPASS"), false));

            if (ArchipelagoOptions.WorkshopSanity)
            {
                Plugin.ModItemManager.AddItem(new UniqueItem("ELECTROMAGNET", GameObjectExtensions.FindGameObject("ELECTROMAGNET"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("LUCKY PURSE", GameObjectExtensions.FindGameObject("LUCKY PURSE"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("PICK SOUND AMPLIFIER", GameObjectExtensions.FindGameObject("PICK SOUND AMPLIFIER"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("BURNING GLASS", GameObjectExtensions.FindGameObject("BURNING GLASS"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("DETECTOR SHOVEL", GameObjectExtensions.FindGameObject("DETECTOR SHOVEL"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("DOWSING ROD", GameObjectExtensions.FindGameObject("DOWSING ROD"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("JACK HAMMER", GameObjectExtensions.FindGameObject("JACK HAMMER"), false));
                Plugin.ModItemManager.AddItem(new UniqueItem("POWER HAMMER", GameObjectExtensions.FindGameObject("POWER HAMMER"), false));
            }

            foreach (var entry in ArchipelagoOptions.FillerItemDistribution) {
                if (entry.Value > 0) {
                    ParseAndRegisterFillerItems(entry.Key, entry.Value);
                }
            }

            //TODO Add PermanentUnlocks (Eg. Orchard)
        }

    }
    
}
