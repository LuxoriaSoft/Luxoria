//using LuxStudio.COM.Auth;
//using LuxStudio.COM.Models;
//using LuxStudio.COM.Services;
//using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
//using System.Diagnostics;
//using System.Net.Http.Headers;

//ConfigService configSvc = new("https://studio.pluto.luxoria.bluepelicansoft.com");

//Console.WriteLine("Lux Studio URL: " + configSvc.GetFrontUrl());
//Console.WriteLine("Lux Studio API URL: " + await configSvc.GetApiUrlAsync());

//var config = await configSvc.GetConfigAsync();

// * MANUAL AUTHENTICATION FLOW

//        AuthService authSvc = new(config);

//        bool status = await authSvc.StartLoginFlowAsync(300); // 5min timeout
//        if (!status)
//        {
//            Console.WriteLine("Login flow failed or timed out.");
//            return;
//        }

//        Debug.WriteLine(authSvc.AuthorizationCode);

//        (string AccessToken, string RefreshToken) value = await authSvc.ExchangeAuthorizationCode(authSvc.AuthorizationCode ?? "");

//        Debug.WriteLine(value.AccessToken);
//        Debug.WriteLine(value.RefreshToken);

//        await authSvc.RefreshAccessToken(value.RefreshToken);

//*/

//AuthManager authManager = new(config ?? throw new InvalidOperationException("Configuration cannot be null. Ensure the config service is properly initialized."));

//Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());

//string token = await authManager.GetAccessTokenAsync();

//Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
//Debug.WriteLine("Access Token: " + token);

//string token2 = await authManager.GetAccessTokenAsync();

//Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
//Debug.WriteLine("Access Token (second call): " + token2);

//string token3 = await authManager.GetAccessTokenAsync();

//Debug.WriteLine("Is Authenticated: " + authManager.IsAuthenticated());
//Debug.WriteLine("Access Token (third call): " + token3);

//Debug.WriteLine(await authManager.GetUserInfoAsync());

////var cs = new CollectionService(config ?? throw new InvalidOperationException("Configuration cannot be null. Ensure the config service is properly initialized."), );
//ICollection<LuxCollection> collections = await cs.GetAllAsync(token);

//Debug.WriteLine("Collections Count: " + collections.Count);


//// Upload
//StreamContent CreateStreamContent(string filePath,
//                                                string contentType = "application/octet-stream")
//{
//    ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

//    if (!File.Exists(filePath))
//        throw new FileNotFoundException("File not found.", filePath);

//    var stream = new FileStream(
//        path: filePath,
//        mode: FileMode.Open,
//        access: FileAccess.Read,
//        share: FileShare.Read,
//        bufferSize: 4096,
//        useAsync: true);

//    var content = new StreamContent(stream);
//    content.Headers.ContentType =
//        new MediaTypeHeaderValue(contentType);

//    content.Headers.ContentDisposition =
//        new ContentDispositionHeaderValue("form-data")
//        {
//            Name = "\"file\"",
//            FileName = $"\"{Path.GetFileName(filePath)}\""
//        };

//    return content;
//}


////StreamContent streamContent = CreateStreamContent("filepath to image", "image/jpg");

////await cs.UploadAssetAsync(token, new ("0197bad5-af3f-7e79-bb97-3d1513d2debf"), "hazy.jpg", streamContent);

//await cs.CreateCollectionAsync(token, "superCollection", "descriptionDeFou", ["a.a@a.a"]);

using System.Diagnostics;

///*
Debug.WriteLine("Lux Studio Main Test started");