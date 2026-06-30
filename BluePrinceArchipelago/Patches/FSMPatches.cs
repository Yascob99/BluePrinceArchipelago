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

        public static void UpgradeDiskOverride(PlayMakerFSM GlobalFSM) {
            // Disable the Global Manager FSM states to not give this item in inventory
            FsmState state = Plugin.UniqueItemManager.GetPickupState("Upgrade Disk");
            if (state != null)
            {
                if (GlobalFSM.GetState("State 35").Actions.Length < 3)
                {
                    PlayMakerFSM UsedFSM = GameObject.Find("__SYSTEM/Upgrade Disks").GetComponent<PlayMakerFSM>();
                    // Create a boolean for tracking the current state of the 
                    FsmState ArchiveState = GlobalFSM.GetState("State 35");
                    GlobalFSM.AddGlobalTransition("Archives Upgrade Disk Pickup", "State 35");
                    FsmState TradingPostState = GlobalFSM.GetState("State 27");
                    GlobalFSM.AddGlobalTransition("Trading Post Dynamite Upgrade Disk Pickup", "State 27");
                    FsmState TombState = GlobalFSM.GetState("State 23");
                    GlobalFSM.AddGlobalTransition("Tomb Upgrade Disk Pickup", "State 22");
                    FsmState CommissaryState = GlobalFSM.GetState("State 33");
                    GlobalFSM.AddGlobalTransition("Commissary Upgrade Disk Pickup", "State 33");
                    FsmState FoundationState = GlobalFSM.GetState("State 22");
                    GlobalFSM.AddGlobalTransition("Foundation Disk Pickup", "State 22");
                    FsmState FreezerState = GlobalFSM.GetState("State 25");
                    GlobalFSM.AddGlobalTransition("Freezer Upgrade Disk Pickup", "State 22");
                    FsmState GarageState = GlobalFSM.GetState("State 30");
                    GlobalFSM.AddGlobalTransition("Garage Upgrade Disk Pickup", "State 30");
                    FsmState GreatHallState = GlobalFSM.GetState("State 29");
                    GlobalFSM.AddGlobalTransition("Great Hall Upgrade Disk Pickup", "State 29");
                    FsmState LostAndFoundState = GlobalFSM.GetState("State 28");
                    GlobalFSM.AddGlobalTransition("Lost and Found Upgrade Disk Pickup", "State 28");
                    FsmState HLCState = GlobalFSM.GetState("State 20");
                    GlobalFSM.AddGlobalTransition("Her Ladyships Chamber Upgrade Disk Pickup", "State 20");
                    FsmState MechanariumState = GlobalFSM.GetState("State 24");
                    GlobalFSM.AddGlobalTransition("Mechanarium Upgrade Disk Pickup", "State 24");
                    FsmState MorningRoomState = GlobalFSM.GetState("State 21");
                    GlobalFSM.AddGlobalTransition("Morning Room Upgrade Disk Pickup", "State 21");
                    FsmState OfficeState = GlobalFSM.GetState("State 34");
                    GlobalFSM.AddGlobalTransition("Office Upgrade Disk Pickup", "State 34");
                    FsmState VaultState = GlobalFSM.GetState("State 26");
                    GlobalFSM.AddGlobalTransition("Vault Disk Pickup", "State 26");
                    FsmState AbandonedMineState = GlobalFSM.GetState("State 31");
                    GlobalFSM.AddGlobalTransition("Abandoned Mine Pickup", "State 31");

                    FsmState roomCheck = GlobalFSM.GetState("State 19");
                    StringContains[] checks = roomCheck.GetActionsOfType<StringContains>();

                    checks[0].containsString = "adyship"; // Fix HLC check
                    checks[3].containsString = "omb"; // Fix Tomb Check
                    state.DisableActionsOfType<SendEvent>(); //Prevents the Game from Getting Softlocked by a Freeze.
                    FsmStateAction freeze = state.GetFirstActionOfType<SendEvent>();

                    //Unfreeze 
                    SendEventByName unfreeze = new SendEventByName()
                    {
                        eventTarget = new FsmEventTarget()
                        {
                            target = FsmEventTarget.EventTarget.GameObject,
                            gameObject = new FsmOwnerDefault()
                            {
                                gameObject = GameObject.Find("__SYSTEM/FPS Home/FPSController - Prince"),
                                ownerOption = OwnerDefaultOption.SpecifyGameObject
                            },
                            fsmName = "FSM",
                            sendToChildren = false,
                            excludeSelf = false
                        },
                        sendEvent = "UnFreeze",
                        delay = 0f,
                        everyFrame = false
                    };

                    // Add the Archive pickup Check
                    ArchiveState.InsertAction(1, freeze);
                    ArchiveState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[0], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    ArchiveState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Trading Post Pickup Check
                    TradingPostState.InsertAction(1, freeze);
                    TradingPostState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[1], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    TradingPostState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Tomb Pickup Check
                    TombState.InsertAction(1, freeze);
                    TombState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[2], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    TombState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Commissary Buy Check
                    CommissaryState.InsertAction(1, freeze);
                    CommissaryState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[3], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    CommissaryState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Foundation Pickup Check
                    FoundationState.InsertAction(1, freeze);
                    FoundationState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[4], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    FoundationState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Freezer Pickup Check
                    FreezerState.InsertAction(1, freeze);
                    FreezerState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[5], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    FreezerState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Garage Pickup Check
                    GarageState.InsertAction(1, freeze);
                    GarageState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[6], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    GarageState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Great Hall Pickup Check
                    GreatHallState.InsertAction(1, freeze);
                    GreatHallState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[7], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    GreatHallState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Lost and Found Pickup Check
                    LostAndFoundState.InsertAction(1, freeze);
                    LostAndFoundState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[8], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    LostAndFoundState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Her Ladyship's Chamber Pickup Check
                    HLCState.InsertAction(1, freeze);
                    HLCState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[9], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    HLCState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Mechanarium Pickup Check
                    MechanariumState.InsertAction(1, freeze);
                    MechanariumState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[10], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    MechanariumState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Morning Room Pickup Check
                    MorningRoomState.InsertAction(1, freeze);
                    MorningRoomState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[11], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    MorningRoomState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Office Pickup Check
                    OfficeState.InsertAction(1, freeze);
                    OfficeState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[12], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    OfficeState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Vault Pickup Check
                    VaultState.InsertAction(1, freeze);
                    VaultState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[14], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    VaultState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Add the Abandoned Mine Pickup Check
                    AbandonedMineState.InsertAction(1, freeze);
                    AbandonedMineState.InsertAction(2, new ActivateGameObject() { gameObject = new FsmOwnerDefault() { gameObject = UpgradeDisks.YouFoundObjects[15], ownerOption = OwnerDefaultOption.SpecifyGameObject }, activate = true, recursive = false, resetOnExit = false, everyFrame = false });
                    AbandonedMineState.InsertAction(3, unfreeze); // Backup in case the YouFound doesn't work.

                    // Remove the original Pickup actions
                    state.DisableActionsOfType<ArrayListAdd>();
                    state.DisableActionsOfType<ActivateGameObject>();
                    state.DisableActionsOfType<SendEvent>();
                    // Fix the "Finished Transition"
                    state.ChangeTransition("FINISHED", "State 19"); //Fix transitions
                    Logging.Log("Upgrade Disk Override Applied.");

                    //Commissary Replacement Code
                    PlayMakerFSM CommissaryMenu = GameObject.Find("UI OVERLAY CAM").transform.Find("Commissary Menu")?.GetComponent<PlayMakerFSM>();
                    // Prevent the Default add to inventory behavior.
                    FsmState UpgradeDisksAdd = CommissaryMenu.GetState("Upgrade Disks Add");
                    UpgradeDisksAdd.DisableFirstActionOfType<SetFsmInt>();
                    UpgradeDisksAdd.DisableFirstActionOfType<SendEvent>();

                    FsmState UpgradeDiskPurchaseState = CommissaryMenu.GetState("Upgrade Disk Purchase 2");
                    UpgradeDiskPurchaseState.DisableActionsOfType<ArrayListAdd>();
                    // Attempt to create a SendEvent to send info the 
                    // There's a solid chance this just breaks.
                    UpgradeDiskPurchaseState.DisableFirstActionOfType<ActivateGameObject>();
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

            //The Kennel
            PlayMakerFSM TheKennelDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/THE KENNEL/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState TheKennelAddState = TheKennelDraftButton.GetState("Add this Floorplan to your DRAFT POOL");

            //Vestibule
            PlayMakerFSM VestibuleDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/VESTIBULE/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState VestibuleAddState = VestibuleDraftButton.GetState("Add this Floorplan to your DRAFT POOL");

            //Dovecote
            PlayMakerFSM DovecoteDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/DOVECOTE/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState DovecoteAddState = DovecoteDraftButton.GetState("Add this Floorplan to your DRAFT POOL");

            //Solarium
            PlayMakerFSM SolariumDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/SOLARIUM/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState SolariumAddState = SolariumDraftButton.GetState("Add this Floorplan to your DRAFT POOL");

            //Dormitory
            PlayMakerFSM DormitoryDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/DORMITORY/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState DormitoryAddState = DormitoryDraftButton.GetState("Add this Floorplan to your DRAFT POOL");

            //Casino
            PlayMakerFSM CasinoDraftButton = GameObject.Find("UI OVERLAY CAM/Drafting Studio UI/CASINO/DRAFT BUTTON").GetComponent<PlayMakerFSM>();
            FsmState CasinoAddState = CasinoDraftButton.GetState("Add this Floorplan to your DRAFT POOL");


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
