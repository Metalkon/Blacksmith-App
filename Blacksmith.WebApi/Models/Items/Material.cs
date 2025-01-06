namespace Blacksmith.WebApi.Models.Items
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Tier { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public Material()
        {
            Name = $"N/A";
            Rarity = ItemRarity.Common;
            Tier = 0;
            Description = "N/A";
            Image = $"./images/Icon/question_mark.jpg";
        }

        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
    }
}
