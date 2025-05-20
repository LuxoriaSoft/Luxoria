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

    public async Task<bool> StartLoginFlowAsync()
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add(_callbackUrl + "/");
        listener.Start();

        // Open browser to login page
        Process.Start(new ProcessStartInfo
        {
            FileName = _loginUrl,
            UseShellExecute = true
        });

        // Wait for the callback request
        var context = await listener.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        var query = HttpUtility.ParseQueryString(request.Url.Query);
        var token = query["token"]; // or "code", depending on your flow

        if (token != null)
        {
            AccessToken = token;
            // Optionally exchange code for tokens here if needed

            // Send a response to the browser
            var responseString = "<html><body>Login successful! You can close this window.</body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.Close();

            return true;
        }

        return false;
    }
}
