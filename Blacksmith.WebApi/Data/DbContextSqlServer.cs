using Blacksmith.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Data
{
    public class DbContextSqlServer : DbContext
    {
        public DbContextSqlServer(DbContextOptions<DbContextSqlServer> options) : base(options)
        {
            
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TestPotato> TestPotatoes { get; set; }
        public DbSet<GameData> GameData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameData>().OwnsMany(x => x.UserMaterials, owned => owned.ToJson());
            modelBuilder.Entity<GameData>().OwnsMany(x => x.UserItems, owned => owned.ToJson());
        }
    }
}
