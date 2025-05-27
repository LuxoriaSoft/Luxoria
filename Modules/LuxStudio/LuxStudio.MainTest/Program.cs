using LuxStudio.COM;
using LuxStudio.COM.Services;
using System;
using System.Diagnostics;

ConfigService configSvc = new("http://localhost:3000");

Console.WriteLine("Lux Studio URL: " + configSvc.GetFrontUrl());
Console.WriteLine("Lux Studio API URL: " + configSvc.GetApiUrl());

var config = configSvc.GetConfig();

AuthService authSvc = new(config);

await authSvc.StartLoginFlowAsync();

Debug.WriteLine(authSvc.AuthorizationCode);