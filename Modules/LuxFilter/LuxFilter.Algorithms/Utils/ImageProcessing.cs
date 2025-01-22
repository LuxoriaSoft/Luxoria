using SkiaSharp;

namespace LuxFilter.Algorithms.Utils
{
    public class ImageProcessing
    {
        /// <summary>
        /// Converts an SKBitmap image to grayscale using a color matrix.
        /// </summary>
        /// <param name="bitmap">The input image to be converted to grayscale.</param>
        /// <returns>A new SKBitmap in grayscale.</returns>
        public static SKBitmap ConvertBitmapToGrayscale(SKBitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap), "Input bitmap cannot be null.");
            }

            // Create a new SKBitmap with Gray8 color type and opaque alpha type
            var grayBitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Gray8, SKAlphaType.Opaque);

            // Use a canvas to draw the original bitmap onto the grayscale bitmap
            using (var canvas = new SKCanvas(grayBitmap))
            {
                // Create a paint object with a grayscale color filter
                var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                        0.299f, 0.587f, 0.114f, 0, 0, // Red contribution
                        0.299f, 0.587f, 0.114f, 0, 0, // Green contribution
                        0.299f, 0.587f, 0.114f, 0, 0, // Blue contribution
                        0,      0,      0,      1, 0  // Alpha
                    })
                };

                // Draw the original bitmap onto the grayscale canvas
                canvas.DrawBitmap(bitmap, 0, 0, paint);
            }

            return grayBitmap;
        }
    }
}
