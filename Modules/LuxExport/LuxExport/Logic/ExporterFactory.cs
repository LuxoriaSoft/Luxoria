using System;
using LuxExport.Interfaces;

namespace LuxExport.Logic;

public static class ExporterFactory
{
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
