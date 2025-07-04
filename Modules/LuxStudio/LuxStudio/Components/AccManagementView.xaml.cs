using CommunityToolkit.WinUI;
using LuxStudio.COM.Auth;
using LuxStudio.COM.Models;
using LuxStudio.COM.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace LuxStudio.Components;

public sealed partial class AccManagementView : Page
{
    private AuthManager? _authMgr;
    private LuxStudioConfig? _config;
    public event Action<LuxStudioConfig, AuthManager>? OnAuthenticated;

    public AccManagementView(ref AuthManager? authMgr)
    {
        this.InitializeComponent();
        _authMgr = authMgr;
        this.Loaded += AccManagementView_Loaded;
    }

    private async void AccManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_authMgr != null && await _authMgr.GetAccessTokenAsync() != string.Empty)
        {
            await ShowMainContentPanelAsync();
        }
        else
        {
            ShowPanel(UrlInputPanel);
        }
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string url = StudioUrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            ShowPanel(LoadingMessagePanel, "Please enter a valid LuxStudio URL.");
            return;
        }

        if (!url.StartsWith("https"))
        {
            ShowPanel(LoadingMessagePanel, "Enabling Secure connection...");
            await Task.Delay(200);
            url = $"https://{url.TrimStart('/')}";
        }

        try
        {
            ShowPanel(LoadingMessagePanel, "Establishing Connection...");
            await Task.Delay(100);

            ConfigService configService = new(url);

            ShowPanel(LoadingMessagePanel, "Retrieving Configuration...");
            var configTask = configService.GetConfigAsync();
            var timeoutTask = Task.Delay(15000);

            var completed = await Task.WhenAny(configTask, timeoutTask);

            if (completed == timeoutTask)
            {
                ShowPanel(LoadingMessagePanel, "Configuration retrieval timed out.");
                return;
            }

            _config = await configTask;

            if (_config == null)
            {
                ShowPanel(LoadingMessagePanel, "Failed to load configuration.");
                return;
            }

            ShowPanel(LoadingMessagePanel, "Configuration Retrieved!");
            await Task.Delay(1000);

            ShowPanel(LoadingMessagePanel, "Processing Authentication...");
            _authMgr = new AuthManager(_config);

            await Task.Delay(500);

            await _authMgr.GetAccessTokenAsync();

            if (_authMgr.IsAuthenticated())
            {
                Debug.WriteLine("Authenticated");
                OnAuthenticated?.Invoke(_config, _authMgr);
                await ShowMainContentPanelAsync();
            }
            else
            {
                throw new InvalidOperationException("Authentication failed. Please check your credentials or the configuration.");
            }
        }
        catch (Exception ex)
        {
            ShowPanel(LoadingMessagePanel, $"Error: {ex.Message}");

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

    /// <summary>
    /// Fetches user info and shows MainContentPanel.
    /// </summary>
    private async Task ShowMainContentPanelAsync()
    {
        if (_authMgr == null)
        {
            ShowPanel(LoadingMessagePanel, "Authentication Manager is not initialized.");
            return;
        }

        try
        {
            UserInfo userInfo = await _authMgr.GetUserInfoAsync();

            await DispatcherQueue.EnqueueAsync(() =>
            {
                UIUsernameText.Text = userInfo.Username ?? "Unknown User";
                UIEmailText.Text = userInfo.Email ?? "No Email Provided";
                MainContentPanel.Visibility = Visibility.Visible;
                LoadingMessagePanel.Visibility = Visibility.Collapsed;
                UrlInputPanel.Visibility = Visibility.Collapsed;
            });
        }
        catch (Exception ex)
        {
            ShowPanel(LoadingMessagePanel, $"Failed to retrieve user information: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows a specific panel and optional message.
    /// </summary>
    private void ShowPanel(UIElement? panelToShow, string? message = null)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            LoadingMessagePanel.Visibility = Visibility.Collapsed;
            UrlInputPanel.Visibility = Visibility.Collapsed;
            MainContentPanel.Visibility = Visibility.Collapsed;

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
