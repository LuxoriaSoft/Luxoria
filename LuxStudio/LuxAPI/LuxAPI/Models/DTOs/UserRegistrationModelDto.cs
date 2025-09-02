using System.ComponentModel.DataAnnotations;

namespace LuxAPI.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for handling user registration requests.
    /// Contains the necessary information to create a new user account.
    /// </summary>
    public class UserRegistrationModelDto
    {
        /// <summary>
        /// The chosen username for the new user.
        /// Maximum length is 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public required string Username { get; set; }

        /// <summary>
        /// The email address of the user.
        /// Must be a valid email format and cannot exceed 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public required string Email { get; set; }

        /// <summary>
        /// The password for the new user.
        /// Must be at least 6 characters long for security reasons.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        /// <summary>
        /// The role of the user (Client or Photographer).
        /// Must be a valid value from the UserRole enum.
        /// </summary>
        [Required(ErrorMessage = "Role is required.")]
        public required UserRole Role { get; set; }
    }
}
