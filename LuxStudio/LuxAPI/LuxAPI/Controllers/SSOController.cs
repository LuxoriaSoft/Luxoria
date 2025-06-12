using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
using LuxAPI.Services;
using LuxAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace LuxAPI.Controllers
{
    /// <summary>
    /// Handles Single Sign-On (SSO) authentication and token management.
    /// Provides endpoints for OAuth2 authorization, token generation, and refresh token handling.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SSOController : ControllerBase
    {
        private readonly ILogger<SSOController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes the SSOController with logging, database context, and configuration.
        /// </summary>
        /// <param name="logger">Logger service for tracking authentication events.</param>
        /// <param name="configuration">Configuration service for retrieving JWT settings.</param>
        /// <param name="context">Database context for managing authentication data.</param>
        public SSOController(
            ILogger<SSOController> logger,
            IConfiguration configuration,
            AppDbContext context,
            IJwtService jwtService)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Handles the OAuth2 authorization request.
        /// Generates an authorization code and redirects the client to the provided redirect URI.
        /// </summary>
        /// <param name="clientId">Client application ID requesting authorization.</param>
        /// <param name="responseType">OAuth2 response type (must be "code").</param>
        /// <param name="redirectUri">URI where the client should be redirected after authorization.</param>
        /// <param name="state">Optional state parameter for request validation.</param>
        /// <returns>Redirect URL with authorization code if successful, otherwise an error response.</returns>
        [HttpGet("authorize")]
        [Authorize] // Requires user authentication
        public IActionResult Authorize([FromQuery] Guid clientId, [FromQuery] string responseType, [FromQuery] string redirectUri, [FromQuery] string state)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt.");
                    return Unauthorized(new { error = "User is not authenticated." });
                }

                if (responseType != "code")
                {
                    _logger.LogWarning("Unsupported response_type: {ResponseType}", responseType);
                    return BadRequest(new { error = "Unsupported response_type. Only 'code' is allowed." });
                }

                var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
                if (client == null || client.RedirectUri != redirectUri)
                {
                    _logger.LogWarning("Invalid client_id or redirect_uri.");
                    return BadRequest(new { error = "Invalid client_id or redirect_uri." });
                }

                var authorizationCode = Guid.NewGuid().ToString();
                _context.AuthorizationCodes.Add(new AuthorizationCode
                {
                    Code = authorizationCode,
                    ClientId = client.ClientId,
                    UserId = Guid.Parse(userId),
                    Expiry = DateTime.UtcNow.AddMinutes(10)
                });
                _context.SaveChanges();

                var redirectUrl = $"{redirectUri}?code={authorizationCode}&state={state}";
                _logger.LogInformation("Authorization successful. Redirecting to: {RedirectUrl}", redirectUrl);
                return Ok(new { redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during authorization.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Exchanges an authorization code for an access token and refresh token.
        /// </summary>
        /// <param name="request">Token request containing client credentials and authorization code.</param>
        /// <returns>Access token, refresh token, and expiration details if successful.</returns>
        [HttpPost("token")]
        public IActionResult Token([FromBody] TokenRequestDto request)
        {
            try
            {
                if (request == null || request.GrantType != "authorization_code")
                {
                    return BadRequest(new { error = "Invalid request payload or grant type." });
                }

                var client = _context.Clients.FirstOrDefault(c => c.ClientId == request.ClientId);
                if (client == null)
                {
                    return BadRequest(new { error = "Invalid client ID." });
                }

                var authorizationCode = _context.AuthorizationCodes.FirstOrDefault(c => c.Code == request.Code);
                if (authorizationCode == null || authorizationCode.Expiry < DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Invalid or expired authorization code." });
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == authorizationCode.UserId);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found." });
                }

                _context.AuthorizationCodes.Remove(authorizationCode);
                _context.SaveChanges();

                var accessToken = _jwtService.GenerateJwtToken(user.Id, user.Username, user.Email);
                var refreshToken = TokenService.GenerateRefreshToken();

                _context.Tokens.Add(new Token { AccessToken = accessToken, RefreshToken = refreshToken, UserId = user.Id, Expiry = DateTime.UtcNow.AddHours(1) });
                _context.RefreshTokens.Add(new RefreshToken { Token = refreshToken, UserId = user.Id, Expiry = DateTime.UtcNow.AddMonths(2) });

                _context.SaveChanges();

                return Ok(new { access_token = accessToken, token_type = "Bearer", expires_in = 3600, refresh_token = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the token request.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="request">Refresh token request containing the existing refresh token.</param>
        /// <returns>New access token and refresh token if successful.</returns>
        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { error = "Invalid refresh token request." });
                }

                var existingToken = _context.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
                if (existingToken == null || existingToken.Expiry < DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Invalid or expired refresh token." });
                }
                
                var user = _context.Users.FirstOrDefault(u => u.Id == existingToken.UserId);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found." });
                }

                var newAccessToken = _jwtService.GenerateJwtToken(user.Id, user.Username, user.Email);
                var newRefreshToken = TokenService.GenerateRefreshToken();

                existingToken.Token = newRefreshToken;
                existingToken.Expiry = DateTime.UtcNow.AddMonths(2);

                _context.Tokens.Add(new Token { AccessToken = newAccessToken, RefreshToken = newRefreshToken, UserId = existingToken.UserId, Expiry = DateTime.UtcNow.AddHours(1) });
                _context.SaveChanges();

                return Ok(new { access_token = newAccessToken, refresh_token = newRefreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the token.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }
    }
}
