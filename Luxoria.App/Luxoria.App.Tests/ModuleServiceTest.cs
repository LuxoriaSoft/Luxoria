using Luxoria.Core.Interfaces;
using Luxoria.Core.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Services;
using Moq;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ModuleServiceTests
    {
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly ModuleService _moduleService;
        private readonly ILoggerService _logger;

        public ModuleServiceTests()
        {
            _mockEventBus = new Mock<IEventBus>();
            _logger = new LoggerService();
            _moduleService = new ModuleService(_mockEventBus.Object, _logger);
        }

        [Fact]
        public void AddModule_ShouldAddModuleToList()
        {
            // Arrange
            var mockModule = new Mock<IModule>();

            // Act
            _moduleService.AddModule(mockModule.Object);

            // Assert
            Assert.Contains(mockModule.Object, _moduleService.GetModules());
        }

        [Fact]
        public void RemoveModule_ShouldRemoveModuleFromList()
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
        public void GetModules_ShouldReturnListOfModules()
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
        public void InitializeModules_ShouldInitializeAllModules()
        {
            // Arrange
            var mockModule = new Mock<IModule>();
            var mockContext = new Mock<IModuleContext>();
            _moduleService.AddModule(mockModule.Object);

            // Act
            _moduleService.InitializeModules(mockContext.Object);

            // Assert
            mockModule.Verify(m => m.Initialize(_mockEventBus.Object, mockContext.Object, _logger), Times.Once);
        }
    }
}