namespace BluePrinceArchipelago.FsmMethods.CustomMethods
{
    public class FileCabinetKeyUsed(string location) : RegisteredCustomFsmMethod
    {
        public new string Name { get; set; } = $"FileCabinetKeyUsed{location}";

        public override void OnCalled()
        {
            //Not implemented yet.
        }
    }
}