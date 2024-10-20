using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System.Diagnostics;

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
        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            _logger?.Log("LuxImport executed", "Mods/LuxImport", LogLevel.Info);
            // You can add more logic here if needed
        }

        /// <summary>
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            // Unsubscribe from events if necessary to avoid memory leaks
            _logger?.Log("LuxImport shutdown", "Mods/LuxImport", LogLevel.Info);
        }
    }
}
