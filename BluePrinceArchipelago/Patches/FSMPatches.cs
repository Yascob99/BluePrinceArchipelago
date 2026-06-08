using BluePrinceArchipelago.Events;
using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace BluePrinceArchipelago.Patches
{
    public static class FSMPatches
    {
        //This additionally prevents the Day 1 Draft 1 forced draft.
        public static void RoomForcer(PlayMakerFSM fsm)
        {
            FsmBool isDraftForced = fsm.AddFsmBool("ForceDraft", false);
            FsmState ForceDraft = fsm.AddState("Force Room Draft");
            FsmState DraftForcedCheck = fsm.AddState("Draft Forced Check");
            FsmGameObject ForcedRoom = fsm.AddFsmGameObject("ForcedRoom", null);
            DraftForcedCheck.RemoveTransitionsTo("FINISHED");
            DraftForcedCheck.AddTransition("Continue Draft", "SLOT 2");
            DraftForcedCheck.AddTransition("Force Draft", "Force Room Draft");
            DraftForcedCheck.AddLastAction(new BoolTest() { boolVariable = isDraftForced, isFalse = FsmEvent.GetFsmEvent("Continue Draft"), isTrue = FsmEvent.GetFsmEvent("Force Draft"), everyFrame = false });
            ForceDraft.AddLastAction(new SetGameObject() { everyFrame = false, gameObject = fsm.GetGameObjectVariable("RoomEngine"), variable = ForcedRoom });
            SendEvent PlanSelected = fsm.GetState("Slot 1").GetFirstActionOfType<SendEvent>();
            ForceDraft.AddLastAction(PlanSelected);
            ForceDraft.RemoveTransitionsTo("FINISHED");
            FsmState DraftCodeStart = fsm.GetState("Draft Code Start");
            DraftCodeStart.ChangeTransition("FINISHED", "Draft Forced Check");
            FsmState PickAnother = fsm.GetState("Pick Another ");
            PickAnother.ChangeTransition("FINISHED", "Draft Forced Check");
            FsmState OuterDraftState = fsm.GetState("Outer slot pick");
            // Add Outer Draft Trigger.
            OuterDraftState.InsertAction(3, FSMEventHandler.RegisteredEvents["Outer Draft Start"].Event);
        }
        // Patch this so functions can be called when the player is not frozen anymore.

        public static void UpgradeDiskOverride(PlayMakerFSM GlobalFSM) {
            // Disable the Global Manager FSM states to not give this item in inventory
            FsmState state = Plugin.UniqueItemManager.GetPickupState("Upgrade Disk");
            if (state != null)
            {
                if (GlobalFSM.GetState("State 35").Actions.Length < 3)
                {
                    // Create a boolean for tracking the current state of the 
                    FsmBool ArchivesDisk = GlobalFSM.AddFsmBool("Archives Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("ARCHIVES"));
                    FsmState ArchiveState = GlobalFSM.GetState("State 35");
                    GlobalFSM.AddGlobalTransition("Archives Upgrade Disk Pickup", "State 35");
                    FsmBool TradingPostDisk = GlobalFSM.AddFsmBool("Trading Post Dynamite Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("TRADING POST DYNAMITE"));
                    FsmState TradingPostState = GlobalFSM.GetState("State 27");
                    GlobalFSM.AddGlobalTransition("Trading Post Dynamite Upgrade Disk Pickup", "State 27");
                    FsmBool TombDisk = GlobalFSM.AddFsmBool("Tomb Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("TOMB"));
                    FsmState TombState = GlobalFSM.GetState("State 23");
                    GlobalFSM.AddGlobalTransition("Tomb Upgrade Disk Pickup", "State 22");
                    FsmBool CommissaryDisk = GlobalFSM.AddFsmBool("Commissary Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("COMMISSARY"));
                    FsmState CommissaryState = GlobalFSM.GetState("State 33");
                    GlobalFSM.AddGlobalTransition("Commissary Upgrade Disk Pickup", "State 33");
                    FsmBool FoundationDisk = GlobalFSM.AddFsmBool("Foundation Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("FOUNDATION"));
                    FsmState FoundationState = GlobalFSM.GetState("State 22");
                    GlobalFSM.AddGlobalTransition("Foundation Disk Pickup", "State 22");
                    FsmBool FreezerDisk = GlobalFSM.AddFsmBool("Freezer Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("FREEZER"));
                    FsmState FreezerState = GlobalFSM.GetState("State 25");
                    GlobalFSM.AddGlobalTransition("Freezer Upgrade Disk Pickup", "State 22");
                    FsmBool GarageDisk = GlobalFSM.AddFsmBool("Garage Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("GARAGE"));
                    FsmState GarageState = GlobalFSM.GetState("State 30");
                    GlobalFSM.AddGlobalTransition("Garage Upgrade Disk Pickup", "State 30");
                    FsmBool GreatHallDisk = GlobalFSM.AddFsmBool("Great Hall Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("GREAT HALL"));
                    FsmState GreatHallState = GlobalFSM.GetState("State 29");
                    GlobalFSM.AddGlobalTransition("Great Hall Upgrade Disk Pickup", "State 29");
                    FsmBool LostAndFoundDisk = GlobalFSM.AddFsmBool("Lost and Found Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("LOST AND FOUND"));
                    FsmState LostAndFoundState = GlobalFSM.GetState("State 28");
                    GlobalFSM.AddGlobalTransition("Lost and Found Upgrade Disk Pickup", "State 28");
                    FsmBool HLCDisk = GlobalFSM.AddFsmBool("Her Ladyships Chamber Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("HER LADYSHIPS CHAMBER"));
                    FsmState HLCState = GlobalFSM.GetState("State 20");
                    GlobalFSM.AddGlobalTransition("Her Ladyships Chamber Upgrade Disk Pickup", "State 20");
                    FsmBool MechanariumDisk = GlobalFSM.AddFsmBool("Mechanarium Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("MECHANARIUM"));
                    FsmState MechanariumState = GlobalFSM.GetState("State 24");
                    GlobalFSM.AddGlobalTransition("Mechanarium Upgrade Disk Pickup", "State 24");
                    FsmBool MorningRoomDisk = GlobalFSM.AddFsmBool("Morning Room Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("MORNING ROOM"));
                    FsmState MorningRoomState = GlobalFSM.GetState("State 21");
                    GlobalFSM.AddGlobalTransition("Morning Room Upgrade Disk Pickup", "State 21");
                    FsmBool OfficeDisk = GlobalFSM.AddFsmBool("Office Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("OFFICE"));
                    FsmState OfficeState = GlobalFSM.GetState("State 34");
                    GlobalFSM.AddGlobalTransition("Office Upgrade Disk Pickup", "State 34");
                    FsmBool VaultDisk = GlobalFSM.AddFsmBool("Vault Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("VAULT"));
                    FsmState VaultState = GlobalFSM.GetState("State 26");
                    GlobalFSM.AddGlobalTransition("Vault Disk Pickup", "State 26");
                    FsmBool AbandonedMineDisk = GlobalFSM.AddFsmBool("Abandoned Mine Disk", ModItemManager.UpgradeDisks.RecievedItems.Contains("ABANDONNED MINE"));
                    FsmState AbandonedMineState = GlobalFSM.GetState("State 31");
                    GlobalFSM.AddGlobalTransition("Abandoned Mine Pickup", "State 31");

                    FsmState roomCheck = GlobalFSM.GetState("State 19");
                    StringContains[] checks = roomCheck.GetActionsOfType<StringContains>();

                    FsmStateAction[] addActions = state.GetActionsOfType<ArrayListAdd>();
                    checks[0].containsString = "adyship"; // Fix HLC check
                    checks[3].containsString = "omb"; // Fix Tomb Check
                    FsmStateAction activateAction = state.GetFirstActionOfType<ActivateGameObject>();

                    // Add the Archive pickup Check
                    ArchiveState.InsertAction(1, new BoolTest() { boolVariable = ArchivesDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    ArchiveState.InsertAction(2, activateAction);
                    ArchiveState.InsertAction(3, addActions[0]);
                    ArchiveState.InsertAction(4, addActions[1]);

                    // Add the Trading Post Pickup Check
                    TradingPostState.InsertAction(1, new BoolTest() { boolVariable = TradingPostDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    TradingPostState.InsertAction(2, activateAction);
                    TradingPostState.InsertAction(3, addActions[0]);
                    TradingPostState.InsertAction(4, addActions[1]);

                    // Add the Tomb Pickup Check
                    TombState.InsertAction(1, new BoolTest() { boolVariable = TombDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    TombState.InsertAction(2, activateAction);
                    TombState.InsertAction(3, addActions[0]);
                    TombState.InsertAction(4, addActions[1]);

                    // Add the Commissary Buy Check
                    CommissaryState.InsertAction(1, new BoolTest() { boolVariable = CommissaryDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    CommissaryState.InsertAction(2, activateAction);
                    CommissaryState.InsertAction(3, addActions[0]);
                    CommissaryState.InsertAction(4, addActions[1]);

                    // Add the Foundation Pickup Check
                    FoundationState.InsertAction(1, new BoolTest() { boolVariable = FoundationDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    FoundationState.InsertAction(2, activateAction);
                    FoundationState.InsertAction(3, addActions[0]);
                    FoundationState.InsertAction(4, addActions[1]);

                    // Add the Freezer Pickup Check
                    FreezerState.InsertAction(1, new BoolTest() { boolVariable = FreezerDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    FreezerState.InsertAction(2, activateAction);
                    FreezerState.InsertAction(3, addActions[0]);
                    FreezerState.InsertAction(4, addActions[1]);

                    // Add the Garage Pickup Check
                    GarageState.InsertAction(1, new BoolTest() { boolVariable = GarageDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    GarageState.InsertAction(2, activateAction);
                    GarageState.InsertAction(3, addActions[0]);
                    GarageState.InsertAction(4, addActions[1]);

                    // Add the Great Hall Pickup Check
                    GreatHallState.InsertAction(1, new BoolTest() { boolVariable = GreatHallDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    GreatHallState.InsertAction(2, activateAction);
                    GreatHallState.InsertAction(3, addActions[0]);
                    GreatHallState.InsertAction(4, addActions[1]);

                    // Add the Lost and Found Pickup Check
                    LostAndFoundState.InsertAction(1, new BoolTest() { boolVariable = LostAndFoundDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    LostAndFoundState.InsertAction(2, activateAction);
                    LostAndFoundState.InsertAction(3, addActions[0]);
                    LostAndFoundState.InsertAction(4, addActions[1]);

                    // Add the Her Ladyship's Chamber Pickup Check
                    HLCState.InsertAction(1, new BoolTest() { boolVariable = HLCDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    HLCState.InsertAction(2, activateAction);
                    HLCState.InsertAction(3, addActions[0]);
                    HLCState.InsertAction(4, addActions[1]);

                    // Add the Mechanarium Pickup Check
                    MechanariumState.InsertAction(1, new BoolTest() { boolVariable = MechanariumDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    MechanariumState.InsertAction(2, activateAction);
                    MechanariumState.InsertAction(3, addActions[0]);
                    MechanariumState.InsertAction(4, addActions[1]);

                    // Add the Morning Room Pickup Check
                    MorningRoomState.InsertAction(1, new BoolTest() { boolVariable = MorningRoomDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    MorningRoomState.InsertAction(2, activateAction);
                    MorningRoomState.InsertAction(3, addActions[0]);
                    MorningRoomState.InsertAction(4, addActions[1]);

                    // Add the Office Pickup Check
                    OfficeState.InsertAction(1, new BoolTest() { boolVariable = OfficeDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    OfficeState.InsertAction(2, activateAction);
                    OfficeState.InsertAction(3, addActions[0]);
                    OfficeState.InsertAction(4, addActions[1]);

                    // Add the Vault Pickup Check
                    VaultState.InsertAction(1, new BoolTest() { boolVariable = VaultDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    VaultState.InsertAction(2, activateAction);
                    VaultState.InsertAction(3, addActions[0]);
                    VaultState.InsertAction(4, addActions[1]);

                    // Add the Abandoned Mine Pickup Check
                    AbandonedMineState.InsertAction(1, new BoolTest() { boolVariable = AbandonedMineDisk, isFalse = FsmEvent.GetFsmEvent("Event 0"), everyFrame = false });
                    AbandonedMineState.InsertAction(2, activateAction);
                    AbandonedMineState.InsertAction(3, addActions[0]);
                    AbandonedMineState.InsertAction(4, addActions[1]);

                    // Remove the original Pickup actions
                    state.DisableActionsOfType<ArrayListAdd>();
                    state.DisableActionsOfType<ActivateGameObject>();
                    // Fix the "Finished Transition"
                    state.ChangeTransition("FINISHED", "State 19"); //Fix transitions
                    Logging.Log("Upgrade Disk Override Applied.");

                    //Commissary Replacement Code
                    PlayMakerFSM CommissaryMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Commissary Menu")?.GetComponent<PlayMakerFSM>();
                    // Prevent the Default add to inventory behavior.
                    FsmState UpgradeDiskPurchaseState = CommissaryMenu.GetState("Upgrade Disk Purchase 2");
                    UpgradeDiskPurchaseState.DisableActionsOfType<ArrayListAdd>();
                    // Attempt to create a SendEvent to send info the 
                    // There's a solid chance this just breaks.
                    UpgradeDiskPurchaseState.InsertAction(3, new SendEventByName()
                    {
                        eventTarget = new FsmEventTarget()
                        {
                            target = FsmEventTarget.EventTarget.GameObject,
                            gameObject = new FsmOwnerDefault()
                            {
                                gameObject = GameObject.Find("Global Manager"),
                                ownerOption = OwnerDefaultOption.SpecifyGameObject
                            },
                            fsmName = "FSM",
                            sendToChildren = false,
                            excludeSelf = false
                        },
                        sendEvent = "Upgrade Disk Pickup",
                        delay = 0f,
                        everyFrame = false
                    });
                }
                else {
                    Logging.Log("Upgrade Disk Override already applied.");
                }
            }
        }
        public static void IntroSkip() {
            // Menu Logo Skips
            var menuSystem = GameObject.Find("/Menu System");
            var fsm = menuSystem.GetComponent<PlayMakerFSM>();

            // Replace the transition to go to State 8 rather than Logo Slates
            fsm.Fsm.GetState("EnterMainMenu").GetTransition(0).ToFsmState = fsm.Fsm.GetState("State 8");
            fsm.Fsm.GetState("EnterMainMenu").GetTransition(1).ToFsmState = fsm.Fsm.GetState("State 8");

            // Because we skip Logo Slates we have to copy the music start action here.
            // This just replaces a fade to black that would've been removed anyway
            fsm.Fsm.GetState("State 8").actions[0] = fsm.Fsm.GetState("Logo Slates").actions[1];

            // Remove the 3 second delay
            var wait = fsm.Fsm.GetState("State 8").actions[2].Cast<Wait>();
            wait.time = new FsmFloat(0f);
        }

        public static void AddedFloorPlanOverrides()
        {
            //Planetarium
            PlayMakerFSM PlanetariumYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Planetarium Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState PlanetariumAddState = PlanetariumYesButton.GetState("State 8");
            PlanetariumAddState.DisableFirstActionOfType<SendEvent>();
            // Get the second Send Event so it can be unfrozen.
            SendEvent[] SendEvents = PlanetariumAddState.GetActionsOfType<SendEvent>();
            SendEvent Unfreeze = SendEvents[1];

            //Conservatory
            PlayMakerFSM ConservatoryYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Conservatory Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState ConservatoryAddState = ConservatoryYesButton.GetState("State 8");
            ConservatoryAddState.DisableFirstActionOfType<SendEvent>();

            //Tunnel
            PlayMakerFSM TunnelYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Tunnel Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState TunnelAddState = TunnelYesButton.GetState("State 8");
            TunnelAddState.DisableActionsOfType<SetFsmBool>();

            //Throne Room
            PlayMakerFSM ThroneRoomYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Throne Room Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState ThroneRoomAddState = ThroneRoomYesButton.GetState("Tomorrow");
            ThroneRoomAddState.DisableFirstActionOfType<ActivateGameObject>();
            ThroneRoomAddState.GetFirstActionOfType<SetFsmBool>().variableName = "Throne Room Added";
            // Throne Room normally calls a popup that unfreezes the player movement, however we don't want that to display so we deactivate it
            ThroneRoomAddState.AddAction(Unfreeze);

            //Treasure Trove
            PlayMakerFSM TreasureTroveYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Treasure Trove Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState TreasureTroveAddState = TreasureTroveYesButton.GetState("State 8");
            TreasureTroveAddState.DisableFirstActionOfType<SendEvent>();

            //Mechanarium
            PlayMakerFSM MechanariumYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/MECHANARIUM Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState MechanariumAddState = MechanariumYesButton.GetState("State 8");
            MechanariumAddState.DisableFirstActionOfType<SendEvent>();

            //Lost & Found
            PlayMakerFSM LostandFoundYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/MINI MENUS/Lost&Found Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState LostandFoundAddState = LostandFoundYesButton.GetState("State 8");
            LostandFoundAddState.DisableFirstActionOfType<SendEvent>();


            //Closed Exhibit
            PlayMakerFSM ClosedExhibitYesButton = GameObject.Find("UI OVERLAY CAM/UI Documents/DOCUMENTS/RED LETTER STUDY - doc/Page 3/Closed Exhibit Find - menu/2 Button Spread (2)/YES BUTTON").GetComponent<PlayMakerFSM>();
            FsmState ClosedExhibitAddState = ClosedExhibitYesButton.GetState("State 8");
            ClosedExhibitAddState.DisableFirstActionOfType<SendEvent>();

            //Drafting Studio Adds

            //Clock Tower
            PlayMakerFSM ClockTowerDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/CLOCK TOWER/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState ClockTowerAddState = ClockTowerDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            ClockTowerAddState.DisableFirstActionOfType<SendEvent>();

            //The Kennel
            PlayMakerFSM TheKennelDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/THE KENNEL/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState TheKennelAddState = TheKennelDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            TheKennelAddState.DisableFirstActionOfType<SendEvent>();

            //Vestibule
            PlayMakerFSM VestibuleDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/VESTIBULE/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState VestibuleAddState = VestibuleDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            VestibuleAddState.DisableFirstActionOfType<SendEvent>();

            //Dovecote
            PlayMakerFSM DovecoteDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/DOVECOTE/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState DovecoteAddState = DovecoteDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            DovecoteAddState.DisableFirstActionOfType<SendEvent>();

            //Solarium
            PlayMakerFSM SolariumDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/SOLARIUM/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState SolariumAddState = SolariumDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            SolariumAddState.DisableFirstActionOfType<SendEvent>();

            //Dormitory
            PlayMakerFSM DormitoryDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/DORMITORY/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState DormitoryAddState = DormitoryDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            DormitoryAddState.DisableFirstActionOfType<SendEvent>();

            //Casino
            PlayMakerFSM CasinoDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/CASINO/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState CasinoAddState = CasinoDraftButton.GetState("Add this Floorplan to your DRAFT POOL");
            CasinoAddState.DisableFirstActionOfType<SendEvent>();


            //Plan Picker
            PlayMakerFSM PlanPicker = ModInstance.PlanPicker.GetComponent<PlayMakerFSM>();
            PlanPicker.GetState("Dormitory Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Clock Tower Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Dovecote Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Solarium Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("The Kennel Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Vestibule Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Planetarium").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Tunnel").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Treasure Trove Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Lost & Found Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Conservatory Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Closed Exhibit Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Mechanarium Add").DisableActionsOfType<ArrayListAdd>();
            PlanPicker.GetState("Casino Add").DisableActionsOfType<ArrayListAdd>();
        }

    }
}
