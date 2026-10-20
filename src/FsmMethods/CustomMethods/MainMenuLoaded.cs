using BluePrinceArchipelago.UI;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    public class MainMenuLoaded() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "MainMenuLoaded";

        public override void OnCalled()
        {
            UIManager.ChangeMenuLayout();
        }
    }
}