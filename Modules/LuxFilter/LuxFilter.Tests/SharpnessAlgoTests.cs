using LuxFilter.Algorithms.ImageQuality;
using SkiaSharp;

namespace LuxFilter.Tests
{
    /// <summary>
    /// Unit tests for the SharpnessAlgo class.
    /// </summary>
    public class SharpnessAlgoTests
    {
        /// <summary>
        /// Tests whether the Compute method returns a non-negative sharpness score.
        /// </summary>
        [Fact]
        public void Compute_ShouldReturnSharpnessScore()
        {
            var algorithm = new SharpnessAlgo();
            var bitmap = new SKBitmap(50, 40);

            var result = algorithm.Compute(bitmap, 50, 40);
            Assert.True(result >= 0);
        }
    }
}
