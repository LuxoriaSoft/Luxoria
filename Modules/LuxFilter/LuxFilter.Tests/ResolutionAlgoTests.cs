using LuxFilter.Algorithms.ImageQuality;
using Luxoria.Modules.Models;
using SkiaSharp;

namespace LuxFilter.Tests
{
    /// <summary>
    /// Unit tests for the ResolutionAlgo class.
    /// </summary>
    public class ResolutionAlgoTests
    {
        /// <summary>
        /// Tests whether the Compute method returns the correct resolution value.
        /// </summary>
        [Fact]
        public void Compute_ShouldReturnCorrectResolution()
        {
            var algorithm = new ResolutionAlgo();
            var bitmap = new SKBitmap(50, 40);

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.Equal(2000, result);
        }

        /// <summary>
        /// Tests whether the Compute method handles cases where width or height is zero.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleZeroWidthOrHeight()
        {
            var algorithm = new ResolutionAlgo();
            var zeroWidthBitmap = new SKBitmap(0, 40);
            var zeroHeightBitmap = new SKBitmap(50, 0);

            var resultZeroWidth = algorithm.Compute(new(zeroWidthBitmap, FileExtension.UNKNOWN));
            var resultZeroHeight = algorithm.Compute(new(zeroHeightBitmap, FileExtension.UNKNOWN));

            Assert.Equal(0, resultZeroWidth);
            Assert.Equal(0, resultZeroHeight);
        }
    }
}
