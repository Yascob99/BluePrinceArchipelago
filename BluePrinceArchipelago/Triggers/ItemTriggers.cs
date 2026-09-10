using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Triggers
{
    /// <summary>
    ///     Triggers related to Items
    /// </summary>
    public static class ItemTriggers
    {
        /// <summary>
        ///     Triggered by a Unique item being spawned.
        /// </summary>
        /// <param name="obj">The GameObject of the spawned item.</param>
        /// <param name="poolName">The PoolName of the spawned item's spawn pool.</param>
        /// <param name="transformObj">The GameObject with contains the position data for where the item will be spawned.</param>
        /// <param name="spawnedObj">The GameObject for the spawned object.</param>
        public static void OnAfterItemSpawned(GameObject obj, string poolName, GameObject transformObj, GameObject spawnedObj) 
        {
            UniqueItem item = Plugin.ModItemManager.GetUniqueItem(obj.name);
            //Check if Connected in before replacing items.
            if (ArchipelagoClient.Authenticated)
            {
                if (item != null)
                {
                    FsmState state = Plugin.UniqueItemManager.GetPickupState(obj.name);
                    // If the item is not already in the inventory
                    if (item.IsUnlocked)
                    {
                        //Re-enable the previously disabled actions.
                        if (ModItemManager.PickedUp.Contains(obj.name))
                        {
                            state.EnableFirstActionOfType<ArrayListAdd>();
                        }
                        else
                        {
                            state.EnableActionsOfType<ArrayListAdd>();
                        }
                    }
                    else if (item.HasBeenFound)
                    {
                        // If the item has been found before but isn't unlocked, destroy the spawned object.
                        Logging.LogWarning("Despawning Item.");
                        GameObject.Destroy(spawnedObj);
                    }
                }
            }
            else if (obj.name.ToUpper().Trim().Contains("UPGRADE DISK"))
            {
                string CurrentRoom = GameObject.Find("__SYSTEM/HUD/Room Text").GetComponent<PlayMakerFSM>().GetStringVariable("Current Room").Value;
                CurrentRoom = CurrentRoom.ToUpper().Replace("'", "").Replace("POST", "POST DYNAMITE"); // HLC, TP Dynamite
                ModItemManager.UpgradeDisks.OnSpawn(CurrentRoom, spawnedObj);
            }
        }

        /// <summary>
        ///     Triggers whenever an item is Traded at the Trading Post
        /// </summary>
        /// <param name="OfferedItem">The GameObject of the item that was traded for.</param>
        /// <param name="Icon">The GameObject of the inventory item for the item that was traded for.</param>
        public static void OnItemTraded(GameObject OfferedItem, GameObject Icon)
        {
            if (OfferedItem != null)
            {
                string itemName = OfferedItem.name;
                Logging.Log($"Traded for {itemName}", "Trades");
                GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                if (itemName.Contains("UPGRADE DISK"))
                {
                    if (ArchipelagoOptions.UpgradeDiskSanity)
                    {
                        UpgradeDiskTriggers.OnUpgradeDiskTraded();
                    }
                    else
                    {
                        if (Icon != null && InventoryIcons != null)
                        {
                            InventoryIcons.Add(Icon, "GameObject");
                        }
                    }
                }
                else
                {
                    UniqueItem Item = Plugin.ModItemManager.GetUniqueItem(itemName);

                    if (Item != null)
                    {
                        if (Item.IsUnlocked)
                        {

                            if (Icon != null && InventoryIcons != null)
                            {
                                ModItemManager.PickedUp.AddIfUnique(OfferedItem);
                                InventoryIcons.Add(Icon, "GameObject");
                                ModItemManager.PreSpawn.RemoveIfExists(itemName);
                            }
                        }
                        else
                        {
                            ModItemManager.PickedUp.RemoveIfExists(itemName);
                            ModItemManager.PreSpawn.AddIfUnique(OfferedItem);
                        }
                        if (!Item.HasBeenFound)
                        {
                            Item.HasBeenFound = true;
                            ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
                            Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Triggers after a Unique Item has been picked up.
        /// </summary>
        /// <param name="Item">the Unique Item of the item that was picked up.</param>
        public static void OnAfterItemPickup(UniqueItem Item) {
            if (!Item.HasBeenFound)
            {
                Item.HasBeenFound = true;
                Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
            }
        }

        /// <summary>
        ///     Triggers before a Unique Item has been picked up.
        /// </summary>
        /// <param name="Item">the Unique Item of the item that was picked up.</param>
        public static void OnBeforeItemPickup(UniqueItem Item) {
            // Handle the rare case of the item being spawned and the unlock for that item arriving before it has been picked up.
            if (Item.IsUnlocked)
            {
                // Re-enable the logic that adds the item to inventory. (Will not cause issues if already enabled).
                FsmState state = Plugin.UniqueItemManager.GetPickupState(Item.Name);
                if (state != null)
                {
                    state.EnableActionsOfType<ArrayListAdd>();
                }
            }
            Item.HasBeenFound = true;
        }

        /// <summary>
        ///     Triggers on a Unique Item being bought.
        /// </summary>
        /// <param name="Item">the Unique Item of the item that was picked up.</param>
        public static void OnItemBought(UniqueItem Item) {
            if (!Item.HasBeenFound)
            {
                Item.HasBeenFound = true;
                Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
            }
            Item.HasBeenFound = true;
        }

        /// <summary>
        ///     Triggers on a Unique Item being dug up.
        /// </summary>
        /// <param name="Item">the Unique Item of the item that was picked up.</param>
        public static void OnItemDugUp(UniqueItem Item) {
            if (!Item.HasBeenFound)
            {
                if (Item.ApplySanity())
                {
                    Item.HasBeenFound = true;
                    Plugin.ModItemManager.RemoveUniqueItemAPSwirly(Item);
                    ModInstance.QueueManager.AddLocationToQueue($"{Item.Name.ToTitleCase()} First Pickup");
                }
            }
        }

    }
}
