﻿using SkiaSharp;

namespace LuxFilter.Algorithms.Interfaces;

public interface IFilterAlgorithm
{
    /// <summary>
    /// Get the algorithm name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the algorithm description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Execute the algorithm
    /// </summary>
    double Compute(SKBitmap bitmap, int height, int width);
}