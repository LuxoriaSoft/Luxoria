﻿using Luxoria.Core.Interfaces;
using Luxoria.Core.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Moq;
using Xunit;

namespace Luxoria.Core.Tests
{
    public class ModuleServiceTests
    {
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly ModuleService _moduleService;
        private readonly Mock<ILoggerService> _mockLogger;

        public ModuleServiceTests()
        {
            _mockEventBus = new Mock<IEventBus>();
            _mockLogger = new Mock<ILoggerService>();
            _moduleService = new ModuleService(_mockEventBus.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullEventBus_ShouldThrowArgumentNullException()
        {
            // Arrange
            IEventBus nullEventBus = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ModuleService(nullEventBus, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange
            ILoggerService nullLogger = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ModuleService(_mockEventBus.Object, nullLogger));
        }

        [Fact]
        public void AddModule_WithValidModule_ShouldAddModuleToList()
        {
            // Arrange
            var mockModule = new Mock<IModule>();

            // Act
            _moduleService.AddModule(mockModule.Object);

            // Assert
            Assert.Contains(mockModule.Object, _moduleService.GetModules());
        }

        [Fact]
        public void AddModule_NullModule_ShouldThrowArgumentNullException()
        {
            // Arrange
            IModule nullModule = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _moduleService.AddModule(nullModule));
        }

        [Fact]
        public void RemoveModule_WithExistingModule_ShouldRemoveModuleFromList()
        {
            // Arrange
            var mockModule = new Mock<IModule>();
            _moduleService.AddModule(mockModule.Object);

            // Act
            _moduleService.RemoveModule(mockModule.Object);

            // Assert
            Assert.DoesNotContain(mockModule.Object, _moduleService.GetModules());
        }

        [Fact]
        public void RemoveModule_WithNonexistentModule_ShouldNotThrow()
        {
            // Arrange
            var mockModule = new Mock<IModule>();

            // Act & Assert
            _moduleService.RemoveModule(mockModule.Object); // Should not throw
            Assert.NotNull(mockModule);
        }

        [Fact]
        public void RemoveModule_NullModule_ShouldThrowArgumentNullException()
        {
            // Arrange
            IModule nullModule = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _moduleService.RemoveModule(nullModule));
        }

        [Fact]
        public void GetModules_WhenModulesExist_ShouldReturnListOfModules()
        {
            // Arrange
            var mockModule = new Mock<IModule>();
            _moduleService.AddModule(mockModule.Object);

            // Act
            var modules = _moduleService.GetModules();

            // Assert
            Assert.Contains(mockModule.Object, modules);
        }

        [Fact]
        public void GetModules_WhenNoModulesExist_ShouldReturnEmptyList()
        {
            // Act
            var modules = _moduleService.GetModules();

            // Assert
            Assert.Empty(modules);
        }

        [Fact]
        public void InitializeModules_WithValidModules_ShouldInitializeAllModules()
        {
            // Arrange
            var mockModule = new Mock<IModule>();
            var mockContext = new Mock<IModuleContext>();
            _moduleService.AddModule(mockModule.Object);

            // Act
            _moduleService.InitializeModules(mockContext.Object);

            // Assert
            mockModule.Verify(m => m.Initialize(_mockEventBus.Object, mockContext.Object, _mockLogger.Object), Times.Once);
        }

        [Fact]
        public void InitializeModules_WithNoModules_ShouldNotThrow()
        {
            // Arrange
            var mockContext = new Mock<IModuleContext>();

            // Act & Assert
            _moduleService.InitializeModules(mockContext.Object); // Should not throw
            Assert.NotNull(mockContext);
        }

        [Fact]
        public void InitializeModules_NullContext_ShouldThrowArgumentNullException()
        {
            // Arrange
            IModuleContext nullContext = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _moduleService.InitializeModules(nullContext));
        }
    }
}
