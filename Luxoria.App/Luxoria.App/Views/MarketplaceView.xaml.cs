using CommunityToolkit.WinUI.UI.Controls;
using Luxoria.Core.Interfaces;
using Luxoria.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luxoria.App.Views
{
    public sealed partial class MarketplaceView : Window
    {
        private readonly IMarketplaceService _mktSvc;
        // Track which module is selected
        private LuxRelease.LuxMod _selectedModule;

        public MarketplaceView(IMarketplaceService marketplaceSvc)
        {
            this.InitializeComponent();
            _mktSvc = marketplaceSvc;
            _ = LoadMarketplaceAsync();
        }

        private async Task LoadMarketplaceAsync()
        {
            try
            {
                ICollection<LuxRelease> releases = await _mktSvc.GetReleases();

                foreach (var version in releases)
                {
                    var versionItem = new NavigationViewItem
                    {
                        Content = version.Name,
                        Tag = version,
                        Icon = new SymbolIcon(Symbol.Folder)
                    };

                    NavView.MenuItems.Add(versionItem);
                }
            }
            catch (Exception ex)
            {
                // show a single error item if the fetch failed
                NavView.MenuItems.Add(new NavigationViewItem
                {
                    Content = $"Error loading marketplace: {ex.Message}"
                });
            }
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer.Tag is LuxRelease.LuxMod module)
            {
                _selectedModule = module;
                try
                {
                    // fetch the raw .md (could be ms-appx or remote URL)
                    /*string md = await _httpClient.GetStringAsync(module.MarkdownUrl);
                    MdViewer.Text = md;
                    InstallButton.IsEnabled = true;*/
                }
                catch (Exception ex)
                {
                    MdViewer.Text = $"Error loading markdown: {ex.Message}";
                    InstallButton.IsEnabled = false;
                }
            }
            else
            {
                _selectedModule = null;
                MdViewer.Text = "";
                InstallButton.IsEnabled = false;
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModule == null)
                return;

            try
            {
                /*var response = await _httpClient.PostAsync(_selectedModule.InstallEndpoint, null);
                response.EnsureSuccessStatusCode();*/

                InstallButton.Content = "Installed";
                InstallButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "Installation failed",
                    Content = ex.Message,
                    CloseButtonText = "OK"
                };
                await dlg.ShowAsync();
            }
        }
    }
}
