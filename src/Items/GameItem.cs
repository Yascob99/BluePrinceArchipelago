using Il2CppSystem.Runtime.InteropServices;
using UnityEngine;

namespace BluePrinceArchipelago.Items
{
    public abstract class GameItem : IObtainable
    {
		private readonly string _name;
		public string Name { get; }

		protected bool _archipelagoUnlocked = false;

        public bool IsUnlocked { get { return _archipelagoUnlocked; } }

        private readonly GameObject _inventoryGameObject;

        public GameItem(GameObject inventoryGameObject)
        {
            _inventoryGameObject = inventoryGameObject;
		}

		public void ArchipelagoUnlocked()
		{
			_archipelagoUnlocked = true;
		}

        public abstract void AddToWorld();
    }
}