using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

public class ItemManager
{
    private readonly IServiceProvider _serviceProvider;
    public List<Item> Items { get; set; }

    public ItemManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Items = new List<Item>();
    }

    public async Task UpdateFromDatabase()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            Items = await dbContext.Items.ToListAsync();               
                        
            //Environment.Exit(1);
        }
    }

    public async Task<ItemDTO> GenerateItemDTO(ItemCrafted userItem)
    {
        Item baseItem = Items.FirstOrDefault(x => x.Id == userItem.ItemId);

        ItemDTO itemDTO = new ItemDTO()
        {
            // BaseItem Values
            Id = baseItem.Id,
            Name = baseItem.Name,
            Rarity = baseItem.Rarity,
            Tier = baseItem.Tier,
            Weight = baseItem.Weight,
            Description = baseItem.Description,
            Image = baseItem.Image,

            // Temporary Crafting Values
            Recipe = baseItem.Recipe,
            Tradable = baseItem.Tradable,
            AttackPower = baseItem.BaseAttackPower + (baseItem.BaseAttackPower * (userItem.Score / 1000)),
            AttackSpeed = baseItem.BaseAttackSpeed + (baseItem.BaseAttackSpeed * (userItem.Score / 1000)),
            MagicPower = baseItem.BaseMagicPower + (baseItem.BaseMagicPower * (userItem.Score / 1000)),
            ProtectionPhysical = baseItem.BaseProtectionPhysical + (baseItem.BaseProtectionPhysical * (userItem.Score / 1000)),
            ProtectionMagic = baseItem.BaseProtectionMagic + (baseItem.BaseProtectionMagic * (userItem.Score / 1000)),

            // UserItem Values
            CraftId = userItem.CraftId,
            Prefix = userItem.PrefixId.ToString(),
            Suffix = userItem.SuffixId.ToString(),
            Score = userItem.Score,
            Durability = userItem.Durability,
            Price = userItem.Price,
        };

        return itemDTO;
    }



}
