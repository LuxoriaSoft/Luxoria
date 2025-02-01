using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Initializes the SystemController with a logger instance.
        /// </summary>
        /// <param name="logger">Logger service for tracking system events.</param>
        public SystemController(ILogger<SystemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to check the system health status.
        /// </summary>
        /// <returns>JSON response indicating that the system is operational.</returns>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { Status = "OK" });
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
