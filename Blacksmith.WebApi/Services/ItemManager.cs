using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;

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

    public async Task CombineItemData()
    {

    }
}
