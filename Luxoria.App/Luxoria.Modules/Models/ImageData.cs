using SkiaSharp;
using System.Collections.ObjectModel;

namespace Luxoria.Modules.Models;

/// <summary>
/// Represents an image with pixel data, dimensions, and format information.
/// </summary>
public class ImageData
{
    /// <summary>
    /// Identifier for the image instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Contains the bitmap data of the image.
    /// </summary>
    public SKBitmap Bitmap { get; }

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int Width => Bitmap.Width;

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int Height => Bitmap.Height;

    /// <summary>
    /// Gets the format of the image (e.g., "PNG", "JPEG").
    /// </summary>
    public FileExtension Format { get; }

    /// <summary>
    /// EXIF metadata for the image.
    /// </summary>
    public ReadOnlyDictionary<string, string> EXIF { get; }

    /// <summary>
    /// Contains the filter scores for the image.
    /// </summary>
    public ReadOnlyDictionary<string, double>? Filters { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageData"/> class.
    /// </summary>
    /// <param name="bitmap">The bitmap (SKBitmap) data of the image.</param>
    /// <param name="format">The format of the image (e.g., "PNG", "JPEG").</param>
    /// <param name="exifMetaData">The EXIF metadata dictionary.</param>
    public ImageData(SKBitmap bitmap, FileExtension format, Dictionary<string, string>? exifMetaData = null)
    {
        Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        Format = format;
        EXIF = new ReadOnlyDictionary<string, string>(exifMetaData ?? []);
        Filters = null;
    }

    /// <summary>
    /// Returns a string representation of the image, including its format and dimensions.
    /// </summary>
    /// <returns>A string representation of the image.</returns>
    public override string ToString()
    {
        return $"{Format} Image: {Width}x{Height}, EXIF Entries: {EXIF.Count}";
    }
}
