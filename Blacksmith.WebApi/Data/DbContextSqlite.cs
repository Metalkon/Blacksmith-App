using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace Blacksmith.WebApi.Data
{
    public class DbContextSqlite : DbContext
    {
        public DbContextSqlite(DbContextOptions<DbContextSqlite> options) : base(options)
        {

        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Material> Materials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().OwnsMany(x => x.Recipe, owned => owned.ToJson());
        }
    }
}