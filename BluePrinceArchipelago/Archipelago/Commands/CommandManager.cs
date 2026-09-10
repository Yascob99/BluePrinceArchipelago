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
        ///     Parses and breaks down a string into the command and it's arguements.
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
}
