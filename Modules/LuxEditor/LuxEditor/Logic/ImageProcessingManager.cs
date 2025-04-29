using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LuxEditor.Logic
{
    public static class ImageProcessingManager
    {
        /// <summary>
        /// Applies filters to an image using SkiaSharp in the most optimized way.
        /// </summary>
        public static async Task<SKBitmap> ApplyFiltersAsync(SKBitmap source, Dictionary<string, float> filters)
        {
            return await Task.Run(() =>
            {
                var target = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);

                using var surface = SKSurface.Create(target.Info);
                using var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                using var paint = new SKPaint
                {
                    ColorFilter = CreateCombinedColorFilter(filters)
                };

                canvas.DrawBitmap(source, 0, 0, paint);
                canvas.Flush();

                surface.ReadPixels(target.Info, target.GetPixels(), target.RowBytes, 0, 0);

                return target;
            });
        }

        /// <summary>
        /// Creates a combined color filter from all applicable filters.
        /// </summary>
        private static SKColorFilter CreateCombinedColorFilter(Dictionary<string, float> filters)
        {
            // Build the color matrix
            var matrix = CreateBaseMatrix(filters);

            return SKColorFilter.CreateColorMatrix(matrix);
        }

        /// <summary>
        /// Constructs a color matrix applying exposure, contrast, saturation, temperature, and tint.
        /// </summary>
        private static float[] CreateBaseMatrix(Dictionary<string, float> filters)
        {
            float exposure = filters.TryGetValue("Exposure", out var exp) ? exp : 0f;
            float contrast = filters.TryGetValue("Contrast", out var con) ? con : 0f;
            float saturation = filters.TryGetValue("Saturation", out var sat) ? sat : 0f;
            float temperature = filters.TryGetValue("Temperature", out var temp) ? temp : 6500f;
            float tint = filters.TryGetValue("Tint", out var ti) ? ti : 0f;

            var exposureGain = MathF.Pow(2, exposure);

            // Temperature
            var (rTemp, gTemp, bTemp) = KelvinToRGB(temperature);

            // Saturation
            const float lumR = 0.2126f;
            const float lumG = 0.7152f;
            const float lumB = 0.0722f;
            float satFactor = 1f + (saturation / 100f);
            float rSat = lumR * (1f - satFactor);
            float gSat = lumG * (1f - satFactor);
            float bSat = lumB * (1f - satFactor);

            // Contrast
            float contrastFactor = 1f + (contrast / 100f);
            float translate = 128f * (1f - contrastFactor);

            // Tint
            float greenScale = 1f + (tint / 150f);
            float blueScale = 1f - (tint / 150f);

            return new float[]
            {
                exposureGain * contrastFactor * rTemp, 0, 0, 0, translate,
                0, exposureGain * contrastFactor * gTemp * greenScale, 0, 0, translate,
                0, 0, exposureGain * contrastFactor * bTemp * blueScale, 0, translate,
                0, 0, 0, 1, 0
            };
        }

        /// <summary>
        /// Approximates the RGB color from a given color temperature (in Kelvin).
        /// </summary>
        private static (float r, float g, float b) KelvinToRGB(float kelvin)
        {
            kelvin = Math.Clamp(kelvin, 1000f, 40000f) / 100f;
            float r, g, b;

            r = kelvin <= 66
                ? 255
                : 329.698727446f * MathF.Pow(kelvin - 60, -0.1332047592f);

            g = kelvin <= 66
                ? 99.4708025861f * MathF.Log(kelvin) - 161.1195681661f
                : 288.1221695283f * MathF.Pow(kelvin - 60, -0.0755148492f);

            b = kelvin >= 66
                ? 255
                : kelvin <= 19
                    ? 0
                    : 138.5177312231f * MathF.Log(kelvin - 10) - 305.0447927307f;

            return (
                Math.Clamp(r / 255f, 0f, 1f),
                Math.Clamp(g / 255f, 0f, 1f),
                Math.Clamp(b / 255f, 0f, 1f)
            );
        }

        public static SKBitmap ResizeBitmap(SKBitmap source, int width, int height)
        {
            SKBitmap resized = new SKBitmap(width, height);

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear();

            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            canvas.DrawImage(SKImage.FromBitmap(source), new SKRect(0, 0, width, height), sampling);
            canvas.Flush();
            surface.Snapshot().ReadPixels(resized.Info, resized.GetPixels(), resized.RowBytes, 0, 0);

            return resized;
        }

        public static SKBitmap GeneratePreview(SKBitmap source, int targetHeight)
        {
            float aspectRatio = (float)source.Width / source.Height;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        public static SKBitmap GenerateMediumResolution(SKBitmap source, int maxHeight = 600)
        {
            if (source.Height <= maxHeight)
                return source;

            float aspectRatio = (float)source.Width / source.Height;
            int targetHeight = maxHeight;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }
    }
}
