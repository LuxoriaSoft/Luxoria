using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.SDK.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuxFilter.Services;

/// <summary>
/// Pipeline service
/// </summary>
public class PipelineService : IPipelineService
{
    private readonly ILoggerService _logger;
    private ICollection<(IFilterAlgorithm, double)> _workflow = [];

    public event EventHandler<TimeSpan> OnPipelineFinished;
    public event EventHandler<(Guid, double)> OnScoreComputed;

    public PipelineService(ILoggerService loggerService)
    {
        _logger = loggerService;
        OnPipelineFinished += (sender, e) => { };
        OnScoreComputed += (sender, e) => { };
    }

    public IPipelineService AddAlgorithm(IFilterAlgorithm algorithm, double weight)
    {
        _workflow.Add((algorithm, weight));
        return this;
    }

    /// <summary>
    /// Compute scores and return a dictionary from Guid to (algorithmName → score) map
    /// </summary>
    public async Task<Dictionary<Guid, Dictionary<string, double>>> Compute(IEnumerable<(Guid, ImageData)> bitmaps)
    {
        if (_workflow == null || !_workflow.Any())
        {
            _logger.Log("Pipeline has no algorithms to execute.");
            throw new InvalidOperationException("Pipeline has no algorithms to execute.");
        }

        DateTime start = DateTime.Now;
        _logger.Log("Executing pipeline...");

        var indexedBitmaps = bitmaps.ToList();
        var results = new ConcurrentDictionary<Guid, Dictionary<string, double>>();

        await Task.Run(() =>
        {
            Parallel.ForEach(indexedBitmaps, indexedBitmap =>
            {
                var (guid, data) = indexedBitmap;
                Dictionary<string, double> scores = [];

                foreach (var (algorithm, weight) in _workflow)
                {
                    try
                    {
                        var score = algorithm.Compute(data);
                        scores[algorithm.Name] = score;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error executing [{algorithm.Name}] on bitmap: {ex.Message}");
                    }
                }

                var fscore = scores.Values.Sum();
                results.TryAdd(guid, scores);
                OnScoreComputed?.Invoke(this, (guid, fscore));
            });
        });

        TimeSpan totalTime = DateTime.Now - start;
        _logger.Log($"Pipeline execution completed in {totalTime.TotalSeconds:F2}s.");
        OnPipelineFinished?.Invoke(this, totalTime);

        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
