using Luxoria.Modules.Models;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ImageDataTests
    {
        [Fact]
        public void ImageData_ShouldInitializeProperties()
        {
            // Arrange
            var pixelData = new byte[] { 0x00, 0x01, 0x02 };
            var width = 1920;
            var height = 1080;
            var format = "JPEG";

            // Act
            var imageData = new ImageData(pixelData, width, height, format);

            // Assert
            Assert.Equal(pixelData, imageData.PixelData);
            Assert.Equal(width, imageData.Width);
            Assert.Equal(height, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }

        [Fact]
        public void ImageData_ShouldHandleEmptyPixelData()
        {
            // Arrange
            var pixelData = new byte[] { };
            var width = 1920;
            var height = 1080;
            var format = "JPEG";

            // Act
            var imageData = new ImageData(pixelData, width, height, format);

            // Assert
            Assert.Empty(imageData.PixelData);
            Assert.Equal(width, imageData.Width);
            Assert.Equal(height, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }

        [Fact]
        public void ImageData_ShouldHandleNullPixelData()
        {
            // Arrange
            byte[] pixelData = null;
            var width = 1920;
            var height = 1080;
            var format = "JPEG";

            // Act
            var imageData = new ImageData(pixelData, width, height, format);

            // Assert
            Assert.Null(imageData.PixelData);
            Assert.Equal(width, imageData.Width);
            Assert.Equal(height, imageData.Height);
            Assert.Equal(format, imageData.Format);
        }
    }
}