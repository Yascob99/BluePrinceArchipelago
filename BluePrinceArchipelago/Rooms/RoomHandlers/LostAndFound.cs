namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class LostAndFound : RoomHandler
{
    public LostAndFound()
    {
        AllowanceTokens.Add("Lost & Found");
    }
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Lost & Found");
    }
}