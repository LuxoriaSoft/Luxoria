using LuxStudio.COM.Auth;
using LuxStudio.COM.Models;
using LuxStudio.COM.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LuxStudio.Components
{
    public sealed partial class AccManagementView : Page
    {
        private AuthManager? _authMgr;
        private LuxStudioConfig? _config;

        public AccManagementView(ref AuthManager? authMgr)
        {
            this.InitializeComponent();
            _authMgr = authMgr;
            this.Loaded += AccManagementView_Loaded;
        }

        private async void AccManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_authMgr != null)
            {
                await _authMgr.GetAccessTokenAsync();
                ShowPanel(UrlInputPanel, "You are already authenticated. You can now access LuxStudio.");
                return;
            }
            ShowPanel(UrlInputPanel);
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string url = StudioUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                ShowPanel(LoadingMessagePanel, "Please enter a valid LuxStudio URL.");
                return;
            }

            try
            {
                // Establish Connection
                ShowPanel(LoadingMessagePanel, "Establishing Connection...");
                await Task.Delay(500);

                ConfigService configService = new(url);
                ShowPanel(LoadingMessagePanel, "Connection Established!");
                await Task.Delay(500);

                // Retrieving Configuration with 15s timeout
                ShowPanel(LoadingMessagePanel, "Retrieving Configuration...");
                var configTask = configService.GetConfigAsync();
                var timeoutTask = Task.Delay(10000);


                var completed = await Task.WhenAny(configTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    ShowPanel(LoadingMessagePanel, "Configuration retrieval timed out.");
                    return;
                }

                // Only await the configTask now that we know it completed
                _config = await configTask;

                if (_config == null)
                {
                    ShowPanel(LoadingMessagePanel, "Failed to load configuration.");
                    return;
                }

                ShowPanel(LoadingMessagePanel, "Configuration Retrieved!");
                await Task.Delay(1000);

                // Processing Authentication
                ShowPanel(LoadingMessagePanel, "Processing Authentication...");
                _authMgr = new AuthManager(_config);

                await Task.Delay(500);

                try
                {
                    if (await _authMgr.GetAccessTokenAsync() != string.Empty)
                    {
                        ShowPanel(UrlInputPanel, "Authentication Successful! You can now access LuxStudio.");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        ShowPanel(LoadingMessagePanel, "Authentication failed. Please try again.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowPanel(LoadingMessagePanel, $"Authentication failed: {ex.Message}");
                    return;
                }

                // Transition
                ShowPanel(null);
            }
            catch (Exception ex)
            {
                ShowPanel(LoadingMessagePanel, $"Error: {ex.Message}");

                // Add a Start Over button if not already added
                if (!LoadingMessagePanel.Children.OfType<Button>().Any(b => (string?)b.Content == "Start Over"))
                {
                    Button startOverButton = new()
                    {
                        Content = "Start Over",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(10)
                    };
                    startOverButton.Click += (s, args) =>
                    {
                        ShowPanel(UrlInputPanel);
                        StudioUrlTextBox.Text = string.Empty;
                    };
                    LoadingMessagePanel.Children.Add(startOverButton);
                }

                LoadingMessagePanel.Visibility = Visibility.Visible;
            }
        }


        private void ShowPanel(UIElement? panelToShow, string? message = null)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                LoadingMessagePanel.Visibility = Visibility.Collapsed;
                UrlInputPanel.Visibility = Visibility.Collapsed;

                if (panelToShow == LoadingMessagePanel && message != null)
                {
                    LoadingMessageText.Text = message;
                }

                if (panelToShow != null)
                {
                    panelToShow.Visibility = Visibility.Visible;
                }
            });
        }
    }
}
