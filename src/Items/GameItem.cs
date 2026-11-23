using Il2CppSystem.Runtime.InteropServices;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    public abstract class GameItem : IObtainable
    {
		protected readonly string _name;
		public string Name { get; }

		protected bool _archipelagoUnlocked = false;

        public bool IsUnlocked { get { return _archipelagoUnlocked; } }

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
    }
}