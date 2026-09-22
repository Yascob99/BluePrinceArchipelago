namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Solarium : RoomHandler
{
    public Solarium()
    {
        AllowanceTokens.Add("Solarium");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Solarium");
    }
}