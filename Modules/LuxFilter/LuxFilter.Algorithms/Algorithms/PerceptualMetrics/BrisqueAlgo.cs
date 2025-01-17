using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;

namespace LuxFilter.Algorithms.PerceptualMetrics;

public class BrisqueAlgo : IFilterAlgorithm
{
    /// <summary>
    /// Get the algorithm name
    /// </summary>
    public string Name => "Brisque";

    /// <summary>
    /// Get the algorithm description
    /// </summary>
    public string Description => "Brisque algorithm";

    /// <summary>
    /// Compute the brisque of the image
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns>Returns the computed score of the algorithm</returns>
    public double Compute(SKBitmap bitmap, int height, int width)
    {
        return 0;
    }
}
