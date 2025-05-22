using LuxFilter.Algorithms.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LuxFilter.Services;

public class FilterService
{
    /// <summary>
    /// Static catalog definition
    /// </summary>
    public static ImmutableDictionary<string, IFilterAlgorithm> Catalog { get; } = ImmutableDictionary.CreateRange(
        new Dictionary<string, IFilterAlgorithm>
        {
            { "Resolution", new Algorithms.ImageQuality.ResolutionAlgo() },
            { "Sharpness", new Algorithms.ImageQuality.SharpnessAlgo() },
            { "Brisque", new Algorithms.PerceptualMetrics.BrisqueAlgo() },
            { "CLIP", new Algorithms.ColorVisualAesthetics.CLIPAlgo() }
        }
    );
}
