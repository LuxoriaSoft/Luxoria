using SkiaSharp;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using LuxExport.Interfaces;

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
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
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
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
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
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Webp, settings.Quality).SaveTo(stream);
            }
        }
    }
}
