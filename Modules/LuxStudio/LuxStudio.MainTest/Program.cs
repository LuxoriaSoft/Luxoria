using LuxStudio.COM;
using LuxStudio.COM.Services;
using System;
using System.Diagnostics;

ConfigService configSvc = new("https://studio.pluto.luxoria.bluepelicansoft.com/");

Console.WriteLine("Lux Studio URL: " + configSvc.GetFrontUrl());
Console.WriteLine("Lux Studio API URL: " + configSvc.GetApiUrl());
//AuthService authSvc = new();

//await authSvc.StartLoginFlowAsync();

//Debug.WriteLine(authSvc.AuthorizationCode);