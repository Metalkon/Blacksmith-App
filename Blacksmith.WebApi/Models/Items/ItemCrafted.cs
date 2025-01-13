namespace Blacksmith.WebApi.Models.Items
{
    public class ItemCrafted
    {
        public int ItemId { get; set; }
        public string CraftId { get; set; }
        public int PrefixId { get; set; }
        public int SuffixId { get; set; }
        public int Score { get; set; }
        public int Durability { get; set; }
        public int Price { get; set; }

        public ItemCrafted()
        {
            CraftId = Guid.NewGuid().ToString("N").Substring(0, 8);
            PrefixId = 0;
            SuffixId = 0;
            Score = 0;
            Durability = 1;
            Price = 1;
        }
    }
}
