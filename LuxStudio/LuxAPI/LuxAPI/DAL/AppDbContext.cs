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
        public DbSet<Client> Clients { get; set; }
        public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Client
            modelBuilder.Entity<Client>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Client>()
                .Property(c => c.ClientId)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Client>()
                .Property(c => c.ClientSecret)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Client>()
                .Property(c => c.RedirectUri)
                .IsRequired()
                .HasMaxLength(200);

            // Configure Token
            modelBuilder.Entity<Token>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Token>()
                .Property(t => t.AccessToken)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Token>()
                .Property(t => t.RefreshToken)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Token>()
                .HasOne(t => t.Client) // One Token references one Client
                .WithMany() // A Client can have many Tokens
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete Tokens when a Client is removed

            // Configure AuthorizationCode
            modelBuilder.Entity<AuthorizationCode>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AuthorizationCode>()
                .Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AuthorizationCode>()
                .Property(a => a.ClientId)
                .IsRequired();

            modelBuilder.Entity<AuthorizationCode>()
                .Property(a => a.Expiry)
                .IsRequired();

            // Configure User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt);
        }
    }
}
