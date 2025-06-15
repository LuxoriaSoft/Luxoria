using CommunityToolkit.WinUI.UI.Controls;
using Luxoria.Core.Interfaces;
using Luxoria.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Luxoria.App.Views
{
    public sealed partial class MarketplaceView : Window
    {
        private readonly IMarketplaceService _mktSvc;
        private readonly HttpClient _httpClient = new();

        // All releases loaded from service
        private IEnumerable<LuxRelease> _allReleases;

        // Currently selected module
        private LuxRelease.LuxMod _selectedModule;
        private Dictionary<string, string> _caches = [];

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
                _allReleases = await _mktSvc.GetReleases();

                foreach (var release in _allReleases)
                {
                    var releaseItem = new NavigationViewItem
                    {
                        Content = release.Name,
                        Tag = release,
                        Icon = new SymbolIcon(Symbol.Folder)
                    };
                    NavView.MenuItems.Add(releaseItem);
                }
            }
            catch (Exception ex)
            {
                NavView.MenuItems.Add(new NavigationViewItem
                {
                    Content = $"Error loading marketplace: {ex.Message}"
                });
            }
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            ModulesListView.ItemsSource = null;
            ModulesListView.IsEnabled = false;
            MdViewer.Text = "";
            InstallButton.IsEnabled = false;

            if (args.InvokedItemContainer.Tag is LuxRelease release)
            {
                Debug.WriteLine($"Selected release: [{release.Id}] / {release.Name}");
                var modules = await _mktSvc.GetRelease(release.Id);
                ModulesListView.ItemsSource = modules;
                ModulesListView.IsEnabled = true;
            }
        }

        private async void ModulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModulesListView.SelectedItem is LuxRelease.LuxMod module)
            {
                _selectedModule = module;
                InstallButton.IsEnabled = true;

                try
                {
                    if (_caches.ContainsKey(module.DownloadUrl)) 
                    {
                        MdViewer.Text = _caches[module.DownloadUrl];
                        return;
                    }
                    string md = await _httpClient.GetStringAsync(module.DownloadUrl);
                    _caches[module.DownloadUrl] = md;
                    MdViewer.Text = md;
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
                //var response = await _httpClient.PostAsync(_selectedModule.InstallEndpoint, null);
                //response.EnsureSuccessStatusCode();

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
