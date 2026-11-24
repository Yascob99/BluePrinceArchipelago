using BluePrinceArchipelago.Archipelago;
using Archipelago.MultiClient.Net.Models;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Rooms.RoomHandlers;
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
using System.Collections.Generic;
using UnityEngine;



namespace BluePrinceArchipelago.Items
{
    public class GameUniqueItem : GameItem
    {
        private readonly bool _isPrespawn;

        private readonly List<GameObject> _gameModelInstancesWithSwirly = [];

        private readonly bool _isPersistent = false;
        public bool IsPersistent { get { return _isPersistent; } }

        private readonly bool _isCommissary = false;
        private readonly bool _isDig = false;
        private readonly bool _isLocksmith = false;

        private readonly CallMethod _commissaryEvent = null;
        private readonly CallMethod _digEvent = null;
        private readonly CallMethod _locksmithEvent = null;

        /// <summary>
        ///     Creates a UniqueItem
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="isPreSpawn">If the item is usually in the prespawn list. Defualts to true.</param>
        /// <param name="isPersistent">If the item is persists across scene transitions.</param>
        /// <param name="shopTags">What shops the item can be in.</param>
        public GameUniqueItem(string name, bool isPreSpawn = true, bool isPersistent = false, List<string> shopTags = null) : base(name, GameItemManager.GetInventoryItem(name))
        {
            _isPrespawn = isPreSpawn;
            _isPersistent = isPersistent;
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
                if (GameItemManager.PreSpawn.Contains(_inventoryGameObject))
                {
                    GameItemManager.PreSpawn.Remove(_inventoryGameObject, "GameObject");
                }
                if (GameItemManager.EstateItems.Contains(_inventoryGameObject))
                {
                    GameItemManager.EstateItems.Remove(_inventoryGameObject, "GameObject");
                }
            }
        }

        /// <summary>
        ///     Handles adding the item to the inventory.
        /// </summary>
        public override void AddToWorld()
        {
            Logging.LogWarning(_name);
            ArchipelagoUnlocked();
            /* Not sure what this part did
            if (!ApplySanity())
            {
                return;
            }
            */
            if (_inventoryGameObject == null && _isPrespawn)
            {
                _inventoryGameObject = GameItemManager.GetInventoryItem(_name);
            }

            if (_inventoryGameObject == null)
            {
                Logging.LogError($"Could not add {_name} to inventory, game object not found.");
                return;
            }
            if (Commissary.CommissaryStates.ContainsKey(_name))
            {
                //Re-enable commissary purchases of the item.
                Commissary.CanStock.Add(_name);
            }
            // This may not cause it to re-trigger.
            // Disable this game action so it doesn't try and display 2 UIs.
            Logging.LogWarning($"Attempting to add {_name} to Inventory");
            GameObject InventoryGO = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory");
            PlayMakerArrayListProxy InventoryIcons = InventoryGO.GetArrayListProxy("Inventory Icons");
            GameObject icon = GameItemManager.GetIconGameObject(_name);
            if (icon != null && InventoryIcons != null)
            {
                GameItemManager.PickedUp.AddIfUnique(_inventoryGameObject);
                InventoryIcons.Add(icon, "GameObject");
                GameItemManager.PreSpawn.RemoveIfExists(_name);
                if (_name == "RUNNING SHOES")
                {
                    ModInstance.RunningEngine.SendEvent("Update");
                }
                if (_name == "STOPWATCH")
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
                    Logging.LogError($"Game instance is null for {_name}", "ITEM");
                }
                GameObject swirlyEffect = gameInstance.GetChild("AP Swirlie");
                if (swirlyEffect == null)
                {
                    Logging.LogError($"Swirly effect not found for item {_name} and object {gameInstance.name}", "ITEM");
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
                FsmState state = ModInstance.CommissaryMenu?.GetState(CommissaryStates[_name.Trim()]);
                DisableItemInMenu(state, _commissaryEvent);
            }
            if (_isDig)
            {
                FsmState state = ModInstance.DigEngine?.GetState(DigStates[_name.Trim()]);
                DisableItemInMenu(state, _digEvent);
            }
            if (_isLocksmith)
            {
                // Does not load properly unless loaded like so.
                FsmState state = GameObject.Find("UI OVERLAY CAM").transform.Find("Locksmith Menu").gameObject.GetComponent<PlayMakerFSM>().GetState(LocksmithStates[_name.Trim()]);
                DisableItemInMenu(state, _locksmithEvent);
            }
        }

