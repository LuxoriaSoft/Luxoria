using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Moq;
using Xunit;

namespace Luxoria.App.Tests
{
    public class EventBusTests
    {
        private readonly EventBus _eventBus;

        public EventBusTests()
        {
            _eventBus = new EventBus();
        }

        [Fact]
        public void Subscribe_ShouldAddSubscriber()
        {
            // Arrange
            var mockHandler = new Mock<Action<LogEvent>>();
            _eventBus.Subscribe(mockHandler.Object);

            // Act
            _eventBus.Publish(new LogEvent("Test message"));

            // Assert
            mockHandler.Verify(handler => handler(It.IsAny<LogEvent>()), Times.Once);
        }

        [Fact]
        public void Unsubscribe_ShouldRemoveSubscriber()
        {
            // Arrange
            var mockHandler = new Mock<Action<LogEvent>>();
            _eventBus.Subscribe(mockHandler.Object);
            _eventBus.Unsubscribe(mockHandler.Object);

            // Act
            _eventBus.Publish(new LogEvent("Test message"));

            // Assert
            mockHandler.Verify(handler => handler(It.IsAny<LogEvent>()), Times.Never);
        }

        [Fact]
        public void Publish_ShouldNotifyAllSubscribers()
        {
            // Arrange
            var mockHandler1 = new Mock<Action<LogEvent>>();
            var mockHandler2 = new Mock<Action<LogEvent>>();
            _eventBus.Subscribe(mockHandler1.Object);
            _eventBus.Subscribe(mockHandler2.Object);

            // Act
            _eventBus.Publish(new LogEvent("Test message"));

            // Assert
            mockHandler1.Verify(handler => handler(It.IsAny<LogEvent>()), Times.Once);
            mockHandler2.Verify(handler => handler(It.IsAny<LogEvent>()), Times.Once);
        }
        
        [Fact]
        public void Constructor_ShouldSetMessageProperty()
        {
            // Arrange
            var message = "Test message";

            // Act
            var logEvent = new LogEvent(message);

            // Assert
            Assert.Equal(message, logEvent.Message);
        }
    }
}