using LuxImport.Repositories;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="ManifestRepository"/> class.
    /// </summary>
    public class ManifestRepositoryTests : IDisposable
    {
        private readonly string _testCollectionPath;
        private readonly string _luxDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestRepositoryTests"/> class.
        /// Sets up a temporary test collection directory and the .lux directory.
        /// </summary>
        public ManifestRepositoryTests()
        {
            _testCollectionPath = Path.Combine(Path.GetTempPath(), "LuxImportManifestTest");
            _luxDirectory = Path.Combine(_testCollectionPath, ".lux");

            Directory.CreateDirectory(_testCollectionPath);
            Directory.CreateDirectory(_luxDirectory);
        }

        /// <summary>
        /// Tests whether <see cref="ManifestRepository.CreateManifest"/> correctly returns a valid manifest.
        /// </summary>
        [Fact]
        public void CreateManifest_ShouldReturnValidManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            Assert.NotNull(manifest);
            Assert.Equal("TestCollection", manifest.Name);
        }

        /// <summary>
        /// Tests whether <see cref="ManifestRepository.SaveManifest"/> correctly persists the manifest file.
        /// </summary>
        [Fact]
        public void SaveManifest_ShouldPersistManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            repo.SaveManifest(manifest);
            Assert.True(File.Exists(Path.Combine(_luxDirectory, "manifest.json")));
        }

        /// <summary>
        /// Tests whether <see cref="ManifestRepository.ReadManifest"/> correctly loads the saved manifest.
        /// </summary>
        [Fact]
        public void ReadManifest_ShouldReturnCorrectManifest()
        {
            var repo = new ManifestRepository("TestCollection", _testCollectionPath);
            var manifest = repo.CreateManifest();
            repo.SaveManifest(manifest);
            var loadedManifest = repo.ReadManifest();
            Assert.Equal(manifest.Name, loadedManifest.Name);
        }

        /// <summary>
        /// Cleans up the test environment by deleting the temporary collection directory.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_testCollectionPath))
            {
                Directory.Delete(_testCollectionPath, true);
            }
        }
    }
}
