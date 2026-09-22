using System;
using System.Collections.Generic;
using HarmonyLib;
#if Bep
using HutongGames.PlayMaker;
#endif
#if ML
using Il2CppHutongGames.PlayMaker;
#endif
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
            "ARCHIVES" => new Archives(),
            "BOOKSHOP" => new Bookshop(),
            "CLOISTER" => new Cloister(),
            "CLOSED EXHIBIT" => new ClosedExhibit(),
            "COMMISSARY" => new Commissary(),
            "DRAFTING STUDIO" => new DraftingStudio(),
            "ENTRANCE HALL" => new EntranceHall(),
            "FREEZER" => new Freezer(),
            "GARAGE" => new Garage(),
            "GIFT SHOP" => new GiftShop(),
            "GREAT HALL" => new GreatHall(),
            "HER LADYSHIP\'S CHAMBER" => new HLC(),
            "LABORATORY" => new Laboratory(),
            "LOCKSMITH" => new Locksmith(),
            "LOST & FOUND" => new LostAndFound(),
            "MASTER BEDROOM" => new MasterBedroom(),
            "MECHANARIUM" => new Mechanarium(),
            "MORNING ROOM" => new MorningRoom(),
            "OFFICE" => new Office(),
            "SHOWROOM" => new Showroom(),
            "SOLARIUM" => new Solarium(),
            "THE ARMORY" => new Armory(),
            "THE FOUNDATION" => new Foundation(),
            "THRONE ROOM" => new ThroneRoom(),
            "TOMB" => new Tomb(),
            "TRADING POST" => new TradingPost(),
            "TUNNEL" => new Tunnel(),
            "UTILITY CLOSET" => new UtilityCloset(),
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