using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Collections.ObjectModel;
using WinRT.Interop;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportView : Page
    {
        public ObservableCollection<string> SelectedFiles { get; } = new();
        private IEventBus _eventBus;

        public ImportView(IEventBus eventBus)
        {
            this.InitializeComponent();
            FileListView.ItemsSource = SelectedFiles;
            _eventBus = eventBus;
        }

        private async void BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<nint>();

            // Publish the event to request the window handle
            _eventBus.Publish(new RequestWindowHandleEvent(handle => tcs.SetResult(handle)));

            // Await the window handle
            nint _windowHandle = await tcs.Task;

            Debug.WriteLine($"/OK Window Handle: {_windowHandle}");

            if (_windowHandle == 0)
            {
                Debug.WriteLine("Failed to get window handle.");
                return;
            }

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");
            picker.ViewMode = PickerViewMode.Thumbnail;

            // Attach picker to the correct window
            InitializeWithWindow.Initialize(picker, _windowHandle);

            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                Debug.WriteLine($"Folder selected: {folder.Path}");
            }
        }



        private async void StartImport_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFiles.Count == 0) return;
            ImportProgress.Visibility = Visibility.Visible;
            ImportProgress.Value = 0;

            for (int i = 0; i < SelectedFiles.Count; i++)
            {
                Debug.WriteLine($"Importing: {SelectedFiles[i]}");
                await Task.Delay(500); // Simulate file processing
                ImportProgress.Value = ((i + 1) / (double)SelectedFiles.Count) * 100;
            }

            Debug.WriteLine("Import Completed");
            ImportProgress.Visibility = Visibility.Collapsed;
        }

        private void CancelImport_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Import Canceled");
            ImportProgress.Visibility = Visibility.Collapsed;
            ImportProgress.Value = 0;
        }
    }
}
