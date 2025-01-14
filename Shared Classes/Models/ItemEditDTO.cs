namespace Shared_Classes.Models
{
    public class ItemEditDTO
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
        public double BaseAttackPower { get; set; }
        public double BaseAttackSpeed { get; set; }
        public double BaseMagicPower { get; set; }
        public double BaseProtectionPhysical { get; set; }
        public double BaseProtectionMagic { get; set; }
    }
}
