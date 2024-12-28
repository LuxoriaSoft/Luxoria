using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Interfaces;
using Luxoria.SDK.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
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

            DateTime start = DateTime.Now;
            _logger.Log("Executing pipeline...");

            var indexedBitmaps = bitmapsWithSizes
                .Select((bitmap, index) => (Index: index, BitmapWithSize: bitmap))
                .ToList();

            var results = new ConcurrentDictionary<int, double>();

            await Task.Run(() =>
            {
                Parallel.ForEach(indexedBitmaps, indexedBitmap =>
                {
                    var (index, bitmapWithSize) = indexedBitmap;

                    double fscore = 0;

                    foreach (var step in _workflow)
                    {
                        IFilterAlgorithm algorithm = step.Item1;
                        double weight = step.Item2;
                        DateTime startingTime = DateTime.Now;

                        _logger.Log($"Executing algorithm: [{algorithm.Name}] (w={weight}) (thrd={Thread.CurrentThread.ManagedThreadId})...");
                        try
                        {
                            var score = algorithm.Compute(bitmapWithSize.Bitmap, bitmapWithSize.Height, bitmapWithSize.Width);
                            DateTime endingTime = DateTime.Now;
                            _logger.Log($"Score for algorithm [{algorithm.Name}] on bitmap: {score}, time consumed : ({endingTime - startingTime})s");
                            fscore += score * weight;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Error executing [{algorithm.Name}] on bitmap: {ex.Message}");
                        }
                    }

                    _logger.Log($"Final score for bitmap at index {index}: {fscore}");
                    results.TryAdd(index, fscore);
                });
            });

            DateTime end = DateTime.Now;
            _logger.Log($"Pipeline execution completed (time consumed = {end - start})s.");

            // Return results ordered by index
            return results.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        }

    }
}
