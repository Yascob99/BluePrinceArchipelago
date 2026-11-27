using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ML
using Il2Cpp;
#endif

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for manipulating items in the inventory.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class ItemCommand(string name) : Command(name)
    {
        private string _Description = "Adds or Removes Items from the inventory.";
        public override string Description
        {
            get { return _Description; }
        }
        private string _Syntax = "Usage\n/Item Add <Item>\n/Item Remove <Item>\n/Item List <prespawn|estateitems|pickedup|coatcheck|useditems>";
        public override string Syntax
        {
            get { return _Syntax; }
        }
        public override void Run(List<string> Args)
        {
            if (!ModInstance.IsInRun)
            {
                ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
                return;
            }
            if (Args.Count > 1)
            {
                string subcommand = Args[0];
                if (subcommand.ToLower() == "list")
                {
                    ArchipelagoConsole.LogMessage($"Item List\n{ModItemManager.ListItems(Args[1])}");
                    return;
                }
                else if (subcommand.ToLower() == "add")
                {
                    string itemName = Args[1];
                    for (int i = 2; i < Args.Count; i++)
                    {
                        itemName += " " + Args[i];
                    }

                    ArchipelagoConsole.LogMessage($"Attemping to add item {itemName}");

                    GameObject item = ModItemManager.GetInventoryItem(itemName);

                    //Handle items that don't start in the prespawn pool.
                    if (item == null)
                    {
                        string iconName = Plugin.UniqueItemManager.GetIconName("UPGRADE DISK");
                        GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                        PlayMakerFSM Inventory = InventoryGO.GetFsm("Inventory Icons");
                        PlayMakerArrayListProxy iconList = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/InventoryIcons").GetComponent<PlayMakerArrayListProxy>();
                        PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                        GameObject icon = null;
                        foreach (var invIcon in iconList.arrayList)
                        {
                            GameObject iconGo = invIcon.TryCast<GameObject>();
                            if (iconGo != null)
                            {
                                if (iconGo.name.Contains(iconName))
                                {
                                    icon = iconGo;
                                }
                            }
                        }
                        if (icon != null && InventoryIcons != null)
                        {
                            InventoryIcons.Add(icon, "GameObject");
                            ModItemManager.PickedUp.Add(item, "GameObject");
                            //Send Event 0 to the Global Manager.
                            Inventory.SendEvent("Update");
                            return;
                        }
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} is not a valid Item Name");
                        return;
                    }
                    else
                    {
                        // Check PreSpawn EstateItems, PickedUp, CoatCheck, UsedItems
                        if (ModItemManager.IsItemSpawnable(item) || true)
                        {
                            GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                            PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                            GameObject icon = Plugin.UniqueItemManager.GetIconGameObject(item.name);

                            if (icon != null && InventoryIcons != null)
                            {
                                ModItemManager.PickedUp.AddIfUnique(item);
                                InventoryIcons.Add(icon, "GameObject");
                                ModItemManager.PreSpawn.RemoveIfExists(item.name);
                                if (Name == ITEM.RUNNING_SHOES)
                                {
                                    ModInstance.RunningEngine.SendEvent("Update");
                                }
                                if (Name == ITEM.STOPWATCH)
                                {
                                    GameObject.Find("__SYSTEM/STOPWATCH controller").GetComponent<PlayMakerFSM>().SendEvent("Begin");
                                }
                                return;
                            }
                            ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} is not a valid Item Name");
                            return;
                        }
                    }
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Can't be added to inventory.");
                    return;
                }
                else if (subcommand.ToLower() == "remove")
                {
                    string itemName = Args[1];
                    for (int i = 2; i < Args.Count; i++)
                    {
                        itemName += " " + Args[i];
                    }

                    ArchipelagoConsole.LogMessage($"Attemping to remove item {itemName}");
                    
                    GameObject item = ModItemManager.GetPickedUpItem(itemName);
                    
                    if (item == null)
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} is not a valid Item Name or is not in your Inventory");
                        return;
                    }

                    GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                    PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                    GameObject icon = Plugin.UniqueItemManager.GetIconGameObject(item.name);

                    //Logging.LogWarning(icon != null);
                    //Logging.LogWarning(InventoryIcons != null);
                    if (icon != null && InventoryIcons != null)
                    {
                        //Logging.LogWarning(ModItemManager.PickedUp.Contains(item.name));
                        if (ModItemManager.PickedUp.Contains(item.name))
                        {
                            ModItemManager.PickedUp.Remove(item, "GameObject");
                        }
                        InventoryIcons.Remove(icon, "GameObject");

                        if (itemName == ITEM.RUNNING_SHOES)
                        {
                            ModInstance.RunningEngine.SendEvent("Update");
                        }
                        /* need to implement timer pause when removing stopwatch from inventory
                        if (item.name == "STOPWATCH")
                        {
                            GameObject.Find("__SYSTEM/STOPWATCH controller").GetComponent<PlayMakerFSM>().SendEvent("End"); //not sure what the event is to pause the stopwatch
                        }
                        */
                        return;
                    }

                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} can't be removed from inventory.");
                    return;

                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
                return;
            }
            else
                ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
        }
    }
}
