using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;

namespace LuxFilter.Interfaces
{
    /// <summary>
    /// Pipeline service interface
    /// </summary>
    public interface IPipelineService
    {
        /// <summary>
        /// Add an algorithm to the pipeline
        /// </summary>
        /// <param name="algorithm">Algorithm to add to the pipeline</param>
        /// <param name="weight">Weight applied to the result of the algorithm (0-1)</param>
        void AddAlgorithm(IFilterAlgorithm algorithm, double weight);

        /// <summary>
        /// Compute scores for a collection of BitmapWithSize objects
        /// </summary>
        Task<List<double>> Compute(IEnumerable<BitmapWithSize> bitmapsWithSizes);
    }

    public class BitmapWithSize
    {
        public SKBitmap Bitmap { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public BitmapWithSize(SKBitmap bitmap)
        {
            Bitmap = bitmap;
            Height = bitmap.Height;
            Width = bitmap.Width;
        }
    }
}
