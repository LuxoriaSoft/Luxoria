using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SSOController : ControllerBase
{
    private readonly ILogger<SSOController> _logger;
    private readonly IConfiguration _configuration;
    private static Dictionary<string, string> AuthorizationCodes = new();
    private static Dictionary<string, string> Tokens = new();

    public SSOController(ILogger<SSOController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // STEP 1: Authorize endpoint
    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] string client_id, [FromQuery] string response_type, [FromQuery] string redirect_uri, [FromQuery] string state)
    {
        if (response_type != "code")
        {
            return BadRequest("Unsupported response_type. Only 'code' is allowed.");
        }

        // Validate the client_id (you can replace this with a database or a more robust validation)
        if (client_id != "lux-app")
        {
            return BadRequest("Invalid client_id.");
        }

        // Simulate user login and generate an authorization code
        var authorizationCode = Guid.NewGuid().ToString();
        AuthorizationCodes[authorizationCode] = client_id; // Store the code for later validation

        // Redirect to the provided redirect_uri with the code and state
        var redirectUrl = $"{redirect_uri}?code={authorizationCode}&state={state}";

        _logger.LogInformation($"Redirecting to: {redirectUrl}");
        return Redirect(redirectUrl);
    }

    // STEP 2: Token endpoint
    [HttpPost("token")]
    public IActionResult Token(string client_id, string client_secret, string code, string grant_type, string redirect_uri)
    {
        if (grant_type != "authorization_code")
        {
            return BadRequest("Unsupported grant_type. Only 'authorization_code' is allowed.");
        }

        // Validate the authorization code
        if (!AuthorizationCodes.ContainsKey(code) || AuthorizationCodes[code] != client_id)
        {
            return BadRequest("Invalid or expired authorization code.");
        }

        // Remove the authorization code after usage
        AuthorizationCodes.Remove(code);

        // Generate tokens
        var accessToken = GenerateJwtToken(client_id);
        var refreshToken = Guid.NewGuid().ToString();
        Tokens[refreshToken] = client_id;

        return Ok(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 3600, // 1 hour
            refresh_token = refreshToken
        });
    }

    // STEP 3: User info endpoint
    [HttpGet("userinfo")]
    public IActionResult UserInfo([FromHeader(Name = "Authorization")] string authorization)
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            return Unauthorized("Invalid token.");
        }

        var token = authorization.Substring("Bearer ".Length);

        // Validate the access token
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

            if (string.IsNullOrEmpty(clientId) || clientId != "lux-app")
            {
                return Unauthorized("Invalid access token.");
            }

            return Ok(new
            {
                client_id = clientId,
                username = "lux-user", // Dummy data
                email = "user@example.com"
            });
        }
        catch
        {
            return Unauthorized("Invalid token.");
        }
    }

    private string GenerateJwtToken(string clientId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
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
