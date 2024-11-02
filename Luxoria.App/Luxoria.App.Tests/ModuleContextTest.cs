using Luxoria.Modules;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.Core.Interfaces;
using Luxoria.Modules.Interfaces;
using Moq;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ModuleContextTests
    {
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly ModuleContext _moduleContext;

        public ModuleContextTests()
        {
            _mockEventBus = new Mock<IEventBus>();
            _moduleContext = new ModuleContext(_mockEventBus.Object);
        }

        [Fact]
        public void GetCurrentImage_ShouldReturnCurrentImage()
        {
            // Arrange
            var pixelData = new byte[] { 0x00, 0x01, 0x02 };
            var imageData = new ImageData(pixelData, 1920, 1080, "JPEG");

            // Act
            _moduleContext.UpdateImage(imageData);
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Equal(imageData, result);
        }

        [Fact]
        public void UpdateImage_ShouldSetCurrentImage()
        {
            // Arrange
            var pixelData = new byte[] { 0x00, 0x01, 0x02 };
            var imageData = new ImageData(pixelData, 1920, 1080, "JPEG");

            // Act
            _moduleContext.UpdateImage(imageData);

            // Assert
            Assert.Equal(imageData, _moduleContext.GetCurrentImage());
            _mockEventBus.Verify(bus => bus.Publish(It.IsAny<LogEvent>()), Times.Never);
        }

        [Fact]
        public void LogMessage_ShouldLogMessage()
        {
            // Arrange
            var message = "Test message";

            // Act
            _moduleContext.LogMessage(message);

            // Assert
            _mockEventBus.Verify(bus => bus.Publish(It.Is<LogEvent>(e => e.Message == message)), Times.Once);
        }

        [Fact]
        public void UpdateImage_ShouldNotLogMessage()
        {
            // Arrange
            var pixelData = new byte[] { 0x00, 0x01, 0x02 };
            var imageData = new ImageData(pixelData, 1920, 1080, "JPEG");

            // Act
            _moduleContext.UpdateImage(imageData);

            // Assert
            _mockEventBus.Verify(bus => bus.Publish(It.IsAny<LogEvent>()), Times.Never);
        }

        [Fact]
        public void GetCurrentImage_ShouldReturnNull_WhenNoImageIsSet()
        {
            // Act
            var result = _moduleContext.GetCurrentImage();

            // Assert
            Assert.Null(result);
        }
        
    }
}