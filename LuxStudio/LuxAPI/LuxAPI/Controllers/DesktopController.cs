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

        /// <summary>
        /// Initializes the SystemController with a logger instance.
        /// </summary>
        /// <param name="logger">Logger service for tracking system events.</param>
        public DesktopController(ILogger<SystemController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Endpoint to retrieve the current API version.
        /// </summary>
        /// <returns>JSON response containing the API version.</returns>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new { Version = "1.0.0" });
        }
    }
}
