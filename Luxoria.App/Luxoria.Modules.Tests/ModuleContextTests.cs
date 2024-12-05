using Luxoria.Modules;
using Luxoria.Modules.Models;

namespace Luxoria.App.Tests
{
    public class ModuleContextTests
    {
        private readonly ModuleContext _moduleContext;

        public ModuleContextTests()
        {
            // Centralized Setup
            _moduleContext = new ModuleContext();
        }

        [Fact]
        public void GetCurrentImage_ShouldReturnCurrentImage()
        {
            // Arrange
            var pixelData = new byte[] { 0, 255, 127 };
            var image = new ImageData(pixelData, 100, 200, FileExtension.JPEG);
            _moduleContext.UpdateImage(image);

            // Act
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Equal(image, result);
        }

        [Fact]
        public void UpdateImage_ShouldSetCurrentImage()
        {
            // Arrange
            var pixelData = new byte[] { 1, 2, 3, 4 };
            var newImage = new ImageData(pixelData, 300, 400, FileExtension.JPEG);

            // Act
            _moduleContext.UpdateImage(newImage);
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Equal(newImage, result);
        }

        [Fact]
        public void UpdateImage_WithNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            ImageData nullImage = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _moduleContext.UpdateImage(nullImage));
        }

        [Fact]
        public void LogMessage_WithValidMessage_ShouldNotThrow()
        {
            // Arrange
            var logMessage = "Test log message";

            // Act & Assert
            var exception = Record.Exception(() => _moduleContext.LogMessage(logMessage));
            Assert.Null(exception); // Ensure that no exception is thrown
        }

        [Fact]
        public void LogMessage_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Arrange
            string logMessage = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _moduleContext.LogMessage(logMessage));
        }
    }
}
