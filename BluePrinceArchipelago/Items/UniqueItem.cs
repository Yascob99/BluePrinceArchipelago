using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
                    ModInstance.ModEventHandler.OnFirstFound(this);
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
                string iconName = Name.ToTitleCase() + " Icon(Clone)001";
                GameObject icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                // Some icons use 
                if (icon == null)
                {
                    iconName = Name.ToTitleCase() + " icon(Clone)001";
                    icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                }
                if (icon == null)
                {
                    iconName = Name.ToTitleCase();
                    icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                }
                PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                if (icon != null && InventoryIcons != null)
                {
                    Logging.LogWarning("Here 2");

                    ModItemManager.PickedUp.Add(GameObj, "GameObject");
                    InventoryIcons.Add(icon, "GameObject");
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

            // Fixes a name difference for the vault keys and rabbit's foot and puts name into lower case.
            name = name.ToLower().Replace("vault", "saftey deposit").Replace("rabbit's", "rabbbit's").Replace(" kit", "");
            // Check each Global Transition in the Global Manager.
            foreach (FsmTransition transition in ModInstance.GlobalManager.FsmGlobalTransitions)
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
