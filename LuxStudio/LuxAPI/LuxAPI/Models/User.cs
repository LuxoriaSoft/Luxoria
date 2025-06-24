using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models
{
    /// <summary>
    /// Defines available roles for the user.
    /// </summary>
    public enum UserRole
    {
        Client,
        Photographer
    }

    /// <summary>
    /// Represents a user in the LuxAPI system.
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the avatar filename for the user.
        /// </summary>
        public string? AvatarFileName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the role for the user.
        /// Defaults to Client.
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.Client;
    }
}
