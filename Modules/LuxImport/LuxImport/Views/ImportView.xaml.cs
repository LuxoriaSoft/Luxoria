using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;

namespace LuxImport.Views
{
    public sealed partial class ImportView : Page
    {
        private IEventBus _eventBus;
        private StorageFolder? _selectedFolder;
        public string? CollectionName { get; set; }

        public ImportView(IEventBus eventBus)
        {
            this.InitializeComponent();
            _eventBus = eventBus;
            CreateCollectionButton.Click += CreateCollectionButton_Click;

            // Set the initial state of the UI
            this.Height = 300;
            this.Width = 500;
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
                await CheckInitialization();
            }
        }

        private async Task CheckInitialization()
        {
            var initFile = await _selectedFolder?.TryGetItemAsync("init.lux");

            DispatcherQueue.TryEnqueue(async () =>
            {
                if (initFile == null)
                {
                    CollectionInputPanel.Visibility = Visibility.Visible;
                    IndexingText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    await RunIndexing();
                }
            });
        }

        private async void CreateCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null) return;

            CollectionName = CollectionNameInput.Text;
            if (!string.IsNullOrWhiteSpace(CollectionName))
            {
                //var initFile = await _selectedFolder.CreateFileAsync("init.lux", CreationCollisionOption.ReplaceExisting);
                //await FileIO.WriteTextAsync(initFile, CollectionName);
                Debug.WriteLine($"Collection '{CollectionName}' created.");
                await RunIndexing();
            }
        }

        private async Task RunIndexing()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                CollectionInputPanel.Visibility = Visibility.Collapsed;
                IndexingText.Visibility = Visibility.Visible;
                Debug.WriteLine("Folder already initialized. Indexing in progress...");
            });

            if (_selectedFolder == null) return;

            await Task.Delay(2000);
            //await _eventBus.Publish(new IndexCollectionEvent(_selectedFolder.Path, CollectionName));

            DispatcherQueue.TryEnqueue(() =>
            {
                Debug.WriteLine("Indexing completed.");
            });
        }
    }
}