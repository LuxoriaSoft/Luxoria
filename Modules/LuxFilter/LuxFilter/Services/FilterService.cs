using LuxFilter.Algorithms.ColorVisualAesthetics;
using LuxFilter.Algorithms.ImageQuality;
using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Algorithms.PerceptualMetrics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LuxFilter.Services;

public class FilterService
{
    /// <summary>
    /// Static catalog definition
    /// </summary>
    private static readonly Dictionary<string, Lazy<IFilterAlgorithm>> _lazyCatalog = new()
    {
        { "Resolution", new Lazy<IFilterAlgorithm>(() => new ResolutionAlgo()) },
        { "Sharpness", new Lazy<IFilterAlgorithm>(() => new SharpnessAlgo()) },
        { "Brisque", new Lazy<IFilterAlgorithm>(() => new BrisqueAlgo()) },
        { "CLIP", new Lazy<IFilterAlgorithm>(() => { 
            try {
                return new CLIPAlgo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing CLIPAlgo: {ex.Message}");
                return new ResolutionAlgo();
            }
        }) },
    };

    public static ImmutableDictionary<string, IFilterAlgorithm> Catalog { get; } =
        _lazyCatalog.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.Value);

}
