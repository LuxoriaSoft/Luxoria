using LuxStudio.COM;
using LuxStudio.COM.Services;
using System.Diagnostics;

AuthService authSvc = new AuthService();

await authSvc.StartLoginFlowAsync();

Debug.WriteLine(authSvc.AuthorizationCode);