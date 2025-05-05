using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LuxEditor.Processing
{
    public static class ImageProcessor
    {
        /// <summary>
        /// Applies the current filters to the given image using optimized pixel-wise and matrix processing.
        /// </summary>
        public static SKBitmap ApplyFilters(SKBitmap source, Dictionary<string, float> filters)
        {
            SKBitmap result = new SKBitmap(source.Width, source.Height);
            var srcPixels = source.Pixels;
            var dstPixels = result.Pixels;

            Parallel.For(0, srcPixels.Length, i =>
            {
                var color = srcPixels[i];
                color = ApplyExposure(color, filters);
                color = ApplyContrast(color, filters);
                color = ApplySaturation(color, filters);
                dstPixels[i] = color;
            });

            // Matrix filters like temperature/tint must be applied globally
            result = ApplyTemperature(result, filters);
            result = ApplyTint(result, filters);

            return result;
        }

        private static SKColor ApplyExposure(SKColor color, Dictionary<string, float> filters)
        {
            float exposure = filters.TryGetValue("Exposure", out var val) ? val : 0f;
            float gain = MathF.Pow(2, exposure);

            return new SKColor(
                ClampByte(color.Red * gain),
                ClampByte(color.Green * gain),
                ClampByte(color.Blue * gain),
                color.Alpha
            );
        }

        private static SKColor ApplyContrast(SKColor color, Dictionary<string, float> filters)
        {
            float contrast = filters.TryGetValue("Contrast", out var val) ? val : 0f;
            float factor = 1f + (contrast / 100f);
            float midpoint = 128f;

            return new SKColor(
                ClampByte(((color.Red - midpoint) * factor + midpoint) / 100),
                ClampByte(((color.Green - midpoint) * factor + midpoint) / 100),
                ClampByte(((color.Blue - midpoint) * factor + midpoint) / 100),
                color.Alpha
            );
        }

        private static SKColor ApplySaturation(SKColor color, Dictionary<string, float> filters)
        {
            float saturation = filters.TryGetValue("Saturation", out var val) ? val / 100f : 0f;
            float r = color.Red, g = color.Green, b = color.Blue;

            float gray = r * 0.2126f + g * 0.7152f + b * 0.0722f;

            return new SKColor(
                ClampByte(gray + (r - gray) * (1 + saturation)),
                ClampByte(gray + (g - gray) * (1 + saturation)),
                ClampByte(gray + (b - gray) * (1 + saturation)),
                color.Alpha
            );
        }

        private static SKBitmap ApplyTemperature(SKBitmap source, Dictionary<string, float> filters)
        {
            float temperature = filters.TryGetValue("Temperature", out var val) ? val : 6500f;
            var (r, g, b) = KelvinToRGB(temperature);

            float[] matrix = new float[]
            {
                r, 0, 0, 0, 0,
                0, g, 0, 0, 0,
                0, 0, b, 0, 0,
                0, 0, 0, 1, 0
            };

            return ApplyColorMatrix(source, matrix);
        }

        private static SKBitmap ApplyTint(SKBitmap source, Dictionary<string, float> filters)
        {
            float tint = filters.TryGetValue("Tint", out var ti) ? ti / 150f : 0f;
            float g = 1 + tint;
            float r = 1;
            float b = 1 - tint;

            float[] matrix = new float[]
            {
                r, 0, 0, 0, 0,
                0, g, 0, 0, 0,
                0, 0, b, 0, 0,
                0, 0, 0, 1, 0
            };

            return ApplyColorMatrix(source, matrix);
        }

        private static SKBitmap ApplyColorMatrix(SKBitmap source, float[] matrix)
        {
            SKBitmap result = new SKBitmap(source.Width, source.Height);

            using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height));
            var canvas = surface.Canvas;
            canvas.Clear();

            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(matrix)
            };

            canvas.DrawBitmap(source, 0, 0, paint);
            canvas.Flush();
            surface.Snapshot().ReadPixels(result.Info, result.GetPixels(), result.RowBytes, 0, 0);

            return result;
        }

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

        private static byte ClampByte(float value)
        {
            return (byte)Math.Clamp((int)value, 0, 255);
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
