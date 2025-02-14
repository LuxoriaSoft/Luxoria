using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndexicationView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainImportView _parent;
        private readonly string _collectionName;
        private readonly string _collectionPath;

        public IndexicationView(IEventBus eventBus, MainImportView parent, string collectionName, string collectionPath)
        {
            _eventBus = eventBus;
            _parent = parent;
            _collectionName = collectionName;
            _collectionPath = collectionPath;

            this.InitializeComponent();
            LoadCollection();
        }

        private void LoadCollection()
        {
            OpenCollectionEvent openCollectionEvent = new OpenCollectionEvent(_collectionName, _collectionPath);

            // Subscribe to progress updates
            openCollectionEvent.ProgressMessage += (message, progressValue) =>
            {
                Debug.WriteLine($"Progress: {message}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    StepProgressText.Text = message;
                    if (progressValue.HasValue)
                    {
                        StepProgressBar.Value = progressValue.Value;
                    }

                    // Add log to ListBox
                    LogViewer.Items.Add(message);
                    LogViewer.ScrollIntoView(LogViewer.Items.Last()); // Auto-scroll to latest entry
                });
            };

            // Event Completed
            openCollectionEvent.OnEventCompleted += (sender, args) =>
            {
                Debug.WriteLine("Event completed successfully.");

                DispatcherQueue.TryEnqueue(() =>
                {
                    StepProgressText.Text = "Import Completed!";
                    StepProgressBar.Value = 100;
                    LogViewer.Items.Add("Import Completed!");
                    LogViewer.ScrollIntoView(LogViewer.Items.Last());
                });
            };

            // Event Failed
            openCollectionEvent.OnImportFailed += (sender, args) =>
            {
                Debug.WriteLine("Import failed!");

                DispatcherQueue.TryEnqueue(() =>
                {
                    StepProgressText.Text = "Import Failed!";
                    LogViewer.Items.Add("Import Failed!");
                    LogViewer.ScrollIntoView(LogViewer.Items.Last());
                });
            };

            _eventBus.Publish(openCollectionEvent);
        }
    }
}
