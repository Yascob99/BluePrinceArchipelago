namespace BluePrinceArchipelago.Models
{
    /// <summary>
    ///     A model for key data about the session.
    /// </summary>
    public class SessionData
    {
        public string Seed { get; set; }

        public int SaveSlot { get; set; }

        public int ItemIndex { get; set; }
    }
}
