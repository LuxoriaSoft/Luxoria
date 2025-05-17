using LuxExport.Logic;
using SkiaSharp;
using System.Threading.Tasks;

namespace LuxExport.Interfaces
{
    /// <summary>
    /// Interface for exporting images to various formats.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Exports an image to the specified path with the given format and settings.
        /// </summary>
        /// <param name="image">The image to export.</param>
        /// <param name="path">The path where the exported file will be saved.</param>
        /// <param name="format">The format to export the image in.</param>
        /// <param name="settings">The settings to use for the export, such as quality and color space.</param>
        void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings);
    }
}
