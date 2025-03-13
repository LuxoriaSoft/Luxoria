using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private Dictionary<string, float> filterValues = new()
        {
            { "Temperature", 0 },
            { "Tint", 0 },
            { "Exposure", 0 },
            { "Contrast", 0 },
            { "Highlights", 0 },
            { "Shadows", 0 },
            { "Whites", 0 },
            { "Blacks", 0 },
            { "Texture", 0 },
            { "Clarity", 0 }
        };

        private SKBitmap originalBitmap;
        private SKBitmap editedBitmap;
        private SKSurface skSurface;
        private Task applyFiltersTask;
        private bool pendingUpdate;
        private readonly object updateLock = new();

        public event Action<BitmapImage> OnEditorImageUpdated;

        public Editor()
        {
            this.InitializeComponent();
            InitializeSliders();
        }

        public void SetOriginalBitmap(SKBitmap bitmap)
        {
            originalBitmap = bitmap;
            editedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            skSurface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
            ApplyFilters();
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
            await Task.Delay(100);

            lock (updateLock)
            {
                if (pendingUpdate)
                {
                    pendingUpdate = false;
                    applyFiltersTask = ApplyFiltersAsync();
                    return;
                }
            }

            if (originalBitmap == null || editedBitmap == null) return;

            skSurface.Canvas.Clear();
            skSurface.Canvas.DrawBitmap(originalBitmap, 0, 0);

            using (var paint = new SKPaint { ColorFilter = CreateColorFilter() })
            {
                skSurface.Canvas.DrawBitmap(originalBitmap, 0, 0, paint);
            }

            skSurface.Canvas.Flush();

            using (var snapshot = skSurface.Snapshot())
            {
                SKBitmap newBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                snapshot.ReadPixels(newBitmap.Info, newBitmap.GetPixels(), newBitmap.RowBytes, 0, 0);
                editedBitmap = newBitmap;
            }

            UpdateImage();
        }


        private SKColorFilter CreateColorFilter()
        {
            float temperature = filterValues["Temperature"] / 50000f;
            float tint = filterValues["Tint"] / 150f;
            float exposure = filterValues["Exposure"] / 5f;
            float contrast = filterValues["Contrast"] / 100f;

            float[] colorMatrix = new float[]
            {
                1 + contrast, 0, tint, 0, temperature,
                0, 1 + contrast, 0, 0, exposure,
                0, 0, 1 + contrast, 0, 0,
                0, 0, 0, 1, 0
            };

            return SKColorFilter.CreateColorMatrix(colorMatrix);
        }

        private void UpdateImage()
        {
            using (var image = SKImage.FromBitmap(editedBitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                var outputStream = stream.AsStreamForWrite();
                data.SaveTo(outputStream);
                outputStream.Flush();
                outputStream.Position = 0;

                BitmapImage bitmapImage = new();
                bitmapImage.SetSource(stream);
                OnEditorImageUpdated?.Invoke(bitmapImage);
            }
        }

        private void InitializeSliders()
        {
            TemperatureSlider.ValueChanged += SliderValueChanged;
            TintSlider.ValueChanged += SliderValueChanged;
            ExposureSlider.ValueChanged += SliderValueChanged;
            ContrastSlider.ValueChanged += SliderValueChanged;
            HighlightsSlider.ValueChanged += SliderValueChanged;
            ShadowsSlider.ValueChanged += SliderValueChanged;
            WhitesSlider.ValueChanged += SliderValueChanged;
            BlacksSlider.ValueChanged += SliderValueChanged;
            TextureSlider.ValueChanged += SliderValueChanged;
            ClaritySlider.ValueChanged += SliderValueChanged;
        }

        private void SliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider && slider.Tag is string filterKey && filterValues.ContainsKey(filterKey))
            {
                float newValue = (float)e.NewValue;
                if (Math.Abs(filterValues[filterKey] - newValue) > 0.01f)
                {
                    filterValues[filterKey] = newValue;
                    ApplyFilters();
                }
            }
        }
    }
}
