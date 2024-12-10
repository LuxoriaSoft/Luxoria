using Luxoria.App.Components.Dialogs;
using Luxoria.App.EventHandlers;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Luxoria.App
{
    public sealed partial class MainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _loggerService;

        // Handlers for different events
        private readonly ImageUpdatedHandler _imageUpdatedHandler;
        private readonly CollectionUpdatedHandler _collectionUpdatedHandler;

        // LMGUI regions
        public StackPanel MainView { get; set; }
        public StackPanel Toolbar { get; set; }
        public StackPanel Sidebar { get; set; }
        public StackPanel Footer { get; set; }

        /// <summary>
        /// Constructor for the main window of the application.
        /// </summary>
        public MainWindow(IEventBus eventBus, ILoggerService loggerService)
        {
            InitializeComponent();

            // Dependency injection
            _eventBus = eventBus;
            _loggerService = loggerService;

            // Initialize event handlers
            _imageUpdatedHandler = new ImageUpdatedHandler(_loggerService);
            _collectionUpdatedHandler = new CollectionUpdatedHandler(_loggerService);

            // Subscribe handlers to the event bus
            InitializeEventBus();

            // Render the GUI elements
            MainView = new StackPanel();
            Toolbar = new StackPanel();
            Sidebar = new StackPanel();
            Footer = new StackPanel();

            // Add regions to the layout
            var grid = new Grid();
            grid.Children.Add(MainView);
            grid.Children.Add(Toolbar);
            grid.Children.Add(Sidebar);
            grid.Children.Add(Footer);
            Content = grid;
        }

        /// <summary>
        /// Initialize the event bus and subscribe handlers to events.
        /// </summary>
        private void InitializeEventBus()
        {
            // Subscribe to events that will be published through the event bus
            _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);
        }

        /// <summary>
        /// Event handler for the Open Collection button.
        /// </summary>
        private async void OpenCollection_Click(object sender, RoutedEventArgs e)
        {
            // Display the Open Collection dialog
            await OpenCollectionDialog.ShowAsync(_eventBus, _loggerService, MainGrid.XamlRoot);
        }

        /// <summary>
        /// Event handler for the Send to Module button.
        /// </summary>
        private void SendToModule_Click(object sender, RoutedEventArgs e)
        {
            // This button will eventually trigger module-specific logic
        }
    }
}
