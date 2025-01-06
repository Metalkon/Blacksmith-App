using Blacksmith.WebApi.Models.Items;
using System.ComponentModel.DataAnnotations;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Models
{
    public class GameData
    {
        [Key]
        public int Id { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }
        public List<ItemCrafted> UserItems { get; set; }
        public List<MaterialQuantity> UserMaterials { get; set; }

        public GameData()
        {
            Level = 1;
            Experience = 0;
            Gold = 500;
            UserItems = new List<ItemCrafted>();
            UserMaterials = new List<MaterialQuantity>();

            UserItems.Add(new ItemCrafted() { Score = 444 });
            UserItems.Add(new ItemCrafted() { Score = 575 });
            UserItems.Add(new ItemCrafted() { Score = 310 });

        }
    }
}
