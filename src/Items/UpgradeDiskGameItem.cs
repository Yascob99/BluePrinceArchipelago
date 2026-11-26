using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using UnityEngine;
#if Bep
using PathologicalGames;
#endif
#if ML
using Il2Cpp;
using Il2CppPathologicalGames;
#endif

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A Handler for all the Upgrade disks and their associated functions. Upgrade Disks persist across days.
    /// </summary>
    public class UpgradeDisksGameItem : GameItem
    {
        private readonly string _upgradeDiskName;
        private readonly string _locationName;
        private readonly string _usedVariableName;
        private readonly int _index;
        private bool _received = false;
        private bool _used = false;
        private bool _found = false;


        private static readonly List<string> _itemNames = ["ARCHIVES", "TRADING POST DYNAMITE", "TOMB", "COMMISSARY", "FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST AND FOUND", "HER LADYSHIPS CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        private static readonly List<string> _locationNames = ["ARCHIVES", "TRADING POST TRADE", "TOMB", "COMMISSARY", "THE FOUNDATION", "FREEZER", "GARAGE", "GREAT HALL", "LOST & FOUND", "HER LADYSHIP'S CHAMBER", "MECHANARIUM", "MORNING ROOM", "OFFICE", "TRADING POST TRADE", "VAULT", "ABANDONED MINE"];
        private static readonly List<string> _usedVariables = ["Upgrade Disc - Archives", "Upgrade Disc - Bootleg", "Upgrade Disc - Cloister", "Upgrade Disc - Commissary", "Upgrade Disc - Foundation", "Upgrade Disc - Freezer", "Upgrade Disc - Garage", "Upgrade Disc - Great Hall", "Upgrade Disc - LostFound", "Upgrade Disc - Master Bedroom", "Upgrade Disc - Mechanarium", "Upgrade Disc - Morning Room", "Upgrade Disc - Office", "Upgrade Disc - Shop", "Upgrade Disc - Tomb", "Upgrade Disc - Torch Room"];

        private static readonly List<string> _foundLocations = [];
        public static List<GameObject> YouFoundObjects = [];


        public UpgradeDisksGameItem(string upgradeDiskName) : base("UPGRADE DISK", GameObject.Find("__SYSTEM/Upgrade Disks"))
        {
            _upgradeDiskName = upgradeDiskName;
            _index = _itemNames.IndexOf(upgradeDiskName);
            _locationName = _locationNames[_index];
            _usedVariableName = _usedVariables[_index];
        }

        /// <summary>
        ///     Unlocks an upgrade disk location if it exists and hasn't already been found.
        /// </summary>
        /// <returns>If the location was unlocked.</returns>
        public bool UnlockLocation()
        {
            if (_found)
            {
                return false;
            }
            _found = true;
            State.UpdateUpgradeDiskData();
            return true;
        }


        /// <summary>
        ///     Handles adding unlocked upgrade disks to the the players inventory until they are used.
        /// </summary>
        public void StartOfDay()
        {
            // Skip this Start of Day if it's a reconnect from crash or quit.
            Logging.LogWarning("Updating Upgrade Disk Used States");
            if (!ArchipelagoOptions.UpgradeDiskSanity)
            {
                return;
            }

            // Logging.LogWarning($"[{UsedLocations.Join(", ")}]");
            if (_received)
            {
                if (!_used)
                {
                    AddToWorld();
                }
                if (_found)
                {
                    UpgradeDiskUsed();
                }
                else
                {
                    UpgradeDiskUnused();
                }
            }

            // Despawn Foundation Upgrade Disk
            if (_found && _upgradeDiskName.Contains("THE FOUNDATION"))
            {
                Transform foundationSpawn = GameObject.Find("UNDERGROUND").transform.Find("Below Foundation (Cullable)").Find("Below Foundation - Prefab").Find("_GAMEPLAY").Find("5");
                if (foundationSpawn.childCount > 0)
                {
                    Logging.LogWarning("Despawning Foundation Upgrade Disk.");
                    GameObject.Destroy(foundationSpawn.GetChild(0).gameObject);
                }
                return;
            }
            GameObject gameObject = GameObject.Find("UNDERGROUND").transform.Find("Below Foundation (Cullable)").Find("Below Foundation - Prefab").Find("_GAMEPLAY").Find("5").gameObject;
            if (gameObject == null)
            {
                Logging.LogWarning("Unable to Successfuly Spawn Upgrade Disk");
                return;
            }
            Quaternion rot = gameObject.transform.rotation;
            Transform spawnedTransform = PoolManager.Pools["Pickup"].Spawn(GameObjectExtensions.FindGameObject("UPGRADE DISK").transform, Vector3.zero, rot);
            if (spawnedTransform == null)
            {
                Logging.LogWarning("Unable to Successfuly Spawn Upgrade Disk");
                return;
            }
            spawnedTransform.SetParent((gameObject.transform == null) ? null : gameObject.transform, true);
            Logging.LogWarning("Foundation Upgrade Disk Successfully spawned.");
        }

        /// <summary>
        ///     Handles setting the game state when an upgrade disk is used.
        /// </summary>
        public void OnUsed()
        {
            Logging.Log($"Upgrade With ID {_index} used.", "UpgradeDisks");
            if (_used)
            {
                Logging.LogWarning("Unable to set Location as used, Upgrade Disk already used.", "UpgradeDisks");
                return;
            }
            if (!_used)
            {
                _used = true;
                State.UpdateUpgradeDiskData();
            }
            // Prevent the bool for being set for trading post trade specifically.
            if (_found && _upgradeDiskName != "TRADING POST TRADE")
            {
                UpgradeDiskUsed();
            }
            else
            {
                UpgradeDiskUnused();
            }
        }

        /// <summary>
        ///     Sends the location for the found upgrade disk.   
        /// </summary>
        public void OnFind()
        {
            Logging.LogWarning(_upgradeDiskName);
            if (_found)
            {
                Logging.LogWarning($"Upgrade disk {_upgradeDiskName} already found");
                return;
            }
            _found = true;
            State.UpdateUpgradeDiskData();
            ModInstance.ModEventHandler.OnUgradeDiskFound(_locationName);
            UpgradeDiskUsed();
        }

        public void OnTrade()
        {
            if (!_found && _upgradeDiskName.Contains("TRADING POST TRADE"))
            {
                _found = true;
                State.UpdateUpgradeDiskData();
                ModInstance.ModEventHandler.OnUgradeDiskFound("TRADING POST TRADE");
            }
        }

        /// <summary>
        ///     A sanity check for despawning Upgrade disks that shouldn't be spawned.
        /// </summary>
        /// <param name="spawnedObj">The spawned object.</param>
        public void OnSpawn(GameObject spawnedObj)
        {
            if (spawnedObj == null)
            {
                Logging.LogWarning($"Unable to despawn Upgrade Disk in {_locationName}, spawnedObj does not exist.");
                return;
            }
            if (ArchipelagoOptions.UpgradeDiskSanity && _found)
            {
                GameObject.Destroy(spawnedObj);
                Logging.LogWarning($"Despawned Upgrade Disk in {_locationName}, since it has been found before.");
            }
            Logging.LogWarning($"Unable to despawn Upgrade Disk in {_locationName}, location is not a valid location.");
        }

        /// <summary>
        ///     Handles adding an upgrade disk to the inventory.
        /// </summary>
        public override void AddToWorld()
        {
            if (!ArchipelagoOptions.UpgradeDiskSanity)
            {
                return;
            }
            Logging.LogWarning("Attempting To Add Upgrade Disk to Inventory.");
            if (!_received)
            {
                _received = true;
            }
            // If UpgradeDiskSanity is off, prevent adding it to inventory.
            GameObject inventoryGameObject = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
            PlayMakerFSM inventory = inventoryGameObject.GetFsm("Inventory Icons");
            PlayMakerArrayListProxy inventoryIcons = inventoryGameObject.GetArrayListProxy("Inventory Icons");
            GameObject icon = GameItemManager.GetIconGameObject("UPGRADE DISK");
            PlayMakerArrayListProxy upgradeDisks = GameObject.Find("__SYSTEM/Upgrade Disks").GetArrayListProxy("upgrade disk pickup");

            if (icon != null && inventoryIcons != null)
            {
                // Prevent adding the same Upgrade disk multiple times.
                int upgradeid = _index + 1;
                if (!upgradeDisks.Contains(upgradeid))
                {
                    upgradeDisks.Add(upgradeid, "Integer");
                    GameItemManager.PickedUp.Add(GameItemManager.GetInventoryItem("UPGRADE DISK"), "GameObject");
                    inventoryIcons.Add(icon, "GameObject");
                }
            }
        }

        /// <summary>
        ///     Internal. Initializes the notification UI objects for when Upgrade Disks are bought or purchased.
        /// </summary>
        public static void InitializeUpgradeDiskNotifications()
        {
            //TODO : Move to FSM Patches (only user)
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

        private void SetUsedVariable(bool value)
        {
            ModInstance.GlobalPersistentManager.GetComponent<PlayMakerFSM>().GetBoolVariable(_usedVariableName).Value = value;
        }

        private void UpgradeDiskUsed()
        {
            SetUsedVariable(true);
        }

        private void UpgradeDiskUnused()
        {
            SetUsedVariable(false);
        }
    }
}