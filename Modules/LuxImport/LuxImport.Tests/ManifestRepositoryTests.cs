using LuxImport.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxImport.Tests
{
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
