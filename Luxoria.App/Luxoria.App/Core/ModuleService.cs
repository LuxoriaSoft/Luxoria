using Luxoria.App.Core.Interfaces;
using Luxoria.Core.LMGUI;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.LMGUI;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luxoria.App.Core
{
    public class ModuleService : IModuleService
    {
        private readonly List<IModule> _modules = new List<IModule>();
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

        public void InitializeModules(GuiRenderer guiRenderer, IModuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "ModuleContext cannot be null");
            }

            foreach (IModule module in _modules)
            {
                module.Initialize(_eventBus, context, _logger);

                if (module is IGuiExtension guiExtension)
                {
                    var elements = guiExtension.GetGuiElements().ToList();
                    _logger.Log($"Rendering {elements.ToString()} GUI elements for module {module.Name}", "ModuleService", LogLevel.Info);
                    guiRenderer.Render(elements);
                }
            }
        }
    }
}