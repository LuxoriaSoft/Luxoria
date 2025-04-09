using SkiaSharp;
using System;
using System.Collections.Generic;

namespace LuxEditor.Processing
{
    public static class ImageProcessor
    {
        public static SKBitmap ApplyFilters(SKBitmap source, Dictionary<string, float> filters)
        {
            SKBitmap bitmap = source.Copy();

            bitmap = ApplyExposure(bitmap, filters);
            bitmap = ApplyContrast(bitmap, filters);
            bitmap = ApplyTemperature(bitmap, filters);
            bitmap = ApplyTint(bitmap, filters);
            bitmap = ApplySaturation(bitmap, filters);

            return bitmap;
        }

        private static SKBitmap ApplyExposure(SKBitmap source, Dictionary<string, float> filters)
        {
            float exposure = filters.TryGetValue("Exposure", out var exp) ? exp : 0f;
            float gain = MathF.Pow(2, exposure); // EV scale

            float[] matrix = new float[]
            {
                gain, 0,    0,    0, 0,
                0,    gain, 0,    0, 0,
                0,    0,    gain, 0, 0,
                0,    0,    0,    1, 0
            };

            return ApplyColorMatrix(source, matrix);
        }

        private static SKBitmap ApplyContrast(SKBitmap source, Dictionary<string, float> filters)
        {
            float contrast = filters.TryGetValue("Contrast", out var con) ? con : 0f;

            float factor = 1f + (contrast / 100f);
            float t = 128f * (1f - factor);

            float[] matrix = new float[]
            {
                factor, 0,      0,      0, t,
                0,      factor, 0,      0, t,
                0,      0,      factor, 0, t,
                0,      0,      0,      1, 0
            };

            return ApplyColorMatrix(source, matrix);
        }

        private static SKBitmap ApplyTemperature(SKBitmap source, Dictionary<string, float> filters)
        {
            float temperature = filters.TryGetValue("Temperature", out var temp) ? temp : 6500f;

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

        private static SKBitmap ApplySaturation(SKBitmap source, Dictionary<string, float> filters)
        {
            float saturation = filters.TryGetValue("Saturation", out var s) ? s / 100f : 0f;
            float[] matrix = CreateSaturationMatrix(1 + saturation);
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

        private static float[] CreateSaturationMatrix(float saturation)
        {
            const float lumR = 0.2126f;
            const float lumG = 0.7152f;
            const float lumB = 0.0722f;

            float invSat = 1 - saturation;
            float r = lumR * invSat;
            float g = lumG * invSat;
            float b = lumB * invSat;

            return new float[]
            {
                r + saturation, g,              b,              0, 0,
                r,              g + saturation, b,              0, 0,
                r,              g,              b + saturation, 0, 0,
                0,              0,              0,              1, 0
            };
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
