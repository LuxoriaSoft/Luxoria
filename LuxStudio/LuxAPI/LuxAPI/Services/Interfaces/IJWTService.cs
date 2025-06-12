namespace LuxAPI.Services.Interfaces;

/// <summary>
/// Interface for JWT service
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for an authenticated user
    /// </summary>
    /// <param name="userId">UserID as Guid</param>
    /// <param name="username">UserName as String</param>
    /// <param name="email">Email as String</param>
    /// <param name="expiryHours">ExpiryHours as Int (default = 48h)</param>
    /// <returns>Returns the token as String</returns>
    string GenerateJwtToken(Guid userId, string username, string email, int expiryHours = 48);
}