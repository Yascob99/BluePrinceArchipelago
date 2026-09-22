namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Cloister : RoomHandler
{
    public Cloister()
    {
        AllowanceTokens.Add("Cloister");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnAllowanceCollected("Cloister Statue");
    }
}