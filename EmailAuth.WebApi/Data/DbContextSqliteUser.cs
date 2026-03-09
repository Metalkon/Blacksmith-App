using EmailAuth.WebApi.Models;
using EmailAuth.WebApi.Models.Items;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace EmailAuth.WebApi.Data
{
    public class DbContextSqliteUser : DbContext
    {
        public DbContextSqliteUser(DbContextOptions<DbContextSqliteUser> options) : base(options)
        {

        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}