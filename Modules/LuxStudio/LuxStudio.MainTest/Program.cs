using LuxStudio.COM;
using LuxStudio.COM.Services;

AuthService authSvc = new AuthService();

await authSvc.StartLoginFlowAsync();