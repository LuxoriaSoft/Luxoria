using LuxFilter.Algorithms.PerceptualMetrics;
using Luxoria.Modules.Models;
using SkiaSharp;

namespace LuxFilter.Tests
{
    /// <summary>
    /// Unit tests for the BrisqueAlgo class.
    /// </summary>
    public class BrisqueAlgoTests
    {
        /// <summary>
        /// Tests whether the Compute method returns a valid Brisque score for a normal image.
        /// </summary>
        [Fact]
        public void Compute_ShouldReturnBrisqueScore()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);

            ImageData data = new(bitmap, FileExtension.UNKNOWN, null);

            var result = algorithm.Compute(data);
            Assert.True(result >= 0, "Brisque score should be non-negative.");
        }

        /// <summary>
        /// Verifies that an entirely black image returns a Brisque score.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleBlackImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);
            bitmap.Erase(SKColors.Black);

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be non-negative for black images.");
        }

        /// <summary>
        /// Ensures that a white image returns a Brisque score.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleWhiteImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);
            bitmap.Erase(SKColors.White);

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be non-negative for white images.");
        }

        /// <summary>
        /// Tests Brisque score for a noisy image.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleNoisyImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);
            Random random = new Random();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    byte value = (byte)random.Next(0, 256);
                    bitmap.SetPixel(x, y, new SKColor(value, value, value));
                }
            }

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be valid for noisy images.");
        }

        /// <summary>
        /// Tests Brisque score for an image with horizontal gradient.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleGradientImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);

            for (int x = 0; x < bitmap.Width; x++)
            {
                byte gray = (byte)(x * 255 / bitmap.Width);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
                }
            }

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be valid for gradient images.");
        }

        /// <summary>
        /// Tests Brisque score for an image with random colors.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleRandomColorImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(50, 40);
            Random random = new Random();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, new SKColor(
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256)
                    ));
                }
            }

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be valid for color images.");
        }

        /// <summary>
        /// Tests Brisque score computation with a large image.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleLargeImage()
        {
            using var algorithm = new BrisqueAlgo();
            using var bitmap = new SKBitmap(1000, 800);
            bitmap.Erase(SKColors.Gray);

            var result = algorithm.Compute(new(bitmap, FileExtension.UNKNOWN));
            Assert.True(result >= 0, "Brisque score should be valid for large images.");
        }
    }
}
