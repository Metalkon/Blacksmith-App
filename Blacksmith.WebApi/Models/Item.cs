using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    public class Item
    {
        // Primary Properties
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ItemId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public bool Tradable { get; set; }
        [Required]
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

        public Item()
        {
            ItemId = 0;
            Name = "Undefined";
            Type = "None";
            Price = 0;
            Quantity = 1;
            Image = "./images/Icon/question_mark.jpg";
        }
    }
}
