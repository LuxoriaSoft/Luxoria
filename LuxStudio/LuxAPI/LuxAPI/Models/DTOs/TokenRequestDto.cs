namespace LuxAPI.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for handling token requests.
    /// This DTO is used when a client exchanges an authorization code for an access token.
    /// </summary>
    public class TokenRequestDto
    {
        /// <summary>
        /// The unique identifier of the client application making the request.
        /// Required for client authentication.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// The secret key of the client application used for authentication.
        /// This should match the stored client secret in the system.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The authorization code received after user authentication.
        /// This code is exchanged for an access token.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The type of grant requested.
        /// Should be "authorization_code" as per OAuth 2.0 standards.
        /// </summary>
        public string GrantType { get; set; }
    }
}
