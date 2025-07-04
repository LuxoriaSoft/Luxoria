using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using LuxStudio.COM.Models;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace LuxStudio.COM.Services;

/// <summary>
/// Service for handling authentication via Single Sign-On (SSO).
/// </summary>
public class AuthService
{
    public string? AuthorizationCode { get; private set; }
    private readonly ILoggerService _logger = new LoggerService(LogLevel.Info, new DebugLogTarget());
    private readonly string _section = "LuxCOM/Authentification";
    private readonly string _clientId;
    private readonly string _redirectUri;
    private readonly string _apiBaseUrl;
    private readonly string _ssoBaseUrl;
    private HttpListener? listener;

    /// <summary>
    /// Consturctor for the AuthService.
    /// </summary>
    public AuthService(LuxStudioConfig config)
    {
        _clientId = config?.Sso?.Params?.ClientId ?? throw new NullReferenceException();
        _apiBaseUrl = config?.ApiUrl ?? throw new NullReferenceException();
        _redirectUri = config?.Sso?.Params?.RedirectUrl ?? throw new NullReferenceException();
        _ssoBaseUrl = config?.Sso?.Url ?? throw new NullReferenceException();
    }

    public static int AccessTokenExpirationDelay => 3600; // 1-hour expiration delay
    public static int RefreshTokenExpirationDelay => 1296000; // 15 days expiration delay

    /// <summary>
    /// Builds the authorization URL for the SSO login flow.
    /// </summary>
    /// <returns>Url to explored</returns>
    private string BuildAuthorizationUrl()
    {
        var encodedRedirectUri = HttpUtility.UrlEncode(_redirectUri);
        return $"{_ssoBaseUrl}?clientId={_clientId}&responseType=code&redirectUri={encodedRedirectUri}";
    }

