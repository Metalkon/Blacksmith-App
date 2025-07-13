using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

public class ItemHelper
{
    private readonly DbContextSqlite _db;

    public ItemHelper(DbContextSqlite context)
    {
        _db = context;
    }
    
    public async Task<ItemDTO> MapCraftedItemToItemDTO(ItemCrafted userItem)
    {
        Item item = await _db.Items.FirstOrDefaultAsync(x => x.Id == userItem.ItemId);
        ItemDTO itemDTO = new ItemDTO()
        {
            // Display
            Id = item.Id,
            Name = item.Name,
            Rarity = item.Rarity,
            Tier = item.Tier,
            Weight = item.Weight,
            Description = item.Description,
            Image = item.Image,
            Recipe = item.Recipe,
            Tradable = item.Tradable,

            // Stat Properties
            AttackPower = Math.Round(item.BaseAttackPower + (item.BaseAttackPower * ((double)userItem.Score * 0.01) / 4), 2),
            AttackSpeed = item.BaseAttackSpeed,
            MagicPower = Math.Round(item.BaseMagicPower + (item.BaseMagicPower * ((double)userItem.Score * 0.01) / 4), 2),
            ProtectionPhysical = Math.Round(item.BaseProtectionPhysical + (item.BaseProtectionPhysical * ((double)userItem.Score * 0.01) / 4), 2),
            ProtectionMagic = Math.Round(item.BaseProtectionMagic + (item.BaseProtectionMagic * ((double)userItem.Score * 0.01) / 4), 2),

            // Craft Properties
            CraftId = userItem.CraftId,
            Prefix = userItem.PrefixId.ToString(), // Temporary (id to name)
            Suffix = userItem.SuffixId.ToString(), // Temporary (id to name)
            Score = userItem.Score + (item.Tier * 100),
            Durability = userItem.Durability,
            Price = userItem.Price,
        };

        // Add Enchant Info & Calculations (Future)

        return itemDTO;
    }

    public async Task<ItemDTO> MapItemToDTO(Item input)
    {
        ItemDTO itemDTO = new ItemDTO();

        var inputProperties = typeof(Item).GetProperties();
        var itemDTOProperties = typeof(ItemDTO).GetProperties();

        foreach (var inputProp in inputProperties)
        {
            var itemDTOProp = itemDTOProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

            if (itemDTOProp != null && itemDTOProp.CanWrite)
            {
                var value = inputProp.GetValue(input);
                itemDTOProp.SetValue(itemDTO, value);
            }
        }

        return itemDTO;
    }

    public async Task<ItemEditDTO> MapItemToEditDTO(Item input)
    {
        ItemEditDTO itemDTO = new ItemEditDTO();

        var inputProperties = typeof(Item).GetProperties();
        var itemDTOProperties = typeof(ItemEditDTO).GetProperties();

        foreach (var inputProp in inputProperties)
        {
            var itemDTOProp = itemDTOProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

            if (itemDTOProp != null && itemDTOProp.CanWrite)
            {
                var value = inputProp.GetValue(input);
                itemDTOProp.SetValue(itemDTO, value);
            }
        }

        return itemDTO;
    }

    public async Task<Item> MapDTOToItem(ItemEditDTO input)
    {
        Item item = new Item();

        var inputProperties = typeof(ItemEditDTO).GetProperties();
        var itemProperties = typeof(Item).GetProperties();

        foreach (var inputProp in inputProperties)
        {
            var itemProp = itemProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

            if (itemProp != null && itemProp.CanWrite)
            {
                var value = inputProp.GetValue(input);
                itemProp.SetValue(item, value);
            }
        }

        return item;
    }

    public async Task<Item> MapDTOToItem(ItemEditDTO input, Item itemDb)
    {
        var inputProperties = typeof(ItemEditDTO).GetProperties();
        var itemProperties = typeof(Item).GetProperties();

        foreach (var inputProp in inputProperties)
        {
            var itemProp = itemProperties.FirstOrDefault(p => p.Name == inputProp.Name && p.PropertyType == inputProp.PropertyType);

            if (itemProp != null && itemProp.CanWrite)
            {
                var value = inputProp.GetValue(input);
                itemProp.SetValue(itemDb, value);
            }
        }

        return itemDb;
    }
}
