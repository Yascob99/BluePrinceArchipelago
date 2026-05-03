using BepInEx;
using BepInEx.Unity.IL2CPP.UnityEngine;
using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using StableNameDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago;

// shamelessly stolen from oc2-modding https://github.com/toasterparty/oc2-modding/blob/main/OC2Modding/GameLog.cs with modifications for Blue Prince.
public static class ArchipelagoConsole
{
    public static bool Hidden = true;

    private static List<string> logLines = new();
    private static Vector2 scrollView;
    private static Rect window;
    private static Rect scroll;
    private static Rect text;
    private static Rect hideShowButton;

    private static GUIStyle textStyle = new();
    private static string scrollText = "";
    private static float lastUpdateTime = Time.time;
    private const float HideTimeout = 15f;

    private static string CommandText = "/help";
    private static Rect CommandTextRect;
    private static Rect SendCommandButton;
    private static List<string> PreviousCommands = [];
    private static int PreviousCommandPointer = -1;

    public static void Awake()
    {
        UpdateWindow();
    }

    public static void LogMessage(string message)
    {
        if (message.IsNullOrWhiteSpace()) return;
        //Handle multiline messages.
        if (message.Contains("\n"))
        {
            foreach (string submessage in message.Split("\n"))
            {
                logLines.Add(submessage);
                Logging.Log(message);
                lastUpdateTime = Time.time;
                UpdateWindow();
            }
        }
        else
        {
            logLines.Add(message);
            Logging.Log(message);
            lastUpdateTime = Time.time;
            UpdateWindow();
        }
    }

