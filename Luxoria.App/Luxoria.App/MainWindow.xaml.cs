using Luxoria.App.Components;
using Luxoria.App.Views;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.UI.Notifications;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Event bus for publishing and subscribing to events.
        /// </summary>
        private readonly IEventBus _eventBus;

        /// <summary>
        /// Logger service for logging messages.
        /// </summary>
        private readonly ILoggerService _loggerService;

        /// <summary>
        /// Constructor for the MainWindow class.
        /// </summary>
        /// <param name="eventBus">EventBus to handle Subscription and Publishing</param>
        public MainWindow(IEventBus eventBus, ILoggerService loggerService)
        {
            // Initialize the component
            this.InitializeComponent();
            // Set the event bus
            _eventBus = eventBus;
            _loggerService = loggerService;

            // Initialize the event bus
            Initialize();
        }

        /// <summary>
        /// Subscribes all handlers decicated to MainWindow to the event bus.
        /// </summary>
        public void Initialize()
        {
            // Subscribe to the ImageUpdatedEvent
            _eventBus.Subscribe<ImageUpdatedEvent>(OnImageUpdated);
            _eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);
        }

        private void SendToModule_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the Open Collection button click event.
        /// </summary>
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
                _loggerService.Log($"Selected folder path: {selectedFolderPath}");

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
                    _loggerService.Log($"Error publishing event: {ex.Message}");

                    // Optionally update the dialog with the error message
                    importationControl.UpdateProgress("Error: " + ex.Message);
                }

                // Await the dialog to ensure it is shown after publishing is complete
                await importationDialogTask;
            }
        }

        /// <summary>
        /// Handles the Image Updated event.
        /// </summary>
        /// <param name="body">Body as ImageUpdatedEvent, contains ImagePath, ...</param>
        private void OnImageUpdated(ImageUpdatedEvent body)
        {
            // Handle the response from the module
            _loggerService.Log($"Image updated: {body.ImagePath}");
        }

        /// <summary>
        /// Handles the Collection Updated event.
        /// </summary>
        /// <param name="body">Body as CollectionUpdatedEvent, contains collection details (name, path, assets)</param>
        private void OnCollectionUpdated(CollectionUpdatedEvent body)
        {
            // Handle the response from the module
            _loggerService.Log($"Collection updated: {body.CollectionName}");
            _loggerService.Log($"Collection path: {body.CollectionPath}");

            // Show a tost notification
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var textNodes = toastXml.GetElementsByTagName("text");
                textNodes[0].AppendChild(toastXml.CreateTextNode($"Updated Collection : {body.CollectionName}"));
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier("Luxoria").Show(toast);
            }

            for (int i = 0; i < body.Assets.Count; i++)
            {
                _loggerService.Log($"Asset {i}: {body.Assets.ElementAt(i).MetaData.Id}");
            }
        }
    }
}
