using System.Diagnostics;
using System.Net;
using System.Web;

namespace LuxStudio.COM.Services;

public class AuthService
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    private readonly string _loginUrl = "http://localhost:5000/login";
    private readonly string _callbackUrl = "http://localhost:5001/callback";

    public AuthService(string loginUrl = "https://studio.pluto.luxoria.bluepelicansoft.com/auth/sso")
    {
        _loginUrl = loginUrl;
    }

    public async Task<bool> StartLoginFlowAsync(int timeoutInSeconds = 120)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add(_callbackUrl + "/");
        listener.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
        try
        {
            // Open browser to login page
            Process.Start(new ProcessStartInfo
            {
                FileName = _loginUrl,
                UseShellExecute = true
            });

            // Wait for the callback request with cancellation
            var contextTask = listener.GetContextAsync();
            var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask != contextTask)
            {
                // Timeout occurred
                listener.Stop();
                return false;
            }

            var context = await contextTask;
            var request = context.Request;
            var response = context.Response;

            var query = HttpUtility.ParseQueryString(request.Url.Query);
            var token = query["token"]; // or "code", depending on your flow

            if (token != null)
            {
                AccessToken = token;

                var responseString = "<html><body>Login successful! You can close this window.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.Close();

                return true;
            }

            response.StatusCode = 400;
            response.Close();
            return false;
        }
        catch (OperationCanceledException)
        {
            listener.Stop(); // Clean up listener
            return false;     // Timeout or user closed the browser without completing
        }
    }
}
