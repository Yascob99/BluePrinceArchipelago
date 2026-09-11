using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A Handler for all the Upgrade disks and their associated functions. Upgrade Disks persist across days.
    /// </summary>
    /// <param name="gameObject">The gameobject of upgradedisk. Defaults to null.</param>
    public class UpgradeDisks(GameObject gameObject = null) : GroupedItems("UPGRADE DISK", gameObject, false, 16, true)
    {
        public new List<string> ItemNames = ["ARCHIVES", "TRADING POST DYNAMITE", "TOMB", "COMMISSARY", "FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST AND FOUND", "HER LADYSHIPS CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        public new List<string> LocationNames = ["ARCHIVES", "TRADING POST TRADE", "TOMB", "COMMISSARY", "THE FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST & FOUND", "HER LADYSHIP'S CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        public static List<GameObject> YouFoundObjects = new List<GameObject>();
        public List<EventID> EventNames = [EventID.Upgrade_Disk_Archives_found, EventID.Upgrade_Disk_BootLeg_found, EventID.Upgrade_Disk_Cloister_found, EventID.Upgrade_Disk_Commissary_found, EventID.Upgrade_Disk_Foundation_found, EventID.Upgrade_Disk_Freezer_found, EventID.Upgrade_Disk_Garage_found, EventID.Upgrade_Disk_GreatHall_found, EventID.Upgrade_Disk_LostFound_found, EventID.Upgrade_Disk_MasterBedroom_found, EventID.Upgrade_Disk_Mechanarium_found, EventID.Upgrade_Disk_MorningRoom_found, EventID.Upgrade_Disk_Office_found, EventID.Upgrade_Disk_TradingPost_found, EventID.Upgrade_Disk_Vault_found, EventID.Upgrade_Disk_TorchRoom_found];
        public List<string> UsedVariables = ["Upgrade Disc - Archives", "Upgrade Disc - Bootleg", "Upgrade Disc - Cloister", "Upgrade Disc - Commissary", "Upgrade Disc - Foundation", "Upgrade Disc - Freezer", "Upgrade Disc - Garage", "Upgrade Disc - Great Hall", "Upgrade Disc - LostFound", "Upgrade Disc - Master Bedroom", "Upgrade Disc - Mechanarium", "Upgrade Disc - Morning Room", "Upgrade Disc - Office", "Upgrade Disc - Shop", "Upgrade Disc - Tomb", "Upgrade Disc - Torch Room"];
        public new GameObject GameObj = gameObject;

        /// <summary>
        ///     Unlocks an upgrade disk location if it exists and hasn't already been found.
        /// </summary>
        /// <param name="locationName">The name of the location.</param>
        /// <returns>If the location was unlocked.</returns>
        public bool UnlockLocationIfExists(string locationName)
        {
            foreach (string location in ItemNames)
            {
                string lowlocation = location.ToLower();
                if (locationName.ToLower().Contains(lowlocation))
                {
                    if (!FoundLocations.Contains(location))
                    {
                        FoundLocations.Add(location);
                        State.UpdateUpgradeDiskData();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Handles adding unlocked upgrade disks to the the players inventory until they are used.
        /// </summary>
        public void StartOfDay()
        {
            // Skip this Start of Day if it's a reconnect from crash or quit.
            Logging.LogWarning("Updating Upgrade Disk Used States");
            if (ArchipelagoOptions.UpgradeDiskSanity)
            {
                // Logging.LogWarning($"[{UsedLocations.Join(", ")}]");
                foreach (string location in RecievedItems)
                {
                    Logging.LogWarning($"{location}");
                    // Check if the item has been used, and if it has, 
                    if (!UsedLocations.Contains(location.ToUpper()))
                    {
                        AddItemToInventory(location);
                    }
                    if (FoundLocations.Contains(location.ToUpper()))
                    {
                        ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[ItemNames.IndexOf(location.ToUpper())]).Value = true;
                    }
                    else
                    {
                        ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[ItemNames.IndexOf(location.ToUpper())]).Value = false;
                    }
                }
                // Despawn Foundation Upgrade Disk
                if (ModItemManager.UpgradeDisks.FoundLocations.Contains("Foundation"))
                {
                    Transform FoundationSpawn = GameObject.Find("UNDERGROUND").transform.Find("Below Foundation (Cullable)").Find("Below Foundation - Prefab").Find("_GAMEPLAY").Find("5");
                    if (FoundationSpawn.childCount > 0)
                    {
                        Logging.LogWarning("Despawning Foundation Upgrade Disk.");
                        GameObject.Destroy(FoundationSpawn.GetChild(0).gameObject);
                    }
                }
            }
        }

        /// <summary>
        ///     Handles setting the game state when an upgrade disk is used.
        /// </summary>
        /// <param name="upgradeid">The Id of the upgrade disk.</param>
        public void OnUsed(int upgradeid)
        {
            Logging.Log($"Upgrade With ID {upgradeid} used.", "UpgradeDisks");
            if (RecievedItems.Count > UsedLocations.Count)
            {
                string location = ItemNames[upgradeid - 1];
                if (!UsedLocations.Contains(location))
                {
                    UsedLocations.Add(location);
                    State.UpdateUpgradeDiskData();
                }
                // Prevent the bool for being set for trading post trade specifically.
                if (FoundLocations.Contains(location) && location != "TRADING POST TRADE")
                {
                    ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[upgradeid - 1]).Value = true;

                }
                else
                {
                    ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[upgradeid - 1]).Value = false;
                }
            }
            else
            {
                Logging.LogWarning("Unable to set Location as used, no received locations are currently unused.", "UpgradeDisks");
            }
        }

        /// <summary>
        ///     Sends the location for the found upgrade disk.   
        /// </summary>
        /// <param name="location">The name of the location.</param>
        public void OnFind(string location)
        {
            Logging.Log("Location");
            if (!FoundLocations.Contains(location.ToUpper()))
            {
                FoundLocations.Add(location.ToUpper());
                State.UpdateUpgradeDiskData();
                ModInstance.ModEventHandler.OnUgradeDiskFound(LocationNames[ItemNames.IndexOf(location.ToUpper())]);
            }
            ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(UsedVariables[LocationNames.IndexOf(location.ToUpper())]).Value = true;
        }

        public void OnTrade()
        {
            if (!FoundLocations.Contains("TRADING POST TRADE"))
            {
                FoundLocations.Add("TRADING POST TRADE");
                State.UpdateUpgradeDiskData();
                ModInstance.ModEventHandler.OnUgradeDiskFound("TRADING POST TRADE");
            }
        }

        /// <summary>
        ///     A sanity check for despawning Upgrade disks that shouldn't be spawned.
        /// </summary>
        /// <param name="location">The location</param>
        /// <param name="spawnedObj">The spawned object.</param>
        public void OnSpawn(string location, GameObject spawnedObj)
        {
            if (spawnedObj != null)
            {
                if (FoundLocations.Contains(location.ToUpper()))
                {
                    GameObject.Destroy(spawnedObj);
                    Logging.LogWarning($"Despawned Upgrade Disk in {location}, since it has been found before.");
                }
                Logging.LogWarning($"Unable to despawn Upgrade Disk in {location}, location is not a valid location.");
                return;
            }
            Logging.LogWarning($"Unable to despawn Upgrade Disk in {location}, spawnedObj does not exist.");
        }

        /// <summary>
        ///     Handles adding an upgrade disk to the inventory.
        /// </summary>
        /// <param name="item">The item name of the Upgrade Disk.</param>
        public void AddItemToInventory(string item)
        {
            Logging.LogWarning("Attempting To Add Upgrade Disk to Inventory.");
            if (!RecievedItems.Contains(item))
            {
                RecievedItems.Add(item);
            }
            // If UpgradeDiskSanity is off, prevent adding it to inventory.
            if (!ArchipelagoOptions.UpgradeDiskSanity)
            {
                return;
            }
            GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
            PlayMakerFSM Inventory = InventoryGO.GetFsm("Inventory Icons");
            PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
            GameObject icon = Plugin.UniqueItemManager.GetIconGameObject("UPGRADE DISK");
            PlayMakerArrayListProxy UpgradeDisks = GameObject.Find("__SYSTEM/Upgrade Disks").GetArrayListProxy("upgrade disk pickup");

            if (icon != null && InventoryIcons != null)
            {
                // Prevent adding the same Upgrade disk multiple times.
                int upgradeid = ItemNames.IndexOf(item) + 1;
                if (!UpgradeDisks.Contains(upgradeid))
                {
                    UpgradeDisks.Add(upgradeid, "Integer");
                    ModItemManager.PickedUp.Add(ModItemManager.GetInventoryItem("UPGRADE DISK"), "GameObject");
                    InventoryIcons.Add(icon, "GameObject");
                }
            }

        }

        /// <summary>
        ///     Internal. Initializes the notification UI objects for when Upgrade Disks are bought or purchased.
        /// </summary>
        public static void InitializeUpgradeDiskNotifications()
        {
            GameObject YouBoughtUpgradeDisk = GameObject.Find("UI OVERLAY CAM/You Found Text/You Bought Upgrade Disk").gameObject;
            GameObject YouFoundUpgradeDisk = GameObject.Find("UI OVERLAY CAM/You Found Text/You Found Upgrade Disk").gameObject;
            GameObject ArchivesDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            ArchivesDiskNotification.SetActive(false);
            ArchivesDiskNotification.name = "You Found Upgrade Disk - Archives";

            GameObject TradingPostDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            TradingPostDiskNotification.SetActive(false);
            TradingPostDiskNotification.name = "You Found Upgrade Disk - Trading Post Dynamite";

            GameObject TombDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            TombDiskNotification.SetActive(false);
            TombDiskNotification.name = "You Found Upgrade Disk - Tomb";

            GameObject CommissaryDiskNotification = GameObject.Instantiate(YouBoughtUpgradeDisk, YouBoughtUpgradeDisk.transform.parent);
            CommissaryDiskNotification.SetActive(false);
            CommissaryDiskNotification.name = "You Bought Upgrade Disk - Commissary";

            GameObject FoundationDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            FoundationDiskNotification.SetActive(false);
            FoundationDiskNotification.name = "You Found Upgrade Disk - Foundation";

            GameObject FreezerDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            FreezerDiskNotification.SetActive(false);
            FreezerDiskNotification.name = "You Found Upgrade Disk - Freezer";

            GameObject GarageDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            GarageDiskNotification.SetActive(false);
            GarageDiskNotification.name = "You Found Upgrade Disk - Garage";

            GameObject GreatHallDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            GreatHallDiskNotification.SetActive(false);
            GreatHallDiskNotification.name = "You Found Upgrade Disk - Great Hall";

            GameObject LostAndFoundDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            LostAndFoundDiskNotification.SetActive(false);
            LostAndFoundDiskNotification.name = "You Found Upgrade Disk - Lost And Found";

            GameObject HLCDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            HLCDiskNotification.SetActive(false);
            HLCDiskNotification.name = "You Found Upgrade Disk - Her Ladyships Chamber";

            GameObject MechanariumDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            MechanariumDiskNotification.SetActive(false);
            MechanariumDiskNotification.name = "You Found Upgrade Disk - Mechanarium";

            GameObject MorningRoomDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            MorningRoomDiskNotification.SetActive(false);
            MorningRoomDiskNotification.name = "You Found Upgrade Disk - Morning Room";

            GameObject OfficeDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            OfficeDiskNotification.SetActive(false);
            OfficeDiskNotification.name = "You Found Upgrade Disk - Office";

            GameObject VaultDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            VaultDiskNotification.SetActive(false);
            VaultDiskNotification.name = "You Found Upgrade Disk - Vault";

            GameObject AbandonedMineDiskNotification = GameObject.Instantiate(YouFoundUpgradeDisk, YouFoundUpgradeDisk.transform.parent);
            AbandonedMineDiskNotification.SetActive(false);
            AbandonedMineDiskNotification.name = "You Found Upgrade Disk - Abandoned Mine";

            YouFoundObjects = [ArchivesDiskNotification, TradingPostDiskNotification, TombDiskNotification, CommissaryDiskNotification, FoundationDiskNotification, FreezerDiskNotification, GarageDiskNotification, GreatHallDiskNotification, LostAndFoundDiskNotification, HLCDiskNotification, MechanariumDiskNotification, MorningRoomDiskNotification, OfficeDiskNotification, null, VaultDiskNotification, AbandonedMineDiskNotification];
        }
    }
}
