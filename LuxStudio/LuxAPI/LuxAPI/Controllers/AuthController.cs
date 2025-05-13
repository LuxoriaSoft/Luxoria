using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace LuxAPI.Controllers
{
    /// <summary>
    /// Authentication controller for handling user registration, login, and authentication.
    /// Provides JWT token-based authentication.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the authentication controller with logging, database context, and configuration.
        /// </summary>
        /// <param name="logger">Logger service for tracking authentication events.</param>
        /// <param name="context">Database context for accessing user data.</param>
        /// <param name="configuration">Configuration service for retrieving JWT settings.</param>
        public AuthController(ILogger<AuthController> logger, AppDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user by storing their credentials securely.
        /// Ensures the username and email are unique before saving.
        /// </summary>
        /// <param name="registration">User registration data (username, email, password).</param>
        /// <returns>Success message if registration is successful, otherwise an error.</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegistrationModelDto registration)
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.Password), // Hash password securely
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _logger.LogInformation("User registered successfully: {Username}", registration.Username);
            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Authenticates a user by validating their credentials and issuing a JWT token.
        /// </summary>
        /// <param name="login">User login data (username, password).</param>
        /// <returns>JWT token if authentication is successful, otherwise an error.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModelDto login)
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

            // Generate a JWT token for the authenticated user
            var token = GenerateJwtToken(user.Id, user.Username, user.Email);

            _logger.LogInformation("User logged in successfully: {Username}", login.Username);
            return Ok(new { token }); // Return the JWT token
        }

            /// <summary>
            /// Generates a JWT token for an authenticated user.
            /// </summary>
            /// <param name="userId">The unique identifier of the user.</param>
            /// <param name="username">The username of the authenticated user.</param>
            /// <param name="email">The email of the authenticated user.</param>
            /// <param name="expiryHours">The expiration time in hours (default is 48 hours).</param>
            /// <returns>A JWT token as a string.</returns>
            private string GenerateJwtToken(Guid userId, string username, string email, int expiryHours = 48)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, email), // ✅ utilisé comme .Name
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Returns information about the currently authenticated user.
        /// This route requires a valid JWT token.
        /// </summary>
        /// <returns>User ID and username of the authenticated user.</returns>
        [HttpGet("whoami")]
        [Authorize] // Requires a valid JWT token
        public IActionResult WhoAmI()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value; // 👈 ici
            return Ok(new { userId, username, userEmail });
        }
    }
}
