
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public class Locksmith : RoomHandler
{
    public static Dictionary<string, Models.ShopItem> LocationMap { get; set; } = [];
    private GameObject _LocksmithMenuGameObject;
    public Locksmith()
    {
        Logging.Log("Initializing Locksmith.");
        _LocksmithMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Locksmith Menu")?.gameObject;
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        if (_LocksmithMenuGameObject == null)
        {
            _LocksmithMenuGameObject = GameObject.Find("UI OVERLAY CAM").transform.Find("Locksmith Menu")?.gameObject;
            if (_LocksmithMenuGameObject == null)
            {
                Logging.LogError("Failed to find Locksmith Menu GameObject, aborting OnRoomDrafted.");
                return;
            }
        }

        var pricesAndNames = _LocksmithMenuGameObject.transform.Find("Prices and Item Names");
        if (pricesAndNames == null)        
        {
            Logging.LogError("Failed to find Prices and Item Names GameObject in Locksmith Menu.)");
            return;
        }

        var itemNameObject = pricesAndNames.transform.Find("Item 4 Name");
        if (itemNameObject == null)
        {
            Logging.LogWarning("Failed to find Item 4 Name GameObject in Locksmith Menu.");
            return;
        }

        var textComponent = itemNameObject.GetComponent<TextMeshPro>();
        var itemName = textComponent?.text;

        if (!LocationMap.ContainsKey(itemName))
        {
            LocationMap.Add(itemName, new Models.ShopItem
            {
                Name = itemName,
            });
        }
        var shopItem = LocationMap[itemName];

        // textComponent.text = shopItem.GetScoutHint();
        textComponent.text = "Placeholder";

    }
}
// UI OVERLAY CAM/Locksmith Menu/Prices and Item Names/Item 4 Name