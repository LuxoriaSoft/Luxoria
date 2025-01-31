﻿using LuxFilter.Algorithms.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    /// <summary>
    /// Tests whether ConvertBitmapToGrayscale handles a null input.
    /// </summary>
    [Fact]
    public void ConvertBitmapToGrayscale_ShouldThrowExceptionForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => ImageProcessing.ConvertBitmapToGrayscale(null));
    }
}
