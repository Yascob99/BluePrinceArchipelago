using BluePrinceArchipelago.Archipelago;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Utils;
using System;

namespace BluePrinceArchipelago.Events
{
    /// <summary>
    ///     The custom Event Args for a location event.
    /// </summary>
    public class LocationEventArgs : EventArgs
    {
        public string LocationName { get; set; }
        public string LocationType { get; set; }

        public LocationEventArgs(string locationName, string locationType)
        {
            LocationName = locationName;
            LocationType = locationType;
        }
    }

    /// <summary>
    ///     The Mod's event handler.
    /// </summary>
    public class ModEventHandler
    {
        public delegate void LocationHandler(System.Object sender, LocationEventArgs args);

        public event LocationHandler LocationFound;
        
        /// <summary>
        ///     Triggers the OnFirstDrafted Event
        /// </summary>
        /// <param name="room">The room that triggered it.</param>
        public void OnFirstDrafted(ModRoom room) => OnFirstDrafted(room.Name);
        
        /// <inheritdoc cref="OnFirstDrafted(ModRoom)"/>
        /// <param name="roomName">The name of the room.</param>
        public void OnFirstDrafted(string roomName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{roomName.ToTitleCase()} First Entering", "First Draft Room"));
            // Send second Bunk Room location.
            if (roomName.ToUpper() == "BUNK ROOM")
            {
                LocationFound.Invoke(this, new LocationEventArgs($"{roomName.ToTitleCase()} First Entering 2", "First Draft Room 2"));
            }
        }

        /// <summary>
        ///     The Classroom First Drafted Event.
        /// </summary>
        /// <param name="classroomNumber">The grade number of the drafted classroom.</param>
        public void OnClassroomFirstDrafted(string classroomNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Classroom {classroomNumber} First Entering", "First Draft Room"));
        }

        /// <summary>
        ///     The item first found event.
        /// </summary>
        /// <param name="item">The ModItem that was found.</param>
        public void OnFirstFound(ModItem item) {
            LocationFound.Invoke(this, new LocationEventArgs($"{item.Name.ToTitleCase()} First Pickup", "Item First Pickup"));
        }

        /// <summary>
        ///     The Upgrade Disk first pickup event.
        /// </summary>
        /// <param name="locationName">The Upgrade disk location name.</param>
        public void OnUgradeDiskFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Upgrade Disk - {locationName.ToTitleCase()}", "Upgrade Disk Found"));
        }

        /// <summary>
        ///     The Vault Key first pickup event.
        /// </summary>
        /// <param name="keyNumber">The number of the key found.</param>
        public void OnVaultKeyFound(string keyNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Vault Key {keyNumber.ToTitleCase()}", "Vault Key Found"));
        }

