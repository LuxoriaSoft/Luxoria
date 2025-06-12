using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LuxAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LuxAPI.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    /// <summary>
    /// JWT settings
    /// </summary>
    private readonly string _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
    private readonly string _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");

    /// <summary>
    /// Generates a JWT token for an authenticated user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>A JWT access token as a string</returns>
    public string GenerateJwtToken(Guid userId, string username, string email, int expiryHours = 48)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            _jwtIssuer,
            _jwtAudience,
            claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}