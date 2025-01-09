using Microsoft.AspNetCore.Mvc;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemController : ControllerBase
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { Status = "OK" });
    }

    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new { Version = "1.0.0" });
    }
}