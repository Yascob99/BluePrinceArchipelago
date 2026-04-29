using BluePrinceArchipelago.Core;
using BluePrinceArchipelago.Utils;
using System;

namespace BluePrinceArchipelago.Events
{
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

    public class ModEventHandler
    {
        public delegate void LocationHandler(System.Object sender, LocationEventArgs args);

        public event LocationHandler LocationFound;

        //Triggers the OnFirstDrafted Event
        public void OnFirstDrafted(ModRoom room)
        {
            LocationFound.Invoke(this, new LocationEventArgs($"{room.Name.ToTitleCase()} First Entering", "First Draft Room"));
        }
        public void OnFirstFound(ModItem item) {
            LocationFound.Invoke(this, new LocationEventArgs($"{item.Name.ToTitleCase()} First Pickup", "Item First Pickup"));
        }
        public void OnUgradeDiskFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Upgrade Disk - {locationName.ToTitleCase()}", "Upgrade Disk Found"));
        }
        public void OnVaultKeyFound(string keyNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Vault Key {keyNumber.ToTitleCase()}", "Vault Key Found"));
        }
        public void OnSanctumKeyFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Sanctum Key - {locationName.ToTitleCase()}", "Sanctum Key Found"));
        }
        public void OnCabinetKeyFound(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"File Cabinet Key - {locationName.ToTitleCase()}", "File Cabinet Key Found"));
        }
        public void OnTrunkOpened(string roomName, int trunkCount) {
            LocationFound.Invoke(this, new LocationEventArgs($"{roomName.ToTitleCase()} Locked Trunk {trunkCount}", "Locked Trunk Unlocked"));
        }
        public void OnTrophyCollected(string itemName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{itemName.ToTitleCase()}", "Trophy Collected"));
        }
        public void OnGateOpened(string gateName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{gateName.ToTitleCase()}", "Gate Opened"));
        }
        public void OnSafeOpened(string safeName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{safeName.ToTitleCase()}", "Safe Opened"));
        }
        public void OnMoraJaiSolved(string puzzleName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{puzzleName.ToTitleCase()} Mora Jai Box", "Mora Jai Puzzle Solved"));
        }
        public void OnFloorplanFound(string floorplanName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{floorplanName.ToTitleCase()} Floorplan", "Floorplan Found"));
        }
        public void OnWallBreak(string wallName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Break {wallName.ToTitleCase()} Wall", "Wall Broken"));
        }
        public void OnUnlockBasementDoor(string doorName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Unlock Basement Door {doorName.ToTitleCase()}", "Basement Door Unlocked"));
        }
        public void OnTombPuzzleSolved(string puzzleNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Solve Tomb Puzzle {puzzleNumber.ToTitleCase()}", "Tomb Puzzle Solved"));
        }
        public void OnOpenTorchChamberShortcut() {
            LocationFound.Invoke(this, new LocationEventArgs($"Open the Torch Chamber Shortcut", "Torch Chamber Shortcut Opened"));
        }
        public void OnOpenDepositBox(string boxNumber) {
            LocationFound.Invoke(this, new LocationEventArgs($"Open Deposit Box {boxNumber.ToTitleCase()}", $"Deposit Box {boxNumber.ToTitleCase()} Opened"));
        }
        public void OnOpenReservoirDoor() {
            LocationFound.Invoke(this, new LocationEventArgs("Open Basement to Reservoir Door", "Reservoir Door Opened"));
        }
        public void OnLowerFoundationElevator() {
            LocationFound.Invoke(this, new LocationEventArgs("Lower The Foundation Elevator", "Foundation Elevator Lowered"));
        }
        public void OnVaseBroken(string vaseName) {
            LocationFound.Invoke(this, new LocationEventArgs($"{vaseName.ToTitleCase()} Vase", "Vase Broken"));
        }
        public void OnCursedCoffersOpened() {
            LocationFound.Invoke(this, new LocationEventArgs("Cursed Coffers", "Cursed Coffers Opened"));
        }
        public void OnGasValveTurned(string valveName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Gasline Valve - {valveName.ToTitleCase()}", "Gas Valve Turned"));
        }
        public void OnSundialScorched() {
            LocationFound.Invoke(this, new LocationEventArgs("Scorch Sundial", "Sundial Scorched"));
        }
        public void OnVACControlsSolved() {
            LocationFound.Invoke(this, new LocationEventArgs("VAC Controls", "VAC Controls Solved"));
        }
        public void OnAllowanceCollected(string locationName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Allowance Token - {locationName.ToTitleCase()}", "Allowance Collected"));
        }
        public void OnCoffersDugUp(string roomName) {
            LocationFound.Invoke(this, new LocationEventArgs($"Dig up The {roomName.ToTitleCase()} Treasure Chest", "Treasure Dug Up"));
        }
    }

}
