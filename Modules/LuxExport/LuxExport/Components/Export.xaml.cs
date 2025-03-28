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

namespace LuxExport
{
    public sealed partial class Export : Window
    {
        private AppWindow? _appWindow;
        private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps = new();
        private ExportViewModel viewModel;


        public Export()
        {
            this.InitializeComponent();
            viewModel = new ExportViewModel();

            viewModel.LoadPresets("C:\\Users\\noahg\\Desktop\\Github\\Luxoria\\assets\\Presets\\FileNamingPresets.json");
            RefreshPresetsMenu();
            

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(myWndId);

            if (_appWindow != null)
            {
                _appWindow.Resize(new SizeInt32(600, 400));
            }
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
                        string customPath = await PickFolderAsync();
                        if (!string.IsNullOrEmpty(customPath))
                        {
                            viewModel.SelectedExportLocation = "Custom Path";
                            viewModel.SetBasePath(customPath);
                        }
                        break;
                }

                viewModel.UpdateExportPath();
            }
        }


        private async Task<string> PickFolderAsync()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path ?? string.Empty;
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
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

            if (viewModel.RenameFile)
            {
                fileName = viewModel.GenerateFileName(originalFileName, metadata);
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

            string basePath = viewModel.ExportFilePath?.Trim();
            if (string.IsNullOrWhiteSpace(basePath))
            {
                Debug.WriteLine("Export path is empty.");
                return;
            }

            string exportPath = basePath;
            if (viewModel.CreateSubfolder && !string.IsNullOrWhiteSpace(viewModel.SubfolderName))
            {
                exportPath = Path.Combine(basePath, viewModel.SubfolderName.Trim());
            }

            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            string fullFilePath = Path.Combine(exportPath, fileName);

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
            viewModel.ExportImage(imageToExport);

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

    }
}
