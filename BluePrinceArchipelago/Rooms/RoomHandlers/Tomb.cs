namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Tomb : RoomHandler
{
    public Tomb()
    {
        AllowanceTokens.Add("Tomb");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Tomb");
    }
}