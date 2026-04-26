using UnityEngine;

namespace BluePrinceArchipelago.RoomHandlers;

public abstract class RoomHandler
{
    protected GameObject RoomGameObject { get; set; }

    public abstract void OnRoomDrafted(GameObject roomGameObject);
    public static RoomHandler CreateRoomHandler(string roomName)
    {
        return roomName switch 
        {
            "COMMISSARY" => new Commissary(),
            "SHOWROOM" => new Showroom(),
            "THE ARMORY" => new Armory(),
            _ => null
        };
    }
}