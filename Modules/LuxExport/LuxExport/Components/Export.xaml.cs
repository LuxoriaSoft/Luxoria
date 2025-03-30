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

namespace LuxExport
{
    public sealed partial class Export : ContentDialog
    {
        private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps = new();
        private ExportViewModel viewModel;
        public IEventBus? _eventBus;

        public Export()
        {
            this.InitializeComponent();
            viewModel = new ExportViewModel();

            viewModel.LoadPresets("C:\\Users\\noahg\\Desktop\\Github\\Luxoria\\assets\\Presets\\FileNamingPresets.json");
            RefreshPresetsMenu();
        }

        private void RefreshPresetsMenu()
        {
            PresetsFlyout.Items.Clear();

            foreach (var preset in viewModel.Presets)
            {
                var item = new MenuFlyoutItem { Text = preset.Name };
                item.Click += (s, e) =>
                {
                    viewModel.CustomFileFormat = preset.Pattern;
                };
                PresetsFlyout.Items.Add(item);
            }
        }


        private async void ExportLocation_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                viewModel.SelectedExportLocation = menuItem.Text;

                switch (menuItem.Text)
                {
                    case "Desktop":
                        viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    case "Documents":
                        viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.MyDocuments);
                        break;
                    case "Pictures":
                        viewModel.ExportFilePath = GetSpecialFolderPath(Environment.SpecialFolder.MyPictures);
                        break;
                    case "Same path as original file":
                        viewModel.ExportFilePath = GetOriginalFilePath();
                        break;
                    case "Custom Path":
                        StorageFolder folder = await BrowseFolderAsync();
                        if (folder != null)
                        {
                            viewModel.SelectedExportLocation = "Custom Path";
                            viewModel.SetBasePath(folder.Path);
                        }
                        else
                        {
                            viewModel.SelectedExportLocation = "Select a path...";
                        }
                        break;
                }

                viewModel.UpdateExportPath();
            }
        }

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

        private void FileConflictResolution_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                viewModel.SelectedFileConflictResolution = menuItem.Text;
            }
        }

        private string GetSpecialFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        private string GetOriginalFilePath()
        {
            if (_bitmaps.Count > 0 && _bitmaps[0].Value.TryGetValue("File Path", out string path))
            {
                return Path.GetDirectoryName(path) ?? "Unknown";
            }
            return "Unknown";
        }

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

        private void ColorSpace_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                viewModel.SelectedColorSpace = item.Text;
            }
        }


        private void ExportButton_Click(object sender, object e)
        {
            if (_bitmaps.Count == 0)
            {
                Debug.WriteLine("No images available for export.");
                return;
            }

            SKBitmap imageToExport = _bitmaps[0].Key;
            string originalFileName = _bitmaps[0].Value["File Name"];
            var metadata = _bitmaps[0].Value;
            string fileName;
            var exporter = ExporterFactory.CreateExporter(viewModel.SelectedFormat);
            var settings = new ExportSettings
            {
                Quality = viewModel.Quality,
                ColorSpace = viewModel.SelectedColorSpace,
                LimitFileSize = viewModel.LimitFileSize,
                MaxFileSizeKB = viewModel.MaxFileSizeKB
            };

            if (viewModel.RenameFile)
            {
                string fileNameWithoutExt = viewModel.GenerateFileName(originalFileName, metadata);
                string ext = viewModel.ExtensionCase == "a..z"
                    ? viewModel.GetExtensionFromFormat().ToLowerInvariant()
                    : viewModel.GetExtensionFromFormat().ToUpperInvariant();

                fileName = $"{fileNameWithoutExt}.{ext}";

            }
            else
            {
                fileName = originalFileName;
            }

            if (imageToExport == null || string.IsNullOrWhiteSpace(fileName))
            {
                Debug.WriteLine("Invalid image or file name.");
                return;
            }

            string path = viewModel.ExportFilePath?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.WriteLine("Export path is empty.");
                return;
            }

            //string exportPath = path;
            //if (viewModel.CreateSubfolder && !string.IsNullOrWhiteSpace(viewModel.SubfolderName))
            //{
            //    exportPath = Path.Combine(path, viewModel.SubfolderName.Trim());
            //}

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullFilePath = Path.Combine(path, fileName);

            switch (viewModel.SelectedFileConflictResolution)
            {
                case "Overwrite":
                    if (File.Exists(fullFilePath)) File.Delete(fullFilePath);
                    break;
                case "Rename":
                    fullFilePath = GetUniqueFilePath(fullFilePath);
                    break;
                case "Skip":
                    if (File.Exists(fullFilePath))
                    {
                        Debug.WriteLine("Export skipped: file already exists.");
                        return;
                    }
                    break;
            }

            viewModel.FilePath = fullFilePath;

            exporter.Export(imageToExport, viewModel.FilePath, viewModel.SelectedFormat, settings);

            Debug.WriteLine($"Export successful: {fullFilePath}");
        }

        private void FileNamingMode_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                viewModel.FileNamingMode = item.Text;
            }
        }

        private void ExtensionCase_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                viewModel.ExtensionCase = item.Text;
            }
        }


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

        private void ImageFormat_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && Enum.TryParse<ExportFormat>(item.Text, true, out var format))
            {
                viewModel.SelectedFormat = format;
            }
        }

        private void CancelButton_Click(object sender, object e)
        {
            Hide();
        }

    }
}