    public static void OnGUI()
    {
        //TODO add keyboard shortcut for hidding/unhidding. Prevent the input from causing the player to move until input has been submitted or window has been rehidden.
        // if (logLines.Count == 0) return;  
        Event e = Event.current;
        //Shows the Input Window
        if (Hidden && Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.Slash))
        {
            Hidden = !Hidden;
            UpdateWindow();
        }
        if (!Hidden && Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.Escape))
        {
            Hidden = !Hidden;
            UpdateWindow();
        }
        if (!Hidden && e.type == EventType.KeyDown)
        {
            if (e.keyCode == UnityEngine.KeyCode.UpArrow)
            {
                if (PreviousCommandPointer > 0)
                {
                    PreviousCommandPointer--;
                    CommandText = PreviousCommands[PreviousCommandPointer];
                }
                else
                {
                    PreviousCommandPointer = PreviousCommands.Count - 1;

                }
            }
        }

        if (!Hidden || Time.time - lastUpdateTime < HideTimeout)
        {
            scrollView = GUI.BeginScrollView(window, scrollView, scroll);
            GUI.Box(text, "");
            GUI.Box(text, scrollText, textStyle);
            GUI.EndScrollView();
        }

        if (GUI.Button(hideShowButton, Hidden ? "Show" : "Hide"))
        {
            Hidden = !Hidden;
            UpdateWindow();
        }

        // draw client/server commands entry if not hidden.
        if (Hidden) return;
        CommandText = GUI.TextField(CommandTextRect, CommandText);
        if (!CommandText.IsNullOrWhiteSpace() && (GUI.Button(SendCommandButton, "Send") || e.type == EventType.KeyDown && (e.keyCode == UnityEngine.KeyCode.Return || e.character == '\n')))
        {
            //local command
            if (CommandText.Trim()[0] == '/')
            {
                CommandManager.RunLocalCommand(CommandText);
                PreviousCommands.Add(CommandText);
                CommandText = "";
                PreviousCommandPointer = -1;
            }
            else if (ArchipelagoClient.Authenticated)
            {
                Plugin.ArchipelagoClient.SendMessage(CommandText);
                PreviousCommands.Add(CommandText);
                CommandText = "";
                PreviousCommandPointer = -1;
            }
        }
    }

    public static void UpdateWindow()
    {
        scrollText = "";

        if (Hidden)
        {
            if (logLines.Count > 0)
            {
                scrollText = logLines[logLines.Count - 1];
            }
        }
        else
        {
            for (var i = 0; i < logLines.Count; i++)
            {
                scrollText += logLines.ElementAt(i);
                if (i < logLines.Count - 1)
                {
                    scrollText += "\n";
                }
            }
        }

        var width = (int)(Screen.width * 0.4f);
        int height;
        int scrollDepth;
        if (Hidden)
        {
            height = (int)(Screen.height * 0.03f);
            scrollDepth = height;
        }
        else
        {
            height = (int)(Screen.height * 0.3f);
            scrollDepth = height * 10;
        }

        window = new Rect(Screen.width / 2 - width / 2, 0, width, height);
        scroll = new Rect(0, 0, width * 0.9f, scrollDepth);
        scrollView = new Vector2(0, scrollDepth);
        text = new Rect(0, 0, width, scrollDepth);

        textStyle.alignment = TextAnchor.LowerLeft;
        textStyle.fontSize = (int)(Screen.height * 0.0165f);
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = !Hidden;

        var xPadding = (int)(Screen.width * 0.01f);
        var yPadding = (int)(Screen.height * 0.01f);

        textStyle.padding = Hidden
            ? new RectOffset(xPadding / 2, xPadding / 2, yPadding / 2, yPadding / 2)
            : new RectOffset(xPadding, xPadding, yPadding, yPadding);

        var buttonWidth = (int)(Screen.width * 0.12f);
        var buttonHeight = (int)(Screen.height * 0.03f);

        hideShowButton = new Rect(Screen.width / 2 + width / 2 + buttonWidth / 3, Screen.height * 0.004f, buttonWidth,
            buttonHeight);

        // draw server command text field and button
        width = (int)(Screen.width * 0.4f);
        var xPos = (int)(Screen.width / 2.0f - width / 2.0f);
        var yPos = (int)(Screen.height * 0.307f);
        height = (int)(Screen.height * 0.022f);

        CommandTextRect = new Rect(xPos, yPos, width, height);

        width = (int)(Screen.width * 0.035f);
        yPos += (int)(Screen.height * 0.03f);
        SendCommandButton = new Rect(xPos, yPos, width, height);
    }
}
public static class CommandManager
{
    private static Dictionary<string, Command> _LocalCommands = new();
    private static Dictionary<string, Command> _ServerCommands = new();
    public static void AddLocalCommand(string commandName, Command command)
    {
        _LocalCommands[commandName.Trim().ToLower()] = command;
    }
    public static void AddServerCommand(string commandName, Command command)
    {
        _ServerCommands[commandName] = command;
    }
    public static void RunLocalCommand(string command)
    {
        ParsedCommand parsedCommand = ParseCommand(command.Substring(1)); //Parse command ignoring the first character which is the command indicator.
        string commandName = parsedCommand.Command.ToLower();
        if (_LocalCommands.ContainsKey(commandName))
        {
            ArchipelagoConsole.LogMessage(command);
            _LocalCommands[commandName].Run(parsedCommand.Args);
            return;
        }
        ArchipelagoConsole.LogMessage($"{commandName} is not a recognized command.");
    }
    public static void RunServerCommand(string command)
    {
        ParsedCommand parsedCommand = ParseCommand(command);
        string commandName = parsedCommand.Command.ToLower();

        if (_ServerCommands.ContainsKey(commandName))
        {
            _ServerCommands[commandName].Run(parsedCommand.Args);
            return;
        }
        ArchipelagoConsole.LogMessage($"{commandName} is not a recognized command.");
    }
    public static void PrintHelpText()
    {
        string[] Keys = _LocalCommands.Keys.ToArray();
        foreach (string key in Keys)
        {
            if (key != "help")
            {
                ArchipelagoConsole.LogMessage("Name:\n\t" + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key));
                ArchipelagoConsole.LogMessage("Description:\n\t" + _LocalCommands[key].Description);
                ArchipelagoConsole.LogMessage(_LocalCommands[key].Syntax);
            }
        }
    }
    public static void initializeLocalCommands()
    {
        _LocalCommands["room"] = new RoomCommand("Room");
        _LocalCommands["roompool"] = new RoomCommand("RoomPool"); // Alias for room command
        _LocalCommands["adjust"] = new AdjustCommand("Adjust");
        _LocalCommands["item"] = new ItemCommand("Item");
        _LocalCommands["help"] = new HelpCommand("Help");
        _LocalCommands["force"] = new ForceCommand("Force");
        _LocalCommands["sync"] = new SyncCommand("Sync"); // New sync command for Archipelago data
        _LocalCommands["debug"] = new DebugCommand("Debug"); // Debug command for investigating game systems
        _LocalCommands["received"] = new ReceivedCommand("Received"); // Show received Archipelago items

        _LocalCommands["collect"] = new CollectCommand("Collect"); // Collect location from the Archipelago item pool, for testing purposes.
    }
    private static ParsedCommand ParseCommand(string command)
    {
        if (command.Length > 1)
        {
            bool quoteOpen = false;
            List<string> args = [];
            string curr = "";
            int count = 0;
            string commandName = "";
            foreach (char c in command)
            {
                if (c == '"')
                {
                    quoteOpen = !quoteOpen;
                }
                else if ((c == ' ') && !quoteOpen)
                {
                    if (count == 0)
                    {
                        commandName = curr;
                        count++;
                        curr = "";
                    }
                    else
                    {
                        args.Add(curr);
                        count++;
                        curr = "";
                    }

                }
                else
                {
                    curr += c;
                }
            }
            if (command.Length == curr.Length)
            {
                commandName = curr;
            }
            else if (curr.Length > 0)
            {
                args.Add(curr);
            }

            return new ParsedCommand(commandName, args);
        }
        return new ParsedCommand("", [""]);
    }
}
public abstract class Command(string name)
{
    public string Name = name;

    public abstract string Description
    {
        get;
    }
    public abstract string Syntax
    {
        get;
    }

