using SkiaSharp;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using LuxExport.Interfaces;

namespace LuxExport.Logic;

public class JpegExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(stream);
        }
    }
}

public class PngExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        }
    }
}

public class BmpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Bmp, 100).SaveTo(stream);
        }
    }
}

public class GifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Gif, 100).SaveTo(stream);
        }
    }
}

public class IcoExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Ico, 100).SaveTo(stream);
        }
    }
}

public class WbmpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Wbmp, 100).SaveTo(stream);
        }
    }
}

public class WebpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Webp, 100).SaveTo(stream);
        }
    }
}

public class PkmExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Pkm, 100).SaveTo(stream);
        }
    }
}

public class KtxExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Ktx, 100).SaveTo(stream);
        }
    }
}

public class AstcExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Astc, 100).SaveTo(stream);
        }
    }
}

public class DngExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Dng, 100).SaveTo(stream);
        }
    }
}

public class HeifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Heif, 100).SaveTo(stream);
        }
    }
}

public class AvifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Avif, 100).SaveTo(stream);
        }
    }
}

public class JpegxlExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Jpegxl, 100).SaveTo(stream);
        }
    }
}