    /// <summary>
    /// Creates a new HttpListener and starts listening for incoming requests.
    /// </summary>
    /// <param name="timeoutInSeconds">Timeout before killing listening</param>
    /// <returns>True if authentified, otherwise False</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> StartLoginFlowAsync(int timeoutInSeconds = 120)
    {
        _logger.Log("Starting SSO login processus...", _section, LogLevel.Info);
        _logger.Log($"Redirect URI: {_redirectUri}", _section, LogLevel.Debug);
        listener = new HttpListener();
        listener.Prefixes.Add(_redirectUri + "/");
        listener.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));

        try
        {
            // Open browser with SSO login URL
            _logger.Log("Opening browser for SSO login...", _section, LogLevel.Info);
            Process.Start(new ProcessStartInfo
            {
                FileName = BuildAuthorizationUrl(),
                UseShellExecute = true
            });

            // Wait for the redirect (or timeout)
            _logger.Log("Waiting for SSO response...", _section, LogLevel.Info);
            _logger.Log($"Timeout set to {timeoutInSeconds} seconds.", _section, LogLevel.Debug);
            var contextTask = listener.GetContextAsync();
            var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask != contextTask)
            {
                // Timeout
                _logger.Log("SSO login timed out.", _section, LogLevel.Warning);
                listener.Stop();
                return false;
            }

            var context = await contextTask;
            var request = context.Request;
            var response = context.Response;

            var query = HttpUtility.ParseQueryString(request?.Url?.Query ?? throw new ArgumentNullException());
            var code = query["code"];

            if (!string.IsNullOrWhiteSpace(code))
            {
                _logger.Log("SSO login successful, received authorization code.", _section, LogLevel.Info);
                AuthorizationCode = code;

                var responseString = "<html><body><h1>Login successful!</h1>You can close this window.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                listener.Stop();
                return true;
            }
            else
            {
                _logger.Log("SSO login failed, no authorization code received.", _section, LogLevel.Error);
            }

            response.StatusCode = 400;
            response.Close();
            listener.Stop();
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.Log("SSO login process was cancelled due to timeout.", _section, LogLevel.Warning);
            listener.Stop();
            return false;
        }
        catch (Exception ex)
        {
            _logger.Log($"Unexpected error during SSO login: {ex.Message}", _section, LogLevel.Error);
            listener.Stop();
            return false;
        }
    }

    /// <summary>
    /// Termines the listener and stops listening for incoming requests.
    /// </summary>
    public void StopLoginFlow()
    {
        _logger.Log("Stopping SSO login flow...", _section, LogLevel.Info);
        try
        {
            _logger.Log("Stopping HttpListener...", _section, LogLevel.Debug);
            listener?.Stop();
        }
        catch (Exception ex)
        {
            _logger.Log($"Error stopping HttpListener: {ex.Message}", _section, LogLevel.Error);
        }
    }

    /// <summary>
    /// Exchanges the authorization code for an access token and refresh token.
    /// </summary>
    /// <param name="authorizationCode">The authorization code to exchange.</param>
    /// <returns>A tuple containing the access token and refresh token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token exchange fails or response is invalid.</exception>
    public async Task<(string AccessToken, string RefreshToken)> ExchangeAuthorizationCode(string authorizationCode)
    {
        _logger.Log("Exchanging authorization code for access token...", _section, LogLevel.Info);

        var requestBody = new
        {
            clientId = _clientId,
            code = authorizationCode,
            grantType = "authorization_code"
        };

        var requestUri = $"{_apiBaseUrl}/sso/token";

        try
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to exchange authorization code: {response.StatusCode}";
                _logger.Log(errorMsg, _section, LogLevel.Error);
                throw new InvalidOperationException(errorMsg);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken) || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                _logger.Log("Invalid response format from token exchange.", _section, LogLevel.Error);
                throw new InvalidOperationException("Invalid response format from token exchange.");
            }

            _logger.Log("Authorization code exchanged successfully.", _section, LogLevel.Info);
            return (tokenResponse.AccessToken, tokenResponse.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.Log($"Exception during token exchange: {ex.Message}", _section, LogLevel.Error);
            throw;
        }
    }

    /// <summary>
    /// Refreshes the access token using the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for getting a new access token.</param>
    /// <returns>A tuple containing the new access token and refresh token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the refresh fails or response is invalid.</exception>
    public async Task<(string AccessToken, string RefreshToken)> RefreshAccessToken(string refreshToken)
    {
        _logger.Log("Refreshing access token...", _section, LogLevel.Info);

        var requestBody = new
        {
            refreshToken
        };

        var requestUri = $"{_apiBaseUrl}/sso/refresh";

        try
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to refresh access token: {response.StatusCode}";
                _logger.Log(errorMsg, _section, LogLevel.Error);
                throw new InvalidOperationException(errorMsg);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent);

            if (tokenResponse == null ||
                string.IsNullOrWhiteSpace(tokenResponse.AccessToken) ||
                string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                _logger.Log("Invalid response format from token refresh.", _section, LogLevel.Error);
                throw new InvalidOperationException("Invalid response format from token refresh.");
            }

            _logger.Log("Access token refreshed successfully.", _section, LogLevel.Info);
            return (tokenResponse.AccessToken, tokenResponse.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.Log($"Exception during access token refresh: {ex.Message}", _section, LogLevel.Error);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user information associated with the provided access token.
    /// </summary>
    /// <param name="accessToken">Access Token (Bearer)</param>
    /// <returns>User information as UserInfo</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<UserInfo> WhoAmIAsync(string accessToken)
    {
        /*
         * Expected response:
         * {
         *   "id": "USERGUID",
         *   "username": "USERNAME",
         *   "email": "EMAIL",
         *   "passwordHash": "ALWAYS EMPTY",
         *   "avatarFileName": null,
         *   "createdAt": "2025-06-03T11:59:25.778395Z",
         *   "updatedAt": "2025-06-03T11:59:25.778395Z"
         * }
         */
        _logger.Log("Fetching user information...", _section, LogLevel.Info);

        var requestUri = $"{_apiBaseUrl}/api/auth/whoami";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Failed to fetch user information: {response.StatusCode}";
                _logger.Log(errorMsg, _section, LogLevel.Error);
                throw new InvalidOperationException(errorMsg);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return userInfo ?? throw new InvalidOperationException("Invalid response format from user information fetch.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Exception during fetching user information: {ex.Message}", _section, LogLevel.Error);
            throw;
        }
    }
}
