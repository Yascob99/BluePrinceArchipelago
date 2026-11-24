using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    public abstract class GameItem : IObtainable
    {
		protected readonly string _name;
		public string Name { get { return _name; } }

		protected bool _archipelagoUnlocked = false;

        public bool IsUnlocked { get { return _archipelagoUnlocked; } }

        protected bool _hasBeenFound = false;
        public bool HasBeenFound
        {
            get { return _hasBeenFound; }
            set
            {
                if (_hasBeenFound && !value)
                {
                    Logging.LogError($"Error trying to set unfound {_name} that has already been found");
                    return;
                }
                _hasBeenFound = value;
            }
        }

        protected GameObject _inventoryGameObject;

        public GameItem(string name, GameObject inventoryGameObject)
        {
            _name = name;
            _inventoryGameObject = inventoryGameObject;
		}

        public void ArchipelagoUnlocked()
        {
            _archipelagoUnlocked = true;
        }

        public abstract void AddToWorld();

        /// <summary>
        ///     Preserve normal behavior for the item.
        /// </summary>
        internal void NotInMultiworld()
        {
            _archipelagoUnlocked = true;
            _hasBeenFound = true;
        }
    }
}