using Luxoria.App.Components.Dialogs;
using Luxoria.App.EventHandlers;
using Luxoria.App.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using System;

namespace Luxoria.App
{
    public sealed partial class NewMainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _loggerService;

        // Handlers for different events
        private readonly ImageUpdatedHandler _imageUpdatedHandler;
        private readonly CollectionUpdatedHandler _collectionUpdatedHandler;
        private readonly IModuleUIService _uiService;


        /// <summary>
        /// Constructor for the main window of the application.
        /// </summary>
        public NewMainWindow(IEventBus eventBus, ILoggerService loggerService, IModuleUIService uiService)
        {
            InitializeComponent();

            // Dependency injection
            _eventBus = eventBus;
            _loggerService = loggerService;

            // Initialize event handlers
            _imageUpdatedHandler = new ImageUpdatedHandler(_loggerService);
            _collectionUpdatedHandler = new CollectionUpdatedHandler(_loggerService);

            _uiService = uiService;

            // Subscribe handlers to the event bus
            InitializeEventBus();

            // LoadComponents
            LoadComponents();
        }

        /// <summary>
        /// Initialize the event bus and subscribe handlers to events.
        /// </summary>
        private void InitializeEventBus()
        {
            // Subscribe to events that will be published through the event bus
            _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);
        }

        /// <summmary>
        /// </summmary>
        public void EnableEasyLoader()
        {

        }

        /// <summary>
        /// </summary>
        private void LoadComponents()
        {

        }
    }
}
