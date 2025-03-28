using SkiaSharp;
using System.Threading.Tasks;


namespace LuxExport.Interfaces
{
    public interface IExporter
    {
        void Export(SKBitmap image, string path, ExportFormat format);
    }
}