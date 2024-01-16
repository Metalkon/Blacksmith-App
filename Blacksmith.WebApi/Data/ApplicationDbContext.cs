using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;


namespace Blacksmith.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<TestPotato> TestPotatoes { get; set; }
    }
}