    public abstract void Run(List<string> Args);
}
public class RoomCommand(string name) : Command(name)
{
    private readonly string _Description = "Manages the room pool - add, remove, list, or clear rooms";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage:\n\t/room add <RoomName> - Add a room to the pool\n\t/room remove <RoomName> - Remove a room from the pool\n\t/room list - List all rooms and their pool status\n\t/room list unlocked - List only unlocked rooms\n\t/room clear - Remove all non-vanilla rooms from pool\n\t/room clearall - Clear ALL rooms (for Archipelago mode)";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args)
    {
        if (Args.Count < 1)
        {
            ArchipelagoConsole.LogMessage($"Error: No subcommand provided.\n{_Syntax}");
            return;
        }

        string subcommand = Args[0].ToLower();

        // List doesn't require being in a run
        if (subcommand == "list")
        {
            bool unlockedOnly = Args.Count > 1 && Args[1].ToLower() == "unlocked";
            ListRooms(unlockedOnly);
            return;
        }

        // Other commands require being in a run
        if (!ModInstance.IsInRun)
        {
            ArchipelagoConsole.LogMessage("You are not currently in a run. You can only modify the pool during a run.");
            return;
        }

        if (subcommand == "add")
        {
            if (Args.Count < 2)
            {
                ArchipelagoConsole.LogMessage("Error: No room name provided.\nUsage: /room add <RoomName>");
                return;
            }
            string roomName = string.Join(" ", Args.Skip(1));
            AddRoomToPool(roomName);
        }
        else if (subcommand == "remove")
        {
            if (Args.Count < 2)
            {
                ArchipelagoConsole.LogMessage("Error: No room name provided.\nUsage: /room remove <RoomName>");
                return;
            }
            string roomName = string.Join(" ", Args.Skip(1));
            RemoveRoomFromPool(roomName);
        }
        else if (subcommand == "clear")
        {
            ClearPool();
        }
        else if (subcommand == "clearall")
        {
            ClearAllForArchipelago();
        }
        else
        {
            ArchipelagoConsole.LogMessage($"Error: Unknown subcommand '{subcommand}'.\n{_Syntax}");
        }
    }

    private void ListRooms(bool unlockedOnly)
    {
        var rooms = Plugin.ModRoomManager.Rooms;
        if (rooms == null || rooms.Count == 0)
        {
            ArchipelagoConsole.LogMessage("No rooms have been initialized yet.");
            return;
        }

        int unlockedCount = 0;
        int lockedCount = 0;
        int vanillaCount = 0;

        ArchipelagoConsole.LogMessage(unlockedOnly ? "=== Unlocked Rooms ===" : "=== All Rooms ===");
        foreach (var room in rooms)
        {
            if (room.IsUnlocked) unlockedCount++;
            else lockedCount++;
            if (room.UseVanilla) vanillaCount++;

            if (unlockedOnly && !room.IsUnlocked) continue;

            string status = room.IsUnlocked ? "[UNLOCKED]" : "[LOCKED]";
            string vanilla = room.UseVanilla ? " (Vanilla)" : " (AP Mode)";
            string poolInfo = $"Pool: {room.RoomsLeftInPool}/{room.RoomPoolCount}";
            ArchipelagoConsole.LogMessage($"  {status} {room.Name}{vanilla} - {poolInfo}");
        }
        ArchipelagoConsole.LogMessage($"Summary: {unlockedCount} unlocked, {lockedCount} locked, {vanillaCount} vanilla mode");
    }

    private void AddRoomToPool(string roomName)
    {
        ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomName.ToUpper());
        if (room == null)
        {
            ArchipelagoConsole.LogMessage($"Error: '{roomName}' is not a valid room name.");
            return;
        }

