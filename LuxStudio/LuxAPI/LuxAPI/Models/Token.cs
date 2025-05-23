using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxAPI.Models
{
    /// <summary>
    /// Represents an access token and its associated refresh token.
    /// Used for authenticating and maintaining user sessions.
    /// </summary>
    [Table("Tokens")]
    public class Token
    {
        /// <summary>
        /// Unique identifier for the token record.
        /// Automatically generated by the database.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// The JWT access token used for authentication.
        /// Stored as a TEXT type to allow variable-length storage.
        /// </summary>
        [Required(ErrorMessage = "Access token is required.")]
        [Column(TypeName = "TEXT")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token associated with the access token.
        /// Used to obtain a new access token when the current one expires.
        /// Stored as a TEXT type to allow variable-length storage.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required.")]
        [Column(TypeName = "TEXT")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The unique identifier of the user associated with this token.
        /// Links the token to a specific user account.
        /// </summary>
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property for the related user entity.
        /// Establishes a foreign key relationship with the User table.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        /// <summary>
        /// The expiration date and time of the access token.
        /// After expiration, the user must use the refresh token to obtain a new access token.
        /// </summary>
        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime Expiry { get; set; }
    }
}
