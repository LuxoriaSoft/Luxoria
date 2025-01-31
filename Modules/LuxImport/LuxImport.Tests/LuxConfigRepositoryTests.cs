using LuxImport.Repositories;
using Luxoria.Modules.Models;

namespace LuxImport.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="LuxConfigRepository"/> class.
    /// </summary>
    public class LuxConfigRepositoryTests : IDisposable
    {
        private readonly string _testCollectionPath;
        private readonly string _testAssetsPath;
        private readonly LuxConfigRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuxConfigRepositoryTests"/> class.
        /// Creates a temporary test directory for storing LuxCfg files.
        /// </summary>
        public LuxConfigRepositoryTests()
        {
            _testCollectionPath = Path.Combine(Path.GetTempPath(), "LuxImportTestAssets");
            _testAssetsPath = Path.Combine(_testCollectionPath, ".lux/assets");

            Directory.CreateDirectory(_testCollectionPath);
            Directory.CreateDirectory(_testAssetsPath);

            _repository = new LuxConfigRepository { CollectionPath = _testCollectionPath };
        }

        /// <summary>
        /// Tests whether <see cref="LuxConfigRepository.Save"/> correctly saves a LuxCfg model to a file.
        /// </summary>
        [Fact]
        public void Save_ShouldPersistLuxCfgModel()
        {
            var model = new LuxCfg("1.0", new Guid(), "TestConfig", "testconfig.jpg", "Hello World", FileExtension.PNG);

            _repository.Save(model);
            string filePath = Path.Combine(_testAssetsPath, $"{model.Id}.luxcfg.json");

            Assert.True(File.Exists(filePath), "Saved file should exist");
            string fileContent = File.ReadAllText(filePath);
            Assert.Contains("\"TestConfig\"", fileContent);
        }

        /// <summary>
        /// Tests whether <see cref="LuxConfigRepository.Load"/> correctly loads a LuxCfg model from a file.
        /// </summary>
        [Fact]
        public void Load_ShouldReturnCorrectLuxCfgModel()
        {
            var model = new LuxCfg("1.0", new Guid(), "TestConfig", "testconfig.jpg", "Hello World", FileExtension.PNG);

            _repository.Save(model);
            var loadedModel = _repository.Load(model.Id);

            Assert.NotNull(loadedModel);
            Assert.Equal(model.Id, loadedModel.Id);
            Assert.Equal(model.Name, loadedModel.Name);
        }

        /// <summary>
        /// Tests whether <see cref="LuxConfigRepository.Load"/> throws an exception when the file does not exist.
        /// </summary>
        [Fact]
        public void Load_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            Assert.Throws<FileNotFoundException>(() => _repository.Load(Guid.NewGuid()));
        }

        /// <summary>
        /// Tests whether <see cref="LuxConfigRepository.Load"/> throws an exception when the assets directory does not exist.
        /// </summary>
        [Fact]
        public void Load_ShouldThrowDirectoryNotFoundException_WhenAssetsDirectoryIsMissing()
        {
            Directory.Delete(_testAssetsPath, true);
            Assert.Throws<DirectoryNotFoundException>(() => _repository.Load(Guid.NewGuid()));
        }

        /// <summary>
        /// Cleans up test files and directories after execution.
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
