using LuxEditor.Components;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LuxEditor
{
    public class LuxEditor : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        public string Name => "Lux Editor";
        public string Description => "Editor module for luxoria.";
        public string Version => "1.0.0";

        public List<ILuxMenuBarItem> Items { get; set; } = new List<ILuxMenuBarItem>();
        private CollectionExplorer? _cExplorer;
        private PhotoViewer? _photoViewer;
        private Infos? _infos;
        private Editor? _editor;

        /// <summary>
        /// Initializes the module with the provided EventBus and ModuleContext.
        /// </summary>f
        /// <param name="eventBus">The event bus for publishing and subscribing to events.</param>
        /// <param name="context">The context for managing module-specific data.</param>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize TestModule1: EventBus or Context is null", "Mods/TestModule1", LogLevel.Error);
                return;
            }

            List<ISmartButton> smartButtons = new List<ISmartButton>();

            Dictionary<SmartButtonType, Object> mainPage = new Dictionary<SmartButtonType, Object>();

            _photoViewer = new PhotoViewer();
            _cExplorer = new CollectionExplorer();
            _editor = new Editor();
            _infos = new Infos();

            _editor.OnEditorImageUpdated += (updatedBitmap) =>
            {
                _photoViewer.SetImage(updatedBitmap);
            };

            _cExplorer.OnImageSelected += (bitmap) =>
            {
                _editor.SetOriginalBitmap(bitmap.Key);
                _photoViewer.SetImage(bitmap.Key);

                _infos?.DisplayExifData(bitmap.Value);
            };

            mainPage.Add(SmartButtonType.MainPanel, _photoViewer);
            mainPage.Add(SmartButtonType.BottomPanel, _cExplorer);
            mainPage.Add(SmartButtonType.RightPanel, _editor);
            mainPage.Add(SmartButtonType.LeftPanel, _infos);

            smartButtons.Add(new SmartButton("Editor", "Editor module", mainPage));

            Items.Add(new LuxMenuBarItem("LuxEditor", false, new Guid(), smartButtons));

            _eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} initialized", "Mods/TestModule1", LogLevel.Info);
        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "Mods/TestModule1", LogLevel.Info);
        }

        /// <summary>
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            _eventBus?.Unsubscribe<CollectionUpdatedEvent>(OnCollectionUpdated);

            _logger?.Log($"{Name} shut down", "Mods/TestModule1", LogLevel.Info);
        }

        /// <summary>
        /// Load the images into the module.
        /// </summary>
        public void OnCollectionUpdated(CollectionUpdatedEvent body)
        {
            _logger?.Log($"Collection updated: {body.CollectionName}");
            _logger?.Log($"Collection path: {body.CollectionPath}");

            for (int i = 0; i < body.Assets.Count; i++)
            {
                ImageData imageData = body.Assets.ElementAt(i).Data;
                _logger?.Log($"Asset {i}: {body.Assets.ElementAt(i).MetaData.Id}");
                _logger?.Log($"Asset info {i} : {imageData.Height}x{imageData.Width}, pixels : {imageData.Height * imageData.Width}");
            }
            List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> lst = body.Assets
                .Select(x => new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(x.Data.Bitmap, x.Data.EXIF))
                .ToList();
            Debug.WriteLine("Calling function ....");

            Debug.WriteLine("Lst count : " + lst.Count);
            Debug.WriteLine(lst);

            _cExplorer?.SetBitmaps(lst);

            Debug.WriteLine("Function called ....");
            Debug.WriteLine("IYAWDHIBAIBHDW IWUHADIWUBHD IBHUBWAHDWH BJDHJB WAJBHDWAJHD WBDA MBVHJ ABWJHDABWJ DHGAVWDJK HGAVWD JHAGWVDAW JHGDVAJHD GVAWDJ HGVAWD JHAWVDAJHGVWD AJHGDVAWJ HDGAVDA JHDV AWDJHAWVDAWJHGDVAJHDGVAW DJHGAVWDJHG");
            _cExplorer?.SetBitmaps(lst);
        }
    }
}