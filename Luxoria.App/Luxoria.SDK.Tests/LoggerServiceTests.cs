using Xunit;
using Moq;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using System;
using System.Diagnostics;

namespace Luxoria.SDK.Tests
{
    public class LoggerServiceTests
    {
        private readonly LoggerService _loggerService;

        public LoggerServiceTests()
        {
            // Centralized Setup
            _loggerService = new LoggerService();
        }

        [Fact]
        public void Log_WithMessageAndDefaultParameters_ShouldLogInfoLevel()
        {
            // Arrange
            string message = "Test message";
            string expectedCategory = "General";
            LogLevel expectedLevel = LogLevel.Info;

            using (var listener = new TestDebugListener())
            {
                Trace.Listeners.Add(listener);

                // Act
                _loggerService.Log(message);

                // Assert
                AssertLoggedMessage(listener, message, expectedCategory, expectedLevel);
            }
        }

        [Fact]
        public void Log_WithCustomCategory_ShouldLogWithSpecifiedCategory()
        {
            // Arrange
            string message = "Test message";
            string customCategory = "CustomCategory";
            LogLevel expectedLevel = LogLevel.Info;

            using (var listener = new TestDebugListener())
            {
                Trace.Listeners.Add(listener);

                // Act
                _loggerService.Log(message, customCategory);

                // Assert
                AssertLoggedMessage(listener, message, customCategory, expectedLevel);
            }
        }

        [Fact]
        public void Log_WithCustomLogLevel_ShouldLogWithSpecifiedLogLevel()
        {
            // Arrange
            string message = "An error occurred";
            string category = "General";
            LogLevel customLevel = LogLevel.Error;

            using (var listener = new TestDebugListener())
            {
                Trace.Listeners.Add(listener);

                // Act
                _loggerService.Log(message, category, customLevel);

                // Assert
                AssertLoggedMessage(listener, message, category, customLevel);
            }
        }

        [Fact]
        public void Log_WithAllCustomParameters_ShouldLogCorrectly()
        {
            // Arrange
            string message = "Warning: potential issue detected";
            string category = "System";
            LogLevel customLevel = LogLevel.Warning;

            using (var listener = new TestDebugListener())
            {
                Trace.Listeners.Add(listener);

                // Act
                _loggerService.Log(message, category, customLevel);

                // Assert
                AssertLoggedMessage(listener, message, category, customLevel);
            }
        }

        private static void AssertLoggedMessage(TestDebugListener listener, string expectedMessage, string expectedCategory, LogLevel expectedLevel)
        {
            var logEntry = listener.LoggedMessages.Find(entry =>
                entry.Contains($"[{expectedLevel}] {expectedCategory}: {expectedMessage}"));

            Assert.NotNull(logEntry);
        }
    }

    public class TestDebugListener : TraceListener
    {
        public List<string> LoggedMessages { get; } = new List<string>();

        public override void Write(string message)
        {
            LoggedMessages.Add(message);
        }

        public override void WriteLine(string message)
        {
            LoggedMessages.Add(message);
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(this);
        }
    }
}
