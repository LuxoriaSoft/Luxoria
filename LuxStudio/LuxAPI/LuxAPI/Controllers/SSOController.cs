using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LuxAPI.DAL;
using LuxAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

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

    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Invalid request payload." });
            }

            if (request.GrantType != "authorization_code")
            {
                _logger.LogWarning("Unsupported grant_type: {GrantType}", request.GrantType);
                return BadRequest(new { error = "Unsupported grant_type. Only 'authorization_code' is allowed." });
            }

            var client = _context.Clients.FirstOrDefault(c => c.ClientId == request.ClientId && c.ClientSecret == request.ClientSecret);
            if (client == null)
            {
                _logger.LogWarning("Invalid client credentials for client_id: {ClientId}", request.ClientId);
                return BadRequest(new { error = "Invalid client credentials." });
            }

            var authorizationCode = _context.AuthorizationCodes.FirstOrDefault(c => c.Code == request.Code);
            if (authorizationCode == null || authorizationCode.Expiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired authorization code: {Code}", request.Code);
                return BadRequest(new { error = "Invalid or expired authorization code." });
            }

            // Vérifier si l'utilisateur existe
            var user = _context.Users.FirstOrDefault(u => u.Id == authorizationCode.UserId);
            if (user == null)
            {
                return BadRequest(new { error = "User not found." });
            }

            // Supprimer le code d'autorisation après validation
            _context.AuthorizationCodes.Remove(authorizationCode);
            _context.SaveChanges();

            // Générer un nouvel access token et refresh token
            var accessToken = GenerateJwtToken(user.Id.ToString());
            var refreshToken = TokenService.GenerateRefreshToken();

            _context.Tokens.Add(new Token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Expiry = DateTime.UtcNow.AddHours(1)
            });

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expiry = DateTime.UtcNow.AddMonths(2)
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

    [HttpGet("protected")]
    [Authorize] // Nécessite un JWT valide
    public IActionResult ProtectedEndpoint()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "User is not authenticated." });
        }

        return Ok(new { message = "Access to protected route granted!", userId });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { error = "Invalid refresh token request." });
            }

            // Vérifier si le refresh token existe
            var existingToken = _context.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (existingToken == null || existingToken.Expiry < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Invalid or expired refresh token." });
            }

            // Générer un nouveau access token et refresh token
            var newAccessToken = GenerateJwtToken(existingToken.UserId.ToString());
            var newRefreshToken = TokenService.GenerateRefreshToken();

            // Mettre à jour le refresh token existant avec le nouveau
            existingToken.Token = newRefreshToken;
            existingToken.Expiry = DateTime.UtcNow.AddMonths(2);

            // Ajouter le nouveau access token à la base de données
            var newToken = new Token
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = existingToken.UserId,
                Expiry = DateTime.UtcNow.AddHours(1)
            };
            _context.Tokens.Add(newToken);
            _context.SaveChanges();

            return Ok(newToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while refreshing the token.");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // DTO pour la requête de rafraîchissement du token
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
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

    public class TokenService
    {
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
    // DTO pour recevoir les paramètres de la requête
    public class TokenRequestDto
    {
        public Guid ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Code { get; set; }
        public string GrantType { get; set; }
    }
}


//créer une route protéger dans l'api et check avec le token si ca fonctionne
//ajouter une fonction qui prend le refresh token et qui génère un nouveau token et refresh token et qui annule l'ancien refresh token utilisé