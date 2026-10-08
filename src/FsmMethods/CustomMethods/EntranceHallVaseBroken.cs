using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods {
    public class EntranceHallVaseBroken (string direction) : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = $"EntranceHallVaseBroken{direction}";

        public string Direction = direction;

        public override void OnCalled()
        {
            EventTriggers.OnEntranceHallVaseBroken($"Entrance Hall {Direction}");
        }

    }
}