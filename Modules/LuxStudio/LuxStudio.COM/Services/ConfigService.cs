using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;

namespace LuxStudio.COM.Services;

/// <summary>
/// Service for managing configuration settings.
/// Retreives and provides access to configuration values.
/// </summary>
public class ConfigService
{
    /// <summary>
    /// Logger for logging debug and error messages.
    /// </summary>
    private readonly ILoggerService _logger = new LoggerService(LogLevel.Info, new DebugLogTarget());
    private readonly string _section = "LuxCOM/Configuration";
    
    /// <summary>
    /// The URL of the Lux Studio configuration endpoint.
    /// </summary>
    private readonly string _luxStudioUrl;

    /// <summary>
    /// Lux Studio API URL.
    /// </summary>
    public readonly string _luxStudioApiUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// Sets the Lux Studio URL for configuration retrieval.
    /// The URL must not be null.
    /// </summary>
    /// <param name="luxStudioURL">Url to LuxStudio (should include http / https) (e.g https://studio.example.com)</param>
    /// <exception cref="ArgumentNullException">Throws if LuxStudioURL is null</exception>
    /// <exception cref="ArgumentException">Throws if the LuxStudioURL do NOT mach an URL format</exception>
    public ConfigService(string luxStudioURL)
    {
        _luxStudioUrl = luxStudioURL ?? throw new ArgumentNullException(nameof(luxStudioURL), "Lux Studio URL cannot be null.");

        // Check if the URL is valid
        if (!Uri.IsWellFormedUriString(_luxStudioUrl, UriKind.Absolute))
        {
            throw new ArgumentException("The provided Lux Studio URL is not valid.", nameof(luxStudioURL));
        }

        // Check if the URL is reachable
        var reached = IsReachableAsync().GetAwaiter().GetResult();
        if (!reached)
        {
            throw new InvalidOperationException("The provided Lux Studio URL is not reachable. Please check the URL and your network connection.");
        }

        // Extract the API URL from the configuration file
        _luxStudioApiUrl = GetApiUrlAsync().GetAwaiter().GetResult();
        if (string.IsNullOrEmpty(_luxStudioApiUrl))
        {
            throw new InvalidOperationException("Failed to retrieve API URL from the Lux Studio configuration.");
        }
    }

    /// <summary>
    /// Basic curl request for checking if LuxStudio is reachable.
    /// </summary>
    private async Task<bool> IsReachableAsync()
    {
        await _logger.LogAsync("Checking if Lux Studio is reachable...", _section, LogLevel.Info);
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(_luxStudioUrl);
            await _logger.LogAsync($"Lux Studio reachability check completed. (status={response.IsSuccessStatusCode})", _section, LogLevel.Info);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Error checking Lux Studio reachability: {ex.Message}", _section, LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// Gets '/config.js' from Lux Studio URL and extracts the API_URL variable.
    /// window.appConfig = {
    ///    API_URL: 'https://api.studio.pluto.luxoria.bluepelicansoft.com'  // This will be replaced at runtime
    /// };
    /// </summary>
    private async Task<string> GetApiUrlAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(new Uri(new Uri(_luxStudioUrl), "/config.js"));

            // Extract the API_URL from the config.js content
            var apiUrlMatch = System.Text.RegularExpressions.Regex.Match(response, @"API_URL:\s*'([^']+)'");
            if (apiUrlMatch.Success && apiUrlMatch.Groups.Count > 1)
            {
                return apiUrlMatch.Groups[1].Value;
            }
            else
            {
                throw new InvalidOperationException("API_URL not found in the configuration file.");
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Error retrieving API URL: {ex.Message}", _section, LogLevel.Error);
            throw;
        }
    }
    
    /// <summary>
    /// Get the Lux Studio URL
    /// </summary>
    /// <returns>Lux Studio URL as a string</returns>
    public string GetFrontUrl() => _luxStudioUrl;
    
    /// <summary>
    /// Get the Lux Studio API URL
    /// </summary>
    /// <returns>LuxAPI as a string</returns>
    public string GetApiUrl() => _luxStudioApiUrl;
}