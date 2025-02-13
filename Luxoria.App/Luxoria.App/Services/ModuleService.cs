﻿using Luxoria.App.Interfaces;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luxoria.App.Services
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

        public void InitializeModules(IModuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "ModuleContext cannot be null");
            }

            foreach (IModule module in _modules)
            {
                module.Initialize(_eventBus, context, _logger);
                if (module is IModuleUI moduleGUI)
                {
                    _logger.Log("Hello i'm a module named: " + moduleGUI.Name);

                    Debug.WriteLine(moduleGUI.Items);
                }
            }
        }
    }
}