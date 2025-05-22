using LuxEditor.EditorUI;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using LuxEditor.EditorUI.Interfaces;
using LuxEditor.EditorUI.Models;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using LuxEditor.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public event Action<SKBitmap> OnEditorImageUpdated;

        public Editor()
        {
            InitializeComponent();
            _panelManager = new EditorPanelManager(EditorStackPanel);
            ImageManager.Instance.OnSelectionChanged += SetEditableImage;
        }

        public void SetEditableImage(EditableImage image)
        {
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
                CreateSliderWithPreset("Tint", new EditorStyle                {
                    GradientStart = Windows.UI.Color.FromArgb(255, 130, 188, 86),
                    GradientEnd = Windows.UI.Color.FromArgb(255, 174, 116, 193)
                }),
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
                //float? wbKelvin = ExifHelper.TryGetRawWhiteBalanceKelvin(meta);

                min = 2000;
                max = 50000;
                def = 6500;
                decimals = 0;
                step = 100;
            }
            else if (key == "Exposure")
            {
                min = -5;
                max = 5;
                def = 0;
                decimals = 2;
                step = 0.05f;
            }
            else if (key == "Highlights" || key == "Shadows" ||
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
            } else if (key == "Contrast")
            {
                min = -1;
                max = 1;
                def = 0;
                decimals = 2;
                step = 0.05f;
            }

            return (min, max, def, decimals, step);
        }

        private EditorSlider CreateSlider(string key, float min, float max, float def,
                                          EditorStyle? style = null, int decimalPlaces = 0, float step = 1f)
        {
            var slider = new EditorSlider(key, min, max, def, decimalPlaces, step);

            slider.OnValueChanged = val =>
            {
                if (currentImage == null) return;
                currentImage.Settings[key] = val;
                ApplyFilters();
                UpdateResetButtonsVisibility();
            };

            if (style != null)
                slider.ApplyStyle(style);

            _panelManager!.RegisterControl(key, slider);
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
                        currentImage.Settings[slider.Key] = slider.DefaultValue;
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
                            currentImage.Settings[slider.Key] = slider.DefaultValue;
                    }
                }
            }

            ApplyFilters();
            UpdateResetButtonsVisibility();
        }

        private void UpdateSliderUI()
        {
            if (currentImage == null || _panelManager == null) return;

            foreach (var (key, value) in currentImage.Settings)
            {
                _panelManager.GetControl<EditorSlider>(key)?.SetValue((float) value);
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

            bool anyChanged = currentImage.Settings.Any(f =>
            {
                var s = _panelManager!.GetControl<EditorSlider>(f.Key);
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
            await Task.Delay(50); // debounce

            lock (updateLock)
            {
                if (pendingUpdate)
                {
                    pendingUpdate = false;
                    applyFiltersTask = ApplyFiltersAsync();
                    return;
                }
            }

            if (currentImage?.OriginalBitmap == null)
                return;

            var filteredBitmap = await ImageProcessingManager.ApplyFiltersAsync(currentImage.OriginalBitmap, currentImage.Settings);

            currentImage.EditedBitmap = filteredBitmap;
            OnEditorImageUpdated?.Invoke(filteredBitmap);
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
