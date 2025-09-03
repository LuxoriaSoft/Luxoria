using SkiaSharp;
using System;
using System.IO;
using LuxExport.Models;

namespace LuxExport.Logic
{
    /// <summary>
    /// Helper class to optimize image export by adjusting quality to meet file size constraints.
    /// </summary>
    public static class FileSizeOptimizer
    {
        /// <summary>
        /// Optimizes image quality to fit within the specified file size limit.
        /// </summary>
        /// <param name="image">The image to export</param>
        /// <param name="format">The export format</param>
        /// <param name="maxFileSizeKB">Maximum file size in KB</param>
        /// <param name="initialQuality">Starting quality (0-100)</param>
        /// <returns>Tuple of (optimized quality, estimated file size in bytes)</returns>
        public static (int quality, long fileSizeBytes) OptimizeForFileSize(SKBitmap image, ExportFormat format, int maxFileSizeKB, int initialQuality = 100)
        {
            if (maxFileSizeKB <= 0)
                return (initialQuality, 0);

            var targetSizeBytes = maxFileSizeKB * 1024L;
            var imageFormat = GetSkiaImageFormat(format);
            
            // PNG doesn't support quality/compression optimization with current library
            if (format == ExportFormat.PNG)
            {
                return (initialQuality, 0);
            }
            
            // Quick size estimation first for JPEG/WEBP
            var estimatedSize = EstimateFileSize(image, format, initialQuality);
            if (estimatedSize <= targetSizeBytes)
                return (initialQuality, estimatedSize);
            
            // Binary search for optimal quality for JPEG/WEBP
            int minQuality = 10; // Minimum reasonable quality
            int maxQuality = initialQuality;
            int bestQuality = minQuality;
            long bestSize = long.MaxValue;
            
            while (minQuality <= maxQuality)
            {
                int currentQuality = (minQuality + maxQuality) / 2;
                long currentSize = GetActualFileSize(image, imageFormat, currentQuality);
                
                if (currentSize <= targetSizeBytes)
                {
                    bestQuality = currentQuality;
                    bestSize = currentSize;
                    minQuality = currentQuality + 1;
                }
                else
                {
                    maxQuality = currentQuality - 1;
                }
            }
            
            return (bestQuality, bestSize);
        }
        
        /// <summary>
        /// Estimates file size based on image dimensions and format without actual encoding.
        /// This is a fast approximation.
        /// </summary>
        private static long EstimateFileSize(SKBitmap image, ExportFormat format, int quality)
        {
            var pixelCount = image.Width * image.Height;
            
            return format switch
            {
                ExportFormat.PNG => (long)(pixelCount * 3.5), // PNG is lossless, roughly 3-4 bytes per pixel
                ExportFormat.JPEG => (long)(pixelCount * (quality / 100.0) * 0.5), // JPEG compression varies with quality
                ExportFormat.WEBP => (long)(pixelCount * (quality / 100.0) * 0.4), // WEBP is more efficient than JPEG
                _ => (long)(pixelCount * 2)
            };
        }
        
        /// <summary>
        /// Gets actual file size by encoding to memory stream.
        /// </summary>
        private static long GetActualFileSize(SKBitmap image, SKEncodedImageFormat format, int quality)
        {
            using var data = image.Encode(format, quality);
            return data?.Size ?? 0;
        }
        
        /// <summary>
        /// Converts ExportFormat to SKEncodedImageFormat.
        /// </summary>
        private static SKEncodedImageFormat GetSkiaImageFormat(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.PNG => SKEncodedImageFormat.Png,
                ExportFormat.JPEG => SKEncodedImageFormat.Jpeg,
                ExportFormat.WEBP => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Jpeg
            };
        }
        
        /// <summary>
        /// Exports image with size optimization if needed.
        /// </summary>
        public static void ExportWithSizeLimit(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            int finalQuality = settings.Quality;
            
            if (settings.LimitFileSize && settings.MaxFileSizeKB > 0)
            {
                var (optimizedQuality, _) = OptimizeForFileSize(image, format, settings.MaxFileSizeKB, settings.Quality);
                finalQuality = optimizedQuality;
            }
            
            var imageFormat = GetSkiaImageFormat(format);
            using var stream = new FileStream(path, FileMode.Create);
            using var data = image.Encode(imageFormat, finalQuality);
            data.SaveTo(stream);
        }
        
        /// <summary>
        /// Exports image to memory with size optimization if needed.
        /// </summary>
        public static SKData ExportToMemoryWithSizeLimit(SKBitmap image, ExportFormat format, ExportSettings settings)
        {
            int finalQuality = settings.Quality;
            
            if (settings.LimitFileSize && settings.MaxFileSizeKB > 0)
            {
                var (optimizedQuality, _) = OptimizeForFileSize(image, format, settings.MaxFileSizeKB, settings.Quality);
                finalQuality = optimizedQuality;
            }
            
            var imageFormat = GetSkiaImageFormat(format);
            return image.Encode(imageFormat, finalQuality);
        }
    }
}