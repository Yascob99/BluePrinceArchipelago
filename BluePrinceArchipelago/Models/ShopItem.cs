
namespace BluePrinceArchipelago.Models
{
    public class ShopItem
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public string ScoutHint { get; set; }

        public string GetScoutHint()
        {
            return ScoutHint ?? "Scout Hint Placeholder";
        }
    }
}