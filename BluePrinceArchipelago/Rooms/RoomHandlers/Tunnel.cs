namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Tunnel : RoomHandler
{
    public Tunnel()
    {
        AllowanceTokens.Add("Tunnel");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Tunnel");
    }
}