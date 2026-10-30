using BluePrinceArchipelago;
using BluePrinceArchipelago.FsmMethods.CustomMethods;
using BluePrinceArchipelago.Rooms;
using BluePrinceArchipelago.Triggers;
using BluePrinceArchipelago.Utils;

public class ItemCodeComplete(ModRoom room) : RegisteredCustomFsmMethod
{
    public new string Name { get; set; } = room.Name.ToTitleCase() + "ItemCodeComplete";

    public ModRoom Room { get; set; } = room ?? null;
    public override void OnCalled()
    {
        if (Room != null)
        {
            RoomTriggers.OnItemCodeComplete(room);
        }
        else
        {
            Logging.Log($"The {Name} custom method couldn't be run due to invalid room: {Room.Name}", "CustomFsmMethods");
        }
    }
}