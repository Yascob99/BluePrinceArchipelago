using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BluePrinceArchipelago.Items
{

    /// <summary>
    ///     The template for Unique items.
    /// </summary>
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

        private static List<string> _ShopTags = new();

        public bool IsPersistent { get; set; }

        public bool IsCommissary { set; get; } = false;

        public bool IsDig { set; get; } = false;

        public bool IsLocksmith { set; get; } = false;

        public bool IsShowRoom { set; get; } = false;

        public SendEvent CommissaryEvent { get; set; } = null;

        public SendEvent DigEvent { get; set; } = null;

        public SendEvent LocksmithEvent { get; set; } = null;

        public SendEvent ShowRoomEvent { get; set; } = null;

        public SendEvent TradingEvent { get; set; } = null;

        public FsmState CommissaryState { get; set; } = null;

        public FsmState DigState { get; set; } = null;

        public FsmState LocksmithState { get; set; } = null;

        public List<FsmState> ShowRoomStates { get; set; } = null;

        public FsmState TradingState { get; set; } = null;

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
        /// <summary>
        ///     Creates a UniqueItem
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="gameObject">The GameObject of the item.</param>
        /// <param name="isUnlocked">If the item is unlocked.</param>
        /// <param name="sanityType">The Sanitytype of the item. Defaults to none.</param>
        /// <param name="isPreSpawn">If the item is usually in the prespawn list. Defualts to true.</param>
        /// <param name="isPersistent">If the item is persists across scene transitions.</param>
        /// <param name="shopTags">What shops the item can be in.</param>
        public UniqueItem(string name, GameObject gameObject, bool isUnlocked, ItemSanityType sanityType = ItemSanityType.None, bool isPreSpawn = true, bool isPersistent = false, List<string> shopTags = null) : base(name, gameObject, isUnlocked)
        {
            _IsPrespawn = isPreSpawn;
            _SanityType = sanityType;
            IsPersistent = isPersistent;
            _ShopTags = shopTags ?? new List<string>();
            foreach (string tag in _ShopTags) {
                if (tag == "Commissary")
                {
                    IsCommissary = true;
                }
                else if (tag == "Dig")
                {
                    IsDig = true;
                }
                else if (tag == "Locksmith") {
                    IsLocksmith = true;
                }
                else if (tag == "Showroom") {
                    IsShowRoom = true;
                }
            }

            FSMEventHandler.AddItemFSMEvent(name, this);
            if (IsCommissary)
            {
                CommissaryEvent = FSMEventHandler.AddBuyFSMEvent("Commissary: Bought " + name, this).Event;
            }
            if (IsDig)
            {
                DigEvent = FSMEventHandler.AddDigFSMEvent("Dug Up " + name, this).Event;
            }
            if (IsLocksmith)
            {
                LocksmithEvent = FSMEventHandler.AddBuyFSMEvent("Locksmith: Bought " + name, this).Event;
            }
            if (IsShowRoom) { 
                ShowRoomEvent = FSMEventHandler.AddBuyFSMEvent("Showroom: Bought " + name, this).Event;
            }
            
        }

        /// <summary>
        ///     Removes the item from the prespawn and estate pool.
        /// </summary>
        public void RemoveFromPool()
        {
            if (!ApplySanity())
            {
                return;
            }
            
            if (HasBeenFound)
            {
                // If item has been found and is not unlocked, remove it from the pool. Otherwise Vanilla behavior.
                if (!IsUnlocked)
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
        }


        /// <summary>
        ///     Handles adding the item to the inventory.
        /// </summary>
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
                Logging.LogWarning($"Attempting to add {Name} to Inventory");
                GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
                PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
                GameObject icon = Plugin.UniqueItemManager.GetIconGameObject(Name);
                if (icon != null && InventoryIcons != null)
                {
                    ModItemManager.PickedUp.AddIfUnique(GameObj);
                    InventoryIcons.Add(icon, "GameObject");
                    ModItemManager.PreSpawn.RemoveIfExists(Name);
                    if (Name == "RUNNING SHOES")
                    {
                        ModInstance.RunningEngine.SendEvent("Update");
                    }
                }
               
            }
        }


        /// <summary>
        ///     Checks if the sanity is relevant to the current item.
        /// </summary>
        /// <returns></returns>
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


    /// <summary>
    ///     A manager for common operations associated with Unique Items.
    /// </summary>
    public class UniqueItemManager
    {
        public List<UniqueItem> SpawnedItems = new List<UniqueItem>();

        public bool ModelsReplaced = false;

        /// <summary>
        ///     Triggered by a Unique item being spawned.
        /// </summary>
        /// <param name="obj">The GameObject of the spawned item.</param>
        /// <param name="poolName">The PoolName of the spawned item's spawn pool.</param>
        /// <param name="transformObj">The GameObject with contains the position data for where the item will be spawned.</param>
        /// <param name="spawnedObj">The GameObject for the spawned object.</param>
        public void OnItemSpawn(GameObject obj, string poolName, GameObject transformObj, GameObject spawnedObj)
        {
            UniqueItem item = Plugin.ModItemManager.GetUniqueItem(obj.name);
            //Check if Connected in before replacing items.
            if (ArchipelagoClient.Authenticated)
            {
                if (item != null)
                {
                    FsmState state = GetPickupState(obj.name);
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
        ///     Code to be run on end of day.
        /// </summary>
        public void EndOfDay()
        {
            //Reset the list of spawned items.
            SpawnedItems = new List<UniqueItem>();
            ModelsReplaced = false;
        }

        /// <summary>
        ///     Code to be run at the start of day.
        /// </summary>
        public void StartOfDay()
        {
            Dictionary<string, string> CommissaryStates = new Dictionary<string, string>()
            {
                {"MAGNIFYING GLASS", "Mag Glass" },
                {"SHOVEL", "Shovel Purchase"},
                {"SALT SHAKER", "Salt Shaker Purchase"},
                {"COMPASS", "Compass Purchase"},
                {"SLEDGE HAMMER", "Sledge Hammer Purchase"},
                {"SLEEPING MASK", "Sleep Mask Purchase"},
                {"RUNNING SHOES", "Running Shoes Purchase"},
                {"METAL DETECTOR", "MEtal Detector Purchase"}
            };
            Dictionary<string, string> DigStates = new Dictionary<string, string>()
            {
                {"BROKEN LEVER", "Broken Lever" },
                {"VAULT KEY 149", "Vault Key 149"},
                {"VAULT KEY 233", "Vault Key 233"},
                {"VAULT KEY 304", "Vault Key 304"},
                {"VAULT KEY 370", "Vault Key 370"},
                {"SILVER KEY", "Silver Key"},
                {"SECRET GARDEN KEY", "Secret Garden Key"},
                {"STOPWATCH", "Stopwatch"},
                {"KNIGHTS SHIELD", "Knight's Sheild"}
            };
            Dictionary<string, string> LocksmithStates = new Dictionary<string, string>()
            {
                {"SECRET GARDEN KEY", "Secret Garden Key Purchase"},
                {"PRISM KEY_0", "Prism Key Purchase"},
                {"SILVER KEY", "Silver Key Purchase"},
                {"CAR KEYS", "Car Keys Purchase"},
                {"MASTER KEY", "Master Key Purchase"},
                {"LOCK PICK KIT", "Lockpick Kit Purchase"}
            };

            Dictionary<string, List<string>> ShowRoomStates = new Dictionary<string, List<string>>()
            {
                {"EMERALD BRACELET", ["Em Purchase", "Em Purachase 2"] },
                {"MOON PENDANT", ["Moon Purchase"]},
                {"ORNATE COMPASS", ["Compass Purchase", "Compass Purchase 2"]},
                {"MASTER KEY", ["Master Key Purchase"]},
                {"CHRONOGRAPH", ["Chronograph Purchase"]},
                { "SILVER SPOON", ["Silver Spoon Purchase"] }

            };

            foreach (UniqueItem item in ModItemManager.UniqueItemList)
            {
                // Handles start of Day Item Removal
                item.RemoveFromPool();
                // Handles updating the Commissary Menu
                if (!item.HasBeenFound && item.IsCommissary)
                {
                    FsmState state = ModInstance.CommissaryMenu?.GetState(CommissaryStates[item.Name.Trim()]);
                    item.CommissaryState = state;
                    if (state != null)
                    {
                        //If the item is not unlocked, prevent it from being added to inventory.
                        if (!item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.DisableActionsOfType<ArrayListAdd>();
                            state.AddAction(item.CommissaryEvent);
                        }
                    }
                }
                if (!item.HasBeenFound && item.IsDig) {
                    FsmState state = ModInstance.DigEngine?.GetState(DigStates[item.Name.Trim()]);
                    item.DigState = state;
                    if (state != null)
                    {
                        //If the item is not unlocked, prevent it from being added to inventory.
                        if (!item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.DisableActionsOfType<ArrayListAdd>();
                            state.AddAction(item.DigEvent);
                        }
                    }
                }
                if (!item.HasBeenFound && item.IsLocksmith)
                {
                    // Does not load properly unless loaded like so.
                    FsmState state = GameObject.Find("UI OVERLAY CAM").transform.Find("Locksmith Menu").gameObject.GetComponent<PlayMakerFSM>().GetState(LocksmithStates[item.Name.Trim()]);
                    item.LocksmithState = state;
                    if (state != null)
                    {
                        // If the item is not unlocked, prevent it from being added to inventory.
                        if (!item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.DisableActionsOfType<ArrayListAdd>();
                            state.AddAction(item.LocksmithEvent);
                        }
                    }
                }
                if (!item.HasBeenFound && item.IsShowRoom) {
                    List<FsmState> states = new List<FsmState>();
                    foreach (string statename in ShowRoomStates[item.Name]) {
                        FsmState state = ModInstance.LocksmithMenu?.GetState(statename);
                        states.Add(state);
                        // If the item is not unlocked, prevent it from being added to inventory.
                        if (!item.IsUnlocked && item.ApplySanity())
                        {
                            //Disable the actions that add the item to inventory.
                            state.DisableActionsOfType<ArrayListAdd>();
                            state.AddAction(item.ShowRoomEvent);
                        }
                    }
                    item.ShowRoomStates = states;
                }
                // Despawn Microchips if Found but not unlocked (since they don't use the spawn system).
                if (item.HasBeenFound && item.Name == "MICROCHIP 1" && !item.IsUnlocked) { 
                    GameObject.Find("TERRAIN/_GAMEPLAY (terrain)/New Clue Dig - menu/COUNTERTOP").SetActive(false);
                }
                else if (item.HasBeenFound && item.Name == "MICROCHIP 2" && !item.IsUnlocked)
                {
                    // Despawn the microchip if it has been autospawned.
                    GameObject.Find("ROOMS/Entrance Hall/_GAMEPLAY/VASES/Vase 2 BROKEN/MICRO 2 SPAWN").SetActive(false);
                }
                else if (item.HasBeenFound && item.Name == "MICROCHIP 3" && !item.IsUnlocked) {
                    GameObject.Find("TERRAIN/EAST SECTOR/_GROTTO/_GROTTO GAMEPLAY/Microchip Pillar/Microchip 3").SetActive(false);
                }
            }
        }

        /// <summary>
        ///     Returns the Inventory Icon name for various items that differ from their usual name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>The name of the icon</returns>
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
                    return "Prism Key";
                case "Electromagnet":
                    return "Powered Electro Magnet Icon";
                case "Key 8":
                    return "Key 8";
                case "Lucky Rabbit's Foot":
                    return "Lucky rabbit's foot Icon";
                case "Salt Shaker":
                    return "Salt Icon";
                case "Vault Key 149":
                    return "Vault Key 149 icon";
                case "Vault Key 233":
                    return "Vault Key 233 icon";
                case "Vault Key 304":
                    return "Vault Key 304 icon";
                case "Vault Key 370":
                    return "Vault Key 370 icon";
                default:
                    return name + " Icon";
            }
        }

        /// <summary>
        ///     Gets the game object for the Inventory Icon of of a given Unique Item.
        /// </summary>
        /// <param name="name">The name of the Unique Item</param>
        /// <returns>The GameObject of the icon</returns>
        public GameObject GetIconGameObject(string name) {
            name = GetIconName(name);
            PlayMakerArrayListProxy Inventory = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/InventoryIconMeshes").GetComponent<PlayMakerArrayListProxy>();
            for (int i=0; i < Inventory.arrayList.Count; i++)
            {
                GameObject child = Inventory.arrayList[i].TryCast<GameObject>();
                if (child.name.Trim() == name.Trim()) {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        ///     Finds the Parent Transform of the you found event based on the provided state.
        /// </summary>
        /// <param name="pickupState">The pickup state of the item in the GlobalFsm</param>
        /// <returns>The Parent Transform</returns>
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
        /// <summary>
        ///     Checks if the item has been picked up before.
        /// </summary>
        /// <param name="name">the name of the item</param>
        /// <returns>True if item is spawned, false if not.</returns>
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
        /// <summary>
        ///     Finds the state in the Global Manager associated with the given item's pickup. Returns null if not found.    
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <returns>The FsmState of the item's pickup handling.</returns>
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

    }

    /// <summary>
    ///     The enum for the Sanity Types.
    /// </summary>
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
