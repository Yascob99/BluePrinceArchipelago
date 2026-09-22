using BluePrinceArchipelago.Items;
using BluePrinceArchipelago.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BluePrinceArchipelago.Rooms.RoomHandlers
{
    public class DraftingStudio : RoomHandler {

        public DraftingStudio()
        {
            Logging.Log("Initializing Drafting Studio.");
        }

        public override void OnRoomDrafted(GameObject roomGameObject)
        {
            ModInstance.GlobalManager.GetBoolVariable("Clock Tower Unlocked").Value = ModRoomManager.GetRoomByName("CLOCK TOWER").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("The Kennel Unlocked").Value = ModRoomManager.GetRoomByName("THE KENNEL").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Vestibule Unlocked").Value = ModRoomManager.GetRoomByName("VESTIBULE").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Dovecote Unlocked").Value = ModRoomManager.GetRoomByName("DOVECOTE").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Solarium Unlocked").Value = ModRoomManager.GetRoomByName("SOLARIUM").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Dormitory Unlocked").Value = ModRoomManager.GetRoomByName("DORMITORY").IsUnlocked;
            ModInstance.GlobalManager.GetBoolVariable("Casino Unlocked").Value = ModRoomManager.GetRoomByName("CASINO").IsUnlocked;

        }
    }
}
