using AsmResolver.PE.DotNet.ReadyToRun;
using BepInEx;
using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Core;
using HutongGames.PlayMaker.Actions;
using Il2CppSystem;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace BluePrinceArchipelago.Utils;

// shamelessly stolen from oc2-modding https://github.com/toasterparty/oc2-modding/blob/main/OC2Modding/GameLog.cs
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
    private const int MaxLogLines = 80;
    private const float HideTimeout = 15f;

    private static string CommandText = "/help";
    private static Rect CommandTextRect;
    private static Rect SendCommandButton;

    public static void Awake()
    {
        UpdateWindow();
    }

    public static void LogMessage(string message)
    {
        if (message.IsNullOrWhiteSpace()) return;

        if (logLines.Count == MaxLogLines)
        {
            logLines.RemoveAt(0);
        }
        if (message.Contains("\n"))
        {
            foreach (string submessage in message.Split("\n"))
            {
                logLines.Add(submessage);
                Plugin.BepinLogger.LogMessage(message);
                lastUpdateTime = Time.time;
                UpdateWindow();
            }
        }
        else
        {
            logLines.Add(message);
            Plugin.BepinLogger.LogMessage(message);
            lastUpdateTime = Time.time;
            UpdateWindow();
        }
    }

    public static void OnGUI()
    {
        if (logLines.Count == 0) return;

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
        if (!CommandText.IsNullOrWhiteSpace() && GUI.Button(SendCommandButton, "Send"))
        {   
            //local command
            if (CommandText.Trim()[0] == '/') { 
                CommandManager.RunLocalCommand(CommandText);
            }
            if (ArchipelagoClient.Authenticated)
            {
                Plugin.ArchipelagoClient.SendMessage(CommandText);
                CommandText = "";
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
                scrollText += "";
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
public static class CommandManager {
    private static Dictionary<string, Command> _LocalCommands = new();
    private static Dictionary<string, Command> _ServerCommands = new();
    public static void AddLocalCommand(string commandName, Command command) {
        _LocalCommands[commandName.Trim().ToLower()] = command;
    }
    public static void AddServerCommand(string commandName, Command command)
    {
        _ServerCommands[commandName] = command;
    }
    public static void RunLocalCommand(string command) {
        ParsedCommand parsedCommand = ParseCommand(command.Substring(1)); //Parse command ignoring the first character which is the command indicator.
        string commandName = parsedCommand.Command.ToLower();
        if (_LocalCommands.ContainsKey(commandName))
        {
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
    public static void PrintHelpText() {
        string[] Keys = _LocalCommands.Keys.ToArray();
        foreach (string key in Keys) {
            if (key != "help") {
                ArchipelagoConsole.LogMessage("Name:\n\t" + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key));
                ArchipelagoConsole.LogMessage("Description:\n\t" + _LocalCommands[key].Description);
                ArchipelagoConsole.LogMessage(_LocalCommands[key].Syntax);
            }
        }
    }
    public static void initializeLocalCommands() {
        _LocalCommands["room"] = new RoomCommand("Room");
        _LocalCommands["adjust"] = new AdjustCommand("Adjust");
        _LocalCommands["item"] = new ItemCommand("Item");
        _LocalCommands["help"] = new HelpCommand("Help");
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
    public abstract string Description {
        get;
    }
    public abstract string Syntax
    {
        get;
    }

    public abstract void Run(List<string> Args);
}
public class RoomCommand(string name): Command(name)
{
    public string Name = name;
    private readonly string _Description = "Adds or removes rooms from the pool";
    public override string Description { 
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage\n\t/RoomPool Add <Room>\n\t/RoomPool Remove <Room>";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args) {
        if (!ModInstance.IsInRun)
        {
            ArchipelagoConsole.LogMessage("You are not currently in a run, you can only run this command during a run.");
            return;
        }
        if (Args.Count > 2)
        {
            string subcommand = Args[0];
            if (subcommand.ToLower() == "add")
            {
                string roomName = Args[1];
                for (int i = 2; i < Args.Count; i++)
                {
                    roomName += " " + Args[i];
                }
                ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomName.ToUpper());
                if (room != null) {
                    Plugin.ModRoomManager.AddRoom(room);
                    ArchipelagoConsole.LogMessage($"Added a copy of {roomName} to the pool.");
                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {roomName} is not a valid Room Name.");
                return;
            }
            else if (subcommand.ToLower() == "remove"){
                string roomName = "";
                for (int i = 1; i < Args.Count; i++)
                {
                    roomName += Args[i];
                }
                ModRoom room = Plugin.ModRoomManager.GetRoomByName(roomName.ToUpper());
                if (room != null)
                {
                    Plugin.ModRoomManager.RemoveRoom(room);
                    ArchipelagoConsole.LogMessage($"Removed a copy of {roomName} to the pool.");
                    return;
                }
                ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {roomName} is not a valid Room Name.");
                return;

            }
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
            return;
        }
    ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
    }
}
public class AdjustCommand(string name) : Command(name)
{
    public string Name = name;
    private string _Description = "Allows you to Adjust the ammount of certain run resources";
    public override string Description
    {
        get { return _Description; }
    }
    private string _Syntax = "Usage:\n\t/Adjust Gems <Adjustment_Amount>\n\t/Adjust Keys <Adjustment_Amount>\n\t/Adjust Dice <Adjustment_Amount>\n\t/Adjust Stars <Adjustment_Amount>\n\t/Adjust Steps <Adjustment_Amount>\n\t/Adjust Gold <Adjustment_Amount>";
    public override string Syntax
    {
        get { return _Syntax; }
    }

    public override void Run(List<string> Args)
    {
        ArchipelagoConsole.LogMessage(Args.Join(" "));
        if (!ModInstance.IsInRun) {
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
                        ModInstance.StarManager.FindIntVariable("TotalStars").Value = count;
                        
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
            ArchipelagoConsole.LogMessage($"Error Running Command {Name}: invalid subcommand {subcommand}");
            return;
        }
        ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
    }
}
public class ItemCommand(string name) : Command(name) {
    public string Name = name;
    private string _Description = "Adds or Removes Items from the inventory.";
    public override string Description
    {
        get { return _Description; }
    }
    private string _Syntax = "Usage\n/Item Add <Room>\n/Item Remove <Room>";
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
        if (Args.Count > 2)
        {
            string subcommand = Args[0];
            if (subcommand.ToLower() == "add")
            {
                string itemName = Args[1];
                for (int i = 2; i < Args.Count; i++)
                {
                    itemName += " " + Args[i];
                }
                GameObject item = Plugin.ModItemManager.GetPreSpawnItem(itemName);
                if (item == null) {
                    ArchipelagoConsole.LogMessage($"Error Running Command {Name} {subcommand}: {itemName} Has already been spawned or is not in the spawn pool");
                    return;
                }
                
                // Check PreSpawn EstateItems, PickedUp, CoatCheck, UsedItems
                if (Plugin.ModItemManager.IsItemSpawnable(item)) {
                    
                    string iconName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName.ToLower()) + " Icon(Clone)001";
                    GameObject icon = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/" + iconName);
                    PlayMakerArrayListProxy InventoryIcons = GameObject.Find("UI OVERLAY CAM/MENU/Blue Print /Inventory/")?.GetArrayListProxy("Inventory");
                    if (icon != null && InventoryIcons != null)
                    {

                        if (ModItemManager.PreSpawn.Contains(item))
                        {
                            ModItemManager.PreSpawn.Remove(item, "GameObject");
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
                        if (icon == null) {
                            if (icon.name == iconName) {
                                found = true;
                            }
                        }
                        i++;
                    }
                    if (!found) {
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
        ArchipelagoConsole.LogMessage($"Error Running Command {Name}: no parameters provided.");
    }
}
public class HelpCommand(string name) : Command(name) {
    public string Name = name;
    private readonly string _Description = "Displays all Local Commands";
    public override string Description
    {
        get { return _Description; }
    }
    private readonly string _Syntax = "Usage\n\t/Help";
    public override string Syntax
    {
        get { return _Syntax; }
    }
    public override void Run(List<string> Args) {
        CommandManager.PrintHelpText();
    }
}

public class ParsedCommand {
    public string Command;
    public List<string> Args;
    public ParsedCommand(string command, List<string> args) {
        Command = command;
        Args = args;
    }
}