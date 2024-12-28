using SkiaSharp;

namespace LuxFilter.Algorithms.Utils
{
    public class ImageProcessing
    {
        public static SKBitmap ConvertBitmapToGrayscale(SKBitmap bitmap)
        {
            var grayBitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Gray8, SKAlphaType.Opaque);
            // Convert the bitmap to grayscale

            using (var canvas = new SKCanvas(grayBitmap))
            {
                var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                        0.299f,
                        0.587f,
                        0.114f,
                        0,
                        0,  // Red
                        0.299f,
                        0.587f,
                        0.114f,
                        0,
                        0,  // Green
                        0.299f,
                        0.587f,
                        0.114f,
                        0,
                        0,  // Blue
                        0,
                        0,
                        0,
                        1,
                        0   // Alpha
                    })
                };
                canvas.DrawBitmap(bitmap, 0, 0, paint);
            }

            return grayBitmap;
        }
    }
}
