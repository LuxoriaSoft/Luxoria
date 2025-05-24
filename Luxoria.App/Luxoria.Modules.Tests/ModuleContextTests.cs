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
            using var bitmap = new SkiaSharp.SKBitmap(100, 200);
            var image = new ImageData(bitmap, FileExtension.JPEG);
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
            using var bitmap = new SkiaSharp.SKBitmap(300, 400);
            var newImage = new ImageData(bitmap, FileExtension.JPEG);

            // Act
            _moduleContext.UpdateImage(newImage);
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Equal(newImage, result);
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
    }
}
