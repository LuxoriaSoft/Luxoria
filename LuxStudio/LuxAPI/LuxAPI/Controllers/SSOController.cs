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
    [Authorize] // Vérifie que l'utilisateur est authentifié
    public IActionResult Authorize([FromQuery] Guid clientId, [FromQuery] string responseType, [FromQuery] string redirectUri, [FromQuery] string state)
    {
        try
        {
            // Extraire l'ID de l'utilisateur authentifié depuis le token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Unauthorized(new { error = "User is not authenticated." });
            }

            // Vérification du paramètre responseType
            if (responseType != "code")
            {
                _logger.LogWarning("Unsupported response_type: {ResponseType}", responseType);
                return BadRequest(new { error = "Unsupported response_type. Only 'code' is allowed." });
            }

            // Valider le client ID
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client == null)
            {
                _logger.LogWarning("Invalid client_id: {ClientId}", clientId);
                return BadRequest(new { error = "Invalid client_id." });
            }

            // Vérifier l'URI de redirection
            if (client.RedirectUri != redirectUri)
            {
                _logger.LogWarning("Invalid redirect_uri: {RedirectUri}", redirectUri);
                return BadRequest(new { error = "Invalid redirect_uri." });
            }

            // Générer un code d'autorisation
            var authorizationCode = Guid.NewGuid().ToString();
            _context.AuthorizationCodes.Add(new AuthorizationCode
            {
                Code = authorizationCode,
                ClientId = client.ClientId,
                UserId = Guid.Parse(userId),
                Expiry = DateTime.UtcNow.AddMinutes(10)
            });
            _context.SaveChanges();

            // Redirection vers l'URI avec le code et l'état
            var redirectUrl = $"{redirectUri}?code={authorizationCode}&state={state}";
            _logger.LogInformation("Authorization successful. Redirecting to: {RedirectUrl}", redirectUrl);
            return Ok(new { redirectUrl = redirectUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during authorization.");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // Token endpoint
    [HttpPost("token")]
    public IActionResult Token([FromForm] Guid clientId, [FromForm] string clientSecret, [FromForm] string code, [FromForm] string grantType)
    {
        try
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
            if (authorizationCode == null || authorizationCode.Expiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired authorization code: {Code}", code);
                return BadRequest(new { error = "Invalid or expired authorization code." });
            }

            _context.AuthorizationCodes.Remove(authorizationCode);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(u => u.Id == authorizationCode.UserId);
            if (user == null)
            {
                return BadRequest(new { error = "User not found." });
            }

            var accessToken = GenerateJwtToken(user.Id.ToString());
            var refreshToken = Guid.NewGuid().ToString();

            _context.Tokens.Add(new Token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ClientId = client.Id,
                Expiry = DateTime.UtcNow.AddHours(1)
            });
            _context.SaveChanges();

            return Ok(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = refreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the token request.");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    private string GenerateJwtToken(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
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
