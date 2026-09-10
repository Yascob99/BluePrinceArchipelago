using BepInEx;
using BepInEx.Unity.IL2CPP.UnityEngine;
using BluePrinceArchipelago.Archipelago.Commands;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Models;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using StableNameDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago;

/// <summary>
///     Shamelessly stolen from oc2-modding https://github.com/toasterparty/oc2-modding/blob/main/OC2Modding/GameLog.cs with modifications for Blue Prince.
/// </summary>
public static class ArchipelagoConsole
{
    public static bool Hidden = true;
    public static bool ShowOnlyRelevantMessages = true;

    private static List<string> logLines = new();
    private static Vector2 scrollView;
    private static Rect window;
    private static Rect scroll;
    private static Rect text;
    private static Rect hideShowButton;

    private static GUIStyle textStyle = new();
    private static string scrollText = "";
    private static string previousScrollText = "";
    private static int previousStart = 0;
    private static int previousEnd = 0;
    private static float lastUpdateTime = Time.time;
    private const float HideTimeout = 15f;
    private const int MaxLogLines = 5000;

    private static string CommandText = "/help";
    private static Rect CommandTextRect;
    private static Rect SendCommandButton;
    private static List<string> PreviousCommands = [];
    private static int PreviousCommandPointer = -1;
    private static List<string> TextFieldNames = ["URI", "SlotName", "Password", "CommandText"];

    /// <summary>
    ///     Unity Monobehaviour Awake()
    /// </summary>
    public static void Awake()
    {
        UpdateWindow();
    }

    /// <summary>
    ///     Logs a Message in the in Game Console.
    /// </summary>
    /// <param name="message">The Message to log.</param>
    /// <param name="logTag">The Tag of the message. Defaults to "ArchipelagoConsole"</param>
    /// <param name="isServerMessage">Whether the message is from the server.</param>
    public static void LogMessage(string message, string logTag = "ArchipelagoConsole", bool isServerMessage = false)
    {
        if (message.IsNullOrWhiteSpace()) return;

        //Handle multiline messages.
        // Log any relevant messages to the archipelago console;
        if (IsRelevantMessage(message))
        {
            Logging.Log(message, logTag);
        }
        if (message.Contains('\n'))
        {
            foreach (string submessage in message.Split("\n"))
            {
                logLines.Add(submessage);
            }
            lastUpdateTime = Time.time;
            UpdateWindow();
        }
        else
        {
            logLines.Add(message);
            lastUpdateTime = Time.time;
            UpdateWindow();
        }
    }

    /// <summary>
    ///     If the message should be logged to the bepinex console.
    /// </summary>
    /// <param name="message">The string message.</param>
    /// <returns>Returns true if the message should be logged. False Otherwise.</returns>
    private static bool IsRelevantMessage(string message)
    {
        if (!ShowOnlyRelevantMessages) return true;
        if (message.Contains(ArchipelagoClient.ServerData.SlotName) || message.Contains("[Server]") || message.Contains("You can't afford the hint")) return true;
        return false;
    }

    /// <summary>
    ///     To be run on a Unity OnGUI update.
    /// </summary>
    public static void OnGUI()
    {
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
                    CommandText = PreviousCommands[PreviousCommandPointer];
                    PreviousCommandPointer--;
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
            //PreviousCursorLockstate = Cursor.lockState;
            UpdateWindow();
        }

