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
                ExportFormat.WEBP => new WebpExporter(),
                ExportFormat.LuxStudio => new LuxStudioExporter(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}
