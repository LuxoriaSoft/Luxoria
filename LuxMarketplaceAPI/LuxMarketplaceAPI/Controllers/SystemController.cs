using Microsoft.AspNetCore.Mvc;

namespace LuxMarketplaceAPI.Controllers;

/// <summary>
/// System Controller
/// Used to check the health of the system.
/// Contains :
/// - ping : Check if the system is alive.
/// - status : Check the status of the system.
/// - version : Check the version of the system.
/// </summary>
[ApiController]
[Route("[controller]")]
public class SystemController : ControllerBase
{
    /// <summary>
    /// Check if the system is alive.
    /// </summary>
    /// <returns>
    /// Returns a 200 OK response if the system is alive.
    /// </returns>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
    
    /// <summary>
    /// Check the status of the system.
    /// </summary>
    /// <returns>
    /// Returns a 200 OK response if the system is alive and running.
    /// </returns>
    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new
        {
            Status = "OK",
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