using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using LuxImport.Services;
using LuxImport.Repositories;

namespace LuxImport.Tests
{
    public class LuxImportServiceTests : IDisposable
    {
        private readonly string _testCollectionPath;

        public LuxImportServiceTests()
        {
            _testCollectionPath = Path.Combine(Path.GetTempPath(), "LuxImportTestCollection");
            Directory.CreateDirectory(_testCollectionPath);
        }

        [Fact]
        public void IsInitialized_ShouldReturnFalseForUninitializedCollection()
        {
            var service = new ImportService("TestCollection", _testCollectionPath);
            Assert.False(service.IsInitialized());
        }

        [Fact]
        public void InitializeDatabase_ShouldCreateManifest()
        {
            var service = new ImportService("TestCollection", _testCollectionPath);
            service.InitializeDatabase();
            Assert.True(service.IsInitialized());
        }

        [Fact]
        public async Task IndexCollectionAsync_ShouldThrowForUninitializedCollection()
        {
            var service = new ImportService("TestCollection", _testCollectionPath);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.IndexCollectionAsync());
        }

        public void Dispose()
        {
            if (Directory.Exists(_testCollectionPath))
            {
                Directory.Delete(_testCollectionPath, true);
            }
        }
    }
}