        private void DespawnMicrochipsIfFoundButNotUnlocked()
        {
            if (_hasBeenFound && !_archipelagoUnlocked)
            {
                if (_name == ITEM.MICROCHIP_SMALL)
                {
                    GameObject.Find("TERRAIN/_GAMEPLAY (terrain)/New Clue Dig - menu/COUNTERTOP").SetActive(false);
                    return;
                }
                if (_name == ITEM.MICROCHIP_MIDDLE_SIZED)
                {
                    GameObject.Find("ROOMS/Entrance Hall/_GAMEPLAY/VASES/Vase 2 BROKEN/MICRO 2 SPAWN").SetActive(false);
                    return;
                }
                if (_name == ITEM.MICROCHIP_LARGE)
                {
                    GameObject.Find("TERRAIN/EAST SECTOR/_GROTTO/_GROTTO GAMEPLAY/Microchip Pillar/Microchip 3").SetActive(false);
                    return;
                }
            }
        }

        private void DisableItemInMenu(FsmState state, CallMethod replacedAction)
        {
            if (state == null)
            {
                return;
            }
            state.AddAction(replacedAction);
            //If the item is not unlocked, prevent it from being added to inventory.
            if (!_archipelagoUnlocked)
            {
                //Disable the actions that add the item to inventory.
                state.DisableActionsOfType<ArrayListAdd>();
            }
        }

