using SkiaSharp;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using LuxExport.Interfaces;

namespace LuxExport.Logic;

public class JpegExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using var stream = new FileStream(path, FileMode.Create);
        image.Encode(SKEncodedImageFormat.Jpeg, settings.Quality).SaveTo(stream);
    }
}


public class PngExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Png, settings.Quality).SaveTo(stream);
        }
    }
}

public class BmpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Bmp, settings.Quality).SaveTo(stream);
        }
    }
}

public class GifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Gif, settings.Quality).SaveTo(stream);
        }
    }
}

public class IcoExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Ico, settings.Quality).SaveTo(stream);
        }
    }
}

public class WbmpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Wbmp, settings.Quality).SaveTo(stream);
        }
    }
}

public class WebpExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Webp, settings.Quality).SaveTo(stream);
        }
    }
}

public class PkmExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Pkm, settings.Quality).SaveTo(stream);
        }
    }
}

public class KtxExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Ktx, settings.Quality).SaveTo(stream);
        }
    }
}

public class AstcExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Astc, settings.Quality).SaveTo(stream);
        }
    }
}

public class DngExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Dng, settings.Quality).SaveTo(stream);
        }
    }
}

public class HeifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Heif, settings.Quality).SaveTo(stream);
        }
    }
}

public class AvifExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Avif, settings.Quality).SaveTo(stream);
        }
    }
}

public class JpegxlExporter : IExporter
{
    public void Export(SKBitmap image, string path, ExportFormat format, ExportSettings settings)
    {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            image.Encode(SKEncodedImageFormat.Jpegxl, settings.Quality).SaveTo(stream);
        }
    }
}
