using Microsoft.EntityFrameworkCore;

using LuxAPI.Models;

namespace LuxAPI.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        // Define models to be used in the database
        public DbSet<User> Users { get; set; }
    }
}