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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Luxoria.App.Views
{
    public sealed partial class MarketplaceView : Window
    {
        private readonly IMarketplaceService _mktSvc;
        private readonly IStorageAPI _cacheSvc;
        private readonly IEventBus _eventBus;
        private readonly HttpClient _httpClient = new();

        private ICollection<LuxRelease> _allReleases;
        private LuxRelease.LuxMod _selectedModule;

        private ContentDialog _installDialog;
        private ProgressBar _installProgressBar;
        private TextBlock _installStatusText;
        private TextBlock _installDetailsText;
        private bool _allowDialogClose;

        public MarketplaceView(IMarketplaceService marketplaceSvc, IStorageAPI cacheSvc, IEventBus eventBus)
        {
            this.InitializeComponent();
            _mktSvc = marketplaceSvc;
            _cacheSvc = cacheSvc;
            _eventBus = eventBus;

            _ = Task.Run(async () =>
            {
                var dataTask = LoadMarketplaceDataAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

                if (await Task.WhenAny(dataTask, timeoutTask) == dataTask)
                {
                    try
                    {
                        _allReleases = await dataTask;

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
                return _cacheSvc.Get<ICollection<LuxRelease>>("releases");
            }
            else
            {
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
                ICollection<LuxRelease.LuxMod> modules;
                if (_cacheSvc.Contains(release.Id.ToString()))
                {
                    modules = _cacheSvc.Get<ICollection<LuxRelease.LuxMod>>(release.Id.ToString());
                }
                else
                {
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

                var selectedModuleToBeInstalled = _selectedModule.AttachedModulesByArch
                    .Where(x => x.Name.EndsWith($".{arch}.zip"))
                    .Select(x => (Name: x.Name.Replace($".{arch}.zip", ""), Url: x.DownloadUrl))
                    .First();

                BuildInstallDialog();

                InstallButton.Content = "Installing...";
                _installStatusText.Text = $"Downloading {selectedModuleToBeInstalled.Name}...";
                _installProgressBar.IsIndeterminate = true;
                _installDetailsText.Text = "Starting...";

                var _ = _installDialog.ShowAsync();

                var progress = new Progress<DownloadProgress>(p =>
                {
                    if (p.TotalBytes.HasValue)
                    {
                        _installProgressBar.IsIndeterminate = false;
                        _installProgressBar.Minimum = 0;
                        _installProgressBar.Maximum = 100;
                        _installProgressBar.Value = p.Percent ?? 0;
                    }

                    string received = FormatBytes(p.BytesReceived);
                    string total = p.TotalBytes.HasValue ? FormatBytes(p.TotalBytes.Value) : "?";
                    string speed = $"{FormatBytes(p.BytesPerSecond)}/s";
                    _installDetailsText.Text = $"{received} of {total} — {speed}";
                });

                string zipPath = await DownloadFileWithProgressAsync(selectedModuleToBeInstalled.Url, progress);

                _installStatusText.Text = "Installing...";
                _installProgressBar.IsIndeterminate = true;

                ModuleInstaller.InstallFromZip(selectedModuleToBeInstalled.Name, zipPath);

                await _eventBus.Publish(new ToastNotificationEvent
                {
                    Title = $"Module {selectedModuleToBeInstalled.Name} Installed",
                    Message = "Please restart Luxoria to load the new module.",
                });

                _allowDialogClose = true;
                _installDialog.Hide();

                InstallButton.Content = "Installed";
                InstallButton.IsEnabled = false;

                TryDeleteFile(zipPath);
            }
            catch (Exception ex)
            {
                _allowDialogClose = true;
                _installDialog?.Hide();

                var dlg = new ContentDialog
                {
                    Title = "Installation failed",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dlg.ShowAsync();
            }
        }

        private void BuildInstallDialog()
        {
            _allowDialogClose = false;

            _installProgressBar = new ProgressBar { IsIndeterminate = true };
            _installStatusText = new TextBlock { Text = "Preparing..." };
            _installDetailsText = new TextBlock { Text = "0 B of 0 B — 0 B/s" };

            var panel = new StackPanel { Spacing = 12, MinWidth = 420 };
            panel.Children.Add(_installStatusText);
            panel.Children.Add(_installProgressBar);
            panel.Children.Add(_installDetailsText);

            _installDialog = new ContentDialog
            {
                Title = "Installing module...",
                Content = panel,
                XamlRoot = this.Content.XamlRoot,
                PrimaryButtonText = null,
                SecondaryButtonText = null,
                CloseButtonText = null
            };

            _installDialog.Closing += (s, e) =>
            {
                if (!_allowDialogClose)
                {
                    e.Cancel = true;
                }
            };
        }

        private sealed class DownloadProgress
        {
            public long BytesReceived { get; init; }
            public long? TotalBytes { get; init; }
            public double? Percent => TotalBytes is > 0 ? (double)BytesReceived / TotalBytes.Value * 100.0 : null;
            public double BytesPerSecond { get; init; }
        }

        private static string FormatBytes(double bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < units.Length - 1)
            {
                bytes /= 1024;
                order++;
            }
            return $"{bytes:0.##} {units[order]}";
        }

        private async Task<string> DownloadFileWithProgressAsync(string url, IProgress<DownloadProgress> progress, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();

            var total = resp.Content.Headers.ContentLength;
            var tmpPath = Path.Combine(Path.GetTempPath(), $"luxoria_{Guid.NewGuid():N}.zip");

            var sw = Stopwatch.StartNew();
            long received = 0;

            using (var src = await resp.Content.ReadAsStreamAsync(ct))
            using (var dst = File.Create(tmpPath))
            {
                var buffer = new byte[81920]; // Buffer defined as 80 Kb fixed size
                int read;
                long lastReported = 0;
                var lastReportTime = TimeSpan.Zero;

                while ((read = await src.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
                {
                    await dst.WriteAsync(buffer.AsMemory(0, read), ct);
                    received += read;

                    if (sw.Elapsed - lastReportTime > TimeSpan.FromMilliseconds(100) || received - lastReported > 256 * 1024)
                    {
                        var bps = received / Math.Max(1, sw.Elapsed.TotalSeconds);
                        progress?.Report(new DownloadProgress { BytesReceived = received, TotalBytes = total, BytesPerSecond = bps });
                        lastReportTime = sw.Elapsed;
                        lastReported = received;
                    }
                }
            }

            var finalBps = (double)received / Math.Max(1, sw.Elapsed.TotalSeconds);
            progress?.Report(new DownloadProgress { BytesReceived = received, TotalBytes = total, BytesPerSecond = finalBps });

            return tmpPath;
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch {
            }
        }
    }
}
