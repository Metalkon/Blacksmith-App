using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    public class GameData
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int Level { get; set; }
        [Required]
        public int Experience { get; set; }
        [Required]
        public int Gold { get; set; }
        [Required]
        public List<Item> Inventory { get; set; }

        // public List<Schematic> Schematics { get; set; }

        public GameData()
        {
            Level = 1;
            Experience = 0;
            Gold = 500;
            Inventory = new List<Item>()
            {
                new Item() { ItemId = 0, Name = "Hammer", Type = "Tool", Price = 0, Quantity = 1, Tradable = false, Image = "./images/Icon/Other/en_craft_9.jpg"}
            };
        }
    }
}
