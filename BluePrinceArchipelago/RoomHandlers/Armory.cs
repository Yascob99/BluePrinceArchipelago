
using System.Collections.Generic;
using Il2CppSystem.Linq;
using TMPro;
using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public class Armory : RoomHandler
{
    private GameObject _ArmoryMenu;
    private Dictionary<string, Models.ShopItem> _ArmoryItemMap = [];
    private readonly Dictionary<string, (string TitlePath, string DescriptionPath1, string DescriptionPath2)> _ArmoryItemUIMap = new()
    {
        {"Morning Star", ("Morning Star/Pop Up/Title", "Morning Star/Pop Up/Description (2)", "Armory Menu/Morning Star/Pop Up/Description (3)")},
        {"Torch", ("Torch/Pop Up/Text/Title", "Torch/Pop Up/Text/GameObject/Description (2)", null)},
        {"Knight's Shield", ("Knight's Shield/Pop Up/Text/Title", "Knight's Shield/Pop Up/Text/Description (2)", "Knight's Shield/Pop Up/Text/Description (3)")},
        {"The Axe", ("The Axe/Pop Up/Text/Title", "The Axe/Pop Up/Text/Description (2)", "The Axe/Pop Up/Text/Desciption (2)/Description (3)")}
    };

    public Armory()
    {
        Logging.Log("Initializing Armory.");
    }

    public override void OnRoomDrafted(GameObject roomGameObject)
    {
        Logging.Log("Armory drafted, setting up.");
        RoomGameObject = roomGameObject;

        if (RoomGameObject == null)
        {
            Logging.LogError("Failed to find Armory room GameObject, aborting OnRoomDrafted.");
            return;
        }

        _ArmoryMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Armory Menu")?.gameObject;

        SetupArmoryItems();
    }

    // Morning Star preview model is misspelled as Mornign Star in scene

    private void SetupArmoryItems()
    {
        foreach (var item in _ArmoryItemUIMap)
        {
            var itemName = item.Key;
            var (TitlePath, DescriptionPath1, DescriptionPath2) = item.Value;

            Logging.Log($"Setting up Armory item: {itemName}");

            var titleTransform = _ArmoryMenu.transform.Find(TitlePath);
            var descriptionTransform1 = _ArmoryMenu.transform.Find(DescriptionPath1);
            var descriptionTransform2 = DescriptionPath2 != null ? _ArmoryMenu.transform.Find(DescriptionPath2) : null;
            if (titleTransform == null)
            {
                Logging.LogError($"Failed to find title transform for {itemName} at path: {TitlePath}");
                continue;
            }
            if (descriptionTransform1 == null)
            {
                Logging.LogError($"Failed to find description transform 1 for {itemName} at path: {DescriptionPath1}");
                continue;
            }
            if (DescriptionPath2 != null && descriptionTransform2 == null)
            {
                Logging.LogError($"Failed to find description transform 2 for {itemName} at path: {DescriptionPath2}");
                continue;
            }

            var titleTMP = _ArmoryMenu.transform.Find(TitlePath)?.GetComponent<TextMeshPro>();
            var descriptionTMP1 = _ArmoryMenu.transform.Find(DescriptionPath1)?.GetComponent<TextMeshProUGUI>();
            var descriptionTMP2 = DescriptionPath2 != null ? _ArmoryMenu.transform.Find(DescriptionPath2)?.GetComponent<TextMeshProUGUI>() : null;

            if (!_ArmoryItemMap.ContainsKey(itemName))
            {
                string[] descriptionLines;

                if (DescriptionPath2 != null)
                    descriptionLines = [descriptionTMP1.text, descriptionTMP2.text];
                else
                    descriptionLines = [descriptionTMP1.text];

                _ArmoryItemMap.Add(itemName, new Models.ShopItem
                {
                    Name = itemName,
                    DescriptionLines = descriptionLines
                });
            }

            var shopItem = _ArmoryItemMap[itemName];

            // var scoutHintParts = shopItem.GetScoutHintParts(DescriptionPath2 != null ? 2 : 1);
            string[] scoutHintParts = ["PLACEHOLDER", "Description line 1", "Description line 2"];

            titleTMP.text = scoutHintParts[0];

            if (DescriptionPath2 != null)
            {
                descriptionTMP1.text = scoutHintParts[1];
                descriptionTMP2.text = scoutHintParts[2];
            }
            else
            {
                descriptionTMP1.text = scoutHintParts[1];
            }
            
        }
    }
}