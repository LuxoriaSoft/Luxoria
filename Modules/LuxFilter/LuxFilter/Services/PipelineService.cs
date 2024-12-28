using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Interfaces;
using Luxoria.SDK.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuxFilter.Services
{
    public class PipelineService : IPipelineService
    {
        private readonly ILoggerService _logger;
        private ICollection<(IFilterAlgorithm, double)> _workflow;
        private double _tweight;

        public PipelineService(ILoggerService loggerService)
        {
            _logger = loggerService;
            _workflow = new List<(IFilterAlgorithm, double)>();
            _tweight = 0.0;
        }

        public void AddAlgorithm(IFilterAlgorithm algorithm, double weight)
        {
            if (_tweight + weight > 1)
            {
                throw new ArgumentException("Pipeline error: Total weight cannot be above 1");
            }

            _workflow.Add((algorithm, weight));
            _tweight += weight;
        }

        public async Task<List<double>> Compute(IEnumerable<BitmapWithSize> bitmapsWithSizes)
        {
            if (_workflow == null || !_workflow.Any())
            {
                _logger.Log("Pipeline has no algorithms to execute.");
                throw new InvalidOperationException("Pipeline has no algorithms to execute.");
            }

            _logger.Log("Executing pipeline...");
            var allBitmapScores = new List<double>(); // To store the final score for each bitmap

            foreach (var bitmapWithSize in bitmapsWithSizes)
            {
                double fscore = 0; // Final score for this bitmap
                int totalAlgo = _workflow.Count;

                // Execute each algorithm on the current bitmap
                foreach (var step in _workflow)
                {
                    IFilterAlgorithm algorithm = step.Item1;
                    double weight = step.Item2;

                    _logger.Log($"Executing algorithm: [{algorithm.Name}] (w={weight}) on bitmap of size {bitmapWithSize.Width}x{bitmapWithSize.Height}...");
                    try
                    {
                        var score = algorithm.Compute(bitmapWithSize.Bitmap, bitmapWithSize.Height, bitmapWithSize.Width);
                        _logger.Log($"Score for algorithm [{algorithm.Name}] on bitmap: {score}");
                        fscore += score * weight;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error executing [{algorithm.Name}] on bitmap: {ex.Message}");
                    }
                }

                _logger.Log($"Final score for bitmap: {fscore}");
                allBitmapScores.Add(fscore);
            }

            _logger.Log("Pipeline execution completed.");
            return allBitmapScores;
        }

    }
}
