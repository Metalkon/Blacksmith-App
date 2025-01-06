using System.ComponentModel.DataAnnotations.Schema;

namespace Blacksmith.WebApi.Models.Items
{
    public class ItemCrafted
    {
        public int ItemId { get; set; }
        public int PrefixId { get; set; }
        public int SuffixId { get; set; }
        public int Score { get; set; }
        public int Durability { get; set; }
        public int Price { get; set; }

        public ItemCrafted()
        {
            ItemId = 1;
            PrefixId = 2;
            SuffixId = 2;
            Score = 580;
            Durability = 65;
            Price = 150;
        }
    }
}
