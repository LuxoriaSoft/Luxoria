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
    /// Exports an image in the BMP format.
    /// </summary>
    public class BmpExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in BMP format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Bmp, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the GIF format.
    /// </summary>
    public class GifExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in GIF format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Gif, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the ICO format.
    /// </summary>
    public class IcoExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in ICO format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Ico, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the WBMP format.
    /// </summary>
    public class WbmpExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in WBMP format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Wbmp, settings.Quality).SaveTo(stream);
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

    /// <summary>
    /// Exports an image in the PKM format.
    /// </summary>
    public class PkmExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in PKM format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Pkm, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the KTX format.
    /// </summary>
    public class KtxExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in KTX format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Ktx, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the ASTC format.
    /// </summary>
    public class AstcExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in ASTC format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Astc, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the DNG format.
    /// </summary>
    public class DngExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in DNG format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Dng, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the HEIF format.
    /// </summary>
    public class HeifExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in HEIF format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Heif, settings.Quality).SaveTo(stream);
            }
        }
    }


    /// <summary>
    /// Exports an image in the AVIF format.
    /// </summary>
    public class AvifExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in AVIF format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Avif, settings.Quality).SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Exports an image in the JPEG-XL format.
    /// </summary>
    public class JpegxlExporter : IExporter
    {
        /// <summary>
        /// Exports the provided image to a file in JPEG-XL format.
        /// </summary>
        public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                image.Encode(SKEncodedImageFormat.Jpegxl, settings.Quality).SaveTo(stream);
            }
        }
    }
}
