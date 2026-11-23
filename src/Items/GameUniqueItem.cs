using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
#endif
using System.Collections.Generic;
using UnityEngine;



namespace BluePrinceArchipelago.Items
{
    public class GameUniqueItem : GameItem
    {
        private readonly bool _isPrespawn;
        private bool _hasBeenFound = false;
        public bool HasBeenFound
        {
            get { return _hasBeenFound; }
            set
            {
                // Send the item found event on the first time it is found.
                if (!_hasBeenFound && value)
                {
                    _hasBeenFound = value;
                }
                // No changes to value once the item has been found once, or if someone is trying to set this to false some reason.
            }
        }

        private readonly ItemSanityType _sanityType;
        private readonly List<GameObject> _gameModelInstancesWithSwirly = [];

        private readonly bool _isPersistent = false;
        public bool IsPersistent { get { return _isPersistent; } }

        private readonly bool _isCommissary = false;
        private readonly bool _isDig = false;
        private readonly bool _isLocksmith = false;

        private readonly CallMethod _commissaryEvent = null;
        private readonly CallMethod _digEvent = null;
        private readonly CallMethod _locksmithEvent = null;
        private readonly FsmState _commissaryState = null;
        private readonly FsmState _digState = null;
        private readonly FsmState _locksmithState = null;

        private GameObject _gameObj = null;

