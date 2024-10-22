using Luxoria.Modules.Models.Events;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ImageUpdatedEventTests
    {
        [Fact]
        public void Constructor_ShouldSetImagePath()
        {
            // Arrange
            var imagePath = "path/to/image.jpg";

            // Act
            var imageUpdatedEvent = new ImageUpdatedEvent(imagePath);

            // Assert
            Assert.Equal(imagePath, imageUpdatedEvent.ImagePath);
        }
    }
}