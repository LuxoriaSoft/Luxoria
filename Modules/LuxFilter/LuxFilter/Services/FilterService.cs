using LuxFilter.Algorithms.Interfaces;
using System.Collections.Generic;
using System;


namespace LuxFilter.Services;

/// <summary>
/// Service for managing filter algorithms
/// </summary>
public class FilterService
{
    /// <summary>
    /// Catalog definition
    /// [Algorithm Display Name, Factory Function to Create an Instance]
    /// </summary>
    public readonly Dictionary<string, IFilterAlgorithm> Catalog =
        new()
        {
            {
                 "Resolution", new Algorithms.ImageQuality.ResolutionAlgo()
            },
            {
                "Sharpness", new Algorithms.ImageQuality.SharpnessAlgo()
            },
            {
                "Brisque", new Algorithms.PerceptualMetrics.BrisqueAlgo()
            }
        };
}
