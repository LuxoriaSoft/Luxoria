using Luxoria.App.Components;
using Luxoria.App.Views;
using Luxoria.Core.Interfaces;
using Luxoria.Core.Services;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly IModuleService _moduleService;

        public MainWindow(IEventBus eventBus, IModuleService moduleService)
        {
            this.InitializeComponent();
            _eventBus = eventBus;
            _moduleService = moduleService;
            Initialize();

        }

        public void Initialize()
        {
            // Subscribe to the ImageUpdatedEvent
            _eventBus.Subscribe<ImageUpdatedEvent>(OnImageUpdated);
        }

        private void SendToModule_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void OpenCollection_Click(object sender, RoutedEventArgs e)
        {
            var openCollectionControl = new OpenCollectionControl();
            // Create the ContentDialog
            var dialog = new ContentDialog
            {
                Title = "Open Collection",
                Content = openCollectionControl,
                CloseButtonText = "Close",
                PrimaryButtonText = "Next",
                XamlRoot = MainGrid.XamlRoot
            };

            // Show the dialog
            var result = await dialog.ShowAsync();

            // Handle dialog result if needed
            if (result == ContentDialogResult.Primary)
            {
                // Retrieve the selected folder path
                string selectedFolderPath = openCollectionControl.SelectedFolderPath;
                // Retrieve the collection name
                string collectionName = openCollectionControl.CollectionName;
                Log($"Selected folder path: {selectedFolderPath}");

                var importationControl = new ImportationControl();
                // Create the ContentDialog
                var importationDialog = new ContentDialog
                {
                    Title = "Importation",
                    Content = importationControl,
                    XamlRoot = MainGrid.XamlRoot
                };

                // Publish the selected folder path to the module
                OpenCollectionEvent openCollectionEvt = new OpenCollectionEvent(collectionName, selectedFolderPath);
                openCollectionEvt.ProgressMessage += (message, progress) =>
                {
                    // Update the importation control with progress messages
                    if (progress.HasValue)
                    {
                        importationControl.UpdateProgress(message, progress.Value);
                    } else
                    {
                        importationControl.UpdateProgress(message);
                    }
                };

                // Show the dialog first, then start publishing
                var importationDialogTask = importationDialog.ShowAsync();

                try
                {
                    // Publish the selected folder path to the module asynchronously
                    await _eventBus.Publish(openCollectionEvt);
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during event publishing
                    Log($"Error publishing event: {ex.Message}");

                    // Optionally update the dialog with the error message
                    importationControl.UpdateProgress("Error: " + ex.Message);
                }

                // Await the dialog to ensure it is shown after publishing is complete
                await importationDialogTask;
            }
        }

        private void OnImageUpdated(ImageUpdatedEvent imageUpdatedEvent)
        {
            // Handle the response from the module
            // For example, display the updated image or log a message
            Log($"Image updated: {imageUpdatedEvent.ImagePath}");
        }

        private void Log(string message)
        {
            // Log the message (e.g., output to console or a log file)
            Debug.WriteLine(message);
        }
    }
}