        room.IsUnlocked = true;
        room.RoomPoolCount++;
        Plugin.ModRoomManager.UpdateRoomPools();
        ArchipelagoConsole.LogMessage($"Added '{room.Name}' to the pool. Pool count: {room.RoomPoolCount}");
    }

    private void RemoveRoomFromPool(string roomName)
    {
        ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomName.ToUpper());
        if (room == null)
        {
            ArchipelagoConsole.LogMessage($"Error: '{roomName}' is not a valid room name.");
            return;
        }

        if (!room.IsUnlocked)
        {
            ArchipelagoConsole.LogMessage($"'{room.Name}' is already not in the pool.");
            return;
        }

        room.IsUnlocked = false;
        Plugin.ModRoomManager.UpdateRoomPools();
        ArchipelagoConsole.LogMessage($"Removed '{room.Name}' from the pool.");
    }

    private void ClearPool()
    {
        Plugin.ModRoomManager.EmptyDraftPool();
        Plugin.ModRoomManager.UpdateRoomPools();
        ArchipelagoConsole.LogMessage("Cleared all non-vanilla rooms from the pool.");
    }

    private void ClearAllForArchipelago()
    {
        Plugin.ModRoomManager.ClearAllRoomsForArchipelago();
        if (ModInstance.IsInRun)
        {
            Plugin.ModRoomManager.UpdateRoomPools();
        }
        ArchipelagoConsole.LogMessage("Cleared ALL rooms and disabled vanilla mode for Archipelago.");
    }
}
public class AdjustCommand(string name) : Command(name)
{
    private string _Description = "Allows you to Adjust the ammount of certain run resources";
    public override string Description
    {
        get { return _Description; }
    }
    private string _Syntax = "Usage:\n\t/Adjust Gems <Adjustment_Amount>\n\t/Adjust Keys <Adjustment_Amount>\n\t/Adjust Dice <Adjustment_Amount>\n\t/Adjust Stars <Adjustment_Amount>\n\t/Adjust Steps <Adjustment_Amount>\n\t/Adjust Gold <Adjustment_Amount>\n\tAdjust Luck <Adjustment_Amount>";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        ArchipelagoConsole.LogMessage(Args.Join(" "));
        if (!ModInstance.IsInRun)
        {
            ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
            return;
        }
        if (Args.Count == 2)
        {
            string subcommand = Args[0];
            if (subcommand.ToLower() == "gems")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    ModInstance.GemManager.FindIntVariable("Gem Adjustment Amount").Value = count;
                    ModInstance.GemManager.SendEvent("Update with Sound");
                    ArchipelagoConsole.LogMessage($"Adjusted Gems by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "gold")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    ModInstance.GoldManager.FindIntVariable("Adjustment Amount").Value = count;
                    ModInstance.GoldManager.SendEvent("Update");
                    ArchipelagoConsole.LogMessage($"Adjusted Gold by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "steps")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    ModInstance.StepManager.FindIntVariable("Adjustment Amount").Value = count;
                    ModInstance.StepManager.SendEvent("Update");
                    ArchipelagoConsole.LogMessage($"Adjusted Steps by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "dice")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    ModInstance.DiceManager.FindIntVariable("Adjustment Amount").Value = count;
                    ModInstance.DiceManager.SendEvent("Update");
                    ArchipelagoConsole.LogMessage($"Adjusted Dice by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "keys")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    ModInstance.KeyManager.FindIntVariable("Adjustment Amount").Value = count;
                    ModInstance.KeyManager.SendEvent("Update");
                    ArchipelagoConsole.LogMessage($"Adjusted Keys by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "stars")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    int totalStars = ModInstance.StarManager.FindIntVariable("TotalStars").Value;
                    if (totalStars + count > 0)
                    {
                        ModInstance.StarManager.FindIntVariable("TotalStars").Value = totalStars + count;

                    }
                    else
                    {
                        ModInstance.StarManager.FindIntVariable("TotalStars").Value = 0;
                    }
                    ArchipelagoConsole.LogMessage($"Adjusted Stars by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            else if (subcommand.ToLower() == "luck")
            {
                try
                {
                    int count = int.Parse(Args[1]);
                    int luck = ModInstance.LuckManager.FindIntVariable("LUCK").Value;
                    if (luck + count > 0)
                    {
                        ModInstance.LuckManager.FindIntVariable("LUCK").Value = luck + count;

                    }
                    else
                    {
                        ModInstance.LuckManager.FindIntVariable("LUCK").Value = 0;
                    }
                    ArchipelagoConsole.LogMessage($"Adjusted Luck by {count}.");
                    return;
                }
                catch
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {Args[1]} is not a valid integer.");
                    return;
                }

            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
            return;
        }
        ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
    }
}
public class ItemCommand(string name) : Command(name)
{
    private string _Description = "Adds or Removes Items from the inventory.";
    public override string Description
    {
        get { return _Description; }
    }
    private string _Syntax = "Usage\n/Item Add <Item>\n/Item Remove <Item>\n/Item List <prespawn|estateitems|pickedup|coatcheck|useditems>";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args)
    {
        if (!ModInstance.IsInRun)
        {
            ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
            return;
        }
        if (Args.Count > 1)
        {
            string subcommand = Args[0];
            if (subcommand.ToLower() == "list")
            {
                ArchipelagoConsole.LogMessage($"Item List\n{Plugin.ModItemManager.ListItems(Args[1])}");
                return;
            }
            else if (subcommand.ToLower() == "add")
            {
                string itemName = Args[1];
                for (int i = 2; i < Args.Count; i++)
                {
                    itemName += " " + Args[i];
                }

                ArchipelagoConsole.LogMessage($"Attemping to add item {itemName}");

                GameObject item = Plugin.ModItemManager.GetPreSpawnItem(itemName);
                if (item == null)
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Has already been spawned or is not in the spawn pool");
                    return;
                }

                // Check PreSpawn EstateItems, PickedUp, CoatCheck, UsedItems
                if (Plugin.ModItemManager.IsItemSpawnable(item))
                {

                    string iconName = itemName.ToTitleCase() + " Icon(Clone)001";
                    GameObject icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                    // Some icons use 
                    if (icon == null)
                    {
                        iconName = itemName.ToTitleCase() + " icon(Clone)001";
                        icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                    }
                    if (icon == null)
                    {
                        iconName = itemName.ToTitleCase();
                        icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                    }
                    PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                    if (icon != null && InventoryIcons != null)
                    {

                        if (ModItemManager.PreSpawn.Contains(item))
                        {
                            //ModItemManager.PreSpawn.Remove(item, "GameObject");
                            ModItemManager.PickedUp.Add(item, "GameObject");
                            InventoryIcons.Add(icon, "GameObject");
                            ArchipelagoConsole.LogMessage($"Added {itemName} to inventory.");
                            return;
                        }
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Has already been spawned.");
                        return;
                    }
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} is not a valid Item Name");
                    return;
                }

                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Can't be added to inventory.");
                return;
            }
            else if (subcommand.ToLower() == "remove")
            {
                string itemName = "";
                for (int i = 1; i < Args.Count; i++)
                {
                    itemName += Args[i];
                }
                GameObject item = Plugin.ModItemManager.GetPickedUpItem(itemName);
                if (item == null)
                {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} is not a valid Item Name or is not in your Inventory");
                    return;
                }
                string iconName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName.ToLower()) + " Icon(Clone)001";

                PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                if (InventoryIcons != null)
                {
                    GameObject icon = new();
                    bool found = false;
                    int i = 0;
                    while (!found && i < InventoryIcons.GetCount())
                    {
                        icon = InventoryIcons.arrayList[i].TryCast<GameObject>();
                        if (icon == null)
                        {
                            if (icon.name == iconName)
                            {
                                found = true;
                            }
                        }
                        i++;
                    }
                    if (!found)
                    {
                        ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName}'s Icon could not be found.");
                        return;
                    }
                    //TODO add a check for if the item should be added to the PreSpawn pool.
                    if (ModItemManager.PickedUp.Contains(item))
                    {
                        ModItemManager.PickedUp.Remove(item, "GameObject");
                        ModItemManager.PreSpawn.Add(item, "GameObject");
                        InventoryIcons.Remove(icon, "GameObject");
                        ArchipelagoConsole.LogMessage($"Removed Item from {itemName} inventory.");
                        return;
                    }
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Has already been spawned.");
                    return;
                }

                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} can't be removed from inventory.");
                return;

            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
            return;
        }
        else
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
    }
}
public class HelpCommand(string name) : Command(name)
{
    private string _Description = "Displays all Local Commands";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage\n\t/Help";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args)
    {
        CommandManager.PrintHelpText();
    }
}
public class ForceCommand(string name) : Command(name)
{
    private readonly string _Description = "Forces a draft of the room when next possible";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage\n\t/Force <Room>\n\t/Force <Room>";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args)
    {
        string roomName = string.Join(" ", Args);
        ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomName);
        if (room != null)
        {
            ModRoomManager.ForceRoomQueue.Add(room);
        }
    }
}

