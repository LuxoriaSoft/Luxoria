using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Models;

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
        IPipelineService AddAlgorithm(IFilterAlgorithm algorithm, double weight);

        /// <summary>
        /// Compute scores for a collection of BitmapWithSize objects
        /// </summary>
        Task<List<double>> Compute(IEnumerable<BitmapWithSize> bitmapsWithSizes);
    }
}
