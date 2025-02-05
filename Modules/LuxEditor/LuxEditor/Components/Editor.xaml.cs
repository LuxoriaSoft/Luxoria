using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private SKBitmap _originalBitmap;

        public event Action<SKBitmap> OnEditorImageUpdated;

        public Editor()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the original unmodified SKBitmap that we will process.
        /// </summary>
        /// <param name="bitmap">The SKBitmap to edit.</param>
        public void SetOriginalBitmap(SKBitmap bitmap)
        {
            _originalBitmap = bitmap;
        }

        // -----------------------
        //    Slider Handlers
        // -----------------------

        private void ExposureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ExposureValueLabel != null)
                ExposureValueLabel.Text = e.NewValue.ToString("F2");

            ProcessImage();
        }

        private void ContrastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ContrastValueLabel != null)
                ContrastValueLabel.Text = e.NewValue.ToString("F2");

            ProcessImage();
        }

        private void HighlightsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (HighlightsValueLabel != null)
                HighlightsValueLabel.Text = e.NewValue.ToString("F2");

            ProcessImage();
        }

        private void ShadowsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ShadowsValueLabel != null)
                ShadowsValueLabel.Text = e.NewValue.ToString("F2");

            ProcessImage();
        }

        private void TemperatureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (TemperatureValueLabel != null)
                TemperatureValueLabel.Text = e.NewValue.ToString("F0");

            ProcessImage();
        }

        private void TintSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (TintValueLabel != null)
                TintValueLabel.Text = e.NewValue.ToString("F0");

            ProcessImage();
        }

        private void SaturationSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (SaturationValueLabel != null)
                SaturationValueLabel.Text = e.NewValue.ToString("F0");

            ProcessImage();
        }

        /// <summary>
        /// Applies the current slider adjustments to the original image,
        /// then raises an event so subscribers can display the updated SKBitmap.
        /// </summary>
        private void ProcessImage()
        {
            if (_originalBitmap == null)
                return;

            // 1) Gather current slider values
            float exposure = (float)ExposureSlider.Value;       // Range: -2 to +2
            float contrast = (float)ContrastSlider.Value;       // Range: -1 to +1
            float highlights = (float)HighlightsSlider.Value;     // Range: -1 to +1 (stub)
            float shadows = (float)ShadowsSlider.Value;        // Range: -1 to +1 (stub)
            float temperature = (float)TemperatureSlider.Value;    // Range: -100 to +100 (stub)
            float tint = (float)TintSlider.Value;           // Range: -100 to +100 (stub)
            float saturation = (float)SaturationSlider.Value;     // Range: -100 to +100

            // 2) Create a new SKBitmap to draw into
            SKBitmap adjustedBitmap = new SKBitmap(_originalBitmap.Width, _originalBitmap.Height);

            using (var canvas = new SKCanvas(adjustedBitmap))
            {
                canvas.Clear(SKColors.Transparent);

                // We'll compose multiple filters for demonstration:
                //    a) Exposure  (simple "lighting" approach)
                //    b) Contrast  (color matrix)
                //    c) Saturation (color matrix)
                //    * Additional stubs for highlights, shadows, temperature, tint

                // a) Exposure: We treat exposure as a brightness shift using a "lighting" filter.
                //    Real-world exposure might require more advanced logic.
                float exposureScale = (float)Math.Pow(2, exposure);
                SKColorFilter exposureFilter = SKColorFilter.CreateLighting(
                    new SKColor(
                        (byte)(255 * exposureScale),
                        (byte)(255 * exposureScale),
                        (byte)(255 * exposureScale)),
                    new SKColor(0, 0, 0));

                // b) Contrast: We'll construct a color matrix for contrast. 
                //    Typical formula around pivot 0.5 is:
                //    newValue = (oldValue - 0.5)*contrastFactor + 0.5
                float contrastFactor = 1f + contrast;
                float translate = 0.5f * (1f - contrastFactor);

                float[] contrastMatrix = {
                    contrastFactor, 0,             0,             0, translate,
                    0,             contrastFactor, 0,             0, translate,
                    0,             0,             contrastFactor, 0, translate,
                    0,             0,             0,             1, 0
                };
                SKColorFilter contrastFilter = SKColorFilter.CreateColorMatrix(contrastMatrix);

                // c) Saturation: Use a color matrix approach.
                //    If slider is -100..+100, let's convert that to [0..2] around 1.0
                float saturationFactor = 1f + (saturation / 100f);  // e.g. 0 = grayscale, 1 = normal, 2 = double
                float lumR = 0.3086f;
                float lumG = 0.6094f;
                float lumB = 0.0820f;

                float oneMinusS = 1f - saturationFactor;
                float r = (oneMinusS * lumR);
                float g = (oneMinusS * lumG);
                float b = (oneMinusS * lumB);

                float[] saturationMatrix = {
                    r + saturationFactor, g,                     b,                     0, 0,
                    r,                     g + saturationFactor, b,                     0, 0,
                    r,                     g,                     b + saturationFactor, 0, 0,
                    0,                     0,                     0,                     1, 0
                };
                SKColorFilter saturationFilter = SKColorFilter.CreateColorMatrix(saturationMatrix);

                // d) Highlights/Shadows, Temperature, Tint:
                //    For demonstration, we'll skip or do minimal. 
                //    In advanced scenarios, implement correct color shifting or curves.
                //    We'll pass them as a "no-op" for now.
                SKColorFilter highlightShadowFilter = null; // TODO
                SKColorFilter temperatureTintFilter = null; // TODO

                // 3) Compose the filters. If you have multiple, chain them:
                //    finalFilter = Filter1 + Filter2 + ...
                //    We'll chain exposure -> contrast -> saturation (others omitted).
                //    "Compose" merges filters in order (the output of the first 
                //    becomes input of the second, etc.).
                SKColorFilter contrastSaturation = SKColorFilter.CreateCompose(contrastFilter, saturationFilter);
                SKColorFilter finalFilter = SKColorFilter.CreateCompose(exposureFilter, contrastSaturation);

                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = finalFilter;
                    canvas.DrawBitmap(_originalBitmap, 0, 0, paint);
                }
            }

            // 4) Notify subscribers (e.g., main page) that we have a new SKBitmap
            OnEditorImageUpdated?.Invoke(adjustedBitmap);
        }
    }
}