public class SyncCommand(string name) : Command(name)
{
    private readonly string _Description = "Syncs room pool with Archipelago received items";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage:\n\t/sync rooms - Sync room pool from Archipelago received items\n\t/sync status - Show sync status";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        if (Args.Count < 1)
        {
            ArchipelagoConsole.LogMessage($"Error: No subcommand provided.\n{_Syntax}");
            return;
        }

        string subcommand = Args[0].ToLower();

        if (subcommand == "rooms")
        {
            SyncRoomsFromArchipelago();
        }
        else if (subcommand == "status")
        {
            ShowSyncStatus();
        }
        else
        {
            ArchipelagoConsole.LogMessage($"Error: Unknown subcommand '{subcommand}'.\n{_Syntax}");
        }
    }

    private void SyncRoomsFromArchipelago()
    {
        if (!ArchipelagoClient.Authenticated)
        {
            ArchipelagoConsole.LogMessage("Error: Not connected to Archipelago. Please connect first.");
            return;
        }

        if (!ModInstance.HasInitializedRooms)
        {
            ArchipelagoConsole.LogMessage("Error: Rooms have not been initialized yet. Start a run first.");
            return;
        }

        // Check if RoomDraftSanity is enabled
        if (!ArchipelagoOptions.RoomDraftSanity)
        {
            ArchipelagoConsole.LogMessage("RoomDraftSanity is disabled in your Archipelago options.");
            ArchipelagoConsole.LogMessage("Room drafts will use vanilla behavior. No sync needed.");
            return;
        }

        var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;

        // Re-load arrays first to ensure we have fresh references
        ModInstance.ReloadArrays();

        // First, clear ALL rooms for Archipelago mode (disables vanilla handling too)
        Plugin.ModRoomManager.ClearAllRoomsForArchipelago();

        int syncedCount = 0;
        int skippedCount = 0;

        // Then unlock rooms that are in received items
        if (receivedItems != null && receivedItems.Count > 0)
        {
            foreach (string itemName in receivedItems)
            {
                if (Plugin.ModRoomManager.UnlockRoomForArchipelago(itemName))
                {
                    syncedCount++;
                }
                else
                {
                    // Item is not a room, skip it
                    skippedCount++;
                }
            }
        }

        // Update the pools after sync
        Plugin.ModRoomManager.UpdateRoomPools();

        ArchipelagoConsole.LogMessage($"Room sync complete: {syncedCount} rooms unlocked, {skippedCount} non-room items skipped.");
        ArchipelagoConsole.LogMessage("All rooms set to Archipelago mode (vanilla handling disabled).");
    }

    private void ShowSyncStatus()
    {
        if (!ArchipelagoClient.Authenticated)
        {
            ArchipelagoConsole.LogMessage("Status: Not connected to Archipelago");
            return;
        }

        var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
        int receivedRoomCount = 0;
        int unlockedRoomCount = 0;

        // Count received rooms
        if (receivedItems != null)
        {
            foreach (string itemName in receivedItems)
            {
                if (Plugin.ModRoomManager.GetRoomByName(itemName.ToUpper()) != null)
                {
                    receivedRoomCount++;
                }
            }
        }

        // Count unlocked rooms
        foreach (var room in Plugin.ModRoomManager.Rooms)
        {
            if (room.IsUnlocked && !room.UseVanilla)
            {
                unlockedRoomCount++;
            }
        }

        ArchipelagoConsole.LogMessage($"=== Sync Status ===");
        ArchipelagoConsole.LogMessage($"Connected: Yes");
        ArchipelagoConsole.LogMessage($"Received room items: {receivedRoomCount}");
        ArchipelagoConsole.LogMessage($"Currently unlocked (non-vanilla): {unlockedRoomCount}");
        ArchipelagoConsole.LogMessage($"Total items received: {receivedItems?.Count ?? 0}");
    }
}

public class ParsedCommand
{
    public string Command;
    public List<string> Args;
    public ParsedCommand(string command, List<string> args)
    {
        Command = command;
        Args = args;
    }
}

public class ReceivedCommand(string name) : Command(name)
{
    private readonly string _Description = "Lists items received from Archipelago";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage:\n\t/received - List all received items\n\t/received rooms - List only received rooms\n\t/received items - List only received non-room items\n\t/received count - Show counts by category";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        if (!ArchipelagoClient.Authenticated)
        {
            ArchipelagoConsole.LogMessage("Not connected to Archipelago.");
            return;
        }

        var receivedItems = ArchipelagoClient.ServerData.ReceivedItems;
        if (receivedItems == null || receivedItems.Count == 0)
        {
            ArchipelagoConsole.LogMessage("No items received from Archipelago yet.");
            return;
        }

