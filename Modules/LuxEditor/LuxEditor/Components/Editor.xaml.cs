using LuxEditor.Models;
using LuxEditor.Processing;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
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

        public event Action<BitmapImage> OnEditorImageUpdated;

        private readonly Dictionary<string, (float min, float max, float defaultValue)> SliderRanges = new()
        {
            { "Temperature", (2000, 50000, 6500) },
            { "Tint", (-150, 150, 0) },
            { "Exposure", (-5, 5, 0) },
            { "Contrast", (-100, 100, 0) },
            { "Highlights", (-100, 100, 0) },
            { "Shadows", (-100, 100, 0) },
            { "Whites", (-100, 100, 0) },
            { "Blacks", (-100, 100, 0) },
            { "Texture", (-100, 100, 0) },
            { "Clarity", (-100, 100, 0) },
            { "Dehaze", (-100, 100, 0) },
            { "Vibrance", (-100, 100, 0) },
            { "Saturation", (-100, 100, 0) }
        };

        private readonly Dictionary<string, List<string>> FilterGroups = new()
        {
            { "WhiteBalance", new() { "Temperature", "Tint" } },
            { "Tone", new() { "Exposure", "Contrast", "Highlights", "Shadows", "Whites", "Blacks" } },
            { "Presence", new() { "Texture", "Clarity", "Dehaze", "Vibrance", "Saturation" } }
        };

        private bool ctrlPressed = false;

        /// <summary>
        /// Initializes the Editor UI and binds event listeners.
        /// </summary>
        public Editor()
        {
            this.InitializeComponent();
            InitializeSliders();
            ImageManager.Instance.OnSelectionChanged += SetEditableImage;
        }

        /// <summary>
        /// Handles key press events for undo/redo keyboard shortcuts.
        /// </summary>
        private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
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

        /// <summary>
        /// Handles key up events to track control key state.
        /// </summary>
        private void OnKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
                ctrlPressed = false;
        }

        /// <summary>
        /// Sets the currently edited image and refreshes the UI.
        /// </summary>
        public void SetEditableImage(EditableImage image)
        {
            currentImage = image;
            this.Focus(FocusState.Programmatic);
            UpdateSliderUI();
            ApplyFilters();
        }

        /// <summary>
        /// Applies the current filter state asynchronously to the image.
        /// </summary>
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

        /// <summary>
        /// Performs image processing with the current filter values.
        /// </summary>
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

            if (currentImage?.OriginalBitmap == null) return;

            var result = ImageProcessor.ApplyFilters(currentImage.OriginalBitmap, currentImage.Filters);
            currentImage.EditedBitmap = result;

            UpdateImage(currentImage.EditedBitmap);
        }

        /// <summary>
        /// Updates the preview image displayed in the UI.
        /// </summary>
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

        /// <summary>
        /// Initializes all sliders, events, and reset behavior.
        /// </summary>
        private void InitializeSliders()
        {
            void Setup(string key, Slider slider, TextBox label)
            {
                slider.Tag = key;
                slider.ValueChanged += SliderValueChanged;
                AttachDoubleClickReset(slider, key, label);
            }

            Setup("Temperature", TemperatureSlider, TemperatureValueLabel);
            Setup("Tint", TintSlider, TintValueLabel);
            Setup("Exposure", ExposureSlider, ExposureValueLabel);
            Setup("Contrast", ContrastSlider, ContrastValueLabel);
            Setup("Highlights", HighlightsSlider, HighlightsValueLabel);
            Setup("Shadows", ShadowsSlider, ShadowsValueLabel);
            Setup("Whites", WhitesSlider, WhitesValueLabel);
            Setup("Blacks", BlacksSlider, BlacksValueLabel);
            Setup("Texture", TextureSlider, TextureValueLabel);
            Setup("Clarity", ClaritySlider, ClarityValueLabel);
            Setup("Dehaze", DehazeSlider, DehazeValueLabel);
            Setup("Vibrance", VibranceSlider, VibranceValueLabel);
            Setup("Saturation", SaturationSlider, SaturationValueLabel);
        }

        /// <summary>
        /// Handles value changes when a slider is moved.
        /// </summary>
        private void SliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider && slider.Tag is string key && currentImage != null)
            {
                float newValue = (float)e.NewValue;
                if (currentImage.Filters.TryGetValue(key, out float oldValue) && Math.Abs(oldValue - newValue) > 0.01f)
                {
                    currentImage.SaveState();
                    currentImage.Filters[key] = newValue;
                    ApplyFilters();
                    UpdateResetButtonsVisibility();

                    var label = GetTextBoxByKey(key);
                    label.Text = newValue.ToString("0");
                }
            }
        }

        /// <summary>
        /// Handles manual edits to the value TextBoxes.
        /// </summary>
        private void OnManualTextBoxEdited(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && currentImage != null)
            {
                string key = GetKeyFromTextBox(textBox);
                if (float.TryParse(textBox.Text, out float value))
                {
                    float clamped = ClampFilterValue(key, value);
                    currentImage.Filters[key] = clamped;

                    var slider = GetSliderByKey(key);
                    slider.Value = clamped;
                    textBox.Text = clamped.ToString("0");

                    ApplyFilters();
                    UpdateResetButtonsVisibility();
                }
                else
                {
                    textBox.Text = currentImage.Filters[key].ToString("0");
                }
            }
        }

        /// <summary>
        /// Triggers input validation on Enter key from TextBox.
        /// </summary>
        private void OnTextBoxKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if (sender is TextBox tb && tb.Parent is FrameworkElement parent)
                    parent.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Updates the slider values and their associated text labels.
        /// </summary>
        private void UpdateSliderUI()
        {
            foreach (var key in SliderRanges.Keys)
            {
                var slider = GetSliderByKey(key);
                var label = GetTextBoxByKey(key);
                float value = currentImage.Filters[key];
                slider.Value = value;
                label.Text = value.ToString("0");
            }
        }

        /// <summary>
        /// Resets all filters in a specified group.
        /// </summary>
        private void ResetGroup(string groupKey)
        {
            if (!FilterGroups.TryGetValue(groupKey, out var keys) || currentImage == null) return;

            foreach (var key in keys)
            {
                float def = SliderRanges[key].defaultValue;
                currentImage.Filters[key] = def;

                var slider = GetSliderByKey(key);
                var label = GetTextBoxByKey(key);
                slider.Value = def;
                label.Text = def.ToString("0");
            }

            ApplyFilters();
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Resets all filters to their default state.
        /// </summary>
        private void ResetAllClicked(object sender, RoutedEventArgs e)
        {
            foreach (var key in currentImage.Filters.Keys.ToList())
            {
                float def = SliderRanges[key].defaultValue;
                currentImage.Filters[key] = def;

                var slider = GetSliderByKey(key);
                var label = GetTextBoxByKey(key);
                slider.Value = def;
                label.Text = def.ToString("0");
            }

            ApplyFilters();
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Updates the visibility of reset buttons based on changes.
        /// </summary>
        private void UpdateResetButtonsVisibility()
        {
            if (currentImage == null) return;

            foreach (var group in FilterGroups)
            {
                bool modified = group.Value.Any(key => Math.Abs(currentImage.Filters[key] - SliderRanges[key].defaultValue) > 0.01f);

                var button = group.Key switch
                {
                    "WhiteBalance" => ResetWhiteBalanceButton,
                    "Tone" => ResetToneButton,
                    "Presence" => ResetPresenceButton,
                    _ => null
                };

                if (button != null)
                    button.Visibility = modified ? Visibility.Visible : Visibility.Collapsed;
            }

            bool anyChanged = currentImage.Filters.Any(f => Math.Abs(f.Value - SliderRanges[f.Key].defaultValue) > 0.01f);
            ResetAllButton.Visibility = anyChanged ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Attaches a double-click reset to a slider.
        /// </summary>
        private void AttachDoubleClickReset(Slider slider, string key, TextBox label)
        {
            slider.DoubleTapped += (s, e) =>
            {
                if (currentImage == null) return;

                float def = SliderRanges[key].defaultValue;
                currentImage.Filters[key] = def;
                slider.Value = ClampFilterValue(key, def);
                label.Text = def.ToString("0");

                ApplyFilters();
                UpdateResetButtonsVisibility();
            };
        }

        /// <summary>
        /// Clamps a filter value based on its slider bounds.
        /// </summary>
        private float ClampFilterValue(string key, float value)
        {
            return SliderRanges.TryGetValue(key, out var range)
                ? Math.Clamp(value, range.min, range.max)
                : value;
        }

        /// <summary>
        /// Resolves the slider associated with a filter key.
        /// </summary>
        private Slider GetSliderByKey(string key)
        {
            return key switch
            {
                "Temperature" => TemperatureSlider,
                "Tint" => TintSlider,
                "Exposure" => ExposureSlider,
                "Contrast" => ContrastSlider,
                "Highlights" => HighlightsSlider,
                "Shadows" => ShadowsSlider,
                "Whites" => WhitesSlider,
                "Blacks" => BlacksSlider,
                "Texture" => TextureSlider,
                "Clarity" => ClaritySlider,
                "Dehaze" => DehazeSlider,
                "Vibrance" => VibranceSlider,
                "Saturation" => SaturationSlider,
                _ => throw new ArgumentException($"Unknown key: {key}")
            };
        }

        /// <summary>
        /// Resolves the textbox associated with a filter key.
        /// </summary>
        private TextBox GetTextBoxByKey(string key)
        {
            return key switch
            {
                "Temperature" => TemperatureValueLabel,
                "Tint" => TintValueLabel,
                "Exposure" => ExposureValueLabel,
                "Contrast" => ContrastValueLabel,
                "Highlights" => HighlightsValueLabel,
                "Shadows" => ShadowsValueLabel,
                "Whites" => WhitesValueLabel,
                "Blacks" => BlacksValueLabel,
                "Texture" => TextureValueLabel,
                "Clarity" => ClarityValueLabel,
                "Dehaze" => DehazeValueLabel,
                "Vibrance" => VibranceValueLabel,
                "Saturation" => SaturationValueLabel,
                _ => throw new ArgumentException($"Unknown key: {key}")
            };
        }

        /// <summary>
        /// Extracts the filter key from a textbox name.
        /// </summary>
        private string GetKeyFromTextBox(TextBox box)
        {
            return box.Name.Replace("ValueLabel", "");
        }

        private void ResetWhiteBalanceClicked(object sender, RoutedEventArgs e) => ResetGroup("WhiteBalance");
        private void ResetToneClicked(object sender, RoutedEventArgs e) => ResetGroup("Tone");
        private void ResetPresenceClicked(object sender, RoutedEventArgs e) => ResetGroup("Presence");
    }
}

