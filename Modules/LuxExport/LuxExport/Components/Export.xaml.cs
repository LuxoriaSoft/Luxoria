using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.Threading.Tasks;
using LuxExport.Logic;
using Luxoria.Modules.Models.Events;
using Windows.Storage.Pickers;
using Luxoria.Modules.Interfaces;
using Windows.System;
using Luxoria.Modules.Models;
using System.Linq;
using Luxoria.SDK.Interfaces;
using Luxoria.Modules;
using Microsoft.Extensions.Logging.Abstractions;

namespace LuxExport
{
    /// <summary>
    /// The dialog responsible for handling export functionality, including file selection, format settings, and export location.
    /// </summary>
    public sealed partial class Export : Page
    {
        private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps = new();
        private ExportViewModel _viewModel;
        private readonly IEventBus _eventBus;
        public event Action? CloseWindow;
        private ILoggerService _logger;

        private WatermarkService _wmSvc;

        /// <summary>
        /// Initializes the export dialog and loads the necessary presets for file naming.
        /// </summary>
        public Export(IEventBus eventBus, ILoggerService logger)
        {
            this.InitializeComponent();
            _eventBus = eventBus;
            _logger = logger; 
            _viewModel = new ExportViewModel();
            _wmSvc = new WatermarkService(new VaultService(logger));
            _viewModel.Watermark = _wmSvc.Load();
            _viewModel.RefreshLogoPreview();

            _viewModel.LoadPresets(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\..\\..\\..\\assets\\Presets\\FileNamingPresets.json");

            RefreshPresetsMenu();

            RefreshWatermarkUI();
        }

        /// <summary>
        /// Refreshes the presets menu with the available presets loaded from the JSON file.
        /// </summary>
        private void RefreshPresetsMenu()
        {
            PresetsFlyout.Items.Clear();

            // Add each preset to the menu
            foreach (var preset in _viewModel.Presets)
            {
                var item = new MenuFlyoutItem { Text = preset.Name };
                item.Click += (s, e) =>
                {
                    _viewModel.CustomFileFormat = preset.Pattern;
                };
                PresetsFlyout.Items.Add(item);
            }
        }

        /// <summary>
        /// Handles export location selection from the menu, including predefined locations or custom path.
        /// </summary>
        private async void ExportLocation_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                _viewModel.SelectedExportLocation = menuItem.Text;

                switch (menuItem.Text)
                {
                    case "Desktop":
                        _viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    case "Documents":
                        _viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.MyDocuments);
                        break;
                    case "Pictures":
                        _viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.MyPictures);
                        break;
                    case "Same path as original file":
                        _viewModel.ExportFilePath = GetOriginalFilePath();
                        break;
                    case "Custom Path":
                        // Browse for a custom folder
                        StorageFolder folder = await BrowseFolderAsync();
                        if (folder != null)
                        {
                            _viewModel.SelectedExportLocation = "Custom Path";
                            _viewModel.SetBasePath(folder.Path);
                        }
                        else
                        {
                            _viewModel.SelectedExportLocation = "Select a path...";
                        }
                        break;
                }

