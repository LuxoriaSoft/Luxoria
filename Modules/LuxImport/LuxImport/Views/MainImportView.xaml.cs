using LuxImport.Interfaces;
using LuxImport.Repositories;
using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainImportView : Page
    {
        /// <summary>
        /// Event Bus
        /// </summary>
        private readonly IEventBus _eventBus;

        /// <summary>
        /// Recent Imported Collection Repository
        /// </summary>
        public readonly IRICollectionRepository RICollectionRepository;

        /// <summary>
        /// Constructor for the MainImportView
        /// </summary>
        /// <param name="eventBus">Communication system (IPC)</param>
        public MainImportView(IEventBus eventBus)
        {
            _eventBus = eventBus;
            RICollectionRepository = new RICollectionRepository();

            this.InitializeComponent();

            // Load ImportView by default
            EntryPoint();
        }

        public void EntryPoint() => SetImportView();

        // Switch to ImportView
        public void SetImportView()
        {
            ModalContent.Content = new ImportView(_eventBus, this);
            UpdateProgress(1);
        }

        public void SetPropertiesView(string collectionPath)
        {
            ModalContent.Content = new PropertiesView(_eventBus, this, collectionPath);
            UpdateProgress(2);
        }

        public void SetIndexicationView(string collectionName, string collectionPath)
        {
            ModalContent.Content = new IndexicationView(_eventBus, this, collectionName, collectionPath);
            UpdateProgress(3);
        }

        private void UpdateProgress(int step)
        {
            StepProgressBar.Value = step;
        }

    }
}
