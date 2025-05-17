using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Algorithms.Utils;
using Luxoria.Modules.Models;
using SkiaSharp;

namespace LuxFilter.Algorithms.ImageQuality;

public class SharpnessAlgo : IFilterAlgorithm
{
    /// <summary>
    /// Gets the name of the algorithm.
    /// </summary>
    public string Name => "Sharpness";

    /// <summary>
    /// Gets the description of the algorithm.
    /// </summary>
    public string Description => "Sharpness algorithm";

    /// <summary>
    /// Laplacian kernel used for edge detection.
    /// This kernel emphasizes areas of rapid intensity change (edges).
    /// </summary>
    private static readonly int[,] LaplacianKernel = new int[,]
    {
        { 0, -1,  0 },
        { -1,  4, -1 },
        { 0, -1,  0 }
    };

    /// <summary>
    /// Computes the sharpness score of the image based on the variance of the Laplacian.
    /// </summary>
    /// <param name="bitmap">The input image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="width">The width of the image.</param>
    /// <returns>Returns the computed sharpness score.</returns>
    public double Compute(ImageData data)
    {
        using SKBitmap grayScaleBitmap = ImageProcessing.ConvertBitmapToGrayscale(data.Bitmap); // Convert the image to grayscale.
        using SKBitmap laplacianBitmap = ApplyLaplacianKernel(grayScaleBitmap); // Apply the Laplacian kernel to highlight edges.
        return ComputeVariance(laplacianBitmap); // Calculate the variance of the resulting image as the sharpness score.
    }

    /// <summary>
    /// Applies the Laplacian kernel to a single pixel.
    /// This function calculates the intensity of edges by summing weighted neighboring pixel intensities.
    /// </summary>
    /// <param name="bitmap">The grayscale image.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>Returns the new pixel intensity after applying the kernel.</returns>
    private static byte ApplyPixelToLaplacianKernel(SKBitmap bitmap, int x, int y)
    {
        int pixelValue = 0;

        for (int lky = -1; lky <= 1; lky++)
        {
            for (int lkx = -1; lkx <= 1; lkx++)
            {
                int kValue = LaplacianKernel[lky + 1, lkx + 1]; // Kernel value at the current position.
                byte intensity = bitmap.GetPixel(x + lkx, y + lky).Red; // Intensity of the neighboring pixel.
                pixelValue += intensity * kValue; // Weighted sum.
            }
        }

        return (byte)Math.Clamp(pixelValue, 0, 255); // Clamp the result to valid pixel range (0-255).
    }

    /// <summary>
    /// Computes the variance of pixel intensities in the image.
    /// Variance measures the spread of intensity values, indicating sharpness.
    /// </summary>
    /// <param name="bitmap">The grayscale image after applying the Laplacian.</param>
    /// <returns>Returns the variance of the pixel intensities.</returns>
    private static double ComputeVariance(SKBitmap bitmap)
    {
        double mean = 0;
        double squaredSum = 0;
        int width = bitmap.Width;
        int height = bitmap.Height;
        int totalPixels = width * height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixelValue = bitmap.GetPixel(x, y).Red; // Grayscale intensity.
                mean += pixelValue; // Sum of pixel values.
                squaredSum += pixelValue * pixelValue; // Sum of squared pixel values.
            }
        }

        mean /= totalPixels; // Calculate the mean intensity.
        return (squaredSum / totalPixels) - (mean * mean); // Variance formula: E[X^2] - (E[X])^2.
    }

    /// <summary>
    /// Applies the Laplacian kernel to the entire image.
    /// This highlights edges by calculating intensity changes for each pixel.
    /// </summary>
    /// <param name="bitmap">The input grayscale image.</param>
    /// <returns>Returns the processed image with edges highlighted.</returns>
    private static SKBitmap ApplyLaplacianKernel(SKBitmap bitmap)
    {
        SKBitmap target = new SKBitmap(bitmap.Width, bitmap.Height);

        // Skip edge pixels to avoid out-of-bound errors.
        for (int y = 1; y < bitmap.Height - 1; y++)
        {
            for (int x = 1; x < bitmap.Width - 1; x++)
            {
                byte pixelValue = ApplyPixelToLaplacianKernel(bitmap, x, y); // Apply kernel to each pixel.
                target.SetPixel(x, y, new SKColor(pixelValue, pixelValue, pixelValue)); // Set the result.
            }
        }

        return target;
    }
}