        string subcommand = Args.Count > 0 ? Args[1].ToLower() : "all";

        if (subcommand == "rooms")
        {
            ListReceivedRooms(receivedItems);
        }
        else if (subcommand == "items")
        {
            ListReceivedNonRooms(receivedItems);
        }
        else if (subcommand == "count")
        {
            ShowCounts(receivedItems);
        }
        else
        {
            ListAll(receivedItems);
        }
    }

    private void ListReceivedRooms(List<string> receivedItems)
    {
        var rooms = receivedItems.Where(i => Plugin.ModRoomManager.GetRoomByName(i.ToUpper()) != null).ToList();
        ArchipelagoConsole.LogMessage($"=== Received Rooms ({rooms.Count}) ===");
        foreach (var room in rooms)
        {
            ModRoom modRoom = Plugin.ModRoomManager.GetRoomByName(room.ToUpper());
            string poolInfo = modRoom != null ? $" [Pool: {modRoom.RoomsLeftInPool}/{modRoom.RoomPoolCount}]" : "";
            ArchipelagoConsole.LogMessage($"  {room}{poolInfo}");
        }
    }

    private void ListReceivedNonRooms(List<string> receivedItems)
    {
        var nonRooms = receivedItems.Where(i => Plugin.ModRoomManager.GetRoomByName(i.ToUpper()) == null).ToList();
        ArchipelagoConsole.LogMessage($"=== Received Non-Room Items ({nonRooms.Count}) ===");
        foreach (var item in nonRooms)
        {
            string type = Plugin.ModItemManager.GetItemType(item) ?? "Unknown";
            ArchipelagoConsole.LogMessage($"  [{type}] {item}");
        }
    }

    private void ShowCounts(List<string> receivedItems)
    {
        int roomCount = 0;
        int permanentCount = 0;
        int junkCount = 0;
        int unknownCount = 0;

        foreach (var item in receivedItems)
        {
            if (Plugin.ModRoomManager.GetRoomByName(item.ToUpper()) != null)
            {
                roomCount++;
            }
            else
            {
                string type = Plugin.ModItemManager.GetItemType(item);
                if (type == "Permanent") permanentCount++;
                else if (type == "Junk") junkCount++;
                else unknownCount++;
            }
        }

        ArchipelagoConsole.LogMessage($"=== Received Item Counts ===");
        ArchipelagoConsole.LogMessage($"  Rooms:     {roomCount}");
        ArchipelagoConsole.LogMessage($"  Permanent: {permanentCount}");
        ArchipelagoConsole.LogMessage($"  Junk:      {junkCount}");
        ArchipelagoConsole.LogMessage($"  Unknown:   {unknownCount}");
        ArchipelagoConsole.LogMessage($"  Total:     {receivedItems.Count}");
    }

    private void ListAll(List<string> receivedItems)
    {
        ArchipelagoConsole.LogMessage($"=== All Received Items ({receivedItems.Count}) ===");
        foreach (var item in receivedItems)
        {
            bool isRoom = Plugin.ModRoomManager.GetRoomByName(item.ToUpper()) != null;
            string type = isRoom ? "Room" : (Plugin.ModItemManager.GetItemType(item) ?? "Unknown");
            ArchipelagoConsole.LogMessage($"  [{type}] {item}");
        }
    }
}



/// <summary>
/// Debug command to investigate game systems like FSMs, draft pools, and the Entrance Hall.
/// </summary>
public class DebugCommand(string name) : Command(name)
{
    private readonly string _Description = "Debug tools to investigate game systems";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage:\n\t/debug entrance - Investigate Entrance Hall FSM\n\t/debug arrays - List all picker arrays\n\t/debug pool <ArrayName> - List rooms in array with status\n\t/debug poolstatus - Check POOL REMOVAL for all rooms\n\t/debug fsm <path> - Inspect FSM at path\n\t/debug grid - Show current grid/draft info";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        if (Args.Count < 1)
        {
            ArchipelagoConsole.LogMessage($"Error: No subcommand provided.\n{_Syntax}");
            return;
        }

        string subcommand = Args[0].ToLower();