        public void ReplaceUniqueItemWithArchipelagoItem()
        {
            // If the item has not been found yet.
            if (!_hasBeenFound && !_archipelagoUnlocked)
            {
                if (_name == "MICROCHIP 1")
                {
                    FsmState state1 = ModInstance.GlobalManager.GetState("Microchip Pickup");
                    FsmState state2 = ModInstance.GlobalManager.GetState("Microchip Pickup 4");
                    state1.DisableActionsOfType<ArrayListAdd>();
                    state1.AddAction(CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemPickedUpMethodName(_name)));
                    state2.DisableActionsOfType<ArrayListAdd>();
                    state2.AddAction(CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemPickedUpMethodName(_name)));
                }
                else
                {
                    FsmState state = GameItemManager.GetPickupState(_name);
                    if (state != null)
                    {
                        state.DisableActionsOfType<ArrayListAdd>();
                        state.AddAction(CustomFsmMethodManager.GetCallMethod(CustomFsmMethodManager.GetItemPickedUpMethodName(_name)));
                    }
                }
                GameObject spawnObj = FindSpawnObject() ?? _inventoryGameObject;
                AddSwirly(spawnObj);
            }
            else
            {
                if (_isPersistent)
                {
                    RemoveUniqueItemAPSwirly();
                }
            }
        }

        /// <summary>
        ///     Replaces an AP Item's notification with the appropriate item data and the UI model with the AP model.
        /// </summary>
        /// <param name="item">The GameObject of the item.</param>
        /// <param name="scoutname">The scoutname of the object if different than it's name.</param>
        public void ReplaceAPItemNotifications(GameObject item, string scoutname = "")
        {
            List<Transform> FoundModels = new();
            if (scoutname == "")
            {
                scoutname = _name;
            }
            string youName = ITEM.GetYouFoundNotificationName(_name);
            GameObject You___Text = GameObject.Find("UI OVERLAY CAM/You Found Text")?.gameObject;
            if (You___Text != null)
            {
                for (int t = 0; t < You___Text.transform.childCount; t++)
                {
                    Transform child = You___Text.transform.GetChild(t);
                    // Tries to find all related You___Messages"
                    if (child.gameObject.name.Contains(youName) && !CheckSimilar(_name, child))
                    {
                        FoundModels.Add(child);
                        Transform itemModel = child.FindRecursive(ITEM.GetItemModelInNotificationName(_name), true);
                        if (itemModel != null)
                        {

                            // Make a clone of the original gameObject parented under the ModObject for easy editing/retreival later.
                            GameObject clone = GameObject.Instantiate(child.gameObject, Plugin.ModObject.transform);
                            clone.SetActive(false); //Make sure the clone is not visible.
                            if (youName == "Cabinet Key 5")
                            {
                                clone.name =  _name.ToTitleCase();
                            }
                            else
                            {
                                clone.name = child.name;//Make the name match the original so it can be replaced later.
                            }

                            //Instantiate a the AP Object at the original's position
                            GameObject APGO = GameObject.Instantiate(item, itemModel.position, itemModel.rotation, itemModel.parent);
                            APGO.transform.localScale = itemModel.localScale;
                            APGO.name = itemModel.name;
                            itemModel.gameObject.DestroyAllChildren();
                            APGO.MoveChildrenTo(itemModel.gameObject);
                            GameObject.Destroy(APGO);

                            //Import the template Text Prefab.  
                            GameObject textPrefab = ModInstance.Prefabs.GetChild("You Found Text Template");
                            long locationid = Plugin.ArchipelagoClient.GetLocationFromName($"First Pickup - {scoutname.ToTitleCase()}");
                            // Get the location ID of our first pickup.
                            if (scoutname == "CABINET KEY 1")
                            {
                                locationid = Plugin.ArchipelagoClient.GetLocationFromName("First Pickup - File Cabinet Key - Patio");
                            }
                            else if (scoutname == "CABINET KEY 2")
                            {
                                locationid = Plugin.ArchipelagoClient.GetLocationFromName("First Pickup - File Cabinet Key - Laundry Room");
                            }
                            else if (scoutname == "CABINET KEY 3")
                            {
                                locationid = Plugin.ArchipelagoClient.GetLocationFromName("First Pickup - File Cabinet Key - Tunnel Area Past Crates");
                            }
                            // Find the the details of the item that will be sent on pickup.
                            ScoutedItemInfo scout = null;
                            if (locationid != -1)
                            {
                                if (ArchipelagoClient.ServerData.LocationItemMap.ContainsKey(locationid))
                                {
                                    scout = ArchipelagoClient.ServerData.LocationItemMap[locationid];
                                }
                            }
                            // Get the variables for creating our custom pickup message.
                            string playerName = scout?.Player?.Name ?? "";
                            //Check if item is being used.
                            if (playerName != "")
                            {
                                string scoutItemName = scout?.ItemName ?? "";

                                //TODO add logic for the descriptions to be different based on item importance.
                                string description = "";
                                string[] itemWords = scoutItemName.Split(" ");
                                if (itemWords.Length < 4)
                                {
                                    scoutItemName = string.Join("\n", itemWords);
                                }
                                else
                                {
                                    scoutItemName = scoutItemName.Minragged();
                                }
                                int FirstLetterCount = 0;
                                int ItemNameCount = 0;
                                int DescriptionCount = 0;
                                // Update all the fonts and words to be correct
                                Transform textObjects = GameObject.Find($"UI OVERLAY CAM/You Found Text/{child.name}/Text/GameObject")?.transform;
                                // Fix for an error in the Prism Key You Bought (and potentially others);
                                if (textObjects == null)
                                {
                                    textObjects = GameObject.Find($"UI OVERLAY CAM/You Found Text/{child.name}/GameObject")?.transform;
                                }

                                GameObject textObject = GameObject.Instantiate(textPrefab, textObjects.position, textObjects.rotation);
                                Transform FirstFirstLetter = null;
                                Transform FirstItemName = null;
                                //Get rid of the original Text.
                                Transform Prescription = textObject.transform.FindChild("Prescription");
                                Transform Description = textObject.transform.FindChild($"Description (2)");

                                TextMeshPro text = null;
                                List<GameObject> toDestroy = new List<GameObject>();
                                for (int i = 0; i < textObjects.transform.childCount; i++)
                                {

                                    Transform textChild = textObjects.gameObject.transform.GetChild(i);
                                    if (textChild.TryGetComponent<TextMeshPro>(out text))
                                    {
                                        // Add the name of the player who owns the item being spawned.
                                        if (textChild.name.Contains("Prescription"))
                                        {
                                            textChild.name = "Prescription";
                                            textChild.SetLocalPositionAndRotation(Prescription.localPosition, Prescription.localRotation);
                                            textChild.transform.localScale = Prescription.transform.localScale;

                                            // Handle names ending in s with proper apostrophe convention
                                            if (playerName.ToLower().EndsWith('s'))
                                            {
                                                text.text = $"{playerName}'";
                                            }
                                            else
                                            {
                                                text.text = $"{playerName}'s";
                                            }
                                        }
                                        else
                                        {

                                            // The first letter of each word in the item name is handled differently.
                                            if (textChild.name.StartsWith("First Letter"))
                                            {

                                                if (FirstLetterCount != 0)
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                                else
                                                {
                                                    textChild.name = $"First Letter ({FirstLetterCount + 1})";
                                                    Transform FirstLetter = textObject.transform.FindChild($"First Letter ({FirstLetterCount + 1})");
                                                    textChild.SetLocalPositionAndRotation(FirstLetter.localPosition, FirstLetter.localRotation);
                                                    textChild.transform.localScale = FirstLetter.localScale;
                                                    FirstFirstLetter = textChild;
                                                    text.text = scoutItemName.Substring(0, 1);
                                                    FirstLetterCount++;
                                                }

                                            }
                                            // The rest of the word in the item name.
                                            else if (textChild.name.StartsWith("Item Name"))
                                            {
                                                if (ItemNameCount != 0)
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                                else
                                                {

                                                    FirstItemName = textChild;
                                                    textChild.name = $"Item Name ({ItemNameCount + 1})";

                                                    Transform ItemName = textObject.transform.FindChild($"Item Name ({ItemNameCount + 1})");
                                                    textChild.SetLocalPositionAndRotation(ItemName.localPosition, ItemName.localRotation);
                                                    textChild.transform.localScale = ItemName.localScale;
                                                    text.text = scoutItemName.ToUpper().Substring(1);
                                                    text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                                                    ItemNameCount++;
                                                }

                                            }
                                            // Handle the item description.
                                            else if (textChild.name.StartsWith("Description"))
                                            {

                                                textChild.name = "Description";
                                                textChild.SetLocalPositionAndRotation(Description.transform.localPosition, Description.transform.localRotation);
                                                textChild.transform.localScale = Description.transform.localScale;
                                                if (DescriptionCount == 0)
                                                {
                                                    text.text = description;
                                                    text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                                                }
                                                else
                                                {
                                                    toDestroy.Add(textChild.gameObject);
                                                }
                                            }
                                            //Something else got mixed into the item prefab.
                                            else
                                            {
                                                Logging.Log($"An extra game object was found, removing object \"{textChild.name}\"");
                                                toDestroy.Add(textChild.gameObject);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        toDestroy.Add(textChild.gameObject);
                                    }
                                }
                                //Destroy all the game objects slated to be destroyed.
                                foreach (GameObject desObj in toDestroy)
                                {
                                    GameObject.Destroy(desObj);
                                }
                                GameObject.Destroy(textObject);
                            }
                        }
                        else
                        {
                            Logging.LogWarning($"Unable to find the item model for {_name.ToTitleCase()}");
                        }
                    }

                }
                if (FoundModels.Count == 0)
                {
                    Logging.LogWarning($"Unable to find a 'You ___' notification for {_name}.");
                }
            }
            else
            {
                Logging.LogWarning($"Unable to find the 'You Found Text' GameObject.");
            }
        }

        /// <summary>
        /// Add the swirly effect on an item and replace its pickup notification with the AP item collected.
        /// </summary>
        /// <param name="gameInstance">In-game object to add the swirly effect on.</param>
        public void AddSwirly(GameObject gameInstance)
        {
            if (gameInstance == null)
            {
                Logging.LogWarning($"Error finding game object for {_name}.");
                return;
            }
            string prefabName = ITEM.GetPrefabName(_name);
            GameObject prefab = ModInstance.Prefabs.GetChild(prefabName);
            if (prefab == null)
            {
                Logging.LogWarning($"Unable to find prefab for {_name}. Item is either unimplemented or not present in the assets.");
                return;
            }
            //If the Model is not currently already replaced.
            if (gameInstance.transform.FindChild("AP Swirlie") != null)
            {
                // Make sure the notification is changed for persistent items.
                if (!_isPersistent)
                {
                    Logging.LogWarning($"{_name} has already Been Replaced with an AP Item:");
                    return;
                }
                ReplaceAPItemNotifications(gameInstance);
                return;
            }
            //Instantiate a copy of the game object at the location of the spawn pool game object.
            GameObject APGO = GameObject.Instantiate(prefab, gameInstance.transform.position, gameInstance.transform.rotation);
            // Get the APswirly Component of the prefab
            GameObject APswirly = APGO?.transform?.GetChild(0)?.gameObject;
            if (APswirly == null)
            {
                Logging.LogWarning($"Unable to find APSwirly for {_name}.");
                GameObject.Destroy(APGO);
                return;
            }
            // Reparent the the AP Swirly to the Archipelago Mod GameObject.
            APswirly.transform.parent = gameInstance.transform;
            APswirly.layer = 10;
            RegisterModel(gameInstance);
            ReplaceAPItemNotifications(gameInstance);
            GameObject.Destroy(APGO);
        }

        /// <summary>
        ///     Looks to see if the notification is for a similarly named item.
        /// </summary>
        /// <param name="itemName">Item name.</param>
        /// <param name="child">The item's child transform</param>
        /// <returns></returns>
        private static bool CheckSimilar(string itemName, Transform child)
        {

            if (itemName.ToLower() == "compass")
            {
                if (child.gameObject.name.Contains("Ornate Compass"))
                {
                    return true;
                }
            }
            else if (itemName.ToLower() == "crown")
            {
                if (child.gameObject.name.Contains("Paper Crown"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Removes the AP Swirlies from Unique items, for use when the swirlies need to be removed mid-day.
        /// </summary>
        public void RemoveUniqueItemAPSwirly()
        {
            GameObject spawnObj = FindSpawnObject();
            if (spawnObj == null)
            {
                spawnObj = _inventoryGameObject;
            }
            if (spawnObj == null)
            {
                Logging.LogWarning($"Unable to change spawn prefab for {_name}, error finding prefab with name: {_name}(Clone)001");
                return;
            }
            // Delete the AP Swirly SubObject.
            if (spawnObj.transform.FindChild("AP Swirlie") != null)
            {
                GameObject.Destroy(spawnObj.transform.FindChild("AP Swirlie").gameObject);
            }
        }


        /// <summary>
        ///     Finds the spawned version of an item icon,
        /// </summary>
        /// <returns>The GameObject of the inventory Icon.</returns>
        private GameObject FindSpawnObject()
        {
            string name = _name.Replace("_0", "");
            string instanceName = name + "(Clone)001";
            GameObject spawnObj = ModInstance.PickupSpawnPool.transform.FindChild(instanceName)?.gameObject;
            if (spawnObj == null)
            {
                instanceName = name + " (Clone)001";
                spawnObj = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + instanceName);
            }
            if (spawnObj == null)
            {
                GameItemManager.GetInventoryItem(name);
            }
            return spawnObj;
        }

        public void UpdateGameObject(GameObject newGameObject)
        {
            _inventoryGameObject = newGameObject;
        }
    } 
}