
namespace Shared_Classes.Models
{
    public class ItemDTO
    {
        // Primary Properties
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public bool Tradable { get; set; }
        public string Image { get; set; }

        // Secondary Properties

        public string? Description { get; set; }
        public string? Quality { get; set; }
        public string? Rarity { get; set; }
        public int? Weight { get; set; }
        public int? Durability { get; set; }
        public int? AttackPower { get; set; }
        public int? AttackSpeed { get; set; }
        public int? MagicPower { get; set; }
        public int? ProtectionPhysical { get; set; }
        public int? ProtectionMagical { get; set; }
    }
}
