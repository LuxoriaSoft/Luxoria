using Microsoft.AspNetCore.Mvc;

namespace LuxAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return Ok(new { token = "xxx" });
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return Ok("Register");
    }
}
