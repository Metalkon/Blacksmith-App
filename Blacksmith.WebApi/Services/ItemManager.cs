using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;
using static System.Net.Mime.MediaTypeNames;

public class ItemManager
{
    private readonly IServiceProvider _serviceProvider;
    public List<Item> Items { get; set; }

    public ItemManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Items = new List<Item>();
        UpdateFromDatabase();
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

    public async Task UpdateFromDatabase()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    Items = await dbContext.Items.ToListAsync();
                    if (Items != null && Items.Any())
                    {
                        Console.WriteLine("Items successfully loaded.");
                        return;
                    }

                    Console.WriteLine($"Attempt {i + 1}: No items found. Retrying...");
                    await Task.Delay(2000 * i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {i + 1}: Error loading items: {ex.Message}");
                    await Task.Delay(2000 * i);
                }
            }
            Console.WriteLine("Critical error: Failed to load items from the database after 3 attempts. Shutting down...");
            Environment.Exit(1);
        }
    }


}
