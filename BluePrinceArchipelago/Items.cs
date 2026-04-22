using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Utils;
using Il2CppSystem.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Core
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
        public void AddItem(PermanentItem itemToAdd, int count = 1)
        {
            foreach (ModItem item in PermanentItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.Count += 1;
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
                PermanentItem item = new PermanentItem(name, gameObject, isUnlocked, itemType);
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
                if (item.Name.ToLower().Equals(name.ToLower()))
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
                if (item.Name.ToLower().Equals(name.ToLower()))
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
                if (item.Name.ToLower().Equals(name.ToLower()))
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
            if (ItemDict.ContainsKey(itemInfo.ItemName))
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
                int index = Random.Range(0, count);
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
    }

    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;

        public string ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        private bool _IsUnlocked = isUnlocked;
        public new bool IsUnlocked
        {
            get { return _IsUnlocked; }
            set { _IsUnlocked = value; }
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
            else if (_ItemType == "Allowance")
            {
                //TODO Replace with a proper allowance function.
                AdjustGold(_Count * 2);
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
        //Todo replace with allowance.
        private void AdjustGold(int count = 1)
        {
            ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
            ModInstance.GoldManager.SendEvent("Update"); // Might need to be "Add Coins" instead.
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
        private void AdjustStars(int count = 1)
        {
            if (!GameObject.Find("__SYSTEM/HUD/Stars").active)
            {
                //Activate stars to ensure it can properly be updated.
                GameObject.Find("__SYSTEM/HUD/Stars").SetActive(true);
            }
            int totalStars = ModInstance.StarManager.FindIntVariable("TotalStars").Value;
            if (totalStars + count > 0)
            {
                ModInstance.StarManager.FindIntVariable("TotalStars").Value += count;
            }
            else
            {
                ModInstance.StarManager.FindIntVariable("TotalStars").Value = 0;
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

        public void OnFind(string location)
        {
            if (!FoundLocations.Contains(location.ToUpper()))
            {
                FoundLocations.Add(location.ToUpper());
                if (RecievedLocations.Contains(location.ToUpper()))
                {

                }
            }
        }

        public void AddItemToInventory(string location)
        {
            if (!RecievedLocations.Contains(location.ToUpper()))
            {
                RecievedLocations.Add(location.ToUpper());
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


            //Permanent Items
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Allowance 1", null, false, "Allowance", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Allowance 2", null, false, "Allowance", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Dice 1", null, false, "Dice", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Dice 2", null, false, "Dice", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Keys 1", null, false, "Keys", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Keys 2", null, false, "Keys", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 1", null, false, "Steps", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Steps 2", null, false, "Steps", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Gems 1", null, false, "Gems", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Gems 2", null, false, "Gems", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Luck 1", null, false, "Luck", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Starting Luck 2", null, false, "Luck", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Steps 5", null, false, "Steps", 5));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 1", null, false, "Stars", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 2", null, false, "Stars", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 5", null, false, "Stars", 5));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 1", null, false, "Stars", 1));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 2", null, false, "Stars", 2));
            Plugin.ModItemManager.AddItem(new PermanentItem("Extra Stars 5", null, false, "Stars", 5));

            //Junk Items
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
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Keys 3", null, true, "Keys", 3));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Steps 1", null, true, "Steps", 1));
            Plugin.ModItemManager.AddItem(new JunkItem("Extra Steps 2", null, true, "Steps", 2));

            //Traps
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 1", "Steps", -1));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 2", "Steps", -2));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 5", "Steps", -5));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 1", "Stars", -1));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 2", "Stars", -2));
            Plugin.ModItemManager.AddTrap(new LoseTrap("Trap Take Steps 5", "Stars", -5));
            Plugin.ModItemManager.AddTrap(new EndOfDayTrap("Trap End Day", "EOD"));
            Plugin.ModItemManager.AddTrap(new FreezeTrap("Trap Freeze Items", "Freeze"));
            Plugin.ModItemManager.AddTrap(new LoseItemTrap("Trap Lose Item", "Lose Item"));

            //TODO Add PermanentUnlocks (Eg. Orchard)
        }
    }
}
