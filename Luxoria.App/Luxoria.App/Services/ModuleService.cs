using Luxoria.App.Interfaces;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;

namespace Luxoria.App.Services
{
    public class ModuleService : IModuleService
    {
        private readonly List<IModule> _modules = [];
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _logger;

        public ModuleService(IEventBus eventBus, ILoggerService logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "EventBus cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "LoggerService cannot be null");
        }

        public void AddModule(IModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }

            _modules.Add(module);
        }

        public void RemoveModule(IModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module cannot be null");
            }

            _modules.Remove(module);
        }

        public List<IModule> GetModules() => _modules;

        /// <summary>
        /// Initializes all registered modules with the provided context.
        /// Progress is reported via the IProgress interface, indicating the (Module, IsInitializedWithSuccess).
        /// </summary>
        /// <param name="context">Context to be passes through the initialisation process</param>
        /// <param name="progress">Indicating (Module, IsInitializedWithSuccess)</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void InitializeModules(IModuleContext context, IProgress<(IModule, bool)> progress)
        {
            _logger.Log("Initializing Modules...", "ModuleService", LogLevel.Info);
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "ModuleContext cannot be null");
            }

            foreach (IModule module in _modules)
            {
                _logger.Log($"[+] Initializing Module: {module.Name}...", "ModuleService", LogLevel.Info);

                try
                {
                    module.Initialize(_eventBus, context, _logger);
                    progress?.Report((module, true));
                }
                catch (Exception ex)
                {
                    _logger.Log($"[!!]: Error Initializing Module: {module.Name} - Exception: {ex.Message}", "ModuleService", LogLevel.Error);
                    progress?.Report((module, false));
                }

                if (module is IModuleUI)
                {
                    _logger.Log($"[->]: UI Module Detected: {module.Name}...", "ModuleService", LogLevel.Info);
                }
            }
        }
    }
}