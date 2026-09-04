namespace BluePrinceArchipelago.Models
{
    /// <summary>
    ///     A model of the data required for an archipelago connection.
    /// </summary>
    public class ConnectionData
    {
        public string Uri { get; set; }
        public string SlotName { get; set; }
        public string Password { get; set; }
    }
}
