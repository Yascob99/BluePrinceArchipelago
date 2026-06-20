using System;
using System.Collections.Generic;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public abstract class RoomHandler
{
    protected static GameObject UIOverlayCam => GameObject.Find("UI OVERLAY CAM");
    public GameObject RoomGameObject { get; set; }

    public Dictionary<string, HashSet<string>> ObservedFSMStates { get; } = [];
    public HashSet<string> AllowanceTokens { get; } = [];

    public virtual void OnRoomDrafted(GameObject roomGameObject) {}
    public virtual void OnAfterRoomDrafted(GameObject roomGameObject) { }
    public virtual void OnFSMStateChanged(Fsm fsm, string gameObjectName, string newState) { }
    public virtual void OnAllowanceTokenCollected(string token) { }
    public virtual void OnRoomUnlocked(ModRoom room) { 
    }

    public virtual void SetupEventHooks(){}
    
    public static readonly Dictionary<string, RoomHandler> RoomHandlers = new Dictionary<string, RoomHandler>()
    {
        {"BASEMENT", new Basement()},
        {"THE WELL", new Well()},
        {"UNDERPASS", new Underpass()},
        {"TUNNEL AREA", new TunnelArea()},
        {"SANCTUMS", new Sanctums()},
    };

    public static RoomHandler CreateRoomHandler(string roomName)
    {
        if (RoomHandlers.TryGetValue(roomName, out var handler))
        {
            return handler;
        }

        handler = roomName switch
        {
            "COMMISSARY" => new Commissary(),
            "SHOWROOM" => new Showroom(),
            "THE ARMORY" => new Armory(),
            "BOOKSHOP" => new Bookshop(),
            "GIFT SHOP" => new GiftShop(),
            "LOCKSMITH" => new Locksmith(),
            "TRADING POST" => new TradingPost(),
            "DRAFTING STUDIO" => new DraftingStudio(),
            "CLOISTER" => new Cloister(),
            "ENTRANCE HALL" => new EntranceHall(),
            "CLOSED EXHIBIT" => new ClosedExhibit(),
            "TOMB" => new Tomb(),
            "TUNNEL" => new Tunnel(),
            "MASTER BEDROOM" => new MasterBedroom(),
            "SOLARIUM" => new Solarium(),
            "LOST & FOUND" => new LostAndFound(),
            "THRONE ROOM" => new ThroneRoom(),
            "UTILITY CLOSET" => new UtilityCloset(),
            "LABORATORY" => new Laboratory(),
            _ => null
        };

        if (handler != null)
        {
            RoomHandlers[roomName] = handler;
            Logging.Log($"Created RoomHandler for {roomName}.", "RoomHandler");
        }
        return handler;
    }
}

public static class FsmRoomPatches
{
    private static readonly Dictionary<string, string> _LastStates = [];
    private static readonly Dictionary<string, (HashSet<string>, Action<Fsm, string, string>)> _ObservedFSMs = new(){
        {"ZERO STEP ENDING", (["State 3"], OnZeroStepsEnding)},
    };

    [HarmonyPatch(typeof(Fsm), nameof(Fsm.UpdateStateChanges))]
    [HarmonyPostfix]
    public static void Postfix(Fsm __instance)
    {
        try
        {
            if (__instance == null) return;
            var gameObjectName = __instance.GameObjectName;
            
            foreach (var roomHandler in RoomHandler.RoomHandlers.Values)
            {
                if (roomHandler.ObservedFSMStates.ContainsKey(gameObjectName))
                {
                    var lastState = _LastStates.GetValueOrDefault(gameObjectName);
                    var currentState = __instance?.ActiveStateName;
                    if ((lastState == null || lastState != currentState) && roomHandler.ObservedFSMStates[gameObjectName].Contains(currentState))
                    {
                        _LastStates[gameObjectName] = currentState;
                        roomHandler.OnFSMStateChanged(__instance, gameObjectName, currentState);
                    }
                }
            }

            foreach (var (fsmIdentifier, (observedStates, callback)) in _ObservedFSMs)
            {
                if (fsmIdentifier == gameObjectName)
                {
                    var lastState = _LastStates.GetValueOrDefault(gameObjectName);
                    var currentState = __instance?.ActiveStateName;
                    if ((lastState == null || lastState != currentState) && observedStates.Contains(currentState))
                    {
                        _LastStates[gameObjectName] = currentState;
                        callback(__instance, gameObjectName, currentState);
                    }
                }
            }
        } 
        catch (Exception)
        {
            // Logging.Log($"Error in FSM state change postfix: {ex}", "RoomHandler");
        }
    }

    private static void OnZeroStepsEnding(Fsm fsm, string gameObjectName, string newState)
    {
        if (newState == "State 3")
        {
            Logging.Log("Zero Steps Ending reached, sending death link...", "DeathLink");
            Plugin.ArchipelagoClient.DeathLinkHandler.SendStepsDeathLink();
        }
    }

    // TODO: Find a hook that works for Mora Jai Boxes
    // [HarmonyPatch(typeof(MorajaiController), nameof(MorajaiController.CheckCorners))]
    // [HarmonyPostfix]
    // static void MorajaiPostfix(MorajaiController __instance)
    // {
    //     if (__instance == null) return;
    //     var gameObject = __instance.gameObject?.transform?.parent?.gameObject;
    //     if (gameObject == null) return;

    //     var value = __instance.hasSolved;

    //     Logging.Log($"Morajai Puzzle {gameObject.name} solved: {value}");

    //     foreach (var roomHandler in RoomHandler.RoomHandlers.Values)
    //     {
    //         if (roomHandler.MorajaiPuzzles.Contains(gameObject.name) && value)
    //         {
    //             roomHandler.OnMorajaiPuzzleSolved(gameObject.name);
    //         }
    //     }
    // }
}

// Vault 053 FSM = "LOCK CLICK"