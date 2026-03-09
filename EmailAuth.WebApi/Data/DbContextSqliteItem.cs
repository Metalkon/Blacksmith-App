using EmailAuth.WebApi.Models;
using EmailAuth.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace EmailAuth.WebApi.Data
{
    public class DbContextSqliteItem : DbContext
    {
        public DbContextSqliteItem(DbContextOptions<DbContextSqliteItem> options) : base(options)
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