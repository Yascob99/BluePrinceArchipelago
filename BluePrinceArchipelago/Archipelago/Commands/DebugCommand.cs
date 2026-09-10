using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     Debug command to investigate game systems like FSMs, draft pools, and the Entrance Hall.
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

        /// <summary>
        ///     A function for outputting data about the Entrance Hall to the console.
        /// </summary>
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

        /// <summary>
        ///     Outputs information about picker arrays to the console.
        /// </summary>
        private void ListPickerArrays()
        {
            ArchipelagoConsole.LogMessage("=== All Picker Arrays ===");

            if (ModRoomManager.PickerDict == null || ModRoomManager.PickerDict.Count == 0)
            {
                ArchipelagoConsole.LogMessage("No picker arrays loaded.");
                return;
            }

            foreach (var kvp in ModRoomManager.PickerDict)
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
                    bool tracked = ModRoomManager.PickerDict.ContainsKey(child.name.Trim());
                    string trackedInfo = tracked ? "" : " *NOT TRACKED*";

                    ArchipelagoConsole.LogMessage($"  [{i}] {child.name}{proxyInfo}{trackedInfo}");
                }
            }
        }

        /// <summary>
        ///     Outputs information about The Grid to the console.
        /// </summary>
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

        /// <summary>
        ///     Outputs details about a particular FSM.
        /// </summary>
        /// <param name="path">The string path to the game object containing the FSM to inspect.</param>
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
        ///     Lists all rooms in a specific picker array and checks their POOL REMOVAL status.
        ///     Usage: /debug pool "FRONT - Tier 1"
        /// </summary>
        public void InspectPoolArray(string arrayName)
        {
            if (!ModRoomManager.PickerDict.ContainsKey(arrayName))
            {
                ArchipelagoConsole.LogMessage($"Array '{arrayName}' not found in PickerDict.");
                ArchipelagoConsole.LogMessage("Available arrays:");
                foreach (var key in ModRoomManager.PickerDict.Keys.Take(20))
                {
                    ArchipelagoConsole.LogMessage($"  - {key}");
                }
                return;
            }

            var array = ModRoomManager.PickerDict[arrayName];
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
                    var modRoom = ModRoomManager.GetRoomByName(roomName);
                    string modStatus = modRoom != null ? (modRoom.IsUnlocked ? "Unlocked" : "Locked") : "Not tracked";

                    ArchipelagoConsole.LogMessage($"  [{i}] {roomName} - FSM:{poolRemovalStatus}, Mod:{modStatus}");
                }
            }
        }

        /// <summary>
        ///     Check POOL REMOVAL status for all room engines.
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
}
