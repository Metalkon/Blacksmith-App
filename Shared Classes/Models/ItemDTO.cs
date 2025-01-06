﻿
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
        public int BaseAttackPower { get; set; }
        public int BaseAttackSpeed { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseProtectionPhysical { get; set; }
        public int BaseProtectionMagic { get; set; }

        // Crafted Properties
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int Score { get; set; }
        public int Durability { get; set; }
        public int Price { get; set; }

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
