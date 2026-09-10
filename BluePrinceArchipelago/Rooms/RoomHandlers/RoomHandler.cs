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
    public virtual void OnDayStart() {}


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
            "ARCHIVES" => new Archives(),
            "FREEZER" => new Freezer(),
            "GARAGE" => new Garage(),
            "GREAT HALL" => new GreatHall(),
            "HER LADYSHIP\'S CHAMBER" => new HLC(),
            "MECHANARIUM" => new Mechanarium(),
            "MORNING ROOM" => new MorningRoom(),
            "OFFICE" => new Office(),
            "VAULT" => new Vault(),
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

// Vault 053 FSM = "LOCK CLICK"