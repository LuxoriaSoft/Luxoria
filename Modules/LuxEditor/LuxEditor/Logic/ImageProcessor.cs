using SkiaSharp;
using System.Collections.Generic;

namespace LuxEditor.Processing
{
    /// <summary>
    /// Provides static methods for applying image filters and generating resized previews.
    /// </summary>
    public static class ImageProcessor
    {
        /// <summary>
        /// Applies all filters to the provided bitmap and returns a new processed bitmap.
        /// </summary>
        public static SKBitmap ApplyFilters(SKBitmap source, Dictionary<string, float> filters)
        {
            int width = source.Width;
            int height = source.Height;

            SKBitmap result = new SKBitmap(width, height);

            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear();

                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = CreateColorFilter(filters);
                    canvas.DrawBitmap(source, 0, 0, paint);
                }

                canvas.Flush();

                using (var snapshot = surface.Snapshot())
                {
                    snapshot.ReadPixels(result.Info, result.GetPixels(), result.RowBytes, 0, 0);
                }
            }

            return result;
        }

        /// <summary>
        /// Generates a low-resolution preview version of the given bitmap for thumbnails.
        /// </summary>
        public static SKBitmap GeneratePreview(SKBitmap source, int targetHeight)
        {
            float aspectRatio = (float)source.Width / source.Height;
            int targetWidth = (int)(targetHeight * aspectRatio);

            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        /// <summary>
        /// Generates a medium-resolution bitmap for editing purposes.
        /// </summary>
        public static SKBitmap GenerateMediumResolution(SKBitmap source, int maxHeight = 600)
        {
            if (source.Height <= maxHeight)
                return source;

            float aspectRatio = (float)source.Width / source.Height;
            int targetHeight = maxHeight;
            int targetWidth = (int)(targetHeight * aspectRatio);

            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        /// <summary>
        /// Resizes the bitmap to the specified width and height using high-quality scaling.
        /// </summary>
        public static SKBitmap ResizeBitmap(SKBitmap source, int width, int height)
        {
            SKBitmap resized = new SKBitmap(width, height);

            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear();

                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                canvas.DrawImage(SKImage.FromBitmap(source), new SKRect(0, 0, width, height), sampling);

                canvas.Flush();
                surface.Snapshot().ReadPixels(resized.Info, resized.GetPixels(), resized.RowBytes, 0, 0);
            }

            return resized;
        }

        /// <summary>
        /// Creates a color filter matrix based on exposure and contrast settings.
        /// </summary>
        private static SKColorFilter CreateColorFilter(Dictionary<string, float> filters)
        {
            float exposure = filters.ContainsKey("Exposure") ? filters["Exposure"] / 5f : 0;
            float contrast = filters.ContainsKey("Contrast") ? filters["Contrast"] / 100f : 0;

            float[] colorMatrix = new float[]
            {
                1 + contrast, 0, 0, 0, exposure * 255,
                0, 1 + contrast, 0, 0, exposure * 255,
                0, 0, 1 + contrast, 0, exposure * 255,
                0, 0, 0, 1, 0
            };

            return SKColorFilter.CreateColorMatrix(colorMatrix);
        }
    }
}
