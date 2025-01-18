using System.ComponentModel.DataAnnotations;
using LuxAPI.DAL;
using LuxAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _context;

    public AuthController(ILogger<AuthController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationModel registration)
    {
        // Simulated registration process (replace with actual logic)
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration data received.");
            return BadRequest(ModelState);
        }
        
        // Check if the username or email is already taken
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