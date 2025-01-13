
using System;

namespace Shared_Classes.Models
{
    public class ItemDTO
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
        public int BaseDurability { get; set; }
        public int BasePrice { get; set; }
        public int BaseScore { get; set; }
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
    }
}
