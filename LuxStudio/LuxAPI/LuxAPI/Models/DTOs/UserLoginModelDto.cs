namespace LuxAPI.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for handling user login requests.
    /// This DTO is used when a user logs in to the system.
    /// </summary>
    public class UserLoginModelDto
    {
        /// <summary>
        /// The username of the user.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        public required string Password { get; set; }
    }
}