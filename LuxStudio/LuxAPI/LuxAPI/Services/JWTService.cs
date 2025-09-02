using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LuxAPI.Models;
using LuxAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LuxAPI.Services;

public class JwtService : IJwtService
{
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public JwtService(IConfiguration configuration)
    {
        _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
        _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
    }

    /// <summary>
    /// Generates a JWT token for an authenticated user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="username">The username of the user</param>
    /// <param name="email">The email of the user</param>
    /// <param name="role">The role of the user (Client or Photographer)</param>
    /// <param name="expiryHours">The expiration time of the token in hours (default 48h)</param>
    /// <returns>JWT Token as a string</returns>
    public string GenerateJwtToken(Guid userId, string username, string email, UserRole role, int expiryHours = 48)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", ((int)role).ToString()),
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
