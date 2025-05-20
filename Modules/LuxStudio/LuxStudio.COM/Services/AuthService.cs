using System.Diagnostics;
using System.Net;
using System.Web;

namespace LuxStudio.COM.Services;

public class AuthService
{
    public string? AuthorizationCode { get; private set; }

    private readonly string _clientId = "ba258d95-aa1a-4d75-b0ea-669a9db1b4b2";
    private readonly string _redirectUri = "http://localhost:5001/callback";
    private readonly string _ssoBaseUrl = "http://129.12.234.243:3000/sso/authorize";

    public string BuildAuthorizationUrl()
    {
        var encodedRedirectUri = HttpUtility.UrlEncode(_redirectUri);
        return $"{_ssoBaseUrl}?clientId={_clientId}&responseType=code&redirectUri={encodedRedirectUri}";
    }

    public async Task<bool> StartLoginFlowAsync(int timeoutInSeconds = 120)
    {
        using var listener = new HttpListener();
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

            var query = HttpUtility.ParseQueryString(request.Url.Query);
            var code = query["code"]; // SSO returns ?code=...

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
}
