using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Patio : RoomHandler {
    public override void OnAfterRoomDrafted(GameObject roomGameObject) {
        roomGameObject = ModRoomManager.GetRoomInstance("Patio");
        
    }
}