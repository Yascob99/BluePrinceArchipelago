using BluePrinceArchipelago.Tracker;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     Redirects to the in-game AP tracker UI.
    /// </summary>
    public class AddTrackerUI() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "AddArchipelagoUI";

        public override void OnCalled()
        {
            APTrackerPresenter.ShowTrackerInfo();
        }
    }
}
