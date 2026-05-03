using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public abstract class RoomHandler
{
    protected static GameObject UIOverlayCam => GameObject.Find("UI OVERLAY CAM");
    protected GameObject RoomGameObject { get; set; }

    public abstract void OnRoomDrafted(GameObject roomGameObject);
    public virtual void OnAfterRoomDrafted() { }
    public static RoomHandler CreateRoomHandler(string roomName)
    {
        return roomName switch 
        {
            "COMMISSARY" => new Commissary(),
            "SHOWROOM" => new Showroom(),
            "THE ARMORY" => new Armory(),
            "BOOKSHOP" => new Bookshop(),
            "GIFT SHOP" => new GiftShop(),
            "LOCKSMITH" => new Locksmith(),
            "TRADING POST" => new TradingPost(),
            _ => null
        };
    }
}