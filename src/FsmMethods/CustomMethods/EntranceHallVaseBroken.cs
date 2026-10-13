using BluePrinceArchipelago.Triggers;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods {
    public class EntranceHallVaseBroken (string direction, bool outer = false) : RegisteredCustomFsmMethod
    {

        public new string Name { get; set; } = $"EntranceHallVaseBroken{direction}";

        public string Direction = direction;

        public bool isOuter { get; set; } = outer;

        public override void Update()
        {
            //Run outer room checks
        }

        public override void OnCalled()
        {
            if (isOuter) {
                EventTriggers.OnEntranceHallVaseBroken($"Outer Entrance Hall {Direction}");
                isOuter = false;
            }
            EventTriggers.OnEntranceHallVaseBroken($"Entrance Hall {Direction}");
            
        }

    }
}