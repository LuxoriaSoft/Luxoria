using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ModuleContextTests
    {
        private readonly ModuleContext _moduleContext;

        public ModuleContextTests()
        {
            _moduleContext = new ModuleContext();
        }

        [Fact]
        public void GetCurrentImage_ShouldReturnCurrentImage()
        {
            // Arrange
            var pixelData = new byte[] { 0, 255, 127 };
            var image = new ImageData(pixelData, 100, 200, "PNG");
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
            var newImage = new ImageData(pixelData, 300, 400, "JPEG");

            // Act
            _moduleContext.UpdateImage(newImage);
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Equal(newImage, result);
        }

        [Fact]
        public void LogMessage_ShouldBeCallable()
        {
            // Arrange
            var logMessage = "Test log message";

            // Act
            _moduleContext.LogMessage(logMessage);

            // Assert
            // Since LogMessage has no implementation, we're only verifying that it executes without exceptions
            Assert.True(true);
        }
    }
}