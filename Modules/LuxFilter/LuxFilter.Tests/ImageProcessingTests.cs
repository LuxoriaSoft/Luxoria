using LuxFilter.Algorithms.Utils;
using SkiaSharp;

namespace LuxFilter.Tests;

/// <summary>
/// Unit tests for the ImageProcessing class.
/// </summary>
public class ImageProcessingTests
{
    /// <summary>
    /// Tests whether ConvertBitmapToGrayscale correctly converts a color image.
    /// </summary>
    [Fact]
    public void ConvertBitmapToGrayscale_ShouldConvertCorrectly()
    {
        var bitmap = new SKBitmap(50, 40);
        bitmap.Erase(SKColors.Red);

        var grayBitmap = ImageProcessing.ConvertBitmapToGrayscale(bitmap);
        var firstPixel = grayBitmap.GetPixel(0, 0);

        Assert.Equal(firstPixel.Red, firstPixel.Green);
        Assert.Equal(firstPixel.Green, firstPixel.Blue);
    }
}
