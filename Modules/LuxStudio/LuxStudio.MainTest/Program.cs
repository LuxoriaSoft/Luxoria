using LuxStudio.COM.Services;
using System.Diagnostics;

ConfigService configSvc = new("https://studio.pluto.luxoria.bluepelicansoft.com");

Console.WriteLine("Lux Studio URL: " + configSvc.GetFrontUrl());
Console.WriteLine("Lux Studio API URL: " + configSvc.GetApiUrl());

var config = configSvc.GetConfig();

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
