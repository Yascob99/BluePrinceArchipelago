
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers
{
    public class Bookshop : RoomHandler
    {
        private static Dictionary<string, Models.BookshopItem> _BookshopItemMap = [];
        private GameObject _BookshopMenu;
        public Bookshop()
        {
            Logging.Log("Initializing Bookshop.");
            _BookshopMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Bookshop Menu")?.gameObject;
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            if (_BookshopMenu == null)
            {
                _BookshopMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Bookshop Menu")?.gameObject;

                if (_BookshopMenu == null)
                {
                    Logging.LogError("Failed to find Bookshop Menu GameObject.");
                    return;
                }
            }

            var shopPricesAndNames = _BookshopMenu.transform.Find("Prices and Item Names");
            if (shopPricesAndNames == null)
            {
                Logging.LogError("Failed to find Prices and Item Names GameObject in Bookshop Menu.");
                return;
            }

            for (int i = 1; i <= 6; i++)
            {
                var itemNameObject = shopPricesAndNames?.transform.Find($"Item {i} Name")?.gameObject;
                if (itemNameObject == null)
                {
                    Logging.LogError($"Failed to find Item {i} Name GameObject in Bookshop Menu.");
                    continue;
                }

                var itemNameText = itemNameObject.GetComponent<TextMeshPro>();

                var target = itemNameText.text;

                if (!_BookshopItemMap.ContainsKey(target))
                {
                    _BookshopItemMap.Add(target, new Models.BookshopItem
                    {
                        Name = target,
                    });
                }

                var shopItem = _BookshopItemMap[target];

                itemNameText.text = shopItem.GetScoutHint();
            }
        }
    }
}