        // draw client/server commands entry if not hidden.
        if (Hidden) {
            //When the console is hidden make sure keyboard controls are selectable.
            ToggleKeyboardInput(false);
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // show the mod is currently loaded in the corner
        GUI.Label(new Rect(16, 116, 300, 20), Plugin.ModDisplayInfo);

        // Prevents tabbing from affecting the GUI fields (Was getting really annoying with alt-tabbing)
        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == UnityEngine.KeyCode.Tab || Event.current.character == '\t'))
        {
            Event.current.Use(); // Marks the event as used, stopping propagation
        }

        string statusMessage;
        // show the Archipelago Version and whether we're connected or not
        if (ArchipelagoClient.Authenticated)
        {
            // if your game doesn't usually show the cursor this line may be necessary

            statusMessage = " Status: Connected";
            GUI.Label(new Rect(16, 150, 300, 20), Plugin.APDisplayInfo + statusMessage);
        }
        else
        {
            // if your game doesn't usually show the cursor this line may be necessary

            statusMessage = " Status: Disconnected";
            GUI.Label(new Rect(16, 150, 300, 20), Plugin.APDisplayInfo + statusMessage);
            GUI.Label(new Rect(16, 170, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 190, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 210, 150, 20), "Password: ");

            GUI.SetNextControlName("URI");
            ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 170, 150, 20),
                ArchipelagoClient.ServerData.Uri);
            GUI.SetNextControlName("SlotName");
            ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 190, 150, 20),
                ArchipelagoClient.ServerData.SlotName);
            GUI.SetNextControlName("Password");
            ArchipelagoClient.ServerData.Password = GUI.PasswordField(new Rect(150, 210, 150, 20),
                ArchipelagoClient.ServerData.Password, '*');
            // requires that the player at least puts *something* in the slot name
            if (GUI.Button(new Rect(16, 230, 100, 20), "Connect") &&
                !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
            {
                ConnectionData connData = new ConnectionData();
                connData.Uri = ArchipelagoClient.ServerData.Uri;
                connData.SlotName = ArchipelagoClient.ServerData.SlotName;
                connData.Password = ArchipelagoClient.ServerData.Password;
                State.UpdateServerDetails(connData);
                Plugin.ArchipelagoClient.Connect();
            }
        }
        GUI.SetNextControlName("CommandText");
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
        ToggleKeyboardInput(TextFieldNames.Contains(GUI.GetNameOfFocusedControl()));
    }

    /// <summary>
    ///     Whether to allow keyboard input to move the player charcter.
    /// </summary>
    /// <param name="focused">Whether the textfields are currently focused.</param>
    private static void ToggleKeyboardInput(bool focused) {
        var keyboard = Rewired.ReInput.controllers.Keyboard;
        
         if (focused)
         {
            if (keyboard.enabled)
            {
                keyboard.enabled = false;
            }
         }
         else
         {
            keyboard.enabled = true;
         }
    }

    /// <summary>
    ///     Performs the redraw and update of the console window UI.
    /// </summary>
    public static void UpdateWindow()
    {
        scrollText = "";
        int currentLogLines = logLines.Count;
        // Create a behind the scenes text form of the log that is cached;
        int start = Math.Max(0, currentLogLines - MaxLogLines);
        // If the scrolltext has not been initialized.
        if (previousScrollText == "")
        {
            for (var i = start; i < currentLogLines; i++)
            {
                previousScrollText += logLines[i];
                previousScrollText += "\n";
            }
            previousStart = start;
            previousEnd = currentLogLines;
        }
        // Otherwise use the previously cached string as a basis;
        else
        {
            string newLines = previousScrollText;
            // If the starting line has shifted, delete that many lines.
            if (start > previousStart)
            {
                int linesToDelete = start - previousStart;
                int index = 0;
                // Iterate through characters until the next newline is found, or the end is reached.
                while (linesToDelete > 0 && index < newLines.Length)
                {
                    char current = newLines[index];
                    if (current == '\n')
                    {
                        linesToDelete--;
                    }
                    index++;
                }
                // Update the new data;
                newLines = previousScrollText.Substring(index - 1);
                // Update the start to be the new start;
                previousStart = start;
            }
            // If a new line(s) got added, add them to the end of the cached scrolltext;
            // Cache the length in case extra lines get added while updating.
            int lengthDiff = currentLogLines - previousEnd;
            if (lengthDiff > 0)
            {
                for (int i = 0; i < lengthDiff; i++)
                {
                    newLines += logLines[previousEnd + i];
                    newLines += '\n';
                }
                previousEnd += lengthDiff;
            }
            // Finally set the scrollText to the new data;
            previousScrollText = newLines;
        }
        if (Hidden)
        {
            if (currentLogLines > 0)
            {
                scrollText = logLines[^1];
            }
        }
        else
        {
            scrollText = previousScrollText;
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
            if (currentLogLines < 10)
            {
                scrollDepth = (int)(Screen.height * 0.3f);
            }
            else
            {
                scrollDepth = (int)(Screen.height * 0.03f * currentLogLines + 1);
            }
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

