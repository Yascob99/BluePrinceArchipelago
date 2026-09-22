namespace BluePrinceArchipelago.Rooms.RoomHandlers;

class Sanctums : RoomHandler
{
    public Sanctums()
    {
        AllowanceTokens.Add("Arch Aries");
        AllowanceTokens.Add("Corarica");
        AllowanceTokens.Add("Eraja");
        AllowanceTokens.Add("Fenn Aries");
        AllowanceTokens.Add("Mora Jai");
        AllowanceTokens.Add("Orinda Aries");
        AllowanceTokens.Add("Verra");
        AllowanceTokens.Add("Nuance");
    }
    
    public override void OnAllowanceTokenCollected(string token)
    {
        ModInstance.ModEventHandler.OnMoraJaiSolved($"{token} Sanctum");
    }
}