        if (subcommand == "entrance")
        {
            InvestigateEntranceHall();
        }
        else if (subcommand == "arrays")
        {
            ListPickerArrays();
        }
        else if (subcommand == "grid")
        {
            ShowGridInfo();
        }
        else if (subcommand == "fsm" && Args.Count > 1)
        {
            string path = string.Join(" ", Args.Skip(1));
            InspectFSM(path);
        }
        else if (subcommand == "pool" && Args.Count > 1)
        {
            string arrayName = string.Join(" ", Args.Skip(1));
            InspectPoolArray(arrayName);
        }
        else if (subcommand == "poolstatus")
        {
            CheckAllPoolRemoval();
        }
        else
        {
            ArchipelagoConsole.LogMessage($"Error: Unknown subcommand '{subcommand}'.\n{_Syntax}");
        }
    }

    private void InvestigateEntranceHall()
    {
        ArchipelagoConsole.LogMessage("=== Investigating Entrance Hall Draft System ===");

        // Look for the Entrance Hall room engine
        GameObject entranceEngine = GameObject.Find("__SYSTEM/The Room Engines/ENTRANCE HALL");
        if (entranceEngine != null)
        {
            ArchipelagoConsole.LogMessage($"Found Entrance Hall engine at: {entranceEngine.name}");

            // List all FSMs on this object
            var fsms = entranceEngine.GetComponents<PlayMakerFSM>();
            foreach (var fsm in fsms)
            {
                ArchipelagoConsole.LogMessage($"  FSM: {fsm.FsmName}");

                // Look for relevant variables
                foreach (var boolVar in fsm.FsmVariables.BoolVariables)
                {
                    if (boolVar.Name.ToUpper().Contains("POOL") || boolVar.Name.ToUpper().Contains("DRAFT"))
                    {
                        ArchipelagoConsole.LogMessage($"    Bool: {boolVar.Name} = {boolVar.Value}");
                    }
                }
            }
        }
        else
        {
            ArchipelagoConsole.LogMessage("Entrance Hall engine not found!");
        }

        // Look for Entrance Hall specific picker/draft components
        GameObject planPicker = GameObject.Find("__SYSTEM/THE DRAFT/PLAN PICKER");
        if (planPicker != null)
        {
            ArchipelagoConsole.LogMessage($"\nPlan Picker children count: {planPicker.transform.childCount}");

            // Look for anything with "Entrance" or "Hall" in the name
            for (int i = 0; i < planPicker.transform.childCount; i++)
            {
                var child = planPicker.transform.GetChild(i);
                string childName = child.name.ToUpper();
                if (childName.Contains("ENTRANCE") || childName.Contains("HALL") || childName.Contains("FRONT") || childName.Contains("FIRST"))
                {
                    ArchipelagoConsole.LogMessage($"  [{i}] {child.name}");
                    var proxy = child.GetComponent<PlayMakerArrayListProxy>();
                    if (proxy != null)
                    {
                        ArchipelagoConsole.LogMessage($"       Array count: {proxy.GetCount()}");
                    }
                }
            }
        }

        // Look for "Entrance Draft" or similar GameObjects
        string[] searchPaths = [
            "__SYSTEM/THE DRAFT/ENTRANCE",
            "__SYSTEM/THE DRAFT/ENTRANCE HALL",
            "__SYSTEM/THE DRAFT/FRONT DOOR",
            "__SYSTEM/THE DRAFT/PLAN PICKER/ENTRANCE",
            "__SYSTEM/THE DRAFT/PLAN PICKER/FRONT"
        ];

        foreach (var path in searchPaths)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                ArchipelagoConsole.LogMessage($"\nFound: {path}");
                var fsms = obj.GetComponents<PlayMakerFSM>();
                foreach (var fsm in fsms)
                {
                    ArchipelagoConsole.LogMessage($"  FSM: {fsm.FsmName}");
                }
            }
        }

        // Check the Grid for current draft info
        if (ModInstance.TheGrid != null)
        {
            ArchipelagoConsole.LogMessage($"\nGrid Variables:");
            var planPickVar = ModInstance.TheGrid.GetGameObjectVariable("theplanpick");
            if (planPickVar != null && planPickVar.Value != null)
            {
                ArchipelagoConsole.LogMessage($"  Current plan picker: {planPickVar.Value.name}");
            }

            var currentRoom = ModInstance.TheGrid.GetStringVariable("CURRENT ROOM");
            if (currentRoom != null)
            {
                ArchipelagoConsole.LogMessage($"  Current room: {currentRoom.Value}");
            }
        }
    }

    private void ListPickerArrays()
    {
        ArchipelagoConsole.LogMessage("=== All Picker Arrays ===");

        if (ModInstance.PickerDict == null || ModInstance.PickerDict.Count == 0)
        {
            ArchipelagoConsole.LogMessage("No picker arrays loaded.");
            return;
        }

        foreach (var kvp in ModInstance.PickerDict)
        {
            int count = kvp.Value?.GetCount() ?? 0;
            ArchipelagoConsole.LogMessage($"  {kvp.Key}: {count} rooms");
        }

        // Also list all children of Plan Picker to find any we might have missed
        GameObject planPicker = GameObject.Find("__SYSTEM/THE DRAFT/PLAN PICKER");
        if (planPicker != null)
        {
            ArchipelagoConsole.LogMessage($"\nAll Plan Picker children ({planPicker.transform.childCount} total):");
            for (int i = 0; i < Mathf.Min(planPicker.transform.childCount, 65); i++)
            {
                var child = planPicker.transform.GetChild(i);
                var proxy = child.GetComponent<PlayMakerArrayListProxy>();
                string proxyInfo = proxy != null ? $" [Array: {proxy.GetCount()}]" : "";

                // Check if this is in our PickerDict
                bool tracked = ModInstance.PickerDict.ContainsKey(child.name.Trim());
                string trackedInfo = tracked ? "" : " *NOT TRACKED*";

                ArchipelagoConsole.LogMessage($"  [{i}] {child.name}{proxyInfo}{trackedInfo}");
            }
        }
    }

    private void ShowGridInfo()
    {
        ArchipelagoConsole.LogMessage("=== Grid/Draft Info ===");

        if (ModInstance.TheGrid == null)
        {
            ArchipelagoConsole.LogMessage("Grid not initialized.");
            return;
        }

        // List all variables
        foreach (var strVar in ModInstance.TheGrid.FsmVariables.StringVariables)
        {
            ArchipelagoConsole.LogMessage($"  String: {strVar.Name} = {strVar.Value}");
        }

        foreach (var boolVar in ModInstance.TheGrid.FsmVariables.BoolVariables)
        {
            ArchipelagoConsole.LogMessage($"  Bool: {boolVar.Name} = {boolVar.Value}");
        }

        foreach (var goVar in ModInstance.TheGrid.FsmVariables.GameObjectVariables)
        {
            string goName = goVar.Value != null ? goVar.Value.name : "null";
            ArchipelagoConsole.LogMessage($"  GameObject: {goVar.Name} = {goName}");
        }
    }

    private void InspectFSM(string path)
    {
        GameObject obj = GameObject.Find(path);
        if (obj == null)
        {
            ArchipelagoConsole.LogMessage($"GameObject not found at: {path}");
            return;
        }

        ArchipelagoConsole.LogMessage($"=== FSMs at {path} ===");

        var fsms = obj.GetComponents<PlayMakerFSM>();
        if (fsms.Length == 0)
        {
            ArchipelagoConsole.LogMessage("No FSMs found on this object.");
            return;
        }

        foreach (var fsm in fsms)
        {
            ArchipelagoConsole.LogMessage($"\nFSM: {fsm.FsmName}");
            ArchipelagoConsole.LogMessage($"  Active State: {fsm.ActiveStateName}");
            ArchipelagoConsole.LogMessage($"  States ({fsm.FsmStates.Length}):");

            foreach (var state in fsm.FsmStates)
            {
                ArchipelagoConsole.LogMessage($"    - {state.Name}");
            }

            ArchipelagoConsole.LogMessage($"  Global Transitions:");
            foreach (var trans in fsm.FsmGlobalTransitions)
            {
                ArchipelagoConsole.LogMessage($"    - {trans.EventName} -> {trans.ToState}");
            }
        }
    }

    /// <summary>
    /// Lists all rooms in a specific picker array and checks their POOL REMOVAL status.
    /// Usage: /debug pool "FRONT - Tier 1"
    /// </summary>
    public void InspectPoolArray(string arrayName)
    {
        if (!ModInstance.PickerDict.ContainsKey(arrayName))
        {
            ArchipelagoConsole.LogMessage($"Array '{arrayName}' not found in PickerDict.");
            ArchipelagoConsole.LogMessage("Available arrays:");
            foreach (var key in ModInstance.PickerDict.Keys.Take(20))
            {
                ArchipelagoConsole.LogMessage($"  - {key}");
            }
            return;
        }

        var array = ModInstance.PickerDict[arrayName];
        ArchipelagoConsole.LogMessage($"=== Pool Array: {arrayName} ({array.GetCount()} rooms) ===");

        for (int i = 0; i < array.GetCount(); i++)
        {
            var roomObj = array.arrayList[i].TryCast<GameObject>();
            if (roomObj != null)
            {
                string roomName = roomObj.name;

                // Check the room's POOL REMOVAL status
                string poolRemovalStatus = "?";
                var roomEngine = GameObject.Find("__SYSTEM/The Room Engines/" + roomName);
                if (roomEngine != null)
                {
                    var fsm = roomEngine.GetFsm(roomName);
                    if (fsm != null)
                    {
                        var poolRemoval = fsm.GetBoolVariable("POOL REMOVAL");
                        if (poolRemoval != null)
                        {
                            poolRemovalStatus = poolRemoval.Value ? "REMOVED" : "AVAILABLE";
                        }
                    }
                }

                // Check our ModRoom status
                var modRoom = Plugin.ModRoomManager.GetRoomByName(roomName);
                string modStatus = modRoom != null ? (modRoom.IsUnlocked ? "Unlocked" : "Locked") : "Not tracked";

                ArchipelagoConsole.LogMessage($"  [{i}] {roomName} - FSM:{poolRemovalStatus}, Mod:{modStatus}");
            }
        }
    }

    /// <summary>
    /// Check POOL REMOVAL status for all room engines.
    /// </summary>
    public void CheckAllPoolRemoval()
    {
        ArchipelagoConsole.LogMessage("=== Checking POOL REMOVAL for All Room Engines ===");

        var roomEngines = GameObject.Find("__SYSTEM/The Room Engines");
        if (roomEngines == null)
        {
            ArchipelagoConsole.LogMessage("Room Engines not found!");
            return;
        }

        int available = 0;
        int removed = 0;
        int unknown = 0;

        for (int i = 0; i < roomEngines.transform.childCount; i++)
        {
            var child = roomEngines.transform.GetChild(i);
            var fsm = child.GetComponent<PlayMakerFSM>();

            if (fsm != null)
            {
                var poolRemoval = fsm.GetBoolVariable("POOL REMOVAL");
                if (poolRemoval != null)
                {
                    if (poolRemoval.Value)
                    {
                        removed++;
                        // Only log removed rooms (to keep output manageable)
                        // ArchipelagoConsole.LogMessage($"  REMOVED: {child.name}");
                    }
                    else
                    {
                        available++;
                    }
                }
                else
                {
                    unknown++;
                }
            }
        }

        ArchipelagoConsole.LogMessage($"Summary: {available} available, {removed} removed, {unknown} unknown");
        ArchipelagoConsole.LogMessage($"Total room engines: {roomEngines.transform.childCount}");
    }
}

public class CollectCommand(string name) : Command(name)
{
    public override string Description => "Collects a location from the Archipelago item pool (for testing purposes).";

    public override string Syntax => "Usage:\n\t/collect <LocationName>\n\nExample:\n\t/collect Closet First Entering";

    public override void Run(List<string> Args)
    {
        var locationName = string.Join(" ", Args.Skip(1));
        if (locationName.StartsWith("\"") && locationName.EndsWith("\""))
            locationName = locationName[1..^1];

        ModInstance.ModEventHandler.OnOtherLocation(locationName);
    }
}