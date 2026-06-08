using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil;
using PathologicalGames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BluePrinceArchipelago.Items
{
    public class UniqueItem : ModItem
    {

        private bool _IsUnique = true;
        public new bool IsUnique
        {
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }
        private bool _IsPrespawn;

        public bool IsPrespawn { get; set; }

        private bool _HasBeenFound = false;

        private ItemSanityType _SanityType;
        public ItemSanityType SanityType
        {
            get { return _SanityType; }
            set { _SanityType = value; }
        }

        public bool ModelReplaced { get; set; }

        public bool HasBeenFound
        {
            get { return _HasBeenFound; }
            set
            {
                // Send the item found event on the first time it is found.
                if (!_HasBeenFound && value)
                {
                    _HasBeenFound = value;
                }
                // No changes to value once the item has been found once, or if someone is trying to set this to false some reason.
            }
        }
        public UniqueItem(string name, GameObject gameObject, bool isUnlocked, ItemSanityType sanityType = ItemSanityType.None, bool isPreSpawn = true) : base(name, gameObject, isUnlocked)
        {
            _IsPrespawn = isPreSpawn;
            _SanityType = sanityType;
            FSMEventHandler.AddFSMEvent(name, this);
        }

        public void RemoveFromPool()
        {
            if (!ApplySanity())
            {
                return;
            }
            //If item has been found and is not unlocked, remove it from the pool. Otherwise Vanilla behavior.
            if (HasBeenFound && !IsUnlocked)
            {
                if (ModItemManager.PreSpawn.Contains(GameObj))
                {
                    ModItemManager.PreSpawn.Remove(GameObj, "GameObject");
                }
                if (ModItemManager.EstateItems.Contains(GameObj))
                {
                    ModItemManager.EstateItems.Remove(GameObj, "GameObject");
                }
            }
        }

        public override void AddItemToInventory()
        {
            if (!ApplySanity())
            {
                return;
            }
            if (!IsUnlocked)
            {
                IsUnlocked = true;
            }
            if (GameObj == null)
            {
                if (_IsPrespawn)
                {
                    GameObj = Plugin.ModItemManager.GetInventoryItem(Name);
                }
            }
            if (GameObj != null)
            {
                if (Commissary.CommissaryStates.ContainsKey(Name))
                {
                    //Re-enable commissary purchases of the item.
                    Commissary.CanStock.Add(Name);
                }
                // This may not cause it to re-trigger.
                // Disable this game action so it doesn't try and display 2 UIs.
                Logging.LogWarning("Attempting to add item to Inventory");
                GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                PlayMakerFSM Inventory = InventoryGO.GetFsm("Inventory Icons");
                PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                GameObject icon = Plugin.UniqueItemManager.GetIconGameObject(Name);
       
                if (icon != null && InventoryIcons != null)
                {
                    InventoryIcons.Add(icon, "GameObject");
                    if (!ModItemManager.PickedUp.Contains(Name)) {
                        ModItemManager.PickedUp.Add(GameObj, "GameObject");
                    }
                    if (Name == "RUNNING SHOES")
                    {
                        ModInstance.RunningEngine.SendEvent("Update");
                    }
                    //Send Event 0 to the Global Manager.
                }
               
            }
        }

        public bool ApplySanity()
        {
            return SanityType switch
            {
                ItemSanityType.None => false,
                ItemSanityType.Standard => ArchipelagoOptions.StandardItemSanity,
                ItemSanityType.Workshop => ArchipelagoOptions.WorkshopSanity,
                ItemSanityType.UpgradeDisk => ArchipelagoOptions.UpgradeDiskSanity,
                ItemSanityType.Key => ArchipelagoOptions.KeySanity,
                _ => false,
            };
        }
    }

    public class UniqueItemManager
    {
        public List<UniqueItem> SpawnedItems = new List<UniqueItem>();

        public bool ModelsReplaced = false;
        public void OnItemSpawn(GameObject obj, string poolName, GameObject transformObj, GameObject spawnedObj)
        {
            Logging.LogWarning(obj.name);
            UniqueItem item = Plugin.ModItemManager.GetUniqueItem(obj.name);
            //Check if Connected in before replacing items.
            if (ArchipelagoClient.Authenticated)
            {
                if (item != null)
                {
                    FsmState state = GetPickupState(obj.name);
                    if (item.IsUnlocked && item.HasBeenFound)
                    {
                        //Re-enable the previously disabled actions.
                        state.EnableActionsOfType<ArrayListAdd>();
                    }
                }
            }
        }

        public void EndOfDay()
        {
            //Reset the list of spawned items.
            SpawnedItems = new List<UniqueItem>();
            ModelsReplaced = false;
        }
        public void StartOfDay()
        {
            RemoveItemsFromPool();
        }

        private void RemoveItemsFromPool()
        {
            foreach (UniqueItem item in ModItemManager.UniqueItemList)
            {
                //If the item has been found once, remove it from
                item.RemoveFromPool();
            }
        }

        public string GetIconName(string name) {
            name = name.ToTitleCase();
            switch (name){
                case "Cabinet Key 1":
                    return "Cabinet Key Icon";
                case "Cabinet Key 2":
                    return "Cabinet Key Icon";
                case "Cabinet Key 3":
                    return "Cabinet Key Icon";
                case "Prism Key_0":
                    return "Prism Key Icon";
                case "Electromagnet":
                    return "Powered Electro Magnet Icon";
                case "Key 8":
                    return "Key 8";
                case "Lucky Rabbit's Foot":
                    return "Lucky rabbit's foot Icon";
                case "Salt Shaker":
                    return "Salt Icon";
                default:
                    return name + " Icon";
            }
        }
        public GameObject GetIconGameObject(string name) {
            name = GetIconName(name);
            PlayMakerArrayListProxy Inventory = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/InventoryIconMeshes").GetComponent<PlayMakerArrayListProxy>();
            for (int i=0; i < Inventory.arrayList.Count; i++)
            {
                GameObject child = Inventory.arrayList[i].TryCast<GameObject>();
                if (child.name.Contains(name)) {
                    return child;
                }
            }
            return null;
        }

        //Finds the "You Found" Event based on the what "You Found" is called in the GlobalManager Pickup FSM State. Returns null if not found.
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
                        Logging.Log(item.Name, "Events");
                        return item;
                    }
                }
            }
            return null;
        }

        // Finds the state in the Global Manager associated with the given item's pickup. Returns null if not found.
        public FsmState GetPickupState(string name)
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
                        if (name == "treasure map") {
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
        public FsmTransition GetPickupTransition(string name) {
            // Fixes a name difference for the vault keys and rabbit's foot and puts name into lower case.
            name = name.ToLower().Replace("vault", "saftey deposit").Replace("rabbit's", "rabbbit's").Replace(" kit", "");
            // Check each Global Transition in the Global Manager.
            foreach (FsmTransition transition in ModInstance.GlobalManager.FsmGlobalTransitions)
            {
                // If the transition's event name contains the item name it's the transition we want.
                if (transition.EventName.ToLower().Contains(name))
                {

                    return transition;
                }
            }
            return null;
        }

    }

    public enum ItemSanityType
    {
        None,
        Standard,
        Workshop,
        UpgradeDisk,
        Key,
        SpecialShop,
    }
}
