using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Rooms.RoomHandlers;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using PathologicalGames;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppHutongGames.PlayMaker.Actions;
using Il2CppPathologicalGames;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BluePrinceArchipelago.Items
{ 
    /// <summary>
    ///     A manager for common operations associated with Unique Items.
    /// </summary>
    public class UniqueItemManager
    {
        public List<GameUniqueItem> SpawnedItems = [];
        //TODO : Bring UniqueItemList here

        /// <summary>
        ///     Code to be run on end of day.
        /// </summary>
        public void EndOfDay()
        {
            foreach (GameUniqueItem item in SpawnedItems)
            {
                item.EndOfDay();
            }
            SpawnedItems.Clear();
        }

        /// <summary>
        ///     Code to be run at the start of day.
        /// </summary>
        public void StartOfDay()
        {
            foreach (GameUniqueItem item in ModItemManager.GameUniqueItemList)
            {
                item.StartOfDay();   
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
        public GameUniqueItem GetIfSpawned(string name)
        {
            foreach (GameUniqueItem item in SpawnedItems)
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
                        if (name == ITEM.TREASURE_MAP.ToLower()) {
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
}
