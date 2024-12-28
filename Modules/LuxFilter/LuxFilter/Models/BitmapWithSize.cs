using SkiaSharp;

namespace LuxFilter.Models;

/// <summary>
/// Bitmap with size
/// </summary>
public class BitmapWithSize
{
    /// <summary>
    /// Bitmap
    /// </summary>
    public SKBitmap Bitmap { get; set; }

    /// <summary>
    /// Height
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bitmap">Bitmap</param>
    public BitmapWithSize(SKBitmap bitmap)
    {
        Bitmap = bitmap;
        Height = bitmap.Height;
        Width = bitmap.Width;
    }
}