using Luxoria.Modules.Models.Events;
using Xunit;

namespace Luxoria.App.Tests
{
    public class TextInputEventTests
    {
        [Fact]
        public void Constructor_ShouldSetText()
        {
            // Arrange
            var text = "Sample text";

            // Act
            var textInputEvent = new TextInputEvent(text);

            // Assert
            Assert.Equal(text, textInputEvent.Text);
        }
    }
}