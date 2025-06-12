using System.Diagnostics;
using System.Net;
using System.Web;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using LuxStudio.COM.Models;

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
    private readonly string _ssoBaseUrl;
    private HttpListener? listener;

    /// <summary>
    /// Consturctor for the AuthService.
    /// </summary>
    public AuthService(LuxStudioConfig config)
    {
        _clientId = config?.Sso?.Params?.ClientId ?? throw new NullReferenceException();
        _redirectUri = config?.Sso?.Params?.RedirectUrl ?? throw new NullReferenceException();
        _ssoBaseUrl = config?.Sso?.Url ?? throw new NullReferenceException();
    }

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
}
