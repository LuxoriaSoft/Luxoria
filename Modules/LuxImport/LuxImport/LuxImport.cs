using LuxImport.Interfaces;
using LuxImport.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Threading.Tasks;

namespace LuxImport
{
    public class LuxImport : IModule
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        public string Name => "LuxImport";
        public string Description => "Generic Luxoria Importation Module";
        public string Version => "1.0.0";

        /// <summary>
        /// Initializes the module with the provided EventBus and ModuleContext.
        /// </summary>
        /// <param name="eventBus">The event bus for publishing and subscribing to events.</param>
        /// <param name="context">The context for managing module-specific data.</param>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;
            _logger?.Log("LuxImport initialized", "Mods/LuxImport", LogLevel.Info);

            // Subscribe to mandatory events with an async handler
            _eventBus.Subscribe<OpenCollectionEvent>(HandleOnOpenCollectionAsync);
        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            _logger?.Log("LuxImport executed", "Mods/LuxImport", LogLevel.Info);
            // Additional logic can be added here if needed
        }

        /// <summary>
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            // Unsubscribe from events if necessary to avoid memory leaks
            _logger?.Log("LuxImport shutdown", "Mods/LuxImport", LogLevel.Info);
        }

        /// <summary>
        /// Asynchronous handler for the OpenCollectionEvent by importing the collection at the specified path.
        /// </summary>
        /// <param name="event">The OpenCollectionEvent containing the path to the collection.</param>
        public async Task HandleOnOpenCollectionAsync(OpenCollectionEvent @event)
        {
            _logger?.Log($"Importing collection [{@event.CollectionName}] at path: {@event.CollectionPath}", "Mods/LuxImport", LogLevel.Info);

            // Simulate some delay in the import process
            await Task.Delay(1000);

            // Send a message back through the event tunnel
            SendProgressMessage(@event, "Initiating import process...");

            await Task.Delay(1000);

            try
            {
                // Initialize the import service with the collection name and path
                _logger?.Log("Importing collection...", "Mods/LuxImport", LogLevel.Info);
                SendProgressMessage(@event, $"Importing [{@event.CollectionName}] collection...");
                IImportService importService = new ImportService(@event.CollectionName, @event.CollectionPath);
                importService.ProgressMessageSent += (message) => SendProgressMessage(@event, message);

                await Task.Delay(500);

                // Check if the collection is already initialized
                SendProgressMessage(@event, "Checking collection initialization...");
                if (importService.IsInitialized())
                {
                    SendProgressMessage(@event, "Collection is already initialized.");
                } else
                {
                    // Initializing collection's database
                    SendProgressMessage(@event, "Initializing collection's database...");
                    importService.InitializeDatabase();
                    await Task.Delay(1000);
                }

                // Update indexing files
                SendProgressMessage(@event, "Updating indexing files...");
                await importService.IndexCollectionAsync();
                await Task.Delay(1000);

                // Additional simulated delay
                await Task.Delay(1000);
                SendProgressMessage(@event, "Importing collection step 2...");

            }
            catch (Exception ex)
            {
                _logger?.Log($"Error importing collection: {ex.Message}", "Mods/LuxImport", LogLevel.Error);
                SendProgressMessage(@event, $"Error importing collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a progress message to the logger and the event tunnel.
        /// </summary>
        private void SendProgressMessage(OpenCollectionEvent @event, string message)
        {
            // Log the message
            _logger?.Log(message, "Mods/LuxImport", LogLevel.Info);
            // Send the message through the event tunnel
            @event.SendProgressMessage(message);
        }
    }
}
