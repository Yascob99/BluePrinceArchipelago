
using System.Collections.Generic;
using BluePrinceArchipelago.FsmMethods;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
#if Bep
using HutongGames.PlayMaker;
using TMPro;
#endif
#if ML
using Il2Cpp;
using Il2CppHutongGames.PlayMaker;
using Il2CppTMPro;
#endif
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Armory : RoomHandler
{
    private static int currentInstance = 0;
    private GameObject _ArmoryMenu;
    private static Dictionary<string, Models.ShopItem> _ArmoryItemMap = [];
    private static readonly Dictionary<string, (string TitlePath, string DescriptionPath1, string DescriptionPath2)> _ArmoryItemUIMap = new()
    {
        {"Morning Star", ("Morning Star/Pop Up/Title", "Morning Star/Pop Up/Description (2)", "Morning Star/Pop Up/Description (3)")},
        {"Torch", ("Torch/Pop Up/Title", "Torch/Pop Up/Text/GameObject/Description (2)", null)},
        {"Knight's Shield", ("Knight's Shield/Pop Up/Title", "Knight's Shield/Pop Up/Description (2)", "Knight's Shield/Pop Up/Description (3)")},
        {"The Axe", ("The Axe/Pop Up/Title", "The Axe/Pop Up/Description (2)", "The Axe/Pop Up/Desciption (2)/Description (3)")}
    };

    private readonly string[] armoryObjectInstanceNames = { "TORCH", "KNIGHTS SHIELD", "MORNING STAR", "THE AXE (torch replacment", "THE AXE (MStar replacement)", "THE AXE (shield replacement)" };

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
    }

    public override void OnAfterRoomDrafted(GameObject roomGameObject)
    {
        ++currentInstance;
        SetupArmoryItems();

        foreach (string armoryObjectInstanceName in armoryObjectInstanceNames)
        {
            string path = $"__SYSTEM/Room Spawn Pools/The Armory(Clone)00{currentInstance}/_CULLABLE/_NON STATIC/{armoryObjectInstanceName}";
            GameObject armoryObjectInstance = GameObject.Find(path);
            if (armoryObjectInstance == null)
            {
                Logging.LogError($"Game instance for {armoryObjectInstanceName} not found", "ARMORY");
                continue;
            }
            if (!armoryObjectInstance.active)
            {
                Logging.LogError($"Game instance for {armoryObjectInstanceName} not active", "DEBUG");
                continue;
            }
            string uniqueItemName = armoryObjectInstanceName;
            if (armoryObjectInstanceName.Contains("THE AXE"))
            {
                uniqueItemName = "THE AXE";
            }
            UniqueItem armoryItem = ModItemManager.GetUniqueItem(uniqueItemName);
            ModItemManager.AddSwirly(armoryItem, armoryObjectInstance);

            string armoryShopUiName = armoryItem.Name.ToTitleCase();
            if (armoryItem.Name.ToUpper() == "KNIGHTS SHIELD")
            {
                armoryShopUiName = "Knight's Shield";
            }
            PlayMakerFSM buyingArmoryItemFSM = _ArmoryMenu.transform.Find(armoryShopUiName)?.transform.Find("Pop Up")?.transform.Find("2 Button Spread")?.transform.Find("YES BUTTON")?.GetComponent<PlayMakerFSM>();
            if (buyingArmoryItemFSM == null)
            {
                Logging.LogError($"Armory FSM for {armoryItem.Name} unfound", "ARMORY");
                continue;
            }
            FsmState buyEndState = buyingArmoryItemFSM.GetState("State 2");
            if (buyEndState == null)
            {
                Logging.LogError($"Buy state unfound for {armoryItem.Name}", "ARMORY");
                continue;
            }
            CustomFsmMethodManager.AddItemBoughtMethod(armoryItem);
            string customMethodName = CustomFsmMethodManager.GetItemBoughtMethodName(armoryItem.Name);
            buyEndState.AddAction(CustomFsmMethodManager.GetCallMethod(customMethodName));
        }
    }

    // Morning Star preview model is misspelled as Morning Star in scene

    private void SetupArmoryItems()
    {
        foreach (var item in _ArmoryItemUIMap)
        {
            var itemName = item.Key;
            if (itemName == "Knight's Shield")
                itemName = "Knights Shield";

            var (TitlePath, DescriptionPath1, DescriptionPath2) = item.Value;

            Logging.Log($"Setting up Armory item: {itemName}");

            var titleTransform = _ArmoryMenu.transform.Find(TitlePath);
            // var descriptionTransform1 = _ArmoryMenu.transform.Find(DescriptionPath1);
            // var descriptionTransform2 = DescriptionPath2 != null ? _ArmoryMenu.transform.Find(DescriptionPath2) : null;
            if (titleTransform == null)
            {
                Logging.LogError($"Failed to find title transform for {itemName} at path: {TitlePath}");
                continue;
            }
            // if (descriptionTransform1 == null)
            // {
            //     Logging.LogError($"Failed to find description transform 1 for {itemName} at path: {DescriptionPath1}");
            //     continue;
            // }
            // if (DescriptionPath2 != null && descriptionTransform2 == null)
            // {
            //     Logging.LogError($"Failed to find description transform 2 for {itemName} at path: {DescriptionPath2}");
            //     continue;
            // }

            var titleTMP = _ArmoryMenu.transform.Find(TitlePath)?.GetComponent<TextMeshPro>();
            // var descriptionTMP1 = _ArmoryMenu.transform.Find(DescriptionPath1)?.GetComponent<TextMeshProUGUI>();
            // var descriptionTMP2 = DescriptionPath2 != null ? _ArmoryMenu.transform.Find(DescriptionPath2)?.GetComponent<TextMeshProUGUI>() : null;

            if (!_ArmoryItemMap.ContainsKey(itemName))
            {
                // string[] descriptionLines;

                // if (DescriptionPath2 != null)
                //     descriptionLines = [descriptionTMP1.text, descriptionTMP2.text];
                // else
                //     descriptionLines = [descriptionTMP1.text];

                _ArmoryItemMap.Add(itemName, new Models.ShopItem
                {
                    Name = itemName,
                    // DescriptionLines = descriptionLines
                });
            }

            var shopItem = _ArmoryItemMap[itemName];

            // var scoutHintParts = shopItem.GetScoutHintParts(DescriptionPath2 != null ? 2 : 1);
            // string[] scoutHintParts = ["PLACEHOLDER", "Description line 1", "Description line 2"];

            titleTMP.text = shopItem.GetScoutHint();

            // if (DescriptionPath2 != null)
            // {
            //     descriptionTMP1.text = scoutHintParts[1];
            //     descriptionTMP2.text = scoutHintParts[2];
            // }
            // else
            // {
            //     descriptionTMP1.text = scoutHintParts[1];
            // }

        }
    }
}