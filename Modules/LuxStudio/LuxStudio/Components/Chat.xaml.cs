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

            ChatURLUpdated += async (Uri url, AuthManager authManager) => {
                _authManager = authManager;
                WebView2 ChatWebView = new WebView2
                {
                    Source = url,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                WebViewHote.Children.Clear();
                WebViewHote.Children.Add(ChatWebView);
                string accessToken = await _authManager.GetAccessTokenAsync();
                Debug.WriteLine($"Chat URL updated: {url}, Token: {accessToken}");
                _lastUrl = url;
            };

            Loaded += async (s, e) =>
            {
                if (_lastUrl != null && _authManager != null)
                {
                    Debug.WriteLine($"Loading last URL: {_lastUrl}");
                    WebView2 ChatWebView = new WebView2
                    {
                        Source = _lastUrl,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    WebViewHote.Children.Clear();
                    WebViewHote.Children.Add(ChatWebView);
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

                    WebViewHote.Children.Clear();
                    WebViewHote.Children.Add(textBlock);
                }
            };

            //ChatWebView.NavigationCompleted += async (s, e) =>
            //{
            //    Debug.WriteLine($"Navigation completed: {_accessToken}");
            //    await ChatWebView.CoreWebView2.ExecuteScriptAsync(
            //        $@"document.cookie = 'token={_accessToken};
            //            path=/';
            //            localStorage.setItem('token', '{_accessToken}');"
            //    );
            //};
        }           
    }
}