        /// <summary>
        ///     Creates a UniqueItem
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="isUnlocked">If the item is unlocked.</param>
        /// <param name="sanityType">The Sanitytype of the item. Defaults to none.</param>
        /// <param name="isPreSpawn">If the item is usually in the prespawn list. Defualts to true.</param>
        /// <param name="isPersistent">If the item is persists across scene transitions.</param>
        /// <param name="shopTags">What shops the item can be in.</param>
        /// 
        public GameUniqueItem(string name, bool isUnlocked, ItemSanityType sanityType = ItemSanityType.None, bool isPreSpawn = true, bool isPersistent = false, List<string> shopTags = null) : base(name, isUnlocked)
        {
            _isPrespawn = isPreSpawn;
            _sanityType = sanityType;
            _isPersistent = isPersistent;
            _gameObj = ModItemManager.GetInventoryItem(Name);
            List<string> shopTagList = shopTags ?? [];
            foreach (string tag in shopTagList)
            {
                if (tag == "Commissary")
                {
                    _isCommissary = true;
                }
                else if (tag == "Dig")
                {
                    _isDig = true;
                }
                else if (tag == "Locksmith")
                {
                    _isLocksmith = true;
                }
            }
            CustomFsmMethodManager.AddItemPickedUpMethod(this);
            if (_isCommissary)
            {
                CustomFsmMethodManager.AddItemBoughtMethod(this);
                _commissaryEvent = CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemBoughtMethodName(name));
            }
            if (_isDig)
            {
                CustomFsmMethodManager.AddItemDugUpMethod(this);
                _digEvent = CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemDugUpMethodName(name));
            }
            if (_isLocksmith)
            {
                CustomFsmMethodManager.AddItemBoughtMethod(this);
                _locksmithEvent = CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemBoughtMethodName(name));
            }
        }

        /// <summary>
        ///     Removes the item from the prespawn and estate pool.
        /// </summary>
        public void RemoveFromSpawnPoolIfFoundButNotUnlocked()
        {
            if (_archipelagoUnlocked)
            {
                return;
            }

            // If item has been found and is not unlocked, remove it from the pool. Otherwise Vanilla behavior.
            if (HasBeenFound && !_archipelagoUnlocked)
            {
                //TODO Duplication
                if (ModItemManager.PreSpawn.Contains(_gameObj))
                {
                    ModItemManager.PreSpawn.Remove(_gameObj, "GameObject");
                }
                if (ModItemManager.EstateItems.Contains(_gameObj))
                {
                    ModItemManager.EstateItems.Remove(_gameObj, "GameObject");
                }
            }
        }

        /// <summary>
        ///     Handles adding the item to the inventory.
        /// </summary>
        public override void AddToWorld()
        {
            Logging.LogWarning(Name);
            ArchipelagoUnlocked();
            /* Not sure what this part did
            if (!ApplySanity())
            {
                return;
            }
            */
            if (_gameObj == null && _isPrespawn)
            {
                _gameObj = ModItemManager.GetInventoryItem(Name);
            }

            if (_gameObj == null)
            {
                Logging.LogError($"Could not add {Name} to inventory, game object not found.");
                return;
            }
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
                ModItemManager.PickedUp.AddIfUnique(_gameObj);
                InventoryIcons.Add(icon, "GameObject");
                ModItemManager.PreSpawn.RemoveIfExists(Name);
                if (Name == "RUNNING SHOES")
                {
                    ModInstance.RunningEngine.SendEvent("Update");
                }
                if (Name == "STOPWATCH")
                {
                    GameObject.Find("__SYSTEM/STOPWATCH controller").GetComponent<PlayMakerFSM>().SendEvent("Begin");
                }
            }
        }

        public void RegisterModel(GameObject gameInstance)
        {
            _gameModelInstancesWithSwirly.Add(gameInstance);
        }

        public void DeleteAllSwirlies()
        {
            foreach (GameObject gameInstance in _gameModelInstancesWithSwirly)
            {
                if (gameInstance == null)
                {
                    Logging.LogError($"Game instance is null for {Name}", "ITEM");
                }
                GameObject swirlyEffect = gameInstance.GetChild("AP Swirlie");
                if (swirlyEffect == null)
                {
                    Logging.LogError($"Swirly effect not found for item {Name} and object {gameInstance.name}", "ITEM");
                }
                GameObject.Destroy(swirlyEffect);
            }
            _gameModelInstancesWithSwirly.Clear();
        }

        public void EndOfDay()
        {
            DeleteAllSwirlies();
        }

        public void StartOfDay()
        {
            RemoveFromSpawnPoolIfFoundButNotUnlocked();
            OverrideShopAndDigActions();
            DespawnMicrochipsIfFoundButNotUnlocked();
        }

        private void OverrideShopAndDigActions()
        {
            //TODO: This is duplicated from Commissary.cx
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
            if (_isCommissary)
            {
                FsmState state = ModInstance.CommissaryMenu?.GetState(CommissaryStates[Name.Trim()]);
                DisableItemInMenu(state, _commissaryEvent);
            }
            if (_isDig)
            {
                FsmState state = ModInstance.DigEngine?.GetState(DigStates[Name.Trim()]);
                DisableItemInMenu(state, _digEvent);
            }
            if (_isLocksmith)
            {
                // Does not load properly unless loaded like so.
                FsmState state = GameObject.Find("UI OVERLAY CAM").transform.Find("Locksmith Menu").gameObject.GetComponent<PlayMakerFSM>().GetState(LocksmithStates[Name.Trim()]);
                DisableItemInMenu(state, _locksmithEvent);
            }
        }

        private void DespawnMicrochipsIfFoundButNotUnlocked()
        {
            if (_hasBeenFound && !_archipelagoUnlocked)
            {
                if (Name == ITEM.MICROCHIP_SMALL)
                {
                    GameObject.Find("TERRAIN/_GAMEPLAY (terrain)/New Clue Dig - menu/COUNTERTOP").SetActive(false);
                    return;
                }
                if (Name == ITEM.MICROCHIP_MIDDLE_SIZED)
                {
                    GameObject.Find("ROOMS/Entrance Hall/_GAMEPLAY/VASES/Vase 2 BROKEN/MICRO 2 SPAWN").SetActive(false);
                    return;
                }
                if (Name == ITEM.MICROCHIP_LARGE)
                {
                    GameObject.Find("TERRAIN/EAST SECTOR/_GROTTO/_GROTTO GAMEPLAY/Microchip Pillar/Microchip 3").SetActive(false);
                    return;
                }
            }
        }

        private void DisableItemInMenu(FsmState state, CallMethod replacedAction)
        {
            //FsmState state = ModInstance.CommissaryMenu?.GetState(CommissaryStates[item.Name.Trim()]);
            //_commissaryState = state;
            //if (state != null)
            //{
            state.AddAction(replacedAction);
            //If the item is not unlocked, prevent it from being added to inventory.
            if (!_archipelagoUnlocked)
            {
                //Disable the actions that add the item to inventory.
                state.DisableActionsOfType<ArrayListAdd>();
            }
            //}
        }
    } 
}