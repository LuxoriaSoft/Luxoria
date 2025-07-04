using LuxStudio.COM.Auth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxStudio.Components
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chat : Page
    {
        public Action<Uri, AuthManager> ChatURLUpdated { get; set; }
        public Action NoCollectionSelected { get; set; }
        private AuthManager? _authManager;
        private Uri? _lastUrl;

        public Chat()
        {
            this.InitializeComponent();

            NoCollectionSelected += () =>
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "No collection selected. Please select a collection to start chatting.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 24
                };
                WebViewHote.Children.Clear();
                WebViewHote.Children.Add(textBlock);
                _authManager = null;
                _lastUrl = null;
            };

            ChatURLUpdated += async (Uri url, AuthManager authManager) =>
            {
                _authManager = authManager;
                string accessToken = await _authManager.GetAccessTokenAsync();
                Debug.WriteLine($"Chat URL updated: {url}, Token: {accessToken}");
                _lastUrl = url;

                WebView2 chatWebView = new WebView2
                {
                    Source = new Uri("about:blank"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                chatWebView.NavigationCompleted += async (s, e) =>
                {
                    if (chatWebView.CoreWebView2 != null)
                    {
                        string script = $@"
                            document.cookie = 'token={accessToken}; path=/;';
                            localStorage.setItem('token', '{accessToken}');
                        ";
                        await chatWebView.CoreWebView2.ExecuteScriptAsync(script);
                        Debug.WriteLine("Token injected into localStorage and cookies.");
                        if (chatWebView.Source != url)
                            chatWebView.Source = url;
                    }
                };

                WebViewHote.Children.Clear();
                WebViewHote.Children.Add(chatWebView);
            };


            Loaded += async (s, e) =>
            {
                WebViewHote.Children.Clear();
                if (_lastUrl != null && _authManager != null)
                {
                    string accessToken = await _authManager.GetAccessTokenAsync();
                    Debug.WriteLine($"Chat URL updated: {_lastUrl}, Token: {accessToken}");

                    WebView2 chatWebView = new WebView2
                    {
                        Source = new Uri("about:blank"),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    chatWebView.NavigationCompleted += async (s, e) =>
                    {
                        if (chatWebView.CoreWebView2 != null)
                        {
                            string script = $@"
                                document.cookie = 'token={accessToken}; path=/;';
                                localStorage.setItem('token', '{accessToken}');
                            ";
                            await chatWebView.CoreWebView2.ExecuteScriptAsync(script);
                            Debug.WriteLine("Token injected into localStorage and cookies.");
                            if (chatWebView.Source != _lastUrl)
                                chatWebView.Source = _lastUrl;
                        }
                    };

                    WebViewHote.Children.Add(chatWebView);
                }
                else
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = "No chat URL available. Please select a collection first.",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 24
                    };
                    WebViewHote.Children.Add(textBlock);
                }
            };
        }           
    }
}
