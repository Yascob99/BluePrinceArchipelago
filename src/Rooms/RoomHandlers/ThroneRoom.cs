namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class ThroneRoom : RoomHandler
{
    public ThroneRoom()
    {
        AllowanceTokens.Add("Throne Room");
    }
    
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Throne of the Blue Prince");
    }
}