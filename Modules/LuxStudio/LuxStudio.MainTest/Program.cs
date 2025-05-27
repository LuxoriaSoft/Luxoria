using LuxStudio.COM.Auth;
using LuxStudio.COM.Services;
using System.Diagnostics;

ConfigService configSvc = new("https://studio.pluto.luxoria.bluepelicansoft.com");

Console.WriteLine("Lux Studio URL: " + configSvc.GetFrontUrl());
Console.WriteLine("Lux Studio API URL: " + configSvc.GetApiUrl());

var config = configSvc.GetConfig();

/*
 * MANUAL AUTHENTICATION FLOW

        AuthService authSvc = new(config);

        bool status = await authSvc.StartLoginFlowAsync(300); // 5min timeout
        if (!status)
        {
            Console.WriteLine("Login flow failed or timed out.");
            return;
        }

        Debug.WriteLine(authSvc.AuthorizationCode);

        (string AccessToken, string RefreshToken) value = await authSvc.ExchangeAuthorizationCode(authSvc.AuthorizationCode ?? "");

        Debug.WriteLine(value.AccessToken);
        Debug.WriteLine(value.RefreshToken);

        await authSvc.RefreshAccessToken(value.RefreshToken);

*/

AuthManager authManager = new(config);

Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());

string token = await authManager.GetAccessTokenAsync();

Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
Debug.WriteLine("Access Token: " + token);

string token2 = await authManager.GetAccessTokenAsync();

Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
Debug.WriteLine("Access Token (second call): " + token2);

string token3 = await authManager.GetAccessTokenAsync();

Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
Debug.WriteLine("Access Token (third call): " + token3);
