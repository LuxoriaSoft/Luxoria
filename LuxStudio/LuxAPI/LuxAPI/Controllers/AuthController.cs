using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LuxAPI.DAL;
using LuxAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ILogger<AuthController> logger, AppDbContext context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration = configuration; // Injectez la configuration pour accéder à Jwt:Key
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationModel registration)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration data received.");
            return BadRequest(ModelState);
        }
        
        if (_context.Users.Any(u => u.Username == registration.Username))
        {
            _logger.LogWarning("Username already taken: {Username}", registration.Username);
            return BadRequest("Username already taken.");
        }
        
        if (_context.Users.Any(u => u.Email == registration.Email))
        {
            _logger.LogWarning("Email already taken: {Email}", registration.Email);
            return BadRequest("Email already taken.");
        }

        var user = new User
        {
            Username = registration.Username,
            Email = registration.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        _logger.LogInformation("User registered successfully: {Username}", registration.Username);
        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginModel login)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login data received.");
            return BadRequest(ModelState);
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == login.Username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", login.Username);
            return Unauthorized("Invalid username or password.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Invalid password for user: {Username}", login.Username);
            return Unauthorized("Invalid username or password.");
        }

        // Générer le token JWT
        var token = GenerateJwtToken(user.Username);

        _logger.LogInformation("User logged in successfully: {Username}", login.Username);
        return Ok(new { token }); // Retourner le token dans la réponse
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Expire dans 1 heure
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Represents the data required for user registration.
/// </summary>
public class UserRegistrationModel
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}

/// <summary>
/// Represents the data required for user login.
/// </summary>
public class UserLoginModel
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}