                _viewModel.UpdateExportPath();
            }
        }

        /// <summary>
        /// Opens a folder picker to allow the user to select a custom export location.
        /// </summary>
        private async Task<StorageFolder?> BrowseFolderAsync()
        {
            var tcs = new TaskCompletionSource<nint>();
            if (_eventBus == null) return null;
            await _eventBus.Publish(new RequestWindowHandleEvent(handle => tcs.SetResult(handle)));
            nint _windowHandle = await tcs.Task;
            if (_windowHandle == 0) return null;

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, _windowHandle);

            return await picker.PickSingleFolderAsync();
        }

        /// <summary>
        /// Handles file conflict resolution (overwrite, rename, or skip) during export.
        /// </summary>
        private void FileConflictResolution_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                _viewModel.SelectedFileConflictResolution = menuItem.Text;
            }
        }

        /// <summary>
        /// Gets the path for a special system folder like Desktop or Documents.
        /// </summary>
        private string GetSpecialFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        /// <summary>
        /// Gets the original file path for the first bitmap if available.
        /// </summary>
        private string GetOriginalFilePath()
        {
            if (_bitmaps.Count > 0 && _bitmaps[0].Value.TryGetValue("File Path", out string path))
            {
                return Path.GetDirectoryName(path) ?? "Unknown";
            }
            return "Unknown";
        }

        /// <summary>
        /// Sets the bitmaps to be exported, clearing previous selections if necessary.
        /// </summary>
        public void SetBitmaps(List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> bitmaps)
        {
            if (bitmaps == null || bitmaps.Count == 0)
            {
                Debug.WriteLine("SetBitmaps: No bitmaps provided.");
                return;
            }

            _bitmaps.Clear();
            _bitmaps.AddRange(bitmaps);

            Debug.WriteLine($"SetBitmaps: {_bitmaps.Count} bitmaps added.");
        }

        /// <summary>
        /// Handles the color space selection during the export process.
        /// </summary>
        private void ColorSpace_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _viewModel.SelectedColorSpace = item.Text;
            }
        }

        /// <summary>
        /// Handles the export button click, initiating the export process.
        /// </summary>
        private async void ExportButton_Click(object sender, object e)
        {
            if (_bitmaps.Count == 0)
            {
                Debug.WriteLine("No images available for export.");
                return;
            }

            var tcs = new TaskCompletionSource<ICollection<LuxAsset>>();
            await _eventBus.Publish(new RequestLatestCollection(handle => tcs.SetResult(handle)));
            SetBitmaps(tcs.Task.GetAwaiter().GetResult().Select(x => new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(x.Data.Bitmap, x.Data.EXIF)).ToList());

            _wmSvc.Save(_viewModel.Watermark);

            CloseWindow?.Invoke();

            DispatcherQueue.TryEnqueue(() =>
            {
                var progressWindow = new ExportProgressWindow(_bitmaps, _viewModel, _logger);
                progressWindow.Activate();
            });
        }


        /// <summary>
        /// Handles the file naming mode selection.
        /// </summary>
        private void FileNamingMode_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _viewModel.FileNamingMode = item.Text;
            }
        }

        /// <summary>
        /// Handles the extension case selection (lowercase or uppercase).
        /// </summary>
        private void ExtensionCase_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _viewModel.ExtensionCase = item.Text;
            }
        }

        /// <summary>
        /// Generates a unique file path by appending a counter to avoid conflicts.
        /// </summary>
        private string GetUniqueFilePath(string filePath)
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
        /// Handles the image format selection during the export process.
        /// </summary>
        private void ImageFormat_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && Enum.TryParse<ExportFormat>(item.Text, true, out var format))
            {
                _viewModel.SelectedFormat = format;
            }
        }

        /// <summary>
        /// Cancels the export operation and closes the dialog.
        /// </summary>
        private void CancelButton_Click(object sender, object e)
        {
            CloseWindow?.Invoke();
        }

        /// <summary>
        /// Opens a FileOpenPicker, lets the user choose an image and stores it
        /// in the current WatermarkSettings.
        /// </summary>
        private async void OnImportLogoClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker { ViewMode = PickerViewMode.Thumbnail };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var tcs = new TaskCompletionSource<nint>();
            await _eventBus.Publish(new RequestWindowHandleEvent(h => tcs.SetResult(h)));
            nint windowHandle = await tcs.Task;
            if (windowHandle == 0) return;
            InitializeWithWindow.Initialize(picker, windowHandle);

            if (await picker.PickSingleFileAsync() is not StorageFile file) return;

            using var inStream = await file.OpenReadAsync();
            using var skStream = new SKManagedStream(inStream.AsStreamForRead());
            if (SKBitmap.Decode(skStream) is not SKBitmap logo) return;

            _viewModel.Watermark.Logo = logo;
            _viewModel.Watermark.Type = WatermarkType.Logo;
            _viewModel.Watermark.Enabled = true;

            _viewModel.Watermark = _viewModel.Watermark;

            _viewModel.RefreshLogoPreview();

            _wmSvc.Save(_viewModel.Watermark);
        }

        private void RefreshWatermarkUI()
        {
            bool isText = _viewModel.Watermark.Type == WatermarkType.Text;

            LblWatermarkText.Visibility = isText ? Visibility.Visible : Visibility.Collapsed;
            TxtWatermark.Visibility = isText ? Visibility.Visible : Visibility.Collapsed;

            BtnImportLogo.Visibility = isText ? Visibility.Collapsed : Visibility.Visible;
            LogoPreview.Visibility = isText ? Visibility.Collapsed : Visibility.Visible;
        }
        private void OnWatermarkTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox cb) return;

            _viewModel.Watermark.Type = cb.SelectedIndex == 0
                                        ? WatermarkType.Text
                                        : WatermarkType.Logo;

            _viewModel.Watermark = _viewModel.Watermark;
            RefreshWatermarkUI();
        }

    }
}
