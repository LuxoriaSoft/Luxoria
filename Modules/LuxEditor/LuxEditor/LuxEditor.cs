using LuxEditor.Components;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;
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
        public string Version => "1.5.2";

        public List<ILuxMenuBarItem> Items { get; set; } = [];

        private CollectionExplorer? _cExplorer;
        private PhotoViewer? _photoViewer;
        private Infos? _infos;
        private Editor? _editor;

        /// <summary>
        /// Initializes the module and sets up the UI panels and event handlers.
        /// </summary>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            PresetManager.Instance.ConfigureBus(_eventBus);

            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize LuxEditor: EventBus or Context is null", "LuxEditor", LogLevel.Error);
                return;
            }

            List<ISmartButton> smartButtons = new();
            Dictionary<SmartButtonType, object> mainPage = new();

            _photoViewer = new PhotoViewer();
            _cExplorer = new CollectionExplorer();
            _editor = new Editor(null);
            _infos = new Infos(_eventBus);

            _editor.OnEditorImageUpdated += (updatedBitmap) =>
            {
                _photoViewer?.SetImage(updatedBitmap);
                _photoViewer?.ResetOverlay();
            };

            _cExplorer.OnImageSelected += (img) =>
            {
                ImageManager.Instance.SelectImage(img);
            };

            _cExplorer.ExportRequestedEvent += () =>
            {
                ICollection<LuxAsset> images = ImageManager.Instance.OpenedImages.Select(img => img.ToLuxAsset()).ToList();

                _eventBus?.Publish(
                    new ExportRequestEvent
                    {
                        Assets = images,
                    }
                );
            };

            ImageManager.Instance.OnSelectionChanged += (img) =>
            {
                _editor?.SetEditableImage(img);
                _photoViewer?.SetImage(img.PreviewBitmap ?? img.EditedBitmap ?? img.OriginalBitmap);
                _infos?.DisplayExifData(img.Metadata);
                _photoViewer?.SetEditableImage(img);
            };

            mainPage.Add(SmartButtonType.MainPanel, _photoViewer);
            mainPage.Add(SmartButtonType.BottomPanel, _cExplorer);
            mainPage.Add(SmartButtonType.RightPanel, _editor);
            mainPage.Add(SmartButtonType.LeftPanel, _infos);

            smartButtons.Add(new SmartButton("Editor", "Editor module", mainPage));
            Items.Add(new LuxMenuBarItem("LuxEditor", false, Guid.NewGuid(), smartButtons));

            _eventBus.Subscribe<CollectionUpdatedEvent>(OnCollectionUpdated);
            _eventBus?.Subscribe<RequestLatestCollection>(e =>
            {
                e.OnHandleReceived?.Invoke(
                    ImageManager.Instance.OpenedImages.Select(img => img.ToLuxAsset()).ToList()
                );
            });

            _logger?.Log($"{Name} initialized", "LuxEditor", LogLevel.Info);
        }

        /// <summary>
        /// Called when the image collection is updated. Converts assets into EditableImage objects.
        /// </summary>
        public void OnCollectionUpdated(CollectionUpdatedEvent body)
        {
            _logger?.Log($"Collection updated: {body.CollectionName}", "LuxEditor", LogLevel.Info);

            var editableImages = new List<EditableImage>();

            foreach (var asset in body.Assets)
            {
                editableImages.Add(
                    new(asset)
                    {
                        ThumbnailBitmap = ImageProcessingManager.GeneratePreview(asset.Data.Bitmap, 200),
                        PreviewBitmap = ImageProcessingManager.GeneratePreview(asset.Data.Bitmap, 500),
                        MediumBitmap = ImageProcessingManager.GenerateMediumResolution(asset.Data.Bitmap)
                    }
                );
            }

            ImageManager.Instance.LoadImages(editableImages);
            _cExplorer?.SetImages(editableImages);            
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
            _eventBus?.Unsubscribe<CollectionUpdatedEvent>(OnCollectionUpdated);
            _logger?.Log($"{Name} shut down", "LuxEditor", LogLevel.Info);
        }
    }
}
