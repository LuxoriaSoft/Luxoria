using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LuxAPI.DAL;
using LuxAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SSOController : ControllerBase
{
    private readonly ILogger<SSOController> _logger;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public SSOController(ILogger<SSOController> logger, IConfiguration configuration, AppDbContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }

    // Authorize endpoint
    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] Guid clientId, [FromQuery] string responseType, [FromQuery] string redirectUri, [FromQuery] string state)
    {
        if (responseType != "code")
        {
            _logger.LogWarning("Unsupported response_type: {ResponseType}", responseType);
            return BadRequest(new { error = "Unsupported response_type. Only 'code' is allowed." });
        }

        var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
        if (client == null)
        {
            _logger.LogWarning("Invalid client_id: {ClientId}", clientId);
            return BadRequest(new { error = "Invalid client_id." });
        }

        var authorizationCode = Guid.NewGuid().ToString();
        _context.AuthorizationCodes.Add(new AuthorizationCode
        {
            Code = authorizationCode,
            ClientId = client.ClientId,
            Expiry = DateTime.UtcNow.AddMinutes(10)
        });
        _context.SaveChanges();

        var redirectUrl = $"{redirectUri}?code={authorizationCode}&state={state}";
        _logger.LogInformation("Authorization successful. Redirecting to: {RedirectUrl}", redirectUrl);
        return Redirect(redirectUrl);
    }

    // Token endpoint
    [HttpPost("token")]
    public IActionResult Token([FromForm] Guid clientId, [FromForm] string clientSecret, [FromForm] string code, [FromForm] string grantType, [FromForm] string redirectUri)
    {
        if (grantType != "authorization_code")
        {
            _logger.LogWarning("Unsupported grant_type: {GrantType}", grantType);
            return BadRequest(new { error = "Unsupported grant_type. Only 'authorization_code' is allowed." });
        }

        var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId && c.ClientSecret == clientSecret);
        if (client == null)
        {
            _logger.LogWarning("Invalid client credentials for client_id: {ClientId}", clientId);
            return BadRequest(new { error = "Invalid client credentials." });
        }

        var authorizationCode = _context.AuthorizationCodes.FirstOrDefault(c => c.Code == code);
        if (authorizationCode == null || authorizationCode.ClientId != client.Id || authorizationCode.Expiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired authorization code: {Code}", code);
            return BadRequest(new { error = "Invalid or expired authorization code." });
        }

        _context.AuthorizationCodes.Remove(authorizationCode);
        _context.SaveChanges();

        var accessToken = GenerateJwtToken(client.Id.ToString());
        var refreshToken = Guid.NewGuid().ToString();

        _context.Tokens.Add(new Token
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ClientId = client.Id,
            Expiry = DateTime.UtcNow.AddHours(1)
        });
        _context.SaveChanges();

        _logger.LogInformation("Token issued for client_id: {ClientId}", clientId);
        return Ok(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 3600,
            refresh_token = refreshToken
        });
    }

    // User info endpoint
    [HttpGet("userinfo")]
    //[Authorize]
    public IActionResult UserInfo([FromHeader(Name = "Authorization")] string authorization)
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            _logger.LogWarning("Invalid Authorization header.");
            return Unauthorized(new { error = "Invalid token." });
        }

        var token = authorization.Substring("Bearer ".Length);

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

            if (clientId == null)
            {
                _logger.LogWarning("Invalid client_id in token.");
                return Unauthorized(new { error = "Invalid token." });
            }
            
            var storedToken = _context.Tokens.Include(t => t.Client).FirstOrDefault(t => t.AccessToken == token);
            if (storedToken == null || storedToken.ClientId != Guid.Parse(clientId) || storedToken.Expiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired token: {Token}", token);
                return Unauthorized(new { error = "Invalid or expired token." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == clientId);
            if (user == null)
            {
                _logger.LogWarning("No user found for client_id: {ClientId}", clientId);
                return NotFound(new { error = "User not found." });
            }

            return Ok(new
            {
                client_id = clientId,
                username = user.Username,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user info request.");
            return Unauthorized(new { error = "Invalid token." });
        }
    }

    private string GenerateJwtToken(string clientId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("client_id", clientId),
            new Claim("scope", "openid profile email")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
