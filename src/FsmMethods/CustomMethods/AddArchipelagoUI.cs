using BluePrinceArchipelago.Tracker;

namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    /// <summary>
    ///     Redirects to the in-game AP tracker UI.
    /// </summary>
    public class AddArchipelagoUI() : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = "AddArchipelagoUI";

        public override void OnCalled()
        {
            APTrackerPresenter.ShowArchipelagoRoomInfo();
        }
    }
}
