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
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes the SystemController with a logger instance.
        /// </summary>
        /// <param name="logger">Logger service for tracking system events.</param>
        public SystemController(ILogger<SystemController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Endpoint to check the system health status.
        /// </summary>
        /// <returns>JSON response indicating that the system is operational.</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            bool isDbConnected = await _context.Database.CanConnectAsync();

            var status = new Dictionary<string, bool>
            {
                { "Database", isDbConnected },
                { "Cache", true }, // Placeholder for cache status
                { "MessageQueue", true } // Placeholder for message queue status
            };

            return Ok(new
            {
                Status = "OK",
                Components = status,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint to retrieve the current API version.
        /// </summary>
        /// <returns>JSON response containing the API version.</returns>
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok(new { Version = "1.0.0" });
        }
    }
}
