using LuxExport.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using SkiaSharp;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace LuxExport.Logic
{
    /// <summary>
    /// Exports an image in the JPEG format.
    /// </summary>
    public class JpegExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in JPEG format.
        /// </summary>
        public async Task Export(SKBitmap image, LuxAsset asset, string? path, ExportFormat format, ExportSettings settings, IEventBus eventBus)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null for JpegExporter.");
            }
            using var stream = new FileStream(path, FileMode.Create);
            image.Encode(SKEncodedImageFormat.Jpeg, settings.Quality).SaveTo(stream);
        }
    }

    /// <summary>
    /// Exports an image in the PNG format.
    /// </summary>
    public class PngExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in PNG format.
        /// </summary>
        public async Task Export(SKBitmap image, LuxAsset asset, string? path, ExportFormat format, ExportSettings settings, IEventBus eventBus)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null for PngExporter.");
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Png, settings.Quality).SaveTo(stream);
            }
        }
    }


    /// <summary>
    /// Exports an image in the WEBP format.
    /// </summary>
    public class WebpExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in WEBP format.
        /// </summary>
        public async Task Export(SKBitmap image, LuxAsset asset, string? path, ExportFormat format, ExportSettings settings, IEventBus eventBus)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null for WebpExporter.");
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Webp, settings.Quality).SaveTo(stream);
            }
        }
    }

    public class LuxStudioExporter : IExporter
    {
        private static SKEncodedImageFormat GetMimeType(ExportFormat format) => format switch
        {
            ExportFormat.PNG => SKEncodedImageFormat.Png,
            ExportFormat.JPEG => SKEncodedImageFormat.Jpeg,
            ExportFormat.WEBP => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Jpeg
        };




        /// <summary>
        /// Publishes a stream export event with the encoded image.
        /// </summary>
        public async Task Export(SKBitmap image, LuxAsset asset, string? path, ExportFormat format, ExportSettings settings, IEventBus eventBus)
        {
            string outputPath = Path.Combine(Path.GetTempPath(), $"tbe{asset.Id.ToString()}.{format.ToString().ToLower()}");
            using (var data = image.Encode(GetMimeType(format), settings.Quality))
            {
                if (data == null)
                    throw new InvalidOperationException($"Failed to encode bitmap as {format.ToString().ToLower()}");

                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }

            await eventBus.Publish(new RequestExportOnlineEvent(outputPath, asset));
        }
    }
}
