using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppTMPro;
#endif
using Il2CppSystem.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BluePrinceArchipelago.Items
{

    /// <summary>
    ///     The Manager for the Items the mod manages. 
    /// </summary>
    public static class ModItemManager
    {
        public static List<ResourceItem> PermanentItemList = [];
        public static List<ArchipelagoUniqueItem> UniqueItemList = new();
        public static List<ResourceItem> JunkItemList = new();
        public static List<GameUniqueItem> GameUniqueItemList = [];
        public static List<Trap> TrapList = new();
        public static Dictionary<string, ModItem> ItemDict = new(); //Item name is the key, the type of item is the value.

        /// <summary>
        ///     Adds a Unique item to the Unique item list if it's not already tracked.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public static void AddItem(ArchipelagoUniqueItem item)
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
        public static void AddItem(ResourceItem itemToAdd)
        {
            foreach (ModItem item in JunkItemList)
            {
                if (item.Name == itemToAdd.Name)
                {
                    item.StackItem();
                    return;
                }
            }
            ItemDict[itemToAdd.Name] = itemToAdd;
            JunkItemList.Add(itemToAdd);
        }

        /// <summary>
        ///     Gets a UniqueItem.
        ///     Not case sensitive.
        /// </summary>
        /// <param name="name">The name of the unique item to find.</param>
        /// <returns>The UniqueItem or null if not found.</returns>
        public static ArchipelagoUniqueItem GetUniqueItem(string name)
        {
           
            foreach (ArchipelagoUniqueItem item in UniqueItemList)
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
        public static ResourceItem GetJunkItem(string name)
        {
            foreach (ResourceItem item in JunkItemList)
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
        public static ResourceItem GetPermanentItem(string name)
        {
            foreach (ResourceItem item in PermanentItemList)
            {
                if (item.Name.ToLower() == name.ToLower())
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        ///     Gets a trap by name
        /// </summary>
        /// <param name="name">The name of the trap to find.</param>
        /// <returns>The trap if it exists, or null if it doesn't.</returns>
        public static Trap GetTrap(string name)
        {
            return TrapList.FirstOrDefault(trap => trap.Name.ToLower() == name.ToLower());
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
        public static void OnItemCheckReceived(ItemInfo itemInfo)
        {
            Logging.LogWarning(itemInfo.ItemName);
            ModItem item = null;
            //If item exists, retreive it.
            ResourceItem permanentItem = GetPermanentItem(itemInfo.ItemName);
            if (permanentItem != null)
            {
                permanentItem.IsUnlocked = true;
                permanentItem.StackItem();
            }
            else if (ItemDict.ContainsKey(itemInfo.ItemName))
            {
                item = ItemDict[itemInfo.ItemName];
                item.AddToWorld();
                return;
            }
            else
            {
                Logging.Log($"Unable to give {itemInfo.ItemName} to player. The item doesn't exist or isn't currently handled by the mod.");
            }
        }

        public static void RegisterItems()
        {
            //Unique Items
            //  Keys
            AddItem(new ArchipelagoUniqueItem(ITEM.CAR_KEYS, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.KEYCARD, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.SILVER_KEY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.KEY_8, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.BASEMENT_KEY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.VAULT_KEY_149, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.VAULT_KEY_233, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.VAULT_KEY_304, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.VAULT_KEY_370, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.DIARY_KEY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.PRISM_KEY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.KEY_OF_ARIES, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.SECRET_GARDEN_KEY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.MICROCHIP_SMALL, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.MICROCHIP_MIDDLE_SIZED, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.MICROCHIP_LARGE, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.CABINET_KEY_PATIO, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.CABINET_KEY_LAUNDRY, ItemSanityType.Key));
            AddItem(new ArchipelagoUniqueItem(ITEM.CABINET_KEY_TUNNEL, ItemSanityType.Key));

            //  Standard Items     
            AddItem(new ArchipelagoUniqueItem(ITEM.BATTERY_PACK, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.BROKEN_LEVER, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.MAGNIFYING_GLASS, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.METAL_DETECTOR, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.SHOVEL, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.SLEDGE_HAMMER, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.TELESCOPE, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.RUNNING_SHOES, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.SALT_SHAKER, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.SLEEPING_MASK, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.COIN_PURSE, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.COUPON_BOOK, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.LOCK_PICK_KIT, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.LUCKY_RABBITS_FOOT, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.TREASURE_MAP, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.STOPWATCH, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.REPELLENT, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.WATERING_CAN, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.LUNCH_BOX, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.CURSED_EFFIGY, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.CROWN, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.PAPER_CROWN, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.GEAR_WRENCH, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.COMPASS, ItemSanityType.Standard));
            AddItem(new ArchipelagoUniqueItem(ITEM.HALL_PASS, ItemSanityType.Standard));

            // Workshop Items - TODO
            AddItem(new ArchipelagoUniqueItem(ITEM.ELECTROMAGNET, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.LUCKY_PURSE, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.PICK_SOUND_AMPLIFIER, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.BURNING_GLASS, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.DETECTOR_SHOVEL, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.DOWSING_ROD, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.JACK_HAMMER, ItemSanityType.Workshop));
            AddItem(new ArchipelagoUniqueItem(ITEM.POWER_HAMMER, ItemSanityType.Workshop));

            // Showroom            
            AddItem(new ArchipelagoUniqueItem(ITEM.CHRONOGRAPH, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.EMERALD_BRACELET, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.MASTER_KEY, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.MOON_PENDANT, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.ORNATE_COMPASS, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.SILVER_SPOON, ItemSanityType.SpecialShop));

            // Armory - TODO
            AddItem(new ArchipelagoUniqueItem(ITEM.MORNING_STAR, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.THE_AXE, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.KNIGHTS_SHIELD, ItemSanityType.SpecialShop));
            AddItem(new ArchipelagoUniqueItem(ITEM.TORCH, ItemSanityType.SpecialShop));


            // Permanent Items
            AddItem(new ResourceItem("1 Permanent Dice", false, "Dice", 1, true));
            AddItem(new ResourceItem("2 Permanent Dice", false, "Dice", 2, true));
            AddItem(new ResourceItem("1 Permanent Keys", false, "Keys", 1, true));
            AddItem(new ResourceItem("2 Permanent Keys", false, "Keys", 2, true));
            AddItem(new ResourceItem("1 Permanent Steps", false, "Steps", 1, true));
            AddItem(new ResourceItem("2 Permanent Steps", false, "Steps", 2, true));
            AddItem(new ResourceItem("5 Permanent Steps", false, "Steps", 5, true));
            AddItem(new ResourceItem("10 Permanent Steps", false, "Steps", 10, true));
            AddItem(new ResourceItem("1 Permanent Gems", false, "Gems", 1, true));
            AddItem(new ResourceItem("2 Permanent Gems", false, "Gems", 2, true));
            AddItem(new ResourceItem("1 Permanent Luck", false, "Luck", 1, true));
            AddItem(new ResourceItem("2 Permanent Luck", false, "Luck", 2, true));

            // Junk Items
            AddItem(new ResourceItem("1 Allowance", false, "Allowance", 1));
            AddItem(new ResourceItem("2 Allowance", false, "Allowance", 2));
            AddItem(new ResourceItem("1 Stars", false, "Stars", 1));
            AddItem(new ResourceItem("2 Stars", false, "Stars", 2));
            AddItem(new ResourceItem("5 Stars", false, "Stars", 5));
            AddItem(new ResourceItem("Dug Up Nothing", true, "Nothing", 1));
            AddItem(new ResourceItem("1 Gold", true, "Gold", 1));
            AddItem(new ResourceItem("2 Gold", true, "Gold", 2));
            AddItem(new ResourceItem("5 Gold", true, "Gold", 5));
            AddItem(new ResourceItem("1 Dice", true, "Dice", 1));
            AddItem(new ResourceItem("2 Dice", true, "Dice", 2));
            AddItem(new ResourceItem("4 Dice", true, "Dice", 4));
            AddItem(new ResourceItem("1 Gems", true, "Gems", 1));
            AddItem(new ResourceItem("2 Gems", true, "Gems", 2));
            AddItem(new ResourceItem("1 Keys", true, "Keys", 1));
            AddItem(new ResourceItem("2 Keys", true, "Keys", 2));
            AddItem(new ResourceItem("3 Keys", true, "Keys", 3));
            AddItem(new ResourceItem("1 Steps", true, "Steps", 1));
            AddItem(new ResourceItem("2 Steps", true, "Steps", 2));
            AddItem(new ResourceItem("5 Steps", true, "Steps", 5));

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
    }
}
