using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using LuxStudio.COM.Models;
using System.Diagnostics;
using System.Text.Json;

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
    private string _luxStudioUrl;

    /// <summary>
    /// Lux Studio API URL.
    /// </summary>
    private string? _luxStudioApiUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// Sets the Lux Studio URL for configuration retrieval.
    /// The URL must not be null.
    /// </summary>
    public ConfigService(string luxStudioUrl)
    {
        _luxStudioUrl = luxStudioUrl ?? throw new ArgumentNullException(nameof(luxStudioUrl), "Lux Studio URL cannot be null.");

        // Check if the URL is valid
        if (!Uri.IsWellFormedUriString(_luxStudioUrl, UriKind.Absolute))
        {
            throw new ArgumentException("The provided Lux Studio URL is not valid.", nameof(luxStudioUrl));
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
    /// LuxStudio < 2.0
    /// Gets '/config.js' from Lux Studio URL and extracts the API_URL variable.
    /// From the following content:
    /// ```json
    /// window.appConfig = {
    ///    API_URL: 'Url'  // This will be replaced at runtime
    /// };
    /// 
    /// LuxStudio >= 2.0
    /// Gets '/config/api' from Lux Studio URL and returns the API URL.
    /// {
    ///     "url": "https://api.studio.pluto.luxoria.bluepelicansoft.com/"
    /// }
    /// ```
    /// </summary>
    public async Task<string> GetApiUrlAsync(int luxVersion = 2)
    {
        try
        {
            using var httpClient = new HttpClient();
            if (luxVersion < 2)
            {
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

            var responseV2 = await httpClient.GetAsync(new Uri(new Uri(_luxStudioUrl), "/config/api"));

            if (!responseV2.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch config {responseV2.ReasonPhrase}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(await responseV2.Content.ReadAsStringAsync(), options);

            if (config != null)
            {
                foreach (var item in config)
                {
                    await _logger.LogAsync(
                        $"Config item: {item.Key} = {item.Value}",
                        _section,
                        LogLevel.Debug
                    );
                }
            }

            if (config != null && config.TryGetValue("url", out string? apiUrl))
            {
                return apiUrl;
            }
            else
            {
                throw new InvalidOperationException("API URL not found in the configuration response.");
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync($"Error retrieving API URL: {ex.Message}", _section, LogLevel.Error);
            throw;
        }
    }

    /// <summary>
    /// Fetch the Lux Studio configuration from the API URL.
    /// </summary>
    /// <returns>LuxStudioConfig model filled with the data fetched from LuxAPI</returns>
    private async Task<LuxStudioConfig> FetchConfigAsync()
    {
        bool isReachable = await IsReachableAsync();
        if (!isReachable)
        {
            await _logger.LogAsync("Lux Studio is not reachable. Cannot fetch configuration.", _section, LogLevel.Error);
            throw new InvalidOperationException("Lux Studio is not reachable. Please check the URL and your network connection.");
        }

        _luxStudioApiUrl = await GetApiUrlAsync();
        if (_luxStudioApiUrl == null) throw new InvalidOperationException("API URL could not be retrieved from the Lux Studio configuration.");

        string luxStudioConfigUrl = new Uri(new Uri(_luxStudioApiUrl), "/desktop/config").ToString();

        return await LuxStudioConfig.FetchFromUrlAsync(luxStudioConfigUrl) ?? throw new InvalidOperationException("Failed to fetch Lux Studio configuration. Please check the URL and your network connection.");
    }

    /// <summary>
    /// Get the Lux Studio URL
    /// </summary>
    /// <returns>Lux Studio URL as a string</returns>
    public string GetFrontUrl() => _luxStudioUrl;

    /// <summary>
    /// Get the Lux Studio configuration model.
    /// </summary>
    /// <returns>LuxStudioConfig model</returns>
    public async Task<LuxStudioConfig?> GetConfigAsync() => await FetchConfigAsync();
}