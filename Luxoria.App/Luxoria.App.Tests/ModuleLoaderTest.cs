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
        private readonly ModuleLoader _moduleLoader;

        public ModuleLoaderTests()
        {
            _moduleLoader = new ModuleLoader();
        }

        [Fact]
        public void LoadModule_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var path = "nonexistent.dll";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => _moduleLoader.LoadModule(path));
            Assert.Equal("Module not found : [nonexistent.dll]", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenNoValidModuleFound()
        {
            // Arrange
            var path = "test.dll";
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { });

            // Use reflection to set the private field
            typeof(Assembly).GetField("assembly", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(null, mockAssembly.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(path));
            Assert.Equal("No valid module found in assembly.", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldThrowInvalidOperationException_WhenModuleInstanceCreationFails()
        {
            // Arrange
            var path = "test.dll";
            var mockType = new Mock<Type>();
            mockType.Setup(t => t.IsAssignableFrom(It.IsAny<Type>())).Returns(true);
            mockType.Setup(t => t.IsAbstract).Returns(false);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { mockType.Object });

            // Use reflection to set the private field
            typeof(Assembly).GetField("assembly", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(null, mockAssembly.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _moduleLoader.LoadModule(path));
            Assert.Contains("Failed to create instance of module type:", exception.Message);
        }

        [Fact]
        public void LoadModule_ShouldReturnModule_WhenModuleIsFound()
        {
            // Arrange
            var path = "test.dll";
            var mockType = new Mock<Type>();
            mockType.Setup(t => t.IsAssignableFrom(It.IsAny<Type>())).Returns(true);
            mockType.Setup(t => t.IsAbstract).Returns(false);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.GetTypes()).Returns(new Type[] { mockType.Object });

            // Use reflection to set the private field
            typeof(Assembly).GetField("assembly", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(null, mockAssembly.Object);

            // Act
            var result = _moduleLoader.LoadModule(path);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IModule>(result);
        }
    }
}