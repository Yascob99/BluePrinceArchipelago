using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (itemName.Contains("UPGRADE DISK"))
                {
                    if (ArchipelagoOptions.UpgradeDiskSanity)
                    {
                        ModItemManager.UpgradeDisks.OnTrade();
                    }
                    else
                    {
                        GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                        PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");

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
                            GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                            PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");

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
                            if (Item.IsCommissary)
                            {
                                FsmState state = Item.CommissaryState;
                                if (state != null)
                                {
                                    // If the item is not unlocked, prevent it from being added to inventory.
                                    if (Item.IsUnlocked && Item.ApplySanity())
                                    {
                                        //Disable the actions that add the item to inventory.
                                        state.EnableActionsOfType<ArrayListAdd>();
                                        // Check if the event we are trying to remove is the custom event we added.
                                        SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                                        if (CustomEvent.sendEvent.Name.Contains("Commissary"))
                                        {
                                            state.RemoveFirstActionOfType<SendEvent>();
                                        }
                                    }
                                }
                            }
                            if (Item.IsDig)
                            {
                                FsmState state = Item.DigState;
                                if (state != null)
                                {
                                    // If the item is not unlocked, prevent it from being added to inventory.
                                    if (Item.IsUnlocked && Item.ApplySanity())
                                    {
                                        //Disable the actions that add the item to inventory.
                                        state.EnableActionsOfType<ArrayListAdd>();
                                        SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                                        // Check if the event we are trying to remove is the custom event we added.
                                        if (CustomEvent.sendEvent.Name.Contains("Dug Up"))
                                        {
                                            state.RemoveFirstActionOfType<SendEvent>();
                                        }
                                    }
                                }
                            }
                            if (Item.IsLocksmith)
                            {
                                FsmState state = Item.LocksmithState;
                                if (state != null)
                                {
                                    // If the item is not unlocked, prevent it from being added to inventory.
                                    if (Item.IsUnlocked && Item.ApplySanity())
                                    {
                                        //Disable the actions that add the item to inventory.
                                        state.EnableActionsOfType<ArrayListAdd>();
                                        SendEvent CustomEvent = state.GetLastActionOfType<SendEvent>();
                                        // Check if the event we are trying to remove is the custom event we added.
                                        if (CustomEvent.sendEvent.Name.Contains("Locksmith"))
                                        {
                                            state.RemoveFirstActionOfType<SendEvent>();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
