using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;

namespace LuxBenchmark
{
    public class LuxBenchmark : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        public string Name => "Lux Benchmark";
        public string Description => "Benchmark module for luxoria.";
        public string Version => "1.0.0";

        public List<ILuxMenuBarItem> Items { get; set; } = [];

        /// <summary>
        /// Initializes the module and sets up the UI panels and event handlers.
        /// </summary>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize LuxBenchmark: EventBus or Context is null", "LuxBenchmark", LogLevel.Error);
                return;
            }

            List<ISmartButton> smartButtons = new();
            Dictionary<SmartButtonType, object> mainPage = new();


            // TODO: 

            //mainPage.Add(SmartButtonType.MainPanel, );
            //mainPage.Add(SmartButtonType.BottomPanel, );
            //mainPage.Add(SmartButtonType.RightPanel, );
            //mainPage.Add(SmartButtonType.LeftPanel, );

            smartButtons.Add(new SmartButton("Benchmark", "Benchmark module", mainPage));
            Items.Add(new LuxMenuBarItem("Benchmark", false, Guid.NewGuid(), smartButtons));

            _logger?.Log($"{Name} initialized", "LuxBenchmark", LogLevel.Info);
        }

        /// <summary>
        /// Executes the module logic manually.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "LuxEditor", LogLevel.Info);
        }

        /// <summary>
        /// Cleans up the module and unsubscribes from events.
        /// </summary>
        public void Shutdown()
        {
            _logger?.Log($"{Name} shut down", "LuxEditor", LogLevel.Info);
        }
    }
}
