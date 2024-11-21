namespace Luxoria.Modules.Models
{
    public class ImageData
    {
        public byte[] PixelData { get; set; } // Raw pixel data
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } // e.g., "PNG", "JPEG"

        public ImageData(byte[] pixelData, int width, int height, string format)
        {
            if (width <= 0)
            {
                throw new ArgumentException("Width must be greater than zero.", nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException("Height must be greater than zero.", nameof(height));
            }

            PixelData = pixelData;
            Width = width;
            Height = height;
            Format = format;
        }

        // You can add methods for image processing, e.g., resizing, filtering, etc.
    }
}