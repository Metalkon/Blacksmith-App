using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TestPotato> TestPotatoes { get; set; }
        public DbSet<GameData> GameData { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Material> Materials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().OwnsOne(x => x.AccountStatus, owned => owned.ToJson());
            modelBuilder.Entity<UserModel>().OwnsOne(x => x.LoginStatus, owned => owned.ToJson());
            modelBuilder.Entity<GameData>().OwnsOne(x => x.UserMaterials, owned => owned.ToJson());
            modelBuilder.Entity<GameData>().OwnsOne(x => x.UserItems, owned => owned.ToJson());
            modelBuilder.Entity<Item>().OwnsOne(x => x.Recipe, owned => owned.ToJson());



            //modelBuilder.Entity<MaterialQuantity>().HasNoKey();
            //modelBuilder.Entity<ItemCrafted>().HasNoKey();
        }
    }
}
