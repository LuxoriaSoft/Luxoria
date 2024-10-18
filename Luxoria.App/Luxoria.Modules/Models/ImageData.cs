namespace Luxoria.Modules.Models;

public class ImageData
{
    public byte[] PixelData { get; set; } // Raw pixel data
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } // e.g., "PNG", "JPEG"

    public ImageData(byte[] pixelData, int width, int height, string format)
    {
        PixelData = pixelData;
        Width = width;
        Height = height;
        Format = format;
    }

    // You can add methods for image processing, e.g., resizing, filtering, etc.
}
