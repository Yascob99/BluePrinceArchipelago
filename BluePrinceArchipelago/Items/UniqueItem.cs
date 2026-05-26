using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using LibCpp2IL;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


namespace BluePrinceArchipelago.Items
{
    public class UniqueItem(string name, GameObject gameObject, bool isUnlocked, ItemSanityType sanityType = ItemSanityType.None, bool isPreSpawn = true) : ModItem(name, gameObject, isUnlocked)
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

        private ItemSanityType _SanityType = sanityType;
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

        public void RemoveFromPool()
        {
            if (!ApplySanity())
            {
                return;
            }
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
            if (!ApplySanity())
            {
                return;
            }
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
            if (Plugin.ModItemManager.IsItemSpawnable(GameObj, !isSpawned && IsPrespawn))
            {
                string iconName = Name.ToTitleCase() + " Icon(Clone)001";
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
                        if (UniqueItemManager.ComissaryStates.ContainsKey(Name))
                        {
                            //Re-enable commissary purchases of the item.
                            Plugin.UniqueItemManager.EnableCommissaryPurchase(this, UniqueItemManager.ComissaryStates[Name]);
                        }
                        ModItemManager.PickedUp.Add(GameObj, "GameObject");
                        InventoryIcons.Add(icon, "GameObject");
                        ArchipelagoConsole.LogMessage($"Added {Name} to inventory.");
                    }
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

        // A Map of item names to the states in the Comissary.
        public static readonly Dictionary<string, string> ComissaryStates = new Dictionary<string, string>{
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

                    }
                    else if (item.ModelReplaced) {

                    }
                }
                else if (obj.name.ToUpper().Contains("UPGRADE"))
                {
                    //Replace with AP item code to be added here.
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
                }
                else if (uniqueItem.IsUnlocked)
                {
                    EnableCommissaryPurchase(uniqueItem, item.Value);
                }
            }
        }

        //Finds the associated Pickup State and replaces the item.
        private FsmState ReplacePickup(UniqueItem item)
        {
            FsmState state = GetPickupState(item.Name);
            if (state != null)
            {
                //If the item is not unlocked, prevent it from being added to inventory.
                if (!item.IsUnlocked && item.ApplySanity())
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
                if (!item.IsUnlocked && item.ApplySanity())
                {
                    //Disable the actions that add the item to inventory.
                    state.DisableActionsOfType<ArrayListAdd>();
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
                if (!item.IsUnlocked && item.ApplySanity())
                {
                    //Disable the actions that add the item to inventory.
                    state.DisableActionsOfType<ArrayListAdd>();
                    //Disables the You found Text (for now).
                    state.DisableFirstActionOfType<ActivateGameObject>();
                }
                SpawnedItems.Add(Plugin.ModItemManager.GetUniqueItem(item.Name));
                return;
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
            //ReplaceCommissaryItemsWithAP();
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
            name = name.ToLower().Replace("vault", "safety deposit").Replace("rabbit's", "rabbbit's");
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
