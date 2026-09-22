using BluePrinceArchipelago.Rooms.RoomHandlers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    public class ShowroomMenuOpened() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "ShowroomMenuOpened";

        public override void OnCalled()
        {
            Showroom.SetupShowroomItems();
        }
    }
}
