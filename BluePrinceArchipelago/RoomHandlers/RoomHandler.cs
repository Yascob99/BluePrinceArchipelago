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
            "COMMISSARY" => new Commissary(4, 1, 10, 0), // Placeholder values, eventually these should be set in the yaml; seed should be day seed
            _ => null
        };
    }
}