        /// <summary>
        ///     The Sanctum Key first pickup event.
        /// </summary>
        /// <param name="locationName">The location name of the sanctum key that was found.</param>
        public void OnSanctumKeyFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Sanctum Key - {locationName.ToTitleCase()}", "Sanctum Key Found"));
        }

        /// <summary>
        ///     The Cabinet key first pickup event.
        /// </summary>
        /// <param name="locationName">The location name of the first found Cabinet key.</param>
        public void OnCabinetKeyFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"File Cabinet Key - {locationName.ToTitleCase()}", "File Cabinet Key Found"));
        }

        /// <summary>
        ///     The Trunk open event.
        /// </summary>
        /// <param name="roomName">The name of the room the trunk was opened in.</param>
        /// <param name="trunkCount">The current number of opened trunks in that room.</param>
        public void OnTrunkOpened(string roomName, int trunkCount) {
            LocationFound.Invoke(this, new LocationEventArgs($"{roomName.ToTitleCase()} Locked Trunk {trunkCount}", "Locked Trunk Unlocked"));
        }

        /// <summary>
        ///     The Event of picking up a trophy.
        /// </summary>
        /// <param name="itemName">The trophy's name.</param>
        public void OnTrophyCollected(string itemName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{itemName.ToTitleCase()}", "Trophy Collected"));
        }

        /// <summary>
        ///     The event of a gate being opened.
        /// </summary>
        /// <param name="gateName">The name of the oppened gate.</param>
        public void OnGateOpened(string gateName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{gateName.ToTitleCase()}", "Gate Opened"));
        }

        /// <summary>
        ///     The event of a safe being opened.
        /// </summary>
        /// <param name="safeName">The name of the safe.</param>
        public void OnSafeOpened(string safeName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{safeName.ToTitleCase()}", "Safe Opened"));
        }

        /// <summary>
        ///     The event of a Mora Jai box being solved.
        /// </summary>
        /// <param name="puzzleName">The name of the Mora Jai Box.</param>
        public void OnMoraJaiSolved(string puzzleName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{puzzleName.ToTitleCase()} Mora Jai Box", "Mora Jai Puzzle Solved"));
        }

        /// <summary>
        ///     The event of a floorplan being unlocked/found.
        /// </summary>
        /// <param name="floorplanName">The name of the floorplan.</param>
        public void OnFloorplanFound(string floorplanName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{floorplanName.ToTitleCase()} Floorplan", "Floorplan Found"));
        }

        /// <summary>
        ///     The event of a wall being broken.
        /// </summary>
        /// <param name="wallName">The name of the broken wall.</param>
        public void OnWallBreak(string wallName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Break {wallName.ToTitleCase()} Wall", "Wall Broken"));
        }

        /// <summary>
        ///     The event of a basement door being unlocked.
        /// </summary>
        /// <param name="doorName">The name of the unlocked door.</param>
        public void OnUnlockBasementDoor(string doorName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Unlock Basement Door {doorName.ToTitleCase()}", "Basement Door Unlocked"));
        }

        /// <summary>
        ///     The event of a tomb puzzle being solved.
        /// </summary>
        /// <param name="puzzleNumber">The number of the solved tomb puzzle.</param>
        public void OnTombPuzzleSolved(string puzzleNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Solve Tomb Puzzle {puzzleNumber.ToTitleCase()}", "Tomb Puzzle Solved"));
        }

        /// <summary>
        ///     The event of the torch chamber shortcut being opened.
        /// </summary>
        public void OnOpenTorchChamberShortcut() {
            LocationFound.Invoke(this, new LocationEventArgs($"Open the Torch Chamber Shortcut", "Torch Chamber Shortcut Opened"));
        }

        /// <summary>
        ///     The event of a depositbox being opened.
        /// </summary>
        /// <param name="boxNumber">The deposit box that was opened.</param>
        public void OnOpenDepositBox(string boxNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Open Deposit Box {boxNumber.ToTitleCase()}", $"Deposit Box {boxNumber.ToTitleCase()} Opened"));
        }

        /// <summary>
        ///     The event of the Reservoir Door being opened. 
        /// </summary>
        public void OnOpenReservoirDoor() {
            LocationFound.Invoke(this, new LocationEventArgs("Open Basement to Reservoir Door", "Reservoir Door Opened"));
        }

        /// <summary>
        ///     The event of the Foundation Elevator being lowered.
        /// </summary>
        public void OnLowerFoundationElevator() {
            LocationFound.Invoke(this, new LocationEventArgs("Lower The Foundation Elevator", "Foundation Elevator Lowered"));
        }

        /// <summary>
        ///     The event of a vase being broken.
        /// </summary>
        /// <param name="vaseName">The vase which was broken.</param>
        public void OnVaseBroken(string vaseName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{vaseName.ToTitleCase()} Vase", "Vase Broken"));
        }

        /// <summary>
        ///     The event of the cursed coffers being opened.
        /// </summary>
        public void OnCursedCoffersOpened() {
            LocationFound.Invoke(this, new LocationEventArgs("Cursed Coffers", "Cursed Coffers Opened"));
        }

        /// <summary>
        ///     The event of a gas valve being turned.
        /// </summary>
        /// <param name="valveName">The name of the turned valve.</param>
        public void OnGasValveTurned(string valveName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Gasline Valve - {valveName.ToTitleCase()}", "Gas Valve Turned"));
        }

        /// <summary>
        ///     The event of the Sundial being scorched.
        /// </summary>
        public void OnSundialScorched() {
            LocationFound.Invoke(this, new LocationEventArgs("Scorch Sundial", "Sundial Scorched"));
        }

        /// <summary>
        ///     The event of the VAC controls being solved.
        /// </summary>
        public void OnVACControlsSolved() {
            LocationFound.Invoke(this, new LocationEventArgs("VAC Controls", "VAC Controls Solved"));
        }

        /// <summary>
        ///     The event of the satelite disk being raised.
        /// </summary>
        public void OnSatelliteRaised()
        {
            LocationFound.Invoke(this, new LocationEventArgs("Raise Satellite", "Raise Satellite"));
        }

        /// <summary>
        ///     The event of the Laboratory puzzle being solved.
        /// </summary>
        public void OnLaboratoryPuzzleSolved()
        {
            LocationFound.Invoke(this, new LocationEventArgs("Laboratory Puzzle - Blackbridge", "Laboratory Puzzle - Blackbridge"));
        }

        /// <summary>
        ///     The event of the Allowance token being collected.
        /// </summary>
        /// <param name="locationName">The location of the allowance token.</param>
        public void OnAllowanceCollected(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Allowance Token - {locationName.ToTitleCase()}", "Allowance Collected"));
        }

        /// <summary>
        ///     The event of the coffers being dug up.
        /// </summary>
        /// <param name="roomName">The name of the room in which the coffers was dug up.</param>
        public void OnCoffersDugUp(string roomName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Dig up The {roomName.ToTitleCase()} Treasure Chest", "Treasure Dug Up"));
        }

        /// <summary>
        ///     The event of a sanctum being solved.
        /// </summary>
        /// <param name="sanctumName">The name of the solved sanctum.</param>
        public void OnSanctumSolve(string sanctumName) {
            if (ArchipelagoOptions.GoalType == GoalType.option_sanctum) 
            {
                ModInstance.SanctumsSolved.Add(sanctumName);
                if (ModInstance.SanctumsSolved.Count >= ArchipelagoOptions.GoalSanctumSolves) 
                {
                    Plugin.ArchipelagoClient.GoalCompleted();
                }
            }
        }

        /// <summary>
        ///     The event of another location being handled.
        /// </summary>
        /// <param name="locationName">The name of the location.</param>
        /// <param name="locationType">The type of the location.</param>
        public void OnOtherLocation(string locationName, string locationType = null) {
            locationType ??= locationName.ToTitleCase();
            LocationFound.Invoke(this, new LocationEventArgs(locationName.ToTitleCase(), locationType));
        }
    }

}
