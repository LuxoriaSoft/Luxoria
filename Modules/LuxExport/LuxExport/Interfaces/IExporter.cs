using SkiaSharp;
using System.Threading.Tasks;

public interface IExporter
{
    void Export(SKBitmap image, string path, ExportFormat format);
}
