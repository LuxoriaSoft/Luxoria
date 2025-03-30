using System;
using LuxExport.Interfaces;

namespace LuxExport.Logic
{
    /// <summary>
    /// Factory class responsible for creating instances of IExporter based on the selected export format.
    /// </summary>
    public static class ExporterFactory
    {
        /// <summary>
        /// Creates an exporter instance based on the provided export format.
        /// </summary>
        /// <param name="format">The export format that determines which exporter to create.</param>
        /// <returns>An instance of IExporter corresponding to the given format.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported export format is provided.</exception>
        public static IExporter CreateExporter(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.JPEG => new JpegExporter(),
                ExportFormat.PNG => new PngExporter(),
                ExportFormat.BMP => new BmpExporter(),
                ExportFormat.GIF => new GifExporter(),
                ExportFormat.ICO => new IcoExporter(),
                ExportFormat.WBMP => new WbmpExporter(),
                ExportFormat.WEBP => new WebpExporter(),
                ExportFormat.PKM => new PkmExporter(),
                ExportFormat.KTX => new KtxExporter(),
                ExportFormat.ASTC => new AstcExporter(),
                ExportFormat.DNG => new DngExporter(),
                ExportFormat.HEIF => new HeifExporter(),
                ExportFormat.AVIF => new AvifExporter(),
                ExportFormat.JPEGXL => new JpegxlExporter(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}
