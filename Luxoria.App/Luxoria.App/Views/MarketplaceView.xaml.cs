using Luxoria.Core.Interfaces;
using Luxoria.Core.Models;
using Luxoria.Core.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Luxoria.App.Views
{
    public sealed partial class MarketplaceView : Window
    {
        private readonly IMarketplaceService _mktSvc;
        private readonly IStorageAPI _cacheSvc;
        private readonly IEventBus _eventBus;
        private readonly HttpClient _httpClient = new();

        // All releases loaded from service
        private ICollection<LuxRelease> _allReleases;

        // Currently selected module
        private LuxRelease.LuxMod _selectedModule;

        public MarketplaceView(IMarketplaceService marketplaceSvc, IStorageAPI cacheSvc, IEventBus eventBus)
        {
            Debug.Write("Initializing View...");
            this.InitializeComponent();
            Debug.WriteLine(" OK!");
            Debug.Write("Retrieving services...");
            _mktSvc = marketplaceSvc;
            _cacheSvc = cacheSvc;
            _eventBus = eventBus;
            Debug.WriteLine(" OK!");
            Debug.Write("Loading marketplace...");

            _ = Task.Run(async () =>
            {
                var dataTask = LoadMarketplaceDataAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

                if (await Task.WhenAny(dataTask, timeoutTask) == dataTask)
                {
                    try
                    {
                        _allReleases = await dataTask;
                        Debug.WriteLine(" Marketplace data loaded, dispatching UI update...");

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            NavView.MenuItems.Clear();
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
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($" Error loading data: {ex.Message}");
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            NavView.MenuItems.Clear();
                            NavView.MenuItems.Add(new NavigationViewItem
                            {
                                Content = $"Error loading marketplace: {ex.Message}"
                            });
                        });
                    }
                }
                else
                {
                    Debug.WriteLine(" LoadMarketplaceDataAsync timed out after 30 s.");
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        NavView.MenuItems.Clear();
                        NavView.MenuItems.Add(new NavigationViewItem
                        {
                            Content = "Marketplace load timed out."
                        });
                    });
                }
            });
        }

        /// <summary>
        /// Fetches the marketplace releases
        /// </summary>
        private async Task<ICollection<LuxRelease>> LoadMarketplaceDataAsync()
        {
            if (_cacheSvc.Contains("releases"))
            {
                Debug.WriteLine("Loading releases from cache");
                return _cacheSvc.Get<ICollection<LuxRelease>>("releases");
            }
            else
            {
                Debug.WriteLine("Loading releases from service");
                var releases = await _mktSvc.GetReleases();
                _cacheSvc.Save("releases", DateTime.Now.AddHours(6), releases);
                return releases;
            }
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            ModulesListView.ItemsSource = null;
            ModulesListView.IsEnabled = false;
            MdViewer.Text = string.Empty;
            InstallButton.IsEnabled = false;
            InstallButton.Content = "Install";
            DownloadCount.Text = string.Empty;

            if (args.InvokedItemContainer.Tag is LuxRelease release)
            {
                Debug.WriteLine($"Selected release: [{release.Id}] / {release.Name}");
                ICollection<LuxRelease.LuxMod> modules;
                if (_cacheSvc.Contains(release.Id.ToString()))
                {
                    Debug.WriteLine("Fetching release from cache...");
                    modules = _cacheSvc.Get<ICollection<LuxRelease.LuxMod>>(release.Id.ToString());
                }
                else
                {
                    Debug.WriteLine("Fetching release from distant...");
                    modules = await _mktSvc.GetRelease(release.Id);
                    _cacheSvc.Save(release.Id.ToString(), DateTime.Now.AddHours(24), modules);
                }

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
                InstallButton.Content = "Install";
                DownloadCount.Text = $"Downloads: {module.AttachedModulesDownloadCount}";

                try
                {
                    if (_cacheSvc.Contains(module.DownloadUrl))
                    {
                        MdViewer.Text = _cacheSvc.Get<string>(module.DownloadUrl);
                        return;
                    }

                    string md = await _httpClient.GetStringAsync(module.DownloadUrl);
                    _cacheSvc.Save(module.DownloadUrl, DateTime.Now.AddHours(24), md);
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
                MdViewer.Text = string.Empty;
                InstallButton.IsEnabled = false;
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModule == null)
                return;

            try
            {
                string arch = ModuleInstaller.GetShortArch();
                InstallButton.Content = "Installing...";

                var selectedModuleToBeInstalled = _selectedModule.AttachedModulesByArch
                    .Where(x => x.Name.EndsWith($".{arch}.zip"))
                    .Select(x => (Name: x.Name.Replace($".{arch}.zip", ""), Url: x.DownloadUrl))
                    .First();

                Debug.WriteLine($"Downloading module from: {selectedModuleToBeInstalled.Url}");
                await ModuleInstaller.InstallFromUrlAsync(selectedModuleToBeInstalled.Name, selectedModuleToBeInstalled.Url);

                await _eventBus.Publish(new ToastNotificationEvent
                {
                    Title = $"Module {selectedModuleToBeInstalled.Name} Installed",
                    Message = "Please restart Luxoria to load the new module.",
                });

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
