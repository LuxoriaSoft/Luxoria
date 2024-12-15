using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Algorithms.Utils;
using SkiaSharp;

namespace LuxFilter.Algorithms.ColorVisualAesthetics;

public class SharpnessAlgo : IFilterAlgorithm
{
    /// <summary>
    /// Get the algorithm name
    /// </summary>
    public string Name => "Sharpness";

    /// <summary>
    /// Get the algorithm description
    /// </summary>
    public string Description => "Sharpness algorithm";

    /// <summary>
    /// Lacipian kernel
    /// </summary>
    private static readonly int[,] LaplacianKernel = new int[,]
    {
        { 0, -1,  0 },
        { -1,  4, -1 },
        { 0, -1,  0 }
    };

    /// <summary>
    /// Compute the sharpness of the image
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns>Returns the computed score of the algorithm</returns>
    public double Compute(SKBitmap bitmap, int height, int width)
    {

        SKBitmap grayScaleBitmap = ImageProcessing.ConvertBitmapToGrayscale(bitmap);
        SKBitmap laplacianBitmap = ApplyLaplacianKernel(grayScaleBitmap);

        return ComputeVariance(laplacianBitmap);
    }

    /// <summary>
    /// Apply the pixel to the Laplacian kernel
    /// </summary>
    /// <param name="bitmap">SKBitmap</param>
    /// <param name="x">Position x of the pixel</param>
    /// <param name="y">Position y of the pixel</param>
    /// <returns>New pixel value in byte</returns>
    private static byte ApplyPixelToLaplacianKernel(SKBitmap bitmap, int x, int y)
    {
        int pixelValue = 0;

        for (int lky = -1; lky <= 1; lky++)
        {
            for (int lkx = -1; lkx <= 1; lkx++)
            {
                int kValue = LaplacianKernel[lky + 1, lkx + 1];
                byte intensity = bitmap.GetPixel(x + lkx, y + lky).Red;
                pixelValue += intensity * kValue;
            }
        }

        return (byte)Math.Clamp(pixelValue, 0, 255);
    }

    /// <summary>
    /// Compute the variance of the image
    /// </summary>
    /// <param name="bitmap">Grayscale image</param>
    /// <returns></returns>
    private static double ComputeVariance(SKBitmap bitmap)
    {
        double mean = 0;
        double squaredDifferenceSum = 0;
        int width = bitmap.Width;
        int height = bitmap.Height;

        // Calculate mean intensity
        byte[] pixelValues = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixelValue = bitmap.GetPixel(x, y).Red;
                pixelValues[y * width + x] = pixelValue;
                mean += pixelValue;
            }
        }

        mean /= pixelValues.Length;

        // Calculate variance
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double intensity = pixelValues[y * width + x];
                double difference = intensity - mean;
                squaredDifferenceSum += difference * difference;
            }
        }

        return squaredDifferenceSum / pixelValues.Length;
    }


    /// <summary>
    /// Apply Laplacian kernel to the image
    /// </summary>
    private static SKBitmap ApplyLaplacianKernel(SKBitmap bitmap)
    {
        // Create a copy bitmap
        SKBitmap target = new SKBitmap(bitmap.Width, bitmap.Height);

        for (int y = 1; y < bitmap.Width - 1; y++)
        {
            for (int x = 1; x < bitmap.Height - 1; x++)
            {
                byte pixelValue = ApplyPixelToLaplacianKernel(bitmap, x, y);
                target.SetPixel(x, y, new SKColor(pixelValue, pixelValue, pixelValue));
            }
        }

        return target;
    }
}
