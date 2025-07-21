using Shared_Classes.Models;

namespace Blacksmith.WebApi.Models.Items
{
    public class Item
    {
        // Display Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Tier { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        // Crafting Properties
        public List<MaterialQuantity> Recipe { get; set; }
        public bool Tradable { get; set; }
        public double AttackPower { get; set; }
        public double AttackSpeed { get; set; }
        public double MagicPower { get; set; }
        public double ProtectionPhysical { get; set; }
        public double ProtectionMagic { get; set; }

        // Crafted Properties
        public string CraftId { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int Score { get; set; }
        public int Durability { get; set; }
        public int Price { get; set; }

        public Item()
        {
            // Display Properties
            Name = $"N/A";
            Rarity = ItemRarity.Common;
            Tier = 0;
            Weight = 0;
            Description = "N/A";
            Image = $"./images/Icon/question_mark.jpg";

            // Crafting Properties
            Recipe = new List<MaterialQuantity>();
            Tradable = true;
            Durability = 1;
            Score = 0;
            AttackPower = 0;
            AttackSpeed = 0;
            MagicPower = 0;
            ProtectionPhysical = 0;
            ProtectionMagic = 0;

            CraftId = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
        }        
    }
}
