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

    public class ManifestRepositoryTests : IDisposable
    {
        private readonly string _testCollectionPath;
        private readonly string _luxDirectory;

        public ManifestRepositoryTests()
        {
            _testCollectionPath = Path.Combine(Path.GetTempPath(), "LuxImportManifestTest");
            _luxDirectory = Path.Combine(_testCollectionPath, ".lux");

            Directory.CreateDirectory(_testCollectionPath);
            Directory.CreateDirectory(_luxDirectory);
        }

        [Fact]
        public void CreateManifest_ShouldReturnValidManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            Assert.NotNull(manifest);
            Assert.Equal("TestCollection", manifest.Name);
        }

        [Fact]
        public void SaveManifest_ShouldPersistManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            repo.SaveManifest(manifest);
            Assert.True(File.Exists(Path.Combine(_luxDirectory, "manifest.json")));
        }

        [Fact]
        public void ReadManifest_ShouldReturnCorrectManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            repo.SaveManifest(manifest);
            var loadedManifest = repo.ReadManifest();
            Assert.Equal(manifest.Name, loadedManifest.Name);
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
