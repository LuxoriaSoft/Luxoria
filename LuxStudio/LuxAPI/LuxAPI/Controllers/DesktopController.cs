using Microsoft.AspNetCore.Mvc;
using LuxAPI.DAL;

namespace LuxAPI.Controllers
{
    /// <summary>
    /// Controller responsible for system-related endpoints.
    /// Provides API status and version information.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DesktopController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the SystemController with a logger instance.
        /// </summary>
        /// <param name="logger">Logger service for tracking system events.</param>
        public DesktopController(ILogger<SystemController> logger, AppDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Endpoint to retrieve the current API version.
        /// </summary>
        /// <returns>JSON response containing the API version.</returns>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var defaultClient = _context.Clients.FirstOrDefault(c => c.IsDefault);
            if (defaultClient == null)
                return NotFound("No default client configured.");

            // Use IConfiguration instead of Environment.GetEnvironmentVariable
            string frontUrl = _configuration["URI:FrontEnd"]
                ?? throw new InvalidOperationException("Frontend URL is not set.");

            string apiUrl = _configuration["URI:Backend"]
                ?? throw new InvalidOperationException("Backend URL is not set.");

            var ssoAuthorizeUrl = $"{apiUrl}/sso/authorize";

            var config = new
            {
                version = "1.0.0",
                url = frontUrl,
                apiUrl = apiUrl,
                sso = new
                {
                    url = ssoAuthorizeUrl,
                    @params = new
                    {
                        applicationName = "Luxoria.App",
                        clientId = defaultClient.ClientId.ToString(),
                        redirectUrl = defaultClient.RedirectUri
                    }
                },
                updatedAt = DateTime.UtcNow.ToString("o"),
                project = "https://github.com/luxoriasoft/luxoria"
            };

            return Ok(config);
        }

    }
}
