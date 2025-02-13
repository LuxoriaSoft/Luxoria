using System.Security.Cryptography;

namespace LuxAPI.Services
{
    /// <summary>
    /// Service responsible for generating secure tokens.
    /// This service is used to generate refresh tokens for authentication.
    /// </summary>
    public class TokenService
    {
        /// <summary>
        /// Generates a secure random refresh token.
        /// This token is used to obtain a new access token when the current one expires.
        /// </summary>
        /// <returns>A cryptographically secure refresh token as a Base64 string.</returns>
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32]; // 256-bit token
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber); // Fill with cryptographically secure random bytes
                return Convert.ToBase64String(randomNumber); // Convert to Base64 string
            }
        }
    }
}
