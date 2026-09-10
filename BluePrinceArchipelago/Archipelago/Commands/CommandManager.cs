using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using System.Linq;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A Rudimentary manager for in game console commands.
    /// </summary>
    public static class CommandManager
    {
        private static Dictionary<string, Command> _LocalCommands = new();
        private static Dictionary<string, Command> _ServerCommands = new();

        /// <summary>
        ///     Registers a local command.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="command">The command Object to register.</param>
        public static void AddLocalCommand(string commandName, Command command)
        {
            _LocalCommands[commandName.Trim().ToLower()] = command;
        }
        /// <summary>
        ///     Registers a server command.Currently not in use.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="command">The command Object to register.</param>
        public static void AddServerCommand(string commandName, Command command)
        {
            _ServerCommands[commandName] = command;
        }

        /// <summary>
        ///     Evaluates if the given message is a command and runs the relevant command.
        /// </summary>
        /// <param name="command">The message to evaluate.</param>
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
        /// <inheritdoc cref="RunLocalCommand(string)"/>
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

        /// <summary>
        ///     Runs the help text command and outputs it to the console.
        /// </summary>
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

        /// <summary>
        ///     Initializes all of the locally defined commands.
        /// </summary>
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
            _LocalCommands["resetdata"] = new ResetDataCommand("ResetData");
            _LocalCommands["collect"] = new CollectCommand("Collect"); // Collect location from the Archipelago item pool, for testing purposes.
            _LocalCommands["recordevent"] = new RecordEventCommand("RecordEvent"); // records an event to set some of the vanilla states
        }

        /// <summary>
        ///     Parses and breaks down a command into the command and it's arguements.
        /// </summary>
        /// <param name="command">The Command to Parse.</param>
        /// <returns>A ParsedCommand with the command and it's arguements.</returns>
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

    /// <summary>
    ///     A data structure for containing the parsed command name and it's arguements.
    /// </summary>
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

    /// <summary>
    ///     A command for reseting the cached and stored data about the current run.
    /// </summary>
    /// <param name="name">The name of the Command.</param>
    public class ResetDataCommand(string name) : Command(name)
    {
        public override string Description => "Resets the stored data so a new run can be properly started.";

        public override string Syntax => "Usage:\n\t/ResetData";

        public override void Run(List<string> Args)
        {
            State.Reset();
            State.Initialize();
        }
    }

    /// <summary>
    ///     A Command for simulating an in game event for testing permanent unlocks.
    /// </summary>
    /// <param name="name">The name of the Command.</param>
    public class RecordEventCommand(string name) : Command(name)
    {
        public override string Description => "Records an event to set some of the vanilla states (for testing purposes).";

        public override string Syntax => "Usage:\n\t/RecordEvent <EventName>\n\nExample:\n\t/RecordEvent Orchard_Unlocked";

        public override void Run(List<string> Args)
        {
            var eventName = string.Join(" ", Args);
            if (eventName.StartsWith("\"") && eventName.EndsWith("\""))
                eventName = eventName[1..^1];

            var eventID = EventID.Null;
            switch (eventName.ToLower())
            {
                case "west_path_gate_unlocked":
                case var _ when eventName.ToLower().Contains("west") && eventName.ToLower().Contains("gate") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.West_Path_Gate_Unlocked;
                    break;

                case "gemstone_cavern_unlocked":
                case var _ when eventName.ToLower().Contains("gemstone") && eventName.ToLower().Contains("cavern") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.Gemstone_Cavern_Unlocked;
                    break;

                case "orchard_unlocked":
                case var _ when eventName.ToLower().Contains("orchard") && eventName.ToLower().Contains("unlocked"):
                    eventID = EventID.Orchard_Unlocked;
                    break;

                case "satellite_raised":
                case var _ when eventName.ToLower().Contains("satellite") && eventName.ToLower().Contains("raised"):
                    eventID = EventID.Satellite_Raised;
                    break;

                case "blackbridge_powered":
                case var _ when eventName.ToLower().Contains("blackbridge") && eventName.ToLower().Contains("powered"):
                    eventID = EventID.Blackbridge_Powered;
                    break;

                default:
                    ArchipelagoConsole.LogMessage($"Unknown event name: {eventName}");
                    return;
            }

            ModInstance.StatsLogger.GetComponent<StatsLogger>().Record_Event(eventID);
        }
    }
}
