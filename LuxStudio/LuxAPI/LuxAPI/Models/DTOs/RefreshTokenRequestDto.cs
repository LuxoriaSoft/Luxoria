namespace LuxAPI.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for handling refresh token requests.
    /// This DTO is used when a client requests a new access token using a refresh token.
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// The refresh token provided by the client to obtain a new access token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
