using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
                DispatcherQueue.TryEnqueue(() =>
                {
                    StepProgressText.Text = message;
                    if (progressValue.HasValue)
                    {
                        StepProgressBar.Value = progressValue.Value;
                    }

                    // Add log to ListBox
                    LogViewer.Items.Add(message);
                    LogViewer.ScrollIntoView(LogViewer.Items.Last());
                });
            };

            // Event Completed
            openCollectionEvent.OnEventCompleted += (sender, args) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StepProgressText.Text = "Import Completed!";
                    StepProgressBar.Value = 100;
                    LogViewer.Items.Add("Import Completed!");
                    LogViewer.ScrollIntoView(LogViewer.Items.Last());
                });

                _parent.RICollectionRepository.UpdateOrCreate(_collectionName, _collectionPath);
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

        /// <summary>
        /// Start over the import process
        /// </summary>
        private void StartOverButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.SetImportView();
        }
    }
}
