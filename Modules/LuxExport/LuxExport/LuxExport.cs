using LuxExport.Logic;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        private IStorageAPI? _storageAPI;
        private static string VAULT_NAME = "LuxExport";

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

            _storageAPI = Task.Run(async () =>
            {
                var tcs = new TaskCompletionSource<IStorageAPI>();

                await eventBus.Publish(new RequestStorageAPIEvent(VAULT_NAME, handle => tcs.SetResult(handle)));

                return await tcs.Task;
            }).GetAwaiter().GetResult();

            if (!_storageAPI.Contains("fileNamingPresets"))
                _storageAPI.Save("fileNamingPresets", File.ReadAllText(ExtractEmbeddedResource("LuxExport.Assets.Presets.FileNamingPresets.json")));

            _export = new Export(_eventBus, _logger, _storageAPI);
            mainPage.Add(SmartButtonType.Window, _export);

            var smrtBtn = new SmartButton("Export", "Export module", mainPage);
            _export.CloseWindow += () =>
            {
                smrtBtn.OnClose.Invoke();
            };

            smartButtons.Add(smrtBtn);

            Items.Add(new LuxMenuBarItem("Export", false, new Guid(), smartButtons));

            _eventBus.Subscribe<ExportRequestEvent>((e) =>
            {
                e.Assets = e.Assets.Where(x => x.IsVisibleAfterFilter).ToList();
                OnCollectionUpdated(e.Assets);
                Export specificExport = new Export(_eventBus, _logger, _storageAPI);

                specificExport?.SetAssets(e.Assets);

                Window window = new Window() { Content = specificExport };

                specificExport.CloseWindow += () =>
                {
                    window.Close();
                };

                window.AppWindow.Resize(new (800, 650));

                window?.Activate();
            });


            _eventBus.Subscribe<CollectionUpdatedEvent>((e) => { OnCollectionUpdated(e.Assets.Where(x => x.IsVisibleAfterFilter).ToList()); });

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

            _logger?.Log($"{Name} shut down", "Mods/LuxExport", LogLevel.Info);
        }

        /// <summary>
        /// Handles the CollectionUpdatedEvent, which occurs when the collection of assets is updated.
        /// </summary>
        public void OnCollectionUpdated(ICollection<LuxAsset> assets)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                ImageData imageData = assets.ElementAt(i).Data;
                _logger?.Log($"Asset {i}: {assets.ElementAt(i).MetaData.Id}");
                _logger?.Log($"Asset info {i}: {imageData.Height}x{imageData.Width}, pixels: {imageData.Height * imageData.Width}");
            }

            //List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> lst = assets
            //    .Select(x => new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(x.Data.Bitmap, x.Data.EXIF))
            //    .ToList();

            Debug.WriteLine("Calling function ....");
            //Debug.WriteLine("Lst count: " + lst.Count);
            //Debug.WriteLine(lst);

            _export?.SetAssets(assets);
        }

        /// <summary>
        /// Extracts an embedded resource and writes it to a temporary file.
        /// </summary>
        /// <param name="resourceName">The resource name</param>
        /// <returns>Path to the extracted file</returns>
        private string ExtractEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource {resourceName} not found");

            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(resourceName));
            using FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);

            return tempFile;
        }
    }
}
