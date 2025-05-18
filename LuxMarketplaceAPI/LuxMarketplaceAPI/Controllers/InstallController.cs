using Microsoft.AspNetCore.Mvc;

namespace LuxMarketplaceAPI.Controllers;

/// <summary>
/// Handles the installation-related functionality within the system.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InstallController : ControllerBase
{
    /// <summary>
    /// Constructor of InstallController
    /// </summary>
    public InstallController()
    {
    }

    /// <summary>
    /// Get the executable of LDA (Luxoria Desktop Application).
    /// Parameters:
    /// - version: The version of the LDA to be downloaded.
    /// - platform: The platform of the LDA to be downloaded.
    /// </summary>
    /// <returns>
    /// A file result containing the executable of LDA.
    /// </returns>
    [HttpGet("desktop")]
    public IActionResult GetDesktopApp(string version, string platform)
    {
        // Validate platform (x64, x86, arm64)
        if (!ValidatePlatform(platform))
            return BadRequest("Invalid platform specified. Valid options are: x64, x86, arm64.");
        return Ok(version);
    }

    /// <summary>
    /// Returns the list of versions available for download for LDA
    /// </summary>
    [HttpGet("desktop/versions")]
    public IActionResult GetDesktopVersions(string platform)
    {
        // Validate platform (x64, x86, arm64)
        if (!ValidatePlatform(platform))
            return BadRequest("Invalid platform specified. Valid options are: x64, x86, arm64.");

        return Ok(new
        {
            platform = platform,
            versions = new List<string> { "1.0.0", "1.1.0", "1.2.0" },
            latestVersion = "1.2.0"
        });
    }
    
    /// <summary>
    /// Returns the list of versions available for download.
    /// Parameters:
    /// - packageUUID: The UUID of the package.
    /// - platform: The platform of the package.
    /// </summary>
    [HttpGet("versions")]
    public IActionResult GetVersions(Guid packageId, string platform)
    {
        // Validate platform (x64, x86, arm64)
        if (!ValidatePlatform(platform))
            return BadRequest("Invalid platform specified. Valid options are: x64, x86, arm64.");
        
        return Ok(new
        {
            packageId,
            platform,
            versions = new List<string> { "1.0.0", "1.1.0", "1.2.0" },
            latestVersion = "1.2.0"
        });
    }

    /// <summary>
    /// Validates whether the specified platform is one of the supported types: x64, x86, or arm64.
    /// </summary>
    /// <param name="platform">The platform to validate, expected to be one of the following: 'x64', 'x86', or 'arm64'.</param>
    /// <returns>
    /// A boolean value indicating whether the platform is valid. Returns true if the platform is valid; otherwise false.
    /// </returns>
    private bool ValidatePlatform(string platform)
    {
        return !string.IsNullOrEmpty(platform) && 
               (platform.Equals("x64", StringComparison.OrdinalIgnoreCase) || 
                platform.Equals("x86", StringComparison.OrdinalIgnoreCase) || 
                platform.Equals("arm64", StringComparison.OrdinalIgnoreCase));
    }


}