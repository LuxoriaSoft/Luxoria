using LuxExport.Logic;
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
        private readonly List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps;
        private readonly ExportViewModel _viewModel;

        private CancellationTokenSource _cts = new();

        private bool _isPaused;
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        public ExportProgressWindow(
            List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> bitmaps,
            ExportViewModel viewModel)
        {
            InitializeComponent();

            _bitmaps = bitmaps;
            _viewModel = viewModel;

            this.AppWindow.Resize(new SizeInt32(400, 300));

            this.Activated += ExportProgressWindow_Activated;
        }

        private void ExportProgressWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= ExportProgressWindow_Activated;

            StartExportInBackground();
        }

        /// <summary>
        /// Lance la boucle d’exportation sur un thread de fond (Task.Run),
        /// pour ne pas bloquer l’UI.
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
        /// La boucle d’export qui tourne sur un thread de fond.
        /// </summary>
        private async Task DoExportLoopAsync()
        {
            int total = _bitmaps.Count;

            for (int i = 0; i < total; i++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                _pauseEvent.Wait();

                var bitmap = _bitmaps[i].Key;
                var metadata = _bitmaps[i].Value;

                string originalFileName = metadata["File Name"];
                string fileName;

                var exporter = ExporterFactory.CreateExporter(_viewModel.SelectedFormat);

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
                    exporter.Export(bitmap, fullFilePath, _viewModel.SelectedFormat, settings);
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
        /// Convertit un SKBitmap en BitmapImage (pour l’aperçu).
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
        /// Génère un chemin unique "fichier (1).jpg" si le fichier existe déjà.
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
        /// Bouton Pause/Resume
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
        /// Bouton Cancel
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            _pauseEvent.Set();
        }
    }
}
