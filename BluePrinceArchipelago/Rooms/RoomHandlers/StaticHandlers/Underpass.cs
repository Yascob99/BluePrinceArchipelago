namespace BluePrinceArchipelago.Rooms.RoomHandlers;

public class Underpass : RoomHandler
{
    public Underpass()
    {
        AllowanceTokens.Add("Underpass"); // TODO: Find the path of this token
    }

    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved("Underpass");
    }
}