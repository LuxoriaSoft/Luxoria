using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Interfaces;
using Luxoria.SDK.Interfaces;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace LuxFilter.Services
{
    public class PipelineService : IPipelineService
    {
        // Logger service
        private readonly ILoggerService _logger;
        // Collection of filter algorithms
        private Collection<(IFilterAlgorithm, double)> _workflow;
        // Total weight
        private double _tweight;


        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineService"/> class.
        /// </summary>
        public PipelineService(ILoggerService loggerService)
        {
            // Initialize the logger service
            _logger = loggerService;
            // Initialize the collection of workflow
            _workflow = [];
            // Set the total weight to 0
            _tweight = 0.0;
        }

        /// <summary>
        /// Add an algorithm to the pipeline
        /// </summary>
        /// <param name="algorithm">Algorithm to add</param>
        public void AddAlgorithm(IFilterAlgorithm algorithm, double weight)
        {
            if (_tweight + weight > 1)
            {
                throw new ArgumentException("Pipeline error : Total weight cannot be above 1");
            }

            _workflow.Add((algorithm, weight));
            _tweight += weight;
        }

        /// <summary>
        /// Compute all algorithms present in the pipeline
        /// </summary>
        /// <param name="bitmap">Bitmap loaded from SkiaSharp</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="width">Width in pixels</param>
        public void Compute(SKBitmap bitmap, int height, int width)
        {
            _logger.Log("Executing pipeline...");
            int currentAlgo = 0;
            int totalAlgo = _workflow.Count;
            double fscore = 0;

            // Get the delay since the function call

            foreach (var step in _workflow)
            {
                IFilterAlgorithm algorithm = step.Item1;
                double weight = step.Item2;

                _logger.Log($"[{currentAlgo}/{totalAlgo}]: Executing algorithm: [{algorithm.Name}] (w={weight})...");
                var score = algorithm.Compute(bitmap, height, width);
                fscore += score * weight;
                _logger.Log($"[{algorithm.Name}]: Score: [{score}]");
                currentAlgo++;
            }

            _logger.Log($"Pipeline done (fscore={fscore}) !");
        }
    }
}
