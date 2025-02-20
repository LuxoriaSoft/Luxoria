using LuxFilter.Algorithms.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LuxFilter.Services;

public class FilterService
{
    /// <summary>
    /// Catalog definition
    /// [Algorithm Display Name, Factory Function to Create an Instance]
    /// </summary>
    public ImmutableDictionary<string, IFilterAlgorithm> Catalog { get; }

    public FilterService()
    {
        Catalog = ImmutableDictionary.CreateRange(new Dictionary<string, IFilterAlgorithm>
        {
            { "Resolution", new Algorithms.ImageQuality.ResolutionAlgo() },
            { "Sharpness", new Algorithms.ImageQuality.SharpnessAlgo() },
            { "Brisque", new Algorithms.PerceptualMetrics.BrisqueAlgo() }
        });
    }
}
