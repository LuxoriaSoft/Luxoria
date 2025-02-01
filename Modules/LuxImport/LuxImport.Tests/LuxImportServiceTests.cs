using LuxImport.Interfaces;
using LuxImport.Services;
using Luxoria.SDK.Interfaces;
using Moq;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="ImportService"/> class.
    /// </summary>
    public class LuxImportServiceTests : IDisposable
    {
        private readonly string _testCollectionPath;
        private readonly Mock<IManifestRepository> _mockManifestRepository;
        private readonly Mock<ILuxConfigRepository> _mockLuxConfigRepository;
        private readonly Mock<IFileHasherService> _mockFileHasherService;
        private readonly ImportService _importService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuxImportServiceTests"/> class.
        /// Creates a temporary test collection directory and mocks dependencies.
        /// </summary>
        public LuxImportServiceTests()
        {
            _testCollectionPath = Path.Combine(Path.GetTempPath(), "LuxImportTestCollection");
            Directory.CreateDirectory(_testCollectionPath);

            _mockManifestRepository = new Mock<IManifestRepository>();
            _mockLuxConfigRepository = new Mock<ILuxConfigRepository>();
            _mockFileHasherService = new Mock<IFileHasherService>();

            _importService = new ImportService("TestCollection", _testCollectionPath);
        }

        /// <summary>
        /// Tests whether <see cref="ImportService.IsInitialized"/> returns false for an uninitialized collection.
        /// </summary>
        [Fact]
        public void IsInitialized_ShouldReturnFalseForUninitializedCollection()
        {
            Assert.False(_importService.IsInitialized());
        }

        /// <summary>
        /// Tests whether calling <see cref="ImportService.InitializeDatabase"/> correctly initializes the collection.
        /// </summary>
        [Fact]
        public void InitializeDatabase_ShouldCreateManifest()
        {
            _importService.InitializeDatabase();
            Assert.True(_importService.IsInitialized());
        }

        /// <summary>
        /// Tests whether <see cref="ImportService.IndexCollectionAsync"/> throws an exception when the collection is not initialized.
        /// </summary>
        [Fact]
        public async Task IndexCollectionAsync_ShouldThrowForUninitializedCollection()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _importService.IndexCollectionAsync());
        }

        /// <summary>
        /// Tests whether <see cref="ImportService.IndexCollectionAsync"/> processes an empty collection correctly.
        /// </summary>
        [Fact]
        public async Task IndexCollectionAsync_ShouldHandleEmptyCollection()
        {
            _importService.InitializeDatabase();
            await _importService.IndexCollectionAsync();
        }

        /// <summary>
        /// Tests whether <see cref="ImportService.IndexCollectionAsync"/> processes a collection with files.
        /// </summary>
        [Fact]
        public async Task IndexCollectionAsync_ShouldProcessCollectionWithFiles()
        {
            _importService.InitializeDatabase();

            string testFile = Path.Combine(_testCollectionPath, "test_image.png");
            File.WriteAllBytes(testFile, new byte[] { 0xFF, 0xD8, 0xFF });

            await _importService.IndexCollectionAsync();
        }

        /// <summary>
        /// Tests whether <see cref="ImportService.LoadAssets"/> returns assets when initialized properly.
        /// </summary>
        [Fact]
        public void LoadAssets_ShouldReturnAssets()
        {
            _importService.InitializeDatabase();
            var assets = _importService.LoadAssets();
            Assert.NotNull(assets);
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
