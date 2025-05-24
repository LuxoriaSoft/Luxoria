using Luxoria.Modules.Models;
using SkiaSharp;

namespace Luxoria.App.Tests
{
    public class ImageDataTests
    {
        [Fact]
        public void ImageData_WithValidParameters_ShouldInitializeProperties()
        {
            // Arrange
            using var bitmap = new SKBitmap(1920, 1080);
            var format = FileExtension.JPEG;

            // Act
            var imageData = new ImageData(bitmap, format);

            // Assert
            Assert.Equal(bitmap, imageData.Bitmap);
            Assert.Equal(1920, imageData.Width);
            Assert.Equal(1080, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }

        [Fact]
        public void ImageData_WithNullBitmap_ShouldThrowArgumentNullException()
        {
            // Arrange
            SKBitmap? bitmap = null;
            var format = FileExtension.JPEG;

            // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
            var exception = Assert.Throws<ArgumentNullException>(() => new ImageData(bitmap, format));
#pragma warning restore CS8604 // Possible null reference argument.

            // Verify the exception message and parameter
            Assert.Equal("Value cannot be null. (Parameter 'bitmap')", exception.Message);
        }

        [Fact]
        public void ImageData_WithZeroWidthBitmap_ShouldInitializeCorrectly()
        {
            // Arrange
            using var bitmap = new SKBitmap(0, 1080); // Unusual case but valid SKBitmap
            var format = FileExtension.PNG;

            // Act
            var imageData = new ImageData(bitmap, format);

            // Assert
            Assert.Equal(0, imageData.Width);
            Assert.Equal(1080, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }

        [Fact]
        public void ImageData_WithZeroHeightBitmap_ShouldInitializeCorrectly()
        {
            // Arrange
            using var bitmap = new SKBitmap(1920, 0); // Unusual case but valid SKBitmap
            var format = FileExtension.PNG;

            // Act
            var imageData = new ImageData(bitmap, format);

            // Assert
            Assert.Equal(1920, imageData.Width);
            Assert.Equal(0, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }

        [Fact]
        public void ImageData_ToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            using var bitmap = new SKBitmap(1280, 720);
            var format = FileExtension.JPEG;

            // Act
            var imageData = new ImageData(bitmap, format);
            var result = imageData.ToString();

            // Assert
            Assert.Equal("JPEG Image: 1280x720, EXIF Entries: 0", result);
        }
    }
}
