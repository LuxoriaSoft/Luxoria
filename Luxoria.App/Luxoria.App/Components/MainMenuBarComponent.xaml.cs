using Luxoria.App.Views;
using Luxoria.Core.Interfaces;
using Luxoria.GModules.Helpers;
using Luxoria.Modules.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luxoria.App.Components
{
    public sealed partial class MainMenuBarComponent : UserControl
    {
        private List<Button> LeftButtons { get; set; } = [];
        private List<Button> RightButtons { get; set; } = [];
        private static string VAULT_NAME = "Luxoria.App.Marketplace";

        public MainMenuBarComponent()
        {
            InitializeComponent();
        }

        private void ModuleManagement_Click(object sender, RoutedEventArgs e)
        {
            var moduleService = (Application.Current as App)?.ModuleService;

            var newWindow = new Window();
            var moduleManagerPage = new ModuleManagerView(moduleService, newWindow);
            WindowHelper.SetCaption(newWindow.AppWindow, "Luxoria_Icon");
            newWindow.Content = moduleManagerPage;
            newWindow.Activate();
        }

        private void Marketplace_Click(object sender, RoutedEventArgs e)
        {
            IMarketplaceService mkplaceSvc = (Application.Current as App)?.GetHost().Services.GetRequiredService<IMarketplaceService>();
            IStorageAPI cacheSvc = (Application.Current as App)?.GetHost().Services.GetRequiredService<IVaultService>().GetVault(VAULT_NAME);
            IEventBus eventBus = (Application.Current as App)?.GetHost().Services.GetRequiredService<IEventBus>();
            Window window = new MarketplaceView(mkplaceSvc, cacheSvc, eventBus);
            window.Activate();
        }

        public void AddLeftButton(string text, Action onClick)
        {
            var button = CreateButton(text, onClick);
            LeftButtons.Add(button);
            LeftMenu.Children.Add(button);
        }

        public void AddRightButton(string text, Action onClick)
        {
            var button = CreateButton(text, onClick);
            RightButtons.Add(button);
            RightMenu.Children.Add(button);
        }

        public Button GetLeftButton(string text)
        {
            foreach (var button in LeftButtons)
            {
                if (button.Content?.ToString() == text)
                {
                    return button;
                }
            }

            Debug.WriteLine($"[GetLeftButton] No button found with: '{text}'.");
            return null;
        }

        public Button GetRightButton(string text)
        {
            foreach (var button in RightButtons)
            {
                if (button.Content?.ToString() == text)
                {
                    return button;
                }
            }

            Debug.WriteLine($"[GetRightButton] No button found with: '{text}'.");
            return null;
        }

        private Button CreateButton(string text, Action onClick)
        {
            var button = new Button
            {
                Content = text,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorPrimaryBrush"],
                FontSize = 14,
                Padding = new Thickness(8, 4, 8, 4),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 10, 0)
            };

            button.Click += (s, e) => onClick?.Invoke();
            return button;
        }

    }
}