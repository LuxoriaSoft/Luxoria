using LuxFilter.Algorithms.ImageQuality;
using Luxoria.Modules.Models;
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

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0);
        }

        /// <summary>
        /// Verifies that an entirely black image returns a sharpness score of zero.
        /// </summary>

        [Fact]
        public void Compute_ShouldHandleBlackImage()
        {
            var algorithm = new SharpnessAlgo();
            var bitmap = new SKBitmap(50, 40);
            bitmap.Erase(SKColors.Black);
            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Ensures that a color image is correctly converted to grayscale.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleWhiteImage()
        {
            var algorithm = new SharpnessAlgo();
            var bitmap = new SKBitmap(50, 40);
            bitmap.Erase(SKColors.White);
            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.Equal(0, result);
        }
    }
}
