using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxExport
{
    /// <summary>
    /// The LuxExport module handles exporting functionalities for the Luxoria application.
    /// It integrates with the event bus, context, and logger to provide export features.
    /// </summary>
    public class LuxExport : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name => "Lux Export";

        /// <summary>
        /// Gets the description of the module.
        /// </summary>
        public string Description => "Export module for luxoria.";

        /// <summary>
        /// Gets the version of the module.
        /// </summary>
        public string Version => "1.0.0";

        /// <summary>
        /// List of items (smart buttons) associated with this module.
        /// </summary>
        public List<ILuxMenuBarItem> Items { get; set; } = new List<ILuxMenuBarItem>();

        private Export? _export;

        /// <summary>
        /// Initializes the LuxExport module.
        /// </summary>
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
            _export._eventBus = eventBus;

            mainPage.Add(SmartButtonType.Modal, _export);

            smartButtons.Add(new SmartButton("Export", "Export module", mainPage));

            Items.Add(new LuxMenuBarItem("LuxExport", false, new Guid(), smartButtons));

            _eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} initialized", "Mods/LuxExport", LogLevel.Info);
        }

        /// <summary>
        /// Executes the module logic.
        /// This can be triggered to perform specific actions within the module.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "Mods/LuxExport", LogLevel.Info);
        }

        /// <summary>
        /// Shuts down the module, cleaning up resources and unsubscribing from events.
        /// </summary>
        public void Shutdown()
        {
            _eventBus?.Unsubscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} shut down", "Mods/LuxExport", LogLevel.Info);
        }

        /// <summary>
        /// Handles the CollectionUpdatedEvent, which occurs when the collection of assets is updated.
        /// </summary>
        public void OnCollectionUpdated(CollectionUpdatedEvent body)
        {
            _logger?.Log($"Collection updated: {body.CollectionName}");
            _logger?.Log($"Collection path: {body.CollectionPath}");

            for (int i = 0; i < body.Assets.Count; i++)
            {
                ImageData imageData = body.Assets.ElementAt(i).Data;
                _logger?.Log($"Asset {i}: {body.Assets.ElementAt(i).MetaData.Id}");
                _logger?.Log($"Asset info {i}: {imageData.Height}x{imageData.Width}, pixels: {imageData.Height * imageData.Width}");
            }

            List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> lst = body.Assets
                .Select(x => new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(x.Data.Bitmap, x.Data.EXIF))
                .ToList();

            Debug.WriteLine("Calling function ....");
            Debug.WriteLine("Lst count: " + lst.Count);
            Debug.WriteLine(lst);

            _export?.SetBitmaps(lst);
        }
    }
}
