using System.Diagnostics;
using System.Net;
using System.Web;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;

namespace LuxStudio.COM.Services;

public class AuthService
{
    public string? AuthorizationCode { get; private set; }
    private readonly ILoggerService _logger = new LoggerService(LogLevel.Debug, new DebugLogTarget());
    private readonly string _section = "LuxCOM/Authentification";
    private readonly string _clientId = "ba258d95-aa1a-4d75-b0ea-669a9db1b4b2";
    private readonly string _redirectUri = "http://localhost:5001/callback";
    private readonly string _ssoBaseUrl = "https://studio.pluto.luxoria.bluepelicansoft.com/sso/authorize";
    private HttpListener? listener;

    /// <summary>
    /// Builds the authorization URL for the SSO login flow.
    /// </summary>
    /// <returns>Url to explored</returns>
    public string BuildAuthorizationUrl()
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
            Process.Start(new ProcessStartInfo
            {
                FileName = BuildAuthorizationUrl(),
                UseShellExecute = true
            });

            // Wait for the redirect (or timeout)
            var contextTask = listener.GetContextAsync();
            var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask != contextTask)
            {
                // Timeout
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
                AuthorizationCode = code;

                var responseString = "<html><body><h1>Login successful!</h1>You can close this window.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                listener.Stop();
                return true;
            }

            response.StatusCode = 400;
            response.Close();
            listener.Stop();
            return false;
        }
        catch (OperationCanceledException)
        {
            listener.Stop();
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex.Message}");
            listener.Stop();
            return false;
        }
    }

    /// <summary>
    /// Termines the listener and stops listening for incoming requests.
    /// </summary>
    public void StopLoginFlow()
    {
        try
        {
            listener?.Stop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping listener: {ex.Message}");
        }
    }
}
