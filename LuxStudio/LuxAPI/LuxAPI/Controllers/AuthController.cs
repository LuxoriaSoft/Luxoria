using System.Security.Claims;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
using LuxAPI.Services;
using LuxAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LuxAPI.Controllers
{
    /// <summary>
    /// Authentication controller for handling user registration, login, and authentication.
    /// Provides JWT token-based authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly MinioService _minioService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes the authentication controller with logging, database context, and configuration.
        /// </summary>
        /// <param name="logger">Logger service for tracking authentication events.</param>
        /// <param name="context">Database context for accessing user data.</param>
        /// <param name="configuration">Configuration service for retrieving JWT settings.</param>
        /// <param name="minioService">Custom service for MinIO file storage operations.</param>
        public AuthController(
            ILogger<AuthController> logger,
            AppDbContext context,
            IConfiguration configuration,
            MinioService minioService,
            IJwtService jwtService)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _minioService = minioService;
            _jwtService = jwtService;
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
        /// Uploads a user's avatar image to MinIO and stores the filename in the database.
        /// Requires authentication.
        /// </summary>
        /// <param name="file">The avatar image file.</param>
        /// <returns>Public URL of the uploaded avatar.</returns>
        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile? file)
        {
            if (!IsValidFile(file))
                return BadRequest("Invalid file uploaded.");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
                return BadRequest("Unsupported file type.");

            var fileName = $"{userId}_avatar{extension}";
            var filePath = $"avatars/{fileName}";

            using (var stream = file.OpenReadStream())
            {
                await _minioService.UploadFileAsync("user-files", filePath, stream, file.ContentType);
            }

            user.AvatarFileName = fileName;
            await _context.SaveChangesAsync();

            return Ok(new { avatarUrl = $"/auth/avatar/{fileName}" });
        }


        /// <summary>
        /// Retrieves a user's avatar image by filename from MinIO.
        /// </summary>
        /// <param name="fileName">The filename of the avatar image.</param>
        /// <returns>The image file if it exists, or 404 if not.</returns>
        [HttpGet("avatar/{fileName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatar(string fileName)
        {
            var bucket = "user-files";
            var path = $"avatars/{fileName}";

            try
            {
                var stream = await _minioService.GetFileAsync(bucket, path);

                // Si le fichier demandé n’existe pas, fallback vers default_avatar
                if (stream == null || stream.Length == 0)
                {
                    Console.WriteLine($"[Avatar] Fichier introuvable : {path}, fallback vers default_avatar.jpg");

                    var defaultStream = await _minioService.GetFileAsync(bucket, "avatars/default_avatar.jpg");
                    if (defaultStream == null || defaultStream.Length == 0)
                        return NotFound("Avatar par défaut également introuvable.");

                    return File(defaultStream, "image/jpeg");
                }

                return File(stream, GetContentType(fileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Avatar] Erreur : {ex.Message}");
                return StatusCode(500, "Erreur lors de la récupération du fichier.");
            }
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

            //var user = _context.Users.FirstOrDefault(u => u.Username == login.Username);
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == login.Username || u.Email == login.Username);

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
            var token = _jwtService.GenerateJwtToken(user.Id, user.Username, user.Email);

            _logger.LogInformation("User logged in successfully: {Username}", login.Username);
            return Ok(new { token }); // Return the JWT token
        }

        [HttpPost("request-verification")]
        public async Task<IActionResult> RequestVerification([FromBody] UserPendingRegistrationDto data, [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Email == data.Email))
                return BadRequest("Email already registered.");

            if (_context.PendingRegistrations.Any(p => p.Email == data.Email))
                _context.PendingRegistrations.RemoveRange(_context.PendingRegistrations.Where(p => p.Email == data.Email));

            var code = new Random().Next(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            var pending = new PendingRegistration
            {
                Email = data.Email,
                Username = data.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(data.Password),
                Code = code,
                ExpiresAt = expiresAt
            };

            _context.PendingRegistrations.Add(pending);
            await _context.SaveChangesAsync();

            await emailService.SendVerificationCodeAsync(data.Email, data.Username, code, pending.Id);
            _logger.LogInformation("Envoi du mail de code à {Email}", data.Email);

            return Ok("Verification code sent to email.");
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] EmailCodeVerificationDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            var entry = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == input.Email);

            if (entry == null)
                return NotFound(new { message = "No verification request found for this email." });

            if (entry.Code != input.Code)
                return BadRequest(new { message = "Invalid verification code." });

            if (DateTime.UtcNow > entry.ExpiresAt)
                return BadRequest(new { message = "Verification code expired." });


            if (_context.Users.Any(u => u.Email == entry.Email))
                return BadRequest(new { message = "Email already registered." });

            var user = new User
            {
                Username = entry.Username,
                Email = entry.Email,
                PasswordHash = entry.PasswordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.PendingRegistrations.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully." });
        }

        /// <summary>
        /// Retrieve the email of a user by ID. Used for email confirmation flow.
        /// </summary>
        [HttpGet("user/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserEmailById(Guid id)
        {
            var user = await _context.PendingRegistrations
                .Where(p => p.Id == id)
                .Select(p => new { p.Email })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("No pending registration found for this ID.");

            return Ok(user); // returns: { "email": "..." }
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
            // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");
            
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            
            if (user == null)
                return Unauthorized("User not found.");
            
            // Mask sensitive information
            user.PasswordHash = string.Empty;
            
            return Ok(user);
        }

        /// <summary>
        /// Infers the correct MIME type for image file responses.
        /// </summary>
        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    
        /// <summary>
        /// Validates the uploaded file to ensure it meets the required criteria.
        /// </summary>
        /// <param name="file">The file to validate.</param>
        /// <returns>True if the file is valid; otherwise, false.</returns>
        private bool IsValidFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return false;

            return true;
        }
    }
}
