/* 
  Explanation for "F0" etc.:
  - "F0" in the string format displays the slider value as an integer (no decimals).
  - "F2" would show two decimal places.
  Choose the format that best represents each slider's range and usage.
*/

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SkiaSharp;
using System;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private SKBitmap? _originalBitmap;

        public event Action<SKBitmap>? OnEditorImageUpdated;

        public Editor()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the original SKBitmap that we will process.
        /// </summary>
        public void SetOriginalBitmap(SKBitmap bitmap)
        {
            _originalBitmap = bitmap;
        }

        // -----------------------
        // Slider Handlers
        // -----------------------

        private void ExposureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ExposureValueLabel != null)
                ExposureValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            ProcessImage();
        }

        private void ContrastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ContrastValueLabel != null)
                ContrastValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            ProcessImage();
        }

        private void HighlightsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (HighlightsValueLabel != null)
                HighlightsValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            ProcessImage();
        }

        private void ShadowsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ShadowsValueLabel != null)
                ShadowsValueLabel.Text = (e.NewValue / 1000).ToString("F2");
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
        /// Applies all adjustments to the original image, then raises an event.
        /// </summary>
        private void ProcessImage()
        {
            if (_originalBitmap == null)
                return;

            // 1) Retrieve slider values
            float exposure = (float)ExposureSlider.Value / 1000;
            float contrast = (float)ContrastSlider.Value / 1000;
            float highlights = (float)HighlightsSlider.Value / 1000;
            float shadows = (float)ShadowsSlider.Value / 1000;
            float temperature = (float)TemperatureSlider.Value;
            float tint = (float)TintSlider.Value;
            float saturation = (float)SaturationSlider.Value;

            // 2) First pass with color filters (exposure, contrast, saturation)
            SKBitmap firstPassBitmap = new SKBitmap(_originalBitmap.Width, _originalBitmap.Height);
            using (var canvas = new SKCanvas(firstPassBitmap))
            {
                canvas.Clear(SKColors.Transparent);

                // Exposure with lighting filter
                float exposureScale = (float)Math.Pow(2, exposure);
                var exposureFilter = SKColorFilter.CreateLighting(
                    new SKColor(
                        (byte)(255 * exposureScale),
                        (byte)(255 * exposureScale),
                        (byte)(255 * exposureScale)),
                    new SKColor(0, 0, 0));

                // Contrast with color matrix
                float contrastFactor = 1f + contrast;
                float translate = 0.5f * (1f - contrastFactor);
                float[] contrastMatrix =
                {
                    contrastFactor, 0,             0,             0, translate,
                    0,             contrastFactor, 0,             0, translate,
                    0,             0,             contrastFactor, 0, translate,
                    0,             0,             0,             1, 0
                };
                var contrastFilter = SKColorFilter.CreateColorMatrix(contrastMatrix);

                // Saturation with color matrix
                float saturationFactor = 1f + (saturation / 100f);
                float lumR = 0.3086f;
                float lumG = 0.6094f;
                float lumB = 0.0820f;
                float oneMinusS = 1f - saturationFactor;
                float r = (oneMinusS * lumR);
                float g = (oneMinusS * lumG);
                float b = (oneMinusS * lumB);
                float[] saturationMatrix =
                {
                    r + saturationFactor, g,                     b,                     0, 0,
                    r,                     g + saturationFactor, b,                     0, 0,
                    r,                     g,                     b + saturationFactor, 0, 0,
                    0,                     0,                     0,                     1, 0
                };
                var saturationFilter = SKColorFilter.CreateColorMatrix(saturationMatrix);

                // Compose exposure->contrast->saturation
                var contrastSaturation = SKColorFilter.CreateCompose(contrastFilter, saturationFilter);
                var finalFilter = SKColorFilter.CreateCompose(exposureFilter, contrastSaturation);

                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = finalFilter;
                    canvas.DrawBitmap(_originalBitmap, 0, 0, paint);
                }
            }

            // 3) Second pass (CPU) for highlights, shadows, temperature, and tint
            SKBitmap finalBitmap = new SKBitmap(firstPassBitmap.Width, firstPassBitmap.Height);
            for (int y = 0; y < firstPassBitmap.Height; y++)
            {
                for (int x = 0; x < firstPassBitmap.Width; x++)
                {
                    uint pixel = (uint)firstPassBitmap.GetPixel(x, y);
                    byte alpha = (byte)((pixel >> 24) & 0xFF);
                    byte red = (byte)((pixel >> 16) & 0xFF);
                    byte green = (byte)((pixel >> 8) & 0xFF);
                    byte blue = (byte)(pixel & 0xFF);

                    float fr = red / 255f;
                    float fg = green / 255f;
                    float fb = blue / 255f;

                    // Highlights
                    float brightness = (fr + fg + fb) / 3f;
                    if (brightness > 0.5f)
                    {
                        float factor = (brightness - 0.5f) * 2f;
                        float amount = highlights * factor;
                        fr = fr + amount * (1f - fr);
                        fg = fg + amount * (1f - fg);
                        fb = fb + amount * (1f - fb);
                    }

                    // Shadows
                    brightness = (fr + fg + fb) / 3f;
                    if (brightness < 0.5f)
                    {
                        float factor = (0.5f - brightness) * 2f;
                        float amount = shadows * factor;
                        fr = fr + amount * (1f - fr);
                        fg = fg + amount * (1f - fg);
                        fb = fb + amount * (1f - fb);
                    }

                    // Temperature and tint
                    float tempFactor = temperature / 100f;
                    float tintFactor = tint / 100f;

                    // Simple scale for red and blue (temperature)
                    float rScale = 1f + (tempFactor * 0.3f);
                    float bScale = 1f - (tempFactor * 0.3f);
                    fr *= rScale;
                    fb *= bScale;

                    // Simple scale for green (tint)
                    float gScale = 1f + (tintFactor * 0.3f);
                    fg *= gScale;

                    // Optional partial compensation for red/blue with tint
                    float inverseTintScale = 1f - (Math.Abs(tintFactor) * 0.1f);
                    fr *= inverseTintScale;
                    fb *= inverseTintScale;

                    // Clamp
                    fr = Math.Clamp(fr, 0f, 1f);
                    fg = Math.Clamp(fg, 0f, 1f);
                    fb = Math.Clamp(fb, 0f, 1f);

                    // Convert to byte
                    byte nr = (byte)(fr * 255f);
                    byte ng = (byte)(fg * 255f);
                    byte nb = (byte)(fb * 255f);

                    uint newPixel =
                          ((uint)alpha << 24)
                        | ((uint)nr << 16)
                        | ((uint)ng << 8)
                        | (uint)nb;

                    finalBitmap.SetPixel(x, y, newPixel);
                }
            }

            // 4) Send final bitmap
            OnEditorImageUpdated?.Invoke(finalBitmap);
        }
    }
}
