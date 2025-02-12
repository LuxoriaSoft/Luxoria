using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI;
using System.Reflection.Metadata;

namespace LuxImport.Views
{
    public sealed partial class ImportView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainImportView _Parent;
        private StorageFolder? _selectedFolder;

        public ImportView(IEventBus eventBus, MainImportView parent)
        {
            this.InitializeComponent();

            _eventBus = eventBus;
            _Parent = parent;

            // Modal Properties
            Width = 500;

            LoadRecentCollections();
        }

        private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<nint>();
            await _eventBus.Publish(new RequestWindowHandleEvent(handle => tcs.SetResult(handle)));
            nint _windowHandle = await tcs.Task;
            if (_windowHandle == 0) return;

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, _windowHandle);

            _selectedFolder = await picker.PickSingleFolderAsync();
            if (_selectedFolder != null)
            {
                Debug.WriteLine($"Folder selected: {_selectedFolder.Path}");
                _Parent.SetPropertiesView(_selectedFolder.Path);
            }
        }

        private void LoadRecentCollections()
        {
            RecentsList.Children.Clear();
            AddToRecents("Example Collection 1", "C:\\Photos\\Collection1");
            AddToRecents("Example Collection 2", "C:\\Documents\\Collection2");
            AddToRecents("Example Collection 3", "D:\\Work\\Collection3");
            AddToRecents("Example Collection 4", "D:\\Work\\Collection3");
            AddToRecents("Example Collection 5", "D:\\Work\\Collection3");
        }

        private void AddToRecents(string name, string path)
        {
            var button = new Button
            {
                Content = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = name,
                                    FontWeight = FontWeights.SemiBold,
                                    FontSize = 14,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = path,
                                    FontSize = 12,
                                    Opacity = 0.6,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                }
                            }
                        }
                    }
                },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 2, 0, 2)
            };

            button.Click += (s, e) => OnRecentCollectionSelected(name, path);
            RecentsList.Children.Add(button);
        }

        private async void OnRecentCollectionSelected(string name, string path)
        {
            Debug.WriteLine($"Recent Collection Selected: {name} - {path}");

            // Ensure modal is shown on UI thread
            await DispatcherQueue.EnqueueAsync(async () =>
            {
            });
        }
    }
}