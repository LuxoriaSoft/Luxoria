namespace LuxAPI.Services.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(Guid userId, string username, string email, int expiryHours = 48);
}