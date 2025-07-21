namespace Shared_Classes.Models

{
    public class MaterialDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Tier { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}
