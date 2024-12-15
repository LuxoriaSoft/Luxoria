using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;

namespace LuxFilter.Interfaces
{
    public interface IPipelineService
    {
        void AddAlgorithm(IFilterAlgorithm algorithm, double weight);
        double Compute(SKBitmap bitmap, int height, int width);
    }
}
