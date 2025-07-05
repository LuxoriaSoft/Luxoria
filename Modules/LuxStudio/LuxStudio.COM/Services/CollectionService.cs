using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using LuxStudio.COM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LuxStudio.COM.Services;

public class CollectionService
{
    private readonly string _apiBaseUrl;
    private readonly IEventBus _eventBus;
    private readonly string _fronUrl;

    public CollectionService(LuxStudioConfig config, IEventBus eventBus)
    {
        //_clientId = config?.Sso?.Params?.ClientId ?? throw new NullReferenceException();
        _apiBaseUrl = config?.ApiUrl ?? throw new NullReferenceException();
        _eventBus = eventBus;
        _fronUrl = config?.Url ?? throw new NullReferenceException();
        Debug.WriteLine("_fronUrl: " + _fronUrl);
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

    public class UploadResponse
    {
        [property: JsonPropertyName("id")]
        public Guid Id { get; set; }
        [property: JsonPropertyName("collectionId")]
        public Guid CollectionId { get; set; }
        [property: JsonPropertyName("filePath")]
        public string FilePath { get; set; }
        [property: JsonPropertyName("status")]
        public int Status { get; set; }
        [property: JsonPropertyName("comments")]
        public List<object> Comments { get; set; }
    }


    public async Task<bool> UploadAssetAsync(Guid assetId, string accessToken, Guid collectionId, string fileName, StreamContent stream, Guid? overrideId = null)
    {
        Debug.WriteLine("Fetching user information...");

        var requestUri = $"{_apiBaseUrl}/api/collection/{collectionId}/upload";

        Debug.WriteLine("Request URI: " + requestUri);
        Debug.WriteLine("Token: " + accessToken);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var form = new MultipartFormDataContent();

        Debug.WriteLine("Using stream!");

        form.Add(stream, "file", fileName);
        if (overrideId != null)
            form.Add(new StringContent(overrideId.ToString() ?? ""), "photoId");

        Debug.WriteLine("Preparing for upload... with body: " + form.ToString());
        try
        {
            var response = await httpClient.PostAsync(requestUri, form);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Response JSON: " + jsonString);

                var json = JsonSerializer.Deserialize<UploadResponse>(jsonString);

                Debug.WriteLine("Upload successful! Server returned:");
                Debug.WriteLine(json);

                await _eventBus.Publish(new SaveLastUpdatedIdEvent(
                    _fronUrl,
                    json.Id,
                    collectionId,
                    assetId
                ));
                return true;
            }
            else
            {
                Debug.WriteLine($"Error: {response.StatusCode}");
                var errorText = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(errorText);
                return false;
            }
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during fetching user information: {ex.Message}");
            throw;
        }
    }

    public async Task<LuxCollection?> CreateCollectionAsync(string accessToken, string name, string description, ICollection<string> allowedEmails)
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
            Debug.WriteLine($"[CreateCollection] Error: {response.StatusCode}");
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            return null;
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        try
        {
            var collection = JsonSerializer.Deserialize<LuxCollection>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Debug.WriteLine("[CreateCollection] Success:");
            Debug.WriteLine(responseJson);

            return collection;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreateCollection] Failed to deserialize response: {ex.Message}");
            return null;
        }
    }


    public async Task<bool> DeleteCollectionAsync(string accessToken, Guid collectionId)
    {
        var requestUri = $"{_apiBaseUrl}/api/collection/{collectionId}";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error: {response.StatusCode}");
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            return false;
        }
        Debug.WriteLine("Delete collection successful! Server returned:");
        Debug.WriteLine(await response.Content.ReadAsStringAsync());
        return true;
    }

    public class UpdateCollectionDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? AllowedEmails { get; set; }
    }

    public async Task<bool> UpdateCollectionAsync(string accessToken, Guid collectionId, UpdateCollectionDto updateDto)
    {
        var requestUri = $"{_apiBaseUrl}/api/collection/{collectionId}";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(requestUri, content);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error: {response.StatusCode}");
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            return false;
        }
        Debug.WriteLine("Update collection successful! Server returned:");
        Debug.WriteLine(await response.Content.ReadAsStringAsync());
        return true;
    }
}
