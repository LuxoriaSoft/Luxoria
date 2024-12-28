using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;
using System.Collections.Generic;

namespace LuxFilter.Interfaces
{
    public interface IPipelineService
    {
        void AddAlgorithm(IFilterAlgorithm algorithm, double weight);
        // Method to compute scores for a collection of BitmapWithSize
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
