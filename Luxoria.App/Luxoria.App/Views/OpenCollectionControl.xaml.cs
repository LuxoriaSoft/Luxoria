using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App.Views
{
    public sealed partial class OpenCollectionControl : UserControl
    {
        public OpenCollectionControl()
        {
            this.InitializeComponent();
        }

        // Expose the FolderPathTextBox
        public string SelectedFolderPath => FolderPathTextBox.Text;
        // Expose the CollectionNameTextBox
        public string CollectionName => CollectionNameTextBox.Text;


        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a folder picker
            var folderPicker = new FolderPicker();

            folderPicker.FileTypeFilter.Add("*"); // Allow all file types

            // Use the XamlRoot of the UserControl
            var xamlRoot = this.XamlRoot;
            if (xamlRoot == null)
            {
                throw new InvalidOperationException("XamlRoot is null.");
            }

            // Initialize the folder picker with the window handle
            var window = (Application.Current as App)?.Window as MainWindow;
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WinRT.Interop.WindowNative.GetWindowHandle(window));

            // Show the folder picker and get the result
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Update the textbox with the selected folder path
                FolderPathTextBox.Text = folder.Path;
            }
            else
            {
                // Handle the case where no folder was selected, if necessary
                FolderPathTextBox.Text = "No folder selected.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the ContentDialog
            var dialog = (ContentDialog)this.Parent;
            dialog.Hide();
        }
    }
}
