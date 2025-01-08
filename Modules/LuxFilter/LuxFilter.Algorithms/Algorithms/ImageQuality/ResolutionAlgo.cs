﻿using LuxFilter.Algorithms.Interfaces;
using SkiaSharp;

namespace LuxFilter.Algorithms.ImageQuality
{
    public class ResolutionAlgo : IFilterAlgorithm
    {
        /// <summary>
        /// Get the algorithm name
        /// </summary>
        public string Name => "Resolution";

        /// <summary>
        /// Get the algorithm description
        /// </summary>
        public string Description => "Resolution algorithm";

        /// <summary>
        /// Compute the sharpness of the image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns>Returns the computed score of the algorithm</returns>
        public double Compute(SKBitmap bitmap, int height, int width)
        {
            // Get the resolution of the image
            return height * width;
        }
    }
}
