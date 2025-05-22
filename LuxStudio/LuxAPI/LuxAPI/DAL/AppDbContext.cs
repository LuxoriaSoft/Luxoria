using Microsoft.EntityFrameworkCore;
using LuxAPI.Models;

namespace LuxAPI.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Existing models
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionAccess> CollectionAccesses { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoComment> PhotoComments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Client
            modelBuilder.Entity<Client>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Client>()
                .Property(c => c.ClientId)
                .IsRequired();

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
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<RefreshToken>()
                .Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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


            // A Collection has many AllowedEmails (CollectionAccesses)
            modelBuilder.Entity<Collection>()
                .HasMany(c => c.AllowedEmails)
                .WithOne(a => a.Collection)
                .HasForeignKey(a => a.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A Collection has many ChatMessages
            modelBuilder.Entity<Collection>()
                .HasMany(c => c.ChatMessages)
                .WithOne(cm => cm.Collection)
                .HasForeignKey(cm => cm.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A Collection has many Photos
            modelBuilder.Entity<Collection>()
                .HasMany(c => c.Photos)
                .WithOne(p => p.Collection)
                .HasForeignKey(p => p.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A Photo has many Comments (PhotoComments)
            modelBuilder.Entity<Photo>()
                .HasMany(p => p.Comments)
                .WithOne(pc => pc.Photo)
                .HasForeignKey(pc => pc.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure self-referencing for PhotoComment (for replies)
            modelBuilder.Entity<PhotoComment>()
                .HasOne(pc => pc.ParentComment)
                .WithMany(pc => pc.Replies)
                .HasForeignKey(pc => pc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
