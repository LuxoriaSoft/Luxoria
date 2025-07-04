using CommunityToolkit.WinUI;
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
using Microsoft.UI.Dispatching;
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
        private Guid? _collectionId;

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

            _editor.AttachCropController(_photoViewer.CropController);
            _editor.OnEditorImageUpdated += (updatedBitmap) =>
            {
                _photoViewer?.SetImage(updatedBitmap);
                _photoViewer?.ResetOverlay();
            };

            _editor.IsCropModeChanged += (isCropMode) =>
            {
                _photoViewer.IsCropMode = isCropMode;
            };

            _editor.InvalidateCrop += () =>
            {
                _photoViewer?.InvalidateCrop();
            };


            _photoViewer.CropChanged += () => {
                _editor?.RequestFilterUpdate();

            };

            _photoViewer.BeginCropEditing += () => _editor.BeginCropEditing();
            _photoViewer.EndCropEditing += () => _editor.EndCropEditing();

            _editor.CropBoxChanged += box =>
            {
                var ctl = _photoViewer.CropController;
                ctl.SetSize(box.Width, box.Height);
                ctl.SetAngle(box.Angle);
                ctl.LockAspectRatio = _editor.LockAspectToggleIsOn;
                _photoViewer.InvalidateCrop();
            };

            _cExplorer.OnImageSelected += (img) =>
            {
                ImageManager.Instance.SelectImage(img);
                if (_collectionId != null)
                    _infos?.OnWebCollectionSelected(_collectionId ?? throw new Exception("Collection Id cannot be null here"));
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
            _eventBus?.Subscribe<WebCollectionSelectedEvent>(OnWebCollectionSelected);

            _eventBus?.Subscribe<UpdateUpdatedAssetEvent>(OnUpdateUpdatedAsset);

            _logger?.Log($"{Name} initialized", "LuxEditor", LogLevel.Info);
        }

        /// <summary>
        /// Called when the image collection is updated. Converts assets into EditableImage objects.
        /// </summary>
        private void OnCollectionUpdated(CollectionUpdatedEvent body)
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
                    }
                );
            }

            ImageManager.Instance.LoadImages(editableImages);
            _cExplorer?.SetImages(editableImages);            
        }

        private void OnWebCollectionSelected(WebCollectionSelectedEvent body)
        {
            _infos.OnWebCollectionSelected(body.CollectionId);
            _collectionId = body.CollectionId;
        }

        private async void OnUpdateUpdatedAsset(UpdateUpdatedAssetEvent body)
        {
            EditableImage imageToModify = ImageManager.Instance.OpenedImages.First(img => img.Id == body.AssetId);
            int index = ImageManager.Instance.OpenedImages.IndexOf(imageToModify);

            imageToModify.LuxCfg.LastUploadId = body.LastUploadedId;
            imageToModify.LuxCfg.CollectionId = body.CollectionId;
            imageToModify.LuxCfg.StudioUrl = body.Url;

            ImageManager.Instance.OpenedImages[index] = imageToModify;

            if (imageToModify == ImageManager.Instance.SelectedImage && _infos is not null)
            {
                await _infos.DispatcherQueue.EnqueueAsync(() =>
                {
                    _infos.OnWebCollectionSelected(body.CollectionId);
                });
            }
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
