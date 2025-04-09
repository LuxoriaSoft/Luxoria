using LuxEditor.EditorUI;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using LuxEditor.EditorUI.Interfaces;
using LuxEditor.EditorUI.Models;
using LuxEditor.Models;
using LuxEditor.Processing;
using LuxEditor.Services;
using LuxEditor.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private EditableImage? currentImage;
        private Task applyFiltersTask;
        private bool pendingUpdate;
        private readonly object updateLock = new();

        private EditorPanelManager? _panelManager;
        private readonly Dictionary<string, EditorCategory> _categories = new();
        private bool ctrlPressed = false;

        public event Action<BitmapImage> OnEditorImageUpdated;

        public Editor()
        {
            InitializeComponent();

            _panelManager = new EditorPanelManager(EditorStackPanel);

            ImageManager.Instance.OnSelectionChanged += SetEditableImage;
        }

        public void SetEditableImage(EditableImage image)
        {
            ExifHelper.DebugPrintMetadata(image.Metadata, image.FileName);

            currentImage = image;
            this.Focus(FocusState.Programmatic);

            EditorStackPanel.Children.Clear();
            _categories.Clear();
            BuildEditorUI();

            UpdateSliderUI();
            ApplyFilters();
        }

        private void BuildEditorUI()
        {
            var rootExpander = new EditorGroupExpander("Basic");

            AddCategory(rootExpander, "WhiteBalance", "White Balance", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Temperature", new EditorStyle
                {
                    GradientStart = Windows.UI.Color.FromArgb(255, 70, 130, 180),
                    GradientEnd = Windows.UI.Color.FromArgb(255, 255, 140, 0)
                }),
                CreateSliderWithPreset("Tint"),
                CreateSeparator()
            });

            AddCategory(rootExpander, "Tone", "Tone", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Exposure"),
                CreateSliderWithPreset("Contrast"),
                CreateSeparator(),

                CreateSliderWithPreset("Highlights"),
                CreateSliderWithPreset("Shadows"),
                CreateSeparator(),

                CreateSliderWithPreset("Whites"),
                CreateSliderWithPreset("Blacks"),
                CreateSeparator()
            });

            AddCategory(rootExpander, "Presence", "Presence", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Texture"),
                CreateSliderWithPreset("Clarity"),
                CreateSliderWithPreset("Dehaze"),
                CreateSeparator(),

                CreateSliderWithPreset("Vibrance"),
                CreateSliderWithPreset("Saturation")
            });

            _panelManager!.AddCategory(rootExpander);
        }

        private EditorSlider CreateSliderWithPreset(string key, EditorStyle? style = null)
        {
            var (min, max, def, decimals, step) = GetSliderPreset(key);
            return CreateSlider(key, min, max, def, style, decimals, step);
        }

        private (float min, float max, float def, int decimals, float step) GetSliderPreset(string key)
        {
            var meta = currentImage?.Metadata;

            float min = -100, max = 100, def = 0;
            int decimals = 0;
            float step = 1f;

            if (key == "Temperature")
            {
                float? wbKelvin = ExifHelper.TryGetRawWhiteBalanceKelvin(meta);
                if (wbKelvin.HasValue)
                {
                    min = 2000;
                    max = 50000;
                    def = wbKelvin.Value;
                    decimals = 0;
                    step = 100;
                }

            }
            else if (key == "Exposure")
            {
                min = -5;
                max = 5;
                def = 0;
                decimals = 2;
                step = 0.05f;
            }
            else if (key == "Contrast" || key == "Highlights" || key == "Shadows" ||
                     key == "Whites" || key == "Blacks" || key == "Clarity" ||
                     key == "Texture" || key == "Dehaze" || key == "Vibrance" || key == "Saturation")
            {
                min = -100;
                max = 100;
                def = 0;
                decimals = 0;
                step = 1;
            }
            else if (key == "Tint")
            {
                min = -150;
                max = 150;
                def = 0;
                decimals = 0;
                step = 1;
            }

            return (min, max, def, decimals, step);
        }

        private float? TryExtractColorTemperature()
        {
            if (currentImage?.Metadata == null)
                return null;

            var meta = currentImage.Metadata;

            if (meta.TryGetValue("ColorTemperature", out var colorTempRaw) &&
                float.TryParse(colorTempRaw, out float kelvin))
            {
                return kelvin;
            }

            if (meta.TryGetValue("AsShotNeutral", out var neutralRaw))
            {
                var parts = neutralRaw.Split(','); // e.g. "0.512,1,0.610"
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out var r) &&
                    float.TryParse(parts[2], out var b))
                {
                    // simple heuristic based on neutral RGB
                    float ratio = r / b;
                    float estimatedK = 6500 * (1f / ratio);
                    return Math.Clamp(estimatedK, 2000f, 50000f);
                }
            }

            if (meta.TryGetValue("WB_RGGBLevels", out var rggbRaw))
            {
                var vals = rggbRaw.Split(',');
                if (vals.Length == 4 &&
                    float.TryParse(vals[0], out float R) &&
                    float.TryParse(vals[3], out float B))
                {
                    float ratio = R / B;
                    return Math.Clamp(6500f * (1f / ratio), 2000, 50000);
                }
            }

            return null;
        }


        private EditorSlider CreateSlider(string key, float min, float max, float def,
                                          EditorStyle? style = null, int decimalPlaces = 0, float step = 1f)
        {
            var slider = new EditorSlider(key, min, max, def, decimalPlaces, step);

            slider.OnValueChanged = val =>
            {
                if (currentImage == null) return;
                currentImage.Filters[key] = val;
                ApplyFilters();
                UpdateResetButtonsVisibility();
            };

            if (style != null)
                slider.ApplyStyle(style);

            _panelManager!.RegisterSlider(key, slider);
            return slider;
        }

        private EditorSeparator CreateSeparator() => new EditorSeparator();

        private void AddCategory(EditorGroupExpander parent, string key, string title, IEnumerable<IEditorGroupItem> items)
        {
            var category = new EditorCategory(key, title);
            category.OnResetClicked += ResetCategory;

            foreach (var item in items)
                category.AddControl(item);

            _categories[key] = category;
            parent.AddCategory(category);
        }

        private void ResetCategory(string key)
        {
            if (!_categories.TryGetValue(key, out var category)) return;

            foreach (var item in category.GetItems())
            {
                if (item is EditorSlider slider)
                {
                    slider.ResetToDefault();
                    if (currentImage != null)
                        currentImage.Filters[slider.Key] = slider.DefaultValue;
                }
            }

            ApplyFilters();
            UpdateResetButtonsVisibility();
        }

        private void ResetAllClicked(object sender, RoutedEventArgs e)
        {
            foreach (var category in _categories.Values)
            {
                foreach (var item in category.GetItems())
                {
                    if (item is EditorSlider slider)
                    {
                        slider.ResetToDefault();
                        if (currentImage != null)
                            currentImage.Filters[slider.Key] = slider.DefaultValue;
                    }
                }
            }

            ApplyFilters();
            UpdateResetButtonsVisibility();
        }

        private void UpdateSliderUI()
        {
            if (currentImage == null || _panelManager == null) return;

            foreach (var (key, value) in currentImage.Filters)
            {
                _panelManager.GetSlider(key)?.SetValue(value);
            }

            UpdateResetButtonsVisibility();
        }

        private void UpdateResetButtonsVisibility()
        {
            if (currentImage == null) return;

            foreach (var (key, category) in _categories)
            {
                bool modified = category.GetItems().Any(item =>
                    item is EditorSlider slider &&
                    Math.Abs(slider.GetValue() - slider.DefaultValue) > 0.01f
                );

                category.SetResetVisible(modified);
            }

            bool anyChanged = currentImage.Filters.Any(f =>
            {
                var s = _panelManager!.GetSlider(f.Key);
                return s != null && Math.Abs(s.GetValue() - s.DefaultValue) > 0.01f;
            });

            ResetAllButton.Visibility = anyChanged ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ApplyFilters()
        {
            lock (updateLock)
            {
                if (applyFiltersTask != null && !applyFiltersTask.IsCompleted)
                {
                    pendingUpdate = true;
                    return;
                }
                applyFiltersTask = ApplyFiltersAsync();
            }

            await applyFiltersTask;
        }

        private async Task ApplyFiltersAsync()
        {
            await Task.Delay(50);

            lock (updateLock)
            {
                if (pendingUpdate)
                {
                    pendingUpdate = false;
                    applyFiltersTask = ApplyFiltersAsync();
                    return;
                }
            }

            if (currentImage?.OriginalBitmap == null) return;

            var result = ImageProcessor.ApplyFilters(currentImage.OriginalBitmap, currentImage.Filters);
            currentImage.EditedBitmap = result;

            UpdateImage(currentImage.EditedBitmap);
        }

        private void UpdateImage(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var outputStream = stream.AsStreamForWrite();
            data.SaveTo(outputStream);
            outputStream.Flush();
            outputStream.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(stream);
            OnEditorImageUpdated?.Invoke(bitmapImage);
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
                ctrlPressed = true;

            if (ctrlPressed && e.Key == Windows.System.VirtualKey.Z && currentImage?.Undo() == true)
            {
                UpdateSliderUI();
                ApplyFilters();
            }
            else if (ctrlPressed && e.Key == Windows.System.VirtualKey.Y && currentImage?.Redo() == true)
            {
                UpdateSliderUI();
                ApplyFilters();
            }
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
                ctrlPressed = false;
        }
    }
}
