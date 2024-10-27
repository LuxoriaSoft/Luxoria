using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Moq;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Luxoria.App.Tests
{
    public class ModuleLoaderTests
    {
        private ModuleLoader _moduleLoader;
        private readonly Mock<Func<string, bool>> _fileExistsMock;
        private readonly Mock<Func<string, Assembly>> _assemblyLoadFromMock;
        private readonly string _modulePath;

        public ModuleLoaderTests()
        {
            _fileExistsMock = new Mock<Func<string, bool>>();
            _assemblyLoadFromMock = new Mock<Func<string, Assembly>>();
            _modulePath = "fake/path/to/module.dll";
        }

        [Fact]
        public void Constructor_ShouldUseDefaults_WhenNoParametersProvided()
        {
            // Act
            _moduleLoader = new ModuleLoader();

            // Assert
            Assert.NotNull(_moduleLoader);
        }

        [Fact]
        public void LoadModule_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(false);
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal($"Module not found: [{_modulePath}]", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenNoValidModuleFound()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(Array.Empty<Type>());
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("No valid module found in assembly.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldIgnoreAbstractTypes()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(AbstractModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("No valid module found in assembly.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldIgnoreNonIModuleTypes()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(string) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("No valid module found in assembly.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldContinue_WhenInstanceIsNotIModule()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);
            var mockAssembly = new Mock<Assembly>();

            // Simulate a type that is assignable to IModule but returns an object that is not an IModule
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            // Mock createInstance to return an object that's not an IModule
            Func<Type, object?> createInstanceMock = type => new object();
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object, createInstanceMock);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("No valid module found in assembly.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenActivatorReturnsNull()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(NullReturningModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            Func<Type, object?> createInstanceMock = type =>
                type == typeof(NullReturningModule) ? null : Activator.CreateInstance(type);
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object, createInstanceMock);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Contains("Failed to create instance of module type:", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenConstructorThrowsException()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(FailingConstructorModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Contains("Constructor failed", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenMissingMethodExceptionOccurs()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(NoDefaultConstructorModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Contains("No parameterless constructor found.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldReturnFirstValidModule_WhenAssemblyHasMixedTypes()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[]
                { typeof(string), typeof(AbstractModule), typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act
            var result = _moduleLoader.LoadModule(_modulePath);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestModule>(result);

            // Access properties to ensure full coverage
            Assert.NotNull(result.Name);
            Assert.NotNull(result.Description);
            Assert.NotNull(result.Version);
        }

        [Fact]
        public void LoadModule_ShouldUseDefaultActivator_WhenCreateInstanceIsNotProvided()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            // Act
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);
            var result = _moduleLoader.LoadModule(_modulePath);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestModule>(result);
        }

        [Fact]
        public void LoadModule_ShouldSkipInvalidTypes_AndLoadFirstValidModule()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[]
                { typeof(string), typeof(NonModuleClass), typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act
            var result = _moduleLoader.LoadModule(_modulePath);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestModule>(result);
        }

        [Fact]
        public void LoadModule_ShouldUseCustomFileExistsFunction()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(_modulePath)).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(_modulePath)).Returns(mockAssembly.Object);

            // Act
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);
            var result = _moduleLoader.LoadModule(_modulePath);

            // Assert
            Assert.NotNull(result);
            _fileExistsMock.Verify(f => f(_modulePath), Times.Once);
        }

        [Fact]
        public void LoadModule_ShouldUseCustomLoadAssemblyFunction()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(_modulePath)).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(_modulePath)).Returns(mockAssembly.Object);

            // Act
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);
            var result = _moduleLoader.LoadModule(_modulePath);

            // Assert
            Assert.NotNull(result);
            _assemblyLoadFromMock.Verify(a => a(_modulePath), Times.Once);
        }

        [Fact]
        public void LoadModule_ShouldPropagateUnexpectedExceptions()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { typeof(TestModule) });
            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>())).Returns(mockAssembly.Object);

            Func<Type, object?> createInstanceMock =
                type => throw new InvalidOperationException("Unexpected exception");
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object, createInstanceMock);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("Unexpected exception", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldPropagateException_WhenAssemblyFailsToLoad()
        {
            // Arrange
            _fileExistsMock.Setup(f => f(It.IsAny<string>())).Returns(true);

            _assemblyLoadFromMock.Setup(a => a(It.IsAny<string>()))
                .Throws(new BadImageFormatException("Invalid assembly"));
            _moduleLoader = new ModuleLoader(_fileExistsMock.Object, _assemblyLoadFromMock.Object);

            // Act & Assert
            var exception = Assert.Throws<BadImageFormatException>(() => _moduleLoader.LoadModule(_modulePath));
            Assert.Equal("Invalid assembly", exception.Message);
        }

        // Helper classes

        public class TestModule : IModule
        {
            public void Initialize()
            {
                // No implementation needed
            }

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
                // No implementation needed
            }

            public void Execute()
            {
                // No implementation needed
            }

            public void Shutdown()
            {
                // No implementation needed
            }

            public string Name { get; } = "Test Module";
            public string Description { get; } = "A test module.";
            public string Version { get; } = "1.0.0";
        }

        public abstract class AbstractModule : IModule
        {
            public void Initialize()
            {
                // No implementation needed
            }

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
                // No implementation needed
            }

            public void Execute()
            {
                // No implementation needed
            }

            public void Shutdown()
            {
                // No implementation needed
            }

            public string Name { get; } = "Abstract Module";
            public string Description { get; } = "An abstract module.";
            public string Version { get; } = "1.0.0";
        }

        public class NullReturningModule : IModule
        {
            public void Initialize()
            {
                // Simulate a null return from Activator.CreateInstance
            }

            public string Name { get; } = "Null Returning Module";
            public string Description { get; } = "Simulates a null return from Activator.CreateInstance";
            public string Version { get; } = "1.0.0";

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
            }

            public void Execute()
            {
            }

            public void Shutdown()
            {
            }
        }

        public class FailingConstructorModule : IModule
        {
            public FailingConstructorModule()
            {
                throw new InvalidOperationException("Constructor failed");
            }

            public void Initialize()
            {
                // No implementation needed
            }

            public string Name { get; } = "Failing Constructor Module";
            public string Description { get; } = "Module that throws an exception in constructor";
            public string Version { get; } = "1.0.0";

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
            }

            public void Execute()
            {
            }

            public void Shutdown()
            {
            }
        }

        public class NoDefaultConstructorModule : IModule
        {
            public NoDefaultConstructorModule(string param)
            {
            }

            public void Initialize()
            {
                // No implementation needed
            }

            public string Name { get; } = "No Default Constructor Module";
            public string Description { get; } = "Module that has no parameterless constructor";
            public string Version { get; } = "1.0.0";

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
            }

            public void Execute()
            {
            }

            public void Shutdown()
            {
            }
        }

        public class NullInnerExceptionModule : IModule
        {
            public void Initialize()
            {
                // Simulate a TargetInvocationException with null InnerException
            }

            public string Name { get; } = "Null InnerException Module";
            public string Description { get; } = "Simulates a TargetInvocationException with null InnerException";
            public string Version { get; } = "1.0.0";

            public void Initialize(IEventBus eventBus, IModuleContext context)
            {
            }

            public void Execute()
            {
            }

            public void Shutdown()
            {
            }
        }

        public class NonModuleClass
        {
            public NonModuleClass()
            {
                // This class does not implement IModule
            }
        }

        public class TestModuleTests
        {
            [Fact]
            public void TestModule_ShouldInitializeProperties()
            {
                // Arrange & Act
                var module = new TestModule();

                // Assert
                Assert.Equal("Test Module", module.Name);
                Assert.Equal("A test module.", module.Description);
                Assert.Equal("1.0.0", module.Version);
            }

            [Fact]
            public void TestModule_ShouldExecuteMethodsWithoutExceptions()
            {
                // Arrange
                var module = new TestModule();

                // Act & Assert
                module.Initialize();
                module.Initialize(null, null);
                module.Execute();
                module.Shutdown();
                Assert.NotNull(module);
            }
        }

        [Fact]
        public void FailingConstructorModule_ShouldThrowExceptionOnInstantiation()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new FailingConstructorModule());
            Assert.Equal("Constructor failed", exception.Message);
        }

        private class ConcreteModule : AbstractModule
        {
            // No additional implementation needed
        }

        [Fact]
        public void ConcreteModule_ShouldInheritPropertiesFromAbstractModule()
        {
            // Arrange & Act
            var module = new ConcreteModule();

            // Assert
            Assert.Equal("Abstract Module", module.Name);
            Assert.Equal("An abstract module.", module.Description);
            Assert.Equal("1.0.0", module.Version);
        }

        [Fact]
        public void ConcreteModule_ShouldExecuteMethodsWithoutExceptions()
        {
            // Arrange
            var module = new ConcreteModule();

            // Act & Assert
            module.Initialize();
            module.Initialize(null, null);
            module.Execute();
            module.Shutdown();
            Assert.NotNull(module);
        }

        [Fact]
        public void NullReturningModule_ShouldInitializeProperties()
        {
            // Arrange & Act
            var module = new NullReturningModule();

            // Assert
            Assert.Equal("Null Returning Module", module.Name);
            Assert.Equal("Simulates a null return from Activator.CreateInstance", module.Description);
            Assert.Equal("1.0.0", module.Version);
        }

        [Fact]
        public void NullReturningModule_ShouldExecuteMethodsWithoutExceptions()
        {
            // Arrange
            var module = new NullReturningModule();

            // Act & Assert
            module.Initialize();
            module.Initialize(null, null);
            module.Execute();
            module.Shutdown();
            Assert.NotNull(module);
        }

        [Fact]
        public void NoDefaultConstructorModule_ShouldInitializeProperties()
        {
            // Arrange & Act
            var module = new NoDefaultConstructorModule("test parameter");

            // Assert
            Assert.Equal("No Default Constructor Module", module.Name);
            Assert.Equal("Module that has no parameterless constructor", module.Description);
            Assert.Equal("1.0.0", module.Version);
        }

        [Fact]
        public void NoDefaultConstructorModule_ShouldExecuteMethodsWithoutExceptions()
        {
            // Arrange
            var module = new NoDefaultConstructorModule("test parameter");

            // Act & Assert
            module.Initialize();
            module.Initialize(null, null);
            module.Execute();
            module.Shutdown();

            Assert.NotNull(module);
        }

        [Fact]
        public void NullInnerExceptionModule_ShouldInitializeProperties()
        {
            // Arrange & Act
            var module = new NullInnerExceptionModule();

            // Assert
            Assert.Equal("Null InnerException Module", module.Name);
            Assert.Equal("Simulates a TargetInvocationException with null InnerException", module.Description);
            Assert.Equal("1.0.0", module.Version);
        }

        [Fact]
        public void NullInnerExceptionModule_ShouldExecuteMethodsWithoutExceptions()
        {
            // Arrange
            var module = new NullInnerExceptionModule();

            // Act & Assert
            module.Initialize();
            module.Initialize(null, null);
            module.Execute();
            module.Shutdown();

            Assert.NotNull(module);
        }
    }
}
