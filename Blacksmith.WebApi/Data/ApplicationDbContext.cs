using Blacksmith.WebApi.Models;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().OwnsOne(
                user => user.AccountStatus, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                });
            modelBuilder.Entity<UserModel>().OwnsOne(
                user => user.LoginStatus, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                });
        }
    }
}
