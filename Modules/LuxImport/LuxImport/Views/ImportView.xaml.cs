using LuxImport.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LuxImport.Views
{
    public sealed partial class ImportView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainImportView _Parent;
        private StorageFolder? _selectedFolder;

        /// <summary>
        /// Constructor for the ImportView
        /// </summary>
        /// <param name="eventBus">Event bus for internal communications (IPC)</param>
        /// <param name="parent">Parent view</param>
        public ImportView(IEventBus eventBus, MainImportView parent)
        {
            this.InitializeComponent();

            _eventBus = eventBus;
            _Parent = parent;

            // Modal Properties
            Width = 500;

            LoadRecentCollections();
        }

        /// <summary>
        /// Event handler for the Browse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                if (ImportService.IsCollectionInitialized(_selectedFolder.Path))
                {
                    Debug.WriteLine("Collection already initialized.");
                    _Parent.SetIndexicationView(_selectedFolder.Name, _selectedFolder.Path);
                    return;
                }
                _Parent.SetPropertiesView(_selectedFolder.Path);
            }
        }

        /// <summary>
        /// Load recent collections into the RecentsList
        /// </summary>
        private void LoadRecentCollections()
        {
            RecentsList.Children.Clear();

            foreach (var collection in _Parent.RICollectionRepository.GetXLatestImportedCollections(10))
            {
                AddToRecents(collection.Name, collection.Path);
            }
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

        /// <summary>
        /// Event handler for selecting a recent collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        private void OnRecentCollectionSelected(string name, string path)
        {
            Debug.WriteLine($"Recent Collection Selected: {name} - {path}");

            _Parent.SetIndexicationView(name, path);
        }
    }
}