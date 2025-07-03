using LuxExport.Interfaces;
using LuxExport.Logic;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.SDK.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage.Streams;

namespace LuxExport
{
    public sealed partial class ExportProgressWindow : Window
    {
        //private readonly List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps;
        private readonly ICollection<LuxAsset> _assets;
        private readonly ExportViewModel _viewModel;

        private CancellationTokenSource _cts = new();

        private bool _isPaused;
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        ILoggerService _logger;

        private readonly IEventBus _eventBus;

        /// <summary>
        /// Constructs the export progress window with the given bitmaps and view model.
        /// </summary>
        /// <param name="bitmaps"> Bitmaps with Metadata </param>
        /// <param name="viewModel"></param>
        public ExportProgressWindow(ICollection<LuxAsset> assets, ExportViewModel viewModel, ILoggerService logger, IEventBus eventBus)
        {
            InitializeComponent();
            _logger = logger;
            _assets = assets;
            _viewModel = viewModel;

            _eventBus = eventBus;

            this.AppWindow.Resize(new SizeInt32(400, 300));

            this.Activated += ExportProgressWindow_Activated;
        }


        /// <summary>
        /// Starts the export loop on a background thread.
        /// </summary>
        private void ExportProgressWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= ExportProgressWindow_Activated;

            StartExportInBackground();
        }

        /// <summary>
        /// Start Export loop in a background thread
        /// </summary>
        private void StartExportInBackground()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await DoExportLoopAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Export error: {ex.Message}");
                }
                finally
                {
                    await DispatcherQueue.EnqueueAsync(() => this.Close());
                }
            });
        }

        /// <summary>
        /// Export loop
        /// </summary>
        private async Task DoExportLoopAsync()
        {
            int total = _assets.Count;
            var wmService = new WatermarkService(new VaultService(_logger));
            var wm = wmService.Load();
            var i = 0;

            foreach (var asset in _assets)
            {
                i++;
                if (_cts.IsCancellationRequested)
                    break;

                _pauseEvent.Wait();

                var bitmap = asset.Data.Bitmap;
                var metadata = asset.Data.EXIF;

                string originalFileName = metadata["File Name"];
                string fileName;

                IExporter exporter;

                if (_viewModel.IsWebExport)
                {
                    exporter = ExporterFactory.CreateExporter(ExportFormat.LuxStudio);
                }
                else
                {
                    exporter = ExporterFactory.CreateExporter(_viewModel.SelectedFormat);
                }

                    var settings = new ExportSettings
                    {
                        Quality = _viewModel.Quality,
                        ColorSpace = _viewModel.SelectedColorSpace,
                        LimitFileSize = _viewModel.LimitFileSize,
                        MaxFileSizeKB = _viewModel.MaxFileSizeKB
                    };

                if (_viewModel.RenameFile)
                {
                    string nameWithoutExt = _viewModel.GenerateFileName(originalFileName, metadata, i);
                    string ext = _viewModel.ExtensionCase == "a..z"
                        ? _viewModel.GetExtensionFromFormat().ToLowerInvariant()
                        : _viewModel.GetExtensionFromFormat().ToUpperInvariant();

                    fileName = $"{nameWithoutExt}.{ext}";
                }
                else
                {
                    fileName = originalFileName;
                }

                string exportPath = _viewModel.ExportFilePath;
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                string fullFilePath = Path.Combine(exportPath, fileName);

                switch (_viewModel.SelectedFileConflictResolution)
                {
                    case "Overwrite":
                        if (File.Exists(fullFilePath))
                            File.Delete(fullFilePath);
                        break;
                    case "Rename":
                        fullFilePath = GetUniqueFilePath(fullFilePath);
                        break;
                    case "Skip":
                        if (File.Exists(fullFilePath))
                        {
                            continue;
                        }
                        break;
                }

                _viewModel.FilePath = fullFilePath;

                if (!_cts.IsCancellationRequested)
                {
                    var colourConverted = ConvertColorSpace(bitmap, _viewModel.SelectedColorSpace);
                    var wmApplied = WatermarkApplier.Apply(colourConverted, _viewModel.Watermark);
                    await exporter.Export(wmApplied, asset, fullFilePath, _viewModel.SelectedFormat, settings, _eventBus);
                }

                int index = i;
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    ProgressBar.Value = (index + 1) * 100.0 / total;
                    StatusText.Text = $"Exporting {index + 1} / {total}";

                    PreviewImage.Source = await ConvertToBitmapImageAsync(bitmap);
                });
            }
        }
        /// <summary>
        /// Creates an SKColorSpace from the given name.
        /// </summary>
        /// <param name="colorSpaceName"></param>
        /// <returns></returns>
        private SKColorSpace? CreateColorSpaceFromName(string colorSpaceName)
        {
            switch (colorSpaceName)
            {
                case "sRGB":
                    return SKColorSpace.CreateSrgb();

                case "LinearSRGB":
                    return SKColorSpace.CreateSrgbLinear();

                case "AdobeRGB":
                    byte[] adobeIcc = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\..\\..\\..\\assets\\ColorProfile\\AdobeRGB1998.icc");
                    return SKColorSpace.CreateIcc(adobeIcc);

                default:
                    return SKColorSpace.CreateSrgb();
            }
        }

        /// <summary>
        /// Converts an SKBitmap to the specified color space.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="colorSpaceName"></param>:
        /// <returns></returns>
        private SKBitmap ConvertColorSpace(SKBitmap original, string colorSpaceName)
        {
            var targetSpace = CreateColorSpaceFromName(colorSpaceName);
            if (targetSpace == null || targetSpace.Equals(original.ColorSpace))
                return original;

            var newInfo = new SKImageInfo(original.Width, original.Height, original.ColorType, original.AlphaType, targetSpace);
            var newBitmap = new SKBitmap(newInfo);

            using (var canvas = new SKCanvas(newBitmap))
            {
                canvas.DrawBitmap(original, 0, 0);
            }

            return newBitmap;
        }


        /// <summary>
        /// SKBitmap to BitmapImage (for preview).
        /// </summary>
        private static async Task<BitmapImage> ConvertToBitmapImageAsync(SKBitmap bitmap)
        {
            using var ms = new MemoryStream();
            using var wstream = new SKManagedWStream(ms);
            bitmap.Encode(wstream, SKEncodedImageFormat.Jpeg, 20);
            wstream.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var ras = new InMemoryRandomAccessStream();
            await ras.WriteAsync(ms.ToArray().AsBuffer());
            ras.Seek(0);

            var image = new BitmapImage();
            await image.SetSourceAsync(ras);
            return image;
        }

        /// <summary>
        /// Create a new filePath if file path already exist
        /// </summary>
        private static string GetUniqueFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? "";
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int counter = 1;

            string newFilePath = filePath;
            while (File.Exists(newFilePath))
            {
                newFilePath = Path.Combine(directory, $"{fileNameWithoutExt} ({counter}){extension}");
                counter++;
            }

            return newFilePath;
        }

        /// <summary>
        /// Pause And Resume Button
        /// </summary>
        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                _isPaused = false;
                PauseResumeButton.Content = "Pause";
                _pauseEvent.Set();
            }
            else
            {
                _isPaused = true;
                PauseResumeButton.Content = "Resume";
                _pauseEvent.Reset();
            }
        }

        /// <summary>
        /// Cancel Button
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            _pauseEvent.Set();
        }
    }
}
