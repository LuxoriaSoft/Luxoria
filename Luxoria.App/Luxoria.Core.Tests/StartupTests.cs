using Luxoria.Core.Interfaces;
using Luxoria.Core.Logics;
using Luxoria.Core.Services;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Luxoria.Tests
{
    public class StartupTests
    {
        [Fact]
        public void ConfigureServices_RegistersRequiredServices()
        {
            // Arrange
            var hostBuilderContext = new HostBuilderContext(new Dictionary<object, object>());
            var serviceCollection = new ServiceCollection();

            // Mock ILoggerService
            var mockLogger = new Mock<ILoggerService>();
            mockLogger.Setup(x => x.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LogLevel>()));
            serviceCollection.AddSingleton(mockLogger.Object);

            var startup = new Startup();

            // Act
            startup.ConfigureServices(hostBuilderContext, serviceCollection);

            // Assert - Verify services
            AssertServiceRegistered<IEventBus>(serviceCollection, typeof(EventBus));
            AssertServiceRegistered<IModuleService>(serviceCollection, typeof(ModuleService));
            AssertSingletonServiceRegistered<ILoggerService>(serviceCollection, mockLogger.Object);
        }

        private void AssertServiceRegistered<TService>(IServiceCollection services, Type expectedImplementationType)
        {
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TService));
            Assert.NotNull(serviceDescriptor); // Service is registered
            Assert.Equal(expectedImplementationType, serviceDescriptor.ImplementationType); // Correct implementation
        }

        private void AssertSingletonServiceRegistered<TService>(IServiceCollection services, object expectedInstance)
        {
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TService));
            Assert.NotNull(serviceDescriptor); // Service is registered
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime); // Service is Singleton
            Assert.Same(expectedInstance, serviceDescriptor.ImplementationInstance); // Correct instance
        }
    }
}
