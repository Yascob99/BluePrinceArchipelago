using Archipelago.MultiClient.Net.Models;
using Cpp2IL.Core.ISIL;
using HutongGames.PlayMaker.Actions;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace BluePrinceArchipelago.Core
{
    public class ModItemManager
    {
        public static Queue<string> ItemQueue = new(); // stores a string representing which queue to pull from next.
        public static Queue<ModItem> GenericItemQueue = new(); 
        public static Queue<UniqueItem> UniqueItemQueue = new();
        public static Queue<JunkItem> JunkItemQueue = new();
        public static List<PermanentItem> PermanentItemList = []; //Permanent items do not need to use a queue since any active Permanent Items will be added on day start.
        public static List<ModItem> GenericItemList = new();
        public static List<UniqueItem> UniqueItemList = new();
        public static List<JunkItem> JunkItemList = new();
        public static Dictionary<string, string> ItemDict = new(); //Item name is the key, the type of item is the value.


        public ModItemManager()
        {
        }
        public void Initialize() { 

        }
        // Adds a generic item, or 1 to it's count if it already exists.
        public void AddItem(ModItem itemToAdd, int count = 1) {
            foreach (ModItem item in GenericItemList) {
                if (item.Name == itemToAdd.Name) {
                    item.Count+=1;
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = "Generic";
            GenericItemList.Add(itemToAdd);
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
                ItemDict[item.Name] = "Unique";
                UniqueItemList.Add(item);
            }
            else
            {
                Plugin.BepinLogger.LogMessage($"Item {item.Name} already added, can't add multiple copies.");
            }
        }
        public void AddItem(JunkItem itemToAdd, int count = 1) {
            foreach (ModItem item in JunkItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.Count += 1;
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = "Junk";
            JunkItemList.Add(itemToAdd);
        }
        public void AddItem(PermanentItem itemToAdd, int count = 1) {
            foreach (ModItem item in PermanentItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.Count += 1;
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = "Permanent";
            PermanentItemList.Add(itemToAdd);
        }
        public void AddItem(string name, GameObject gameObject, bool isUnlocked, bool isUnique = false, bool isJunk = false, bool isPermanent = false,int count = 1, string itemType = null) {
            if (isUnique)
            {
                if (isJunk || isPermanent || itemType != null || count > 1 || count < 1)
                {
                    Plugin.BepinLogger.LogMessage($"{name} could not be added as a Unique item, invalid parameters");
                    return;
                }
                UniqueItem item = new UniqueItem(name, gameObject, isUnlocked);
                ItemDict[item.Name] = "Unique";
                UniqueItemList.Add(item);
            }
            else if (isJunk)
            {
                if (itemType == null || count == 0 || isPermanent)
                {
                    Plugin.BepinLogger.LogMessage($"{name} could not be added as a Junk/Trap item, invalid parameters");
                    return;
                }
                JunkItem item = new JunkItem(name, gameObject, isUnlocked, itemType, count);
                ItemDict[item.Name] = "Junk";
                JunkItemList.Add(item);
            }
            else if (isPermanent)
            {
                if (itemType == null || count < 1)
                {
                    Plugin.BepinLogger.LogMessage($"{name} could not be added as a Permanent Item, invalid parameters");
                    return;
                }
                PermanentItem item = new PermanentItem(name, gameObject, isUnlocked, itemType);
                ItemDict[item.Name] = "Permanent";
                PermanentItemList.Add(item);
            }
            else {
                if (itemType == null || count < 1) {
                    Plugin.BepinLogger.LogMessage($"{name} could not be added as a Generic Item, invalid parameters");
                    return;
                }
                ModItem item = new ModItem(name, gameObject, isUnlocked);
                ItemDict[item.Name] = "Generic";
                GenericItemList.Add(item);
            }
        }
        public ModItem GetGenericItem(string name) {
            foreach (ModItem item in GenericItemList)
            {
                if (!item.Name.Equals(name)) {
                    return item;
                }
            }
            return null;
        }
        public UniqueItem GetUniqueItem(string name)
        {
            foreach (UniqueItem item in UniqueItemList)
            {
                if (!item.Name.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }

        public JunkItem GetJunkItem(string name) {
            foreach (JunkItem item in JunkItemList)
            {
                if (!item.Name.Equals(name))
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
                if (!item.Name.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }

        public void StartOfDay(int dayNum) {
            AddAllPermanenentItems();
        }
        // returns true if item was released from queue, returns false if no item in queue to release or failed to release the item.
        public bool ReleaseNextItemInQueue() {
            if (ItemQueue.Count > 0) {
                string queueType = ItemQueue.Dequeue();
                if (queueType == "Generic")
                {
                    if (GenericItemQueue.Count > 0) {
                        GenericItemQueue.Dequeue().AddItemToInventory(); //Dequeue the next item to release.
                    }
                }
                else if (queueType == "Unique")
                {
                    if (UniqueItemQueue.Count > 0) { 
                        UniqueItemQueue.Dequeue().AddItemToInventory();
                    }
                }
                else if (queueType == "Junk")
                {
                    if (JunkItemQueue.Count > 0)
                    {
                        JunkItemQueue.Dequeue().AddItemToInventory();
                    }
                }
                else {
                    Plugin.BepinLogger.LogWarning($"{queueType} is not a valid Queue type, no item to release.");
                }
            }
            return false;
        }

        // Adds all permanent items to inventory, meant to be run at start of day.
        public void AddAllPermanenentItems() {
            if (PermanentItemList.Count > 0) {
                foreach (PermanentItem item in PermanentItemList) {
                    if (item.IsUnlocked) {
                        item.AddItemToInventory();
                    }
                }
            }
        }

        public void OnItemCheckRecieved(ItemInfo item) {
            // Handle the code for recieving an item check that results in receiving an item.
            if (ItemDict.ContainsKey(item.ItemName))
            {
                Plugin.BepinLogger.LogMessage($"Unable to find item: {item.ItemName}");
                return;
            }
            string itemType = ItemDict[item.ItemName];

            // Permanent items are added at start of day, add them to the list, then don't worry about them.
            if (itemType == "Permanent")
            {
                ModItemManager.PermanentItemList.Add(GetPermanentItem(item.ItemName));
            }
            else if (ModInstance.IsInRun)
            {


                if (itemType == "Generic")
                {
                    GetGenericItem(item.ItemName).AddItemToInventory();
                }
                else if (itemType == "Unique")
                {
                    GetUniqueItem(item.ItemName).AddItemToInventory();
                }
                else if (itemType == "Junk")
                {
                    GetJunkItem(item.ItemName).AddItemToInventory();
                }
                else
                {
                    Plugin.BepinLogger.LogMessage($"Invalid item type: {itemType} for {item.ItemName}.");
                }
            }
            else {
                if (itemType == "Generic")
                {
                    GetGenericItem(item.ItemName).AddItemToInventory();
                }
                else if (itemType == "Unique")
                {
                    GetUniqueItem(item.ItemName).AddItemToInventory();
                }
                else if (itemType == "Junk")
                {
                    GetJunkItem(item.ItemName).AddItemToInventory();
                }
                else
                {
                    Plugin.BepinLogger.LogMessage($"Invalid item type: {itemType} for {item.ItemName}.");
                }
            }
        }
    }

    public class ModItem( string name, GameObject gameObject, bool isUnlocked, int count = 1){
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
            set { _Count = value;  }
        }
        private bool _IsUnique = false;
        public bool IsUnique { 
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }

        private bool _HasBeenFound = false;

        public bool HasBeenFound {
            get { return _HasBeenFound; }
            set
            {
                //Send the room drafted event on the first time this room is drafted only.
                if (!_HasBeenFound && value)
                {
                    ModInstance.Instance.ModEventHandler.OnFirstFound(this);
                    _HasBeenFound = value;
                }
                // No changes to value once the room has been drafted once, or if someone is not trying to set this to true for some stupid reason.
            }
        }

        public virtual void AddItemToInventory() { 
            //TODO add code handling adding the item.
        }
        public virtual void AddItemToQueue() { 
            ModItemManager.GenericItemQueue.Enqueue(this);
            ModItemManager.ItemQueue.Enqueue("Generic");
        }
    }
    public class UniqueItem(string name, GameObject gameObject, bool isUnlocked) : ModItem(name, gameObject, isUnlocked) {
        
        private bool _IsUnique = true; //override IsUnique.

        public override void AddItemToQueue()
        {
            ModItemManager.UniqueItemQueue.Enqueue(this);
            ModItemManager.ItemQueue.Enqueue("Unique");
        }
        public override void AddItemToInventory() { }
    }

    // handles junk and trap items (as inverse traps).
    public class JunkItem(string name, GameObject gameObject, bool isUnlocked, string itemType, int count = 1) : ModItem(name, gameObject, isUnlocked) {

        private string _ItemType = itemType;
        public string Itemtype 
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        private int _Count = count;
        public new int Count { 
            get { return _Count; } 
            set 
            {
                if (value > 0)
                {
                    _IsTrap = true; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                else { 
                    _IsTrap = false; //Sets IsTrap dynamically (not sure that it's needed, but it's neat).
                }
                _Count = value; 
            }
        }

        private bool _IsTrap = count < 0; 
        public bool IsTrap { 
            get { return _IsTrap; } //No setter since this is connected to count
        }
        public override void AddItemToQueue()
        {
            ModItemManager.JunkItemQueue.Enqueue(this);
            ModItemManager.ItemQueue.Enqueue("Junk");
        }

        public override void AddItemToInventory() {
            //TODO add code handling adding the item.
        }
    }

    public class PermanentItem(string name, GameObject gameObject, bool isUnlocked, string itemType) : ModItem(name, gameObject, isUnlocked)
    {
        private string _ItemType = itemType;

        public string ItemType 
        { 
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        private bool _IsUnlocked = true;
        public new bool IsUnlocked { 
            get { return _IsUnlocked; }
            set { _IsUnlocked = value; }
        }
        public override void AddItemToInventory() {
            //TODO add code handling adding the item.
        }
    }
}
