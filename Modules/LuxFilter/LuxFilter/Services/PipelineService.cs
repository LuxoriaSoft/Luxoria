using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Interfaces;
using Luxoria.SDK.Interfaces;
using SkiaSharp;
using System.Collections.Concurrent;

namespace LuxFilter.Services
{
    /// <summary>
    /// Pipeline service
    /// </summary>
    public class PipelineService : IPipelineService
    {
        /// <summary>
        /// Variables
        /// </summary>
        private readonly ILoggerService _logger;
        private ICollection<(IFilterAlgorithm, double)> _workflow;
        private double _tweight;

        /// <summary>
        /// Event handlers
        /// </summary>
        /// <summary>
        /// Event handler when the pipeline has finished computing scores
        /// </summary>
        public event EventHandler<TimeSpan> OnPipelineFinished;

        /// <summary>
        /// Event handler when a score has been computed
        /// </summary>
        public event EventHandler<(Guid, double)> OnScoreComputed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerService">LoggerService used to log each info, debug, ...</param>
        public PipelineService(ILoggerService loggerService)
        {
            _logger = loggerService;
            _workflow = new List<(IFilterAlgorithm, double)>();
            _tweight = 0.0;

            // Event handlers to avoid null reference exceptions
            OnPipelineFinished += (sender, e) => { };
            OnScoreComputed += (sender, e) => { };
        }

        /// <summary>
        /// Add an algorithm to the pipeline
        /// </summary>
        /// <param name="algorithm">Add an algorithm to the pipeline</param>
        /// <param name="weight">Apply a weight on result</param>
        /// <exception cref="ArgumentException">If weight is lower than 0 or upper than 1, throw an exception</exception>
        public IPipelineService AddAlgorithm(IFilterAlgorithm algorithm, double weight)
        {
            if (_tweight + weight > 1)
            {
                throw new ArgumentException("Pipeline error: Total weight cannot be above 1");
            }

            _workflow.Add((algorithm, weight));
            _tweight += weight;

            // Return this instance to allow chaining
            return this;
        }

        /// <summary>
        /// Compute scores for a collection of BitmapWithSize objects
        /// </summary>
        /// <param name="bitmaps">Bitmap gateways</param>
        /// <returns>Return a list which contains each score of each bitmap</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<(Guid, double)>> Compute(IEnumerable<(Guid, SKBitmap)> bitmaps)
        {
            // Check if there are algorithms in the workflow
            if (_workflow == null || !_workflow.Any())
            {
                _logger.Log("Pipeline has no algorithms to execute.");
                throw new InvalidOperationException("Pipeline has no algorithms to execute.");
            }

            DateTime start = DateTime.Now;
            _logger.Log("Executing pipeline...");

            // Convert bitmaps into indexed format
            var indexedBitmaps = bitmaps.ToList();

            // Concurrent dictionary to store results
            var results = new ConcurrentDictionary<Guid, double>();

            // Execute pipeline asynchronously
            await Task.Run(() =>
            {
                Parallel.ForEach(indexedBitmaps, indexedBitmap =>
                {
                    var (guid, bitmap) = indexedBitmap;
                    double fscore = 0;

                    foreach (var step in _workflow)
                    {
                        IFilterAlgorithm algorithm = step.Item1;
                        double weight = step.Item2;

                        _logger.Log($"Executing algorithm: [{algorithm.Name}] (w={weight}) (thrd={Thread.CurrentThread.ManagedThreadId})...");

                        try
                        {
                            // Measure algorithm execution time
                            DateTime startingTime = DateTime.Now;
                            var score = algorithm.Compute(bitmap, bitmap.Height, bitmap.Width);
                            DateTime endingTime = DateTime.Now;
                            TimeSpan executionTime = endingTime - startingTime;

                            _logger.Log($"Score for algorithm [{algorithm.Name}] on bitmap: {score}, time consumed: ({executionTime.TotalSeconds:F2})s");

                            // Accumulate weighted score
                            fscore += score * weight;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Error executing [{algorithm.Name}] on bitmap: {ex.Message}");
                        }
                    }

                    // Store final score for the bitmap
                    _logger.Log($"Final score for bitmap with Guid {guid}: {fscore}");
                    results.TryAdd(guid, fscore);
                    // Raise OnScoreComputed event
                    OnScoreComputed?.Invoke(this, (guid, fscore));  // Trigger the OnScoreComputed event
                });
            });

            DateTime end = DateTime.Now;
            TimeSpan totalTime = end - start;
            _logger.Log($"Pipeline execution completed (time consumed = {totalTime.TotalSeconds:F2})s.");

            // Raise OnPipelineFinished event
            OnPipelineFinished?.Invoke(this, totalTime);  // Trigger the OnPipelineFinished event

            // Return results as a list of tuples with Guid and the corresponding score, ordered by Guid
            return results.OrderBy(kvp => kvp.Key).Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }
    }
}
