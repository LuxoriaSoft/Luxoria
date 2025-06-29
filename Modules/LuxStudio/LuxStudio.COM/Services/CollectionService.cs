using LuxStudio.COM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuxStudio.COM.Services;

public class CollectionService
{
    private readonly string _apiBaseUrl;

    public CollectionService(LuxStudioConfig config)
    {
        //_clientId = config?.Sso?.Params?.ClientId ?? throw new NullReferenceException();
        _apiBaseUrl = config?.ApiUrl ?? throw new NullReferenceException();
        //_redirectUri = config?.Sso?.Params?.RedirectUrl ?? throw new NullReferenceException();
        //_ssoBaseUrl = config?.Sso?.Url ?? throw new NullReferenceException();

    }

    public async Task<ICollection<LuxCollection>> GetAllAsync(string accessToken)
    {
        Debug.WriteLine("Fetching user information...");

        var requestUri = $"{_apiBaseUrl}/api/collection";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to fetch user information: {response.StatusCode}";
                Debug.WriteLine(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<ICollection<LuxCollection>>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return userInfo ?? throw new InvalidOperationException("Invalid response format from user information fetch.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during fetching user information: {ex.Message}");
            throw;
        }
    }

    public async Task<LuxCollection> GetAsync(string accessToken, Guid collectionId)
    {
        Debug.WriteLine("Fetching user information...");

        var requestUri = $"{_apiBaseUrl}/api/collection/{collectionId}";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to fetch user information: {response.StatusCode}";
                Debug.WriteLine(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<LuxCollection>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return userInfo ?? throw new InvalidOperationException("Invalid response format from user information fetch.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during fetching user information: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UploadAssetAsync(string accessToken, Guid collectionId, string fileName, StreamContent stream)
    {
        Debug.WriteLine("Fetching user information...");

        var requestUri = $"{_apiBaseUrl}/api/collection/{collectionId}/upload";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var form = new MultipartFormDataContent();
        form.Add(stream, "file", fileName);

        try
        {
            var response = await httpClient.PostAsync(requestUri, form);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Upload successful! Server returned:");
                Console.WriteLine(json);
                return true;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorText = await response.Content.ReadAsStringAsync();
                Console.WriteLine(errorText);
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during fetching user information: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> CreateCollectionAsync(string accessToken, string name, string description, ICollection<string> allowedEmails)
    {
        var requestUri = $"{_apiBaseUrl}/api/collection/create";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new
        {
            name,
            description,
            allowedEmails
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error: {response.StatusCode}");
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            return false;
        }

        Debug.WriteLine("Create collection successful! Server returned:");
        Debug.WriteLine(await response.Content.ReadAsStringAsync());
        return true;
    }
}
