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
            var matrixFilter = SKColorFilter.CreateColorMatrix(CreateBaseMatrix(filters));
            var shFilter = CreateShadowsHighlightsFilter(filters);

            if (shFilter != null)
            {
                // outer = shadows/highlights, inner = exposure/contrast/etc.
                return SKColorFilter.CreateCompose(shFilter, matrixFilter);
            }

            return matrixFilter;
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
            var (redShift,  greenShift, blueShift) = CreateWhiteBalanceMatrix(temperature, tint);

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

            return new float[]
            {
                (rSat + satFactor) * exposureGain * contrastFactor * redShift,
                gSat * exposureGain * contrastFactor * redShift,
                bSat * exposureGain * contrastFactor * redShift,
                0,
                translate,

                rSat * exposureGain * contrastFactor * greenShift,
                (gSat + satFactor) * exposureGain * contrastFactor * greenShift,
                bSat * exposureGain * contrastFactor * greenShift,
                0,
                translate,

                rSat * exposureGain * contrastFactor * blueShift,
                gSat * exposureGain * contrastFactor * blueShift,
                (bSat + satFactor) * exposureGain * contrastFactor * blueShift,
                0,
                translate,

                0, 0, 0, 1, 0
            };

        }

        /// <summary>
        /// Builds a table‐based filter that adjusts shadows and highlights.
        /// Shadows/Highlights values are in –100…100.
        /// </summary>
        private static SKColorFilter? CreateShadowsHighlightsFilter(Dictionary<string, float> filters)
        {
            filters.TryGetValue("Shadows", out var rawSh);
            filters.TryGetValue("Highlights", out var rawHi);
            float shadows = rawSh / 100f;
            float highlights = rawHi / 100f;

            if (MathF.Abs(shadows) < 1e-6 && MathF.Abs(highlights) < 1e-6)
                return null;

            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                float v = i / 255f;
                float v2;
                if (v < 0.25f)
                {
                    v2 = v + (0.25f - v) * shadows;
                }
                else if (v > 0.75f)
                {
                    v2 = v + (v - 0.75f) * highlights;
                }
                else
                {
                    v2 = v;
                }
                table[i] = (byte)(Math.Clamp(v2, 0f, 1f) * 255);
            }

            return SKColorFilter.CreateTable(table);
        }

        private static (float r, float g, float b) CreateWhiteBalanceMatrix(float temperature, float tint)
        {
            temperature = Math.Clamp(temperature, 2000f, 50000f);
            float kelvinRef = 6500f;

            float temperatureRatio = (float)Math.Log(temperature / kelvinRef, 2.0);

            float redShift = 1f + 0.2f * temperatureRatio;
            float blueShift = 1f - 0.2f * temperatureRatio;

            float greenShift = 1f - (tint / 100f);
            
            redShift = Math.Clamp(redShift, 0.5f, 2.5f);
            greenShift = Math.Clamp(greenShift, 0.5f, 2.5f);
            blueShift = Math.Clamp(blueShift, 0.5f, 2.5f);

            return (redShift, greenShift, blueShift);
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
