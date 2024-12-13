using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;

namespace LuxFilter.Algorithms.ColorVisualAesthetics
{
    public class SharpnessAlgo : IFilterAlgorithm
    {
        public string Name => "Sharpness";
        public string Description => "Sharpness algorithm";

        /// <summary>
        /// Compute the sharpness of the image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns>Returns the computed score of the algorithm</returns>
        public double Compute(SKBitmap bitmap, int height, int width)
        {
            // Return a random float between 0 to 1
            var random = new Random();

            return random.NextDouble();
        }
    }
}
