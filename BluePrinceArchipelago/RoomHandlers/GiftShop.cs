
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public class GiftShop : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];
    private GameObject _GiftShopMenuGameObject;

    public GiftShop()
    {
        Logging.Log("Initializing Gift Shop.");
        _GiftShopMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Giftshop Menu")?.gameObject;
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        if (_GiftShopMenuGameObject == null)
        {
            _GiftShopMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Giftshop Menu")?.gameObject;
            if (_GiftShopMenuGameObject == null)
            {
                Logging.LogError("Failed to find Gift Shop Menu GameObject, aborting OnRoomDrafted.");
                return;
            }
        }

        var pricesAndNames = _GiftShopMenuGameObject.transform.Find("Prices and Item Names");
        if (pricesAndNames == null)        
        {
            Logging.LogError("Failed to find Prices and Item Names GameObject in Gift Shop Menu.)");
            return;
        }

        for (int i = 1; i <= 7; i++)
        {
            if (i == 4) // skip Dirigiblocks since it can't be purchased
                continue;

            var itemNameObject = pricesAndNames.transform.Find($"Item {i} Name");
            if (itemNameObject == null)
            {
                Logging.LogWarning($"Failed to find Item {i} Name GameObject in Gift Shop Menu.");
                continue;
            }

            var textComponent = itemNameObject.GetComponent<TextMeshPro>();
            var itemName = textComponent?.text;

            if (!LocationMap.ContainsKey(itemName))
            {
                LocationMap.Add(itemName, new Models.GiftShopItem
                {
                    Name = itemName,
                });
            }

            var shopItem = LocationMap[itemName];

            textComponent.text = shopItem.GetScoutHint();

            Logging.Log($"Set up Gift Shop item: {itemName} as {textComponent.text}");
        }
    }

    // UI OVERLAY CAM/Giftshop Menu/Prices and Item Names/Item <N> Name
}