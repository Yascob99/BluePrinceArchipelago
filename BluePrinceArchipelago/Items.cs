using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Core
{
    public class ModItemManager
    {
        private List<ModItem> _Items = [];
        public List<ModItem> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }
        public ModItemManager()
        {
        }

        public void AddItem(ModItem item)
        {
            bool found = false;
            int counter = -1;
            // check if room already exists in the room pool
            while (!found && counter < _Items.Count - 1)
            {
                counter++;
                if (_Items[counter].Name == item.Name)
                {
                    found = true;
                }
            }
            if (!found)
            {
                _Items.Add(item);
            }
            else
            {
                Plugin.BepinLogger.LogMessage($"Item {item.Name} already added, can't add multiple copies.");
            }
        }
        public void StartOfDay() {
            foreach (ModItem item in _Items) {
                if (!item.HasBeenFound)
                {
                    //replace with custom AP Item
                }
                // TODO check if check recieved and waiting, if so add item to the inventory.

            }
        }
        public void OnItemCheckRecieved(string itemCheck) { 
            // Handle the code for recieving an item check that results in receiving an item.
        }
    }

    public class ModItem( string name, GameObject gameObject, bool isUnlocked, bool isUnique, bool isJunk = false, string junkType = "coins"){
        private string _Name = name;
        public string Name { get { return _Name; } set { _Name = value; } }

        private GameObject _GameObj = gameObject;
        public GameObject GameObj { get { return _GameObj; } set { _GameObj = value; } }

        private bool _IsUnlocked = isUnlocked;
        public bool IsUnlocked
        {
            get { return _IsUnlocked; }
            set { _IsUnlocked = value; }
        }
        private bool _HasBeenFound = false;

        public bool HasBeenFound {
            get { return _HasBeenFound; }
            set
            {
                //Send the room drafted event on the first time this room is drafted only.
                if (!_HasBeenFound && value)
                {
                    ModInstance.Instance.ModEventHandler.OnFirstFound(this);
                    _HasBeenFound = value;
                }
                // No changes to value once the room has been drafted once, or if someone is not trying to set this to true for some stupid reason.
            }
        }

        private bool _IsUnique = isUnique;
        public bool IsUnique { 
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }
        private bool _IsJunk = isJunk;

        public bool IsJunk { 
            get { return _IsJunk; }
            set { _IsJunk = value; }
        }
        private string _JunkType = junkType;
        public string JunkType { 
            get { return _JunkType; }
            set { _JunkType = value; }
        }

        public void AddItemToInventory() { 
            //TODO add code handling adding the item.
        }
        public void AddCoins() {
            if (_IsJunk && JunkType == "coins") {
            }
        }
        public void AddSteps() {
            if (_IsJunk && JunkType == "steps")
            {
            }
        }
        public void AddGems() {
            if (_IsJunk && JunkType == "gems")
            {
            }
        }
    }
}
