using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LuxStudio.COM.Models;

public class LuxStudioConfig
{
    public string? Version { get; set; }
    public string? Url { get; set; }
    public string? ApiUrl { get; set; }
    public SsoConfig? Sso { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Project { get; set; }

    public class SsoConfig
    {
        public string? Url { get; set; }
        public SsoParams? Params { get; set; }

        public class SsoParams
        {
            public string? ApplicationName { get; set; }
            public string? ClientId { get; set; }
            public string? RedirectUrl { get; set; }
        }
    }

    public static async Task<LuxStudioConfig?> FetchFromUrlAsync(string url)
    {
        using HttpClient client = new HttpClient();

        try
        {
            var response = await client.GetStringAsync(url);

            return JsonSerializer.Deserialize<LuxStudioConfig>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP error while fetching config: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON error while parsing config: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        return null;
    }
}