using LuxExport.Logic;
using LuxExport.Models;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;

namespace LuxExport
{
    /// <summary>
    /// The dialog responsible for handling export functionality, including file selection, format settings, and export location.
    /// </summary>
    public sealed partial class Export : Page
    {
        //private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _bitmaps = new();
        private ICollection<LuxAsset> _assets = new Collection<LuxAsset>();
        private ExportViewModel _viewModel;
        private readonly IEventBus _eventBus;
        public event Action? CloseWindow;
        private ILoggerService _logger;

        private WatermarkService _wmSvc;
        private ExportPresetService _presetSvc;
        private IStorageAPI _storageAPI;

        /// <summary>
        /// Initializes the export dialog and loads the necessary presets for file naming.
        /// </summary>
        public Export(IEventBus eventBus, ILoggerService logger, IStorageAPI storageAPI)
        {
            this.InitializeComponent();
            _eventBus = eventBus;
            _logger = logger; 
            _storageAPI = storageAPI;
            _viewModel = new ExportViewModel();
            this.DataContext = _viewModel;
            _viewModel.RefreshLogoPreview();

            _wmSvc = new WatermarkService(storageAPI);
            _presetSvc = new ExportPresetService(storageAPI);
            
            // Load file naming presets (legacy support)
            if (storageAPI.Contains("fileNamingPresets"))
                _viewModel.LoadPresets(storageAPI.Get<string>("fileNamingPresets"));
            
            // Load export presets
            _viewModel.LoadExportPresets(_presetSvc.GetPresets());
            
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            _viewModel.Watermark = _wmSvc.Load();
            UpdateWebVisibility();

            RefreshPresetsMenu();
            RefreshExportPresetsMenu();

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
        /// Refreshes the export presets menu with available export presets.
        /// </summary>
        private void RefreshExportPresetsMenu()
        {
            PresetComboBox.ItemsSource = _viewModel.ExportPresets;
            PresetComboBox.SelectedIndex = -1;
            PresetDescription.Text = "Select a preset to see description";
            DeletePresetButton.IsEnabled = false;
        }

        /// <summary>
        /// Applies the selected export preset to current settings.
        /// </summary>
        public void ApplyExportPreset(string presetName)
        {
            var preset = _presetSvc.GetPreset(presetName);
            if (preset != null)
            {
                _viewModel.ApplyExportPreset(preset);
                // Update export path after applying location settings
                _viewModel.UpdateExportPath();
            }
        }

        /// <summary>
        /// Saves current settings as a new export preset.
        /// </summary>
        public void SaveCurrentSettingsAsPreset(string presetName, string? description = null)
        {
            var preset = _viewModel.CreatePresetFromCurrentSettings(presetName, description);
            _presetSvc.AddPreset(preset);
            _viewModel.LoadExportPresets(_presetSvc.GetPresets());
            RefreshExportPresetsMenu();
        }

        /// <summary>
        /// Removes an export preset by name.
        /// </summary>
        public void RemoveExportPreset(string presetName)
        {
            if (_presetSvc.RemovePreset(presetName))
            {
                _viewModel.LoadExportPresets(_presetSvc.GetPresets());
                RefreshExportPresetsMenu();
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
            if (_assets.Count > 0 && _assets.First().Data.EXIF.TryGetValue("File Path", out string path))
            {
                return Path.GetDirectoryName(path) ?? "Unknown";
            }
            return "Unknown";
        }

        /// <summary>
        /// Sets the bitmaps to be exported, clearing previous selections if necessary.
        /// </summary>
        public void SetAssets(ICollection<LuxAsset> assets)
        {
            if (assets == null || assets.Count == 0)
            {
                Debug.WriteLine("SetBitmaps: No assets provided.");
                return;
            }

            _assets.Clear();
            _assets = assets;

            Debug.WriteLine($"SetBitmaps: {_assets.Count} bitmaps added.");
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
            if (_assets.Count == 0)
            {
                Debug.WriteLine("No images available for export.");
                return;
            }
            //if (_viewModel.IsWebExport)
            //{
                //var tcs = new TaskCompletionSource<ICollection<LuxAsset>>();
                //await _eventBus.Publish(new RequestExportOnlineEvent(tcs.Task.GetAwaiter().GetResult()));
            //} else
            //{
            var tcs = new TaskCompletionSource<ICollection<LuxAsset>>();
            await _eventBus.Publish(new RequestLatestCollection(handle => tcs.SetResult(handle)));
            SetAssets(tcs.Task.GetAwaiter().GetResult().Where(x => x.IsVisibleAfterFilter).ToList());

            _wmSvc.Save(_viewModel.Watermark);

            CloseWindow?.Invoke();

            DispatcherQueue.TryEnqueue(() =>
            {
                var progressWindow = new ExportProgressWindow(_assets, _viewModel, _logger, _eventBus, _storageAPI);
                progressWindow.Activate();
            });
            //}
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

        private void ExportTarget_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _viewModel.SelectedExportTarget = item.Text;
                // Remove manual Content update - let data binding handle it
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExportViewModel.IsWebExport))
            {
                UpdateWebVisibility();
            }
        }

        private void UpdateWebVisibility()
        {
            FileNamingExpander.Visibility = _viewModel.IsWebExport
                ? Visibility.Collapsed
                : Visibility.Visible;
            ExportLocationExpander.Visibility = _viewModel.IsWebExport
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        /// <summary>
        /// Handles preset selection changes in the ComboBox.
        /// </summary>
        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ExportPreset selectedPreset)
            {
                // Update description
                PresetDescription.Text = string.IsNullOrWhiteSpace(selectedPreset.Description) 
                    ? "No description available" 
                    : selectedPreset.Description;
                
                // Enable delete button for custom presets (not default ones)
                var defaultPresetNames = new[] { "High Quality JPEG", "Web Optimized", "Social Media", "PNG Lossless", "HEIF Modern", "Print Ready", "Portfolio", "Archive Copy" };
                DeletePresetButton.IsEnabled = !defaultPresetNames.Contains(selectedPreset.Name);
                
                // Apply the preset
                ApplyExportPreset(selectedPreset.Name);
            }
            else
            {
                PresetDescription.Text = "Select a preset to see description";
                DeletePresetButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the Save Preset button click.
        /// </summary>
        private async void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "Save Export Preset",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 10 };
            
            var nameTextBox = new TextBox
            {
                Header = "Preset Name",
                PlaceholderText = "Enter preset name...",
                MaxLength = 50
            };
            
            var descriptionTextBox = new TextBox
            {
                Header = "Description (Optional)",
                PlaceholderText = "Enter preset description...",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                MaxHeight = 60
            };

            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(descriptionTextBox);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                try
                {
                    SaveCurrentSettingsAsPreset(nameTextBox.Text.Trim(), 
                        string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? null : descriptionTextBox.Text.Trim());
                    
                    // Select the newly created preset
                    var newPreset = _viewModel.ExportPresets.FirstOrDefault(p => p.Name == nameTextBox.Text.Trim());
                    if (newPreset != null)
                    {
                        PresetComboBox.SelectedItem = newPreset;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to save preset: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles the Delete Preset button click.
        /// </summary>
        private async void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetComboBox.SelectedItem is not ExportPreset selectedPreset)
                return;

            var dialog = new ContentDialog()
            {
                Title = "Delete Preset",
                Content = $"Are you sure you want to delete the preset '{selectedPreset.Name}'?\n\nThis action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    RemoveExportPreset(selectedPreset.Name);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to delete preset: {ex.Message}");
                }
            }
        }

    }
}
