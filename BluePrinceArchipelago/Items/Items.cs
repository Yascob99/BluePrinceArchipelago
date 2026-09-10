using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    /// <summary>
    ///     A class template for handling the item in a mod context.
    /// </summary>
    /// <param name="name">The Name of the item</param>
    /// <param name="gameObject">The GameObject of the item.</param>
    /// <param name="isUnlocked">If the item is Unlocked.</param>
    /// <param name="count">The number of the item in the pool.</param>
    public class ModItem(string name, GameObject gameObject, bool isUnlocked, int count = 1)
    {
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

        private int _Count = count;
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        private bool _IsUnique = false;
        public bool IsUnique
        {
            get { return _IsUnique; }
            set { _IsUnique = value; }
        }

        /// <summary>
        ///     Handles adding the item to the player inventory.
        /// </summary>
        public virtual void AddItemToInventory()
        {
            // Put out an error if this method was not properly overriden. There should be no base moditems.
            Logging.LogError("Error: The Base Moditem.AddItemToInventory method should be overriden.");
        }
    }
}
