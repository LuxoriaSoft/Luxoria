using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxExport
{
    public class LuxExport : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        public string Name => "Lux Export";
        public string Description => "Export module for luxoria.";
        public string Version => "1.0.0";

        public List<ILuxMenuBarItem> Items { get; set; } = new List<ILuxMenuBarItem>();

        private Export? _export;

        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize LuxExport: EventBus or Context is null", "Mods/LuxExport", LogLevel.Error);
                return;
            }

            List<ISmartButton> smartButtons = new List<ISmartButton>();

            Dictionary<SmartButtonType, Object> mainPage = new Dictionary<SmartButtonType, Object>();

            _export = new Export();

            mainPage.Add(SmartButtonType.Window, _export);

            smartButtons.Add(new SmartButton("Export", "Export module", mainPage));


            Items.Add(new LuxMenuBarItem("LuxExport", false, new Guid(), smartButtons));

            _eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} initialized", "Mods/LuxExport", LogLevel.Info);

        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "Mods/LuxExport", LogLevel.Info);
        }

        /// <summary>Content
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            _eventBus?.Unsubscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} shut down", "Mods/LuxExport", LogLevel.Info);
        }

        public void OnCollectionUpdated(CollectionUpdatedEvent e)
        {
            _logger?.Log($"{Name} received CollectionUpdatedEvent", "Mods/LuxExport", LogLevel.Info);
        }
    }
}
