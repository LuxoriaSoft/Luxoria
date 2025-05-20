using LuxStudio.COM;
using LuxStudio.COM.Services;
using System.Diagnostics;

AuthService authSvc = new();

await authSvc.StartLoginFlowAsync();

Debug.WriteLine(authSvc.AuthorizationCode);