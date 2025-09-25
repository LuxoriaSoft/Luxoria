using Luxoria.Core.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Services;
using Luxoria.SDK.Interfaces;
using Moq;
using Newtonsoft.Json;

namespace Luxoria.Core.Tests.Services
{
    public class StorageAPITests : IDisposable
    {
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly string _testVaultsDir;
        private readonly Guid _testVaultId;
        private readonly StorageAPI _storageAPI;

        public StorageAPITests()
        {
            _mockLogger = new Mock<ILoggerService>();
            _testVaultsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _testVaultId = Guid.NewGuid();
            _storageAPI = new StorageAPI(_mockLogger.Object, _testVaultsDir, _testVaultId);

            Directory.CreateDirectory(_testVaultsDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testVaultsDir))
            {
                Directory.Delete(_testVaultsDir, true);
            }
        }

        [Fact]
        public void StandarizeInput_RemovesUnwantedCharacters()
        {
            // Arrange
            var input = "Test@#$%^&*()_+{}|:\"<>?[]\\;',./`~String";

            // Act
            var result = StorageAPI.StandarizeInput(input);

            // Assert
            Assert.Equal("TestString", result);
        }

        [Fact]
        public void Save_WithValidKeyAndValue_CreatesFile()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";

            // Act
            _storageAPI.Save(key, value);

            // Assert
            var expectedPath = Path.Combine(_testVaultsDir, _testVaultId.ToString(), key);
            Assert.True(File.Exists(expectedPath));
            Assert.Equal("testValue", File.ReadAllText(expectedPath));
        }

        [Fact]
        public void Save_WithExpiration_CreatesFileAndTtl()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            var expiration = DateTime.UtcNow.AddHours(1);

            // Act
            _storageAPI.Save(key, expiration, value);

            // Assert
            var dataPath = Path.Combine(_testVaultsDir, _testVaultId.ToString(), key);
            var ttlPath = Path.Combine(_testVaultsDir, _testVaultId.ToString(), $"{key}_cache.txt");

            Assert.True(File.Exists(dataPath));
            Assert.True(File.Exists(ttlPath));
            Assert.Equal("testValue", File.ReadAllText(dataPath));
            Assert.Equal(expiration.ToString("o"), File.ReadAllText(ttlPath));
        }

        [Fact]
        public void Save_WithComplexObject_SerializesToJson()
        {
            // Arrange
            var key = "testObject";
            var value = new { Name = "Test", Value = 42, Active = true };

            // Act
            _storageAPI.Save(key, value);

            // Assert
            var expectedPath = Path.Combine(_testVaultsDir, _testVaultId.ToString(), key);
            var content = File.ReadAllText(expectedPath);
            var deserialized = JsonConvert.DeserializeAnonymousType(content, value);

            Assert.Equal(value.Name, deserialized.Name);
            Assert.Equal(value.Value, deserialized.Value);
            Assert.Equal(value.Active, deserialized.Active);
        }

        [Fact]
        public void Save_WithEmptyKey_ThrowsArgumentException()
        {
            // Arrange
            var key = "";
            var value = "testValue";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _storageAPI.Save(key, value));
        }

        [Fact]
        public void Save_WithEmptyVaultId_ThrowsInvalidOperationException()
        {
            // Arrange
            var storageAPI = new StorageAPI(_mockLogger.Object, _testVaultsDir, Guid.Empty);
            var key = "testKey";
            var value = "testValue";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => storageAPI.Save(key, value));
        }

        [Fact]
        public void Contains_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            _storageAPI.Save(key, value);

            // Act
            var result = _storageAPI.Contains(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Contains_WithNonExistingKey_ReturnsFalse()
        {
            // Arrange
            var key = "nonExistingKey";

            // Act
            var result = _storageAPI.Contains(key);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Contains_WithValidTtl_ReturnsTrue()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";
            var expiration = DateTime.UtcNow.AddHours(1);
            _storageAPI.Save(key, expiration, value);

            // Act
            var result = _storageAPI.Contains(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Get_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            _storageAPI.Save(key, expectedValue);

            // Act
            var result = _storageAPI.Get<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Get_WithComplexObject_ReturnsDeserializedObject()
        {
            // Arrange
            var key = "testObject";
            var expectedValue = new TestData { Name = "Test", Value = 42, Active = true };
            _storageAPI.Save(key, expectedValue);

            // Act
            var result = _storageAPI.Get<TestData>(key);

            // Assert
            Assert.Equal(expectedValue.Name, result.Name);
            Assert.Equal(expectedValue.Value, result.Value);
            Assert.Equal(expectedValue.Active, result.Active);
        }

        [Fact]
        public void Get_WithNonExistingKey_ThrowsFileNotFoundException()
        {
            // Arrange
            var key = "nonExistingKey";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _storageAPI.Get<string>(key));
        }

        [Fact]
        public void Get_WithValueType_ReturnsConvertedValue()
        {
            // Arrange
            var key = "testInt";
            var expectedValue = 42;
            _storageAPI.Save(key, expectedValue.ToString()); // Save as string

            // Act
            var result = _storageAPI.Get<int>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetObjects_WithEmptyVault_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var emptyVaultStorage = new StorageAPI(_mockLogger.Object, _testVaultsDir, Guid.NewGuid());

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => emptyVaultStorage.GetObjects());
        }

        private class TestData
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public bool Active { get; set; }
        }
    }

    public class VaultServiceTests : IDisposable
    {
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly string _testLuxoriaDir;
        private readonly VaultService _vaultService;

        public VaultServiceTests()
        {
            _mockLogger = new Mock<ILoggerService>();
            _testLuxoriaDir = Path.Combine(Path.GetTempPath(), "LuxoriaTest_" + Guid.NewGuid().ToString());

            Directory.CreateDirectory(_testLuxoriaDir);

            _vaultService = CreateVaultServiceWithCustomDirectory();
        }

        private VaultService CreateVaultServiceWithCustomDirectory()
        {
            var vaultsDir = Path.Combine(_testLuxoriaDir, "IntlSys", "Vaults");
            Directory.CreateDirectory(vaultsDir);

            var manifestPath = Path.Combine(vaultsDir, "manifest.json");
            File.WriteAllText(manifestPath, "{}");

            return new VaultService(_mockLogger.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testLuxoriaDir))
            {
                Directory.Delete(_testLuxoriaDir, true);
            }
        }

        [Fact]
        public void Constructor_CreatesRequiredDirectoriesAndManifest()
        {
            var service = CreateVaultServiceWithCustomDirectory();
            Assert.NotNull(service);
        }

        [Fact]
        public void GetVault_WithNewVaultName_CreatesNewVault()
        {
            // Arrange
            var vaultName = "testVault";

            // Act
            var result = _vaultService.GetVault(vaultName);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StorageAPI>(result);
        }

        [Fact]
        public void GetVault_WithExistingVaultName_ReturnsExistingVault()
        {
            // Arrange
            var vaultName = "testVault";
            var firstVault = _vaultService.GetVault(vaultName);
            var firstVaultId = ((StorageAPI)firstVault).Vault;

            // Act
            var secondVault = _vaultService.GetVault(vaultName);
            var secondVaultId = ((StorageAPI)secondVault).Vault;

            // Assert
            Assert.Equal(firstVaultId, secondVaultId);
        }

        [Fact]
        public void GetVault_WithEmptyVaultName_ThrowsArgumentException()
        {
            // Arrange
            var vaultName = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _vaultService.GetVault(vaultName));
        }

        [Fact]
        public void GetVaults_ReturnsAllVaults()
        {
            // Arrange
            _vaultService.GetVault("vault1");
            _vaultService.GetVault("vault2");
            _vaultService.GetVault("vault3");

            // Act
            var result = _vaultService.GetVaults();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void UsingMultipleVaults_WorksAsExpected()
        {
            // Arrange
            IStorageAPI vault1 = _vaultService.GetVault("vault1");
            IStorageAPI vault2 = _vaultService.GetVault("vault2");
            // Act
            vault1.Save("key1", "value1");
            vault2.Save("key2", "value2");
            var valueFromVault1 = vault1.Get<string>("key1");
            var valueFromVault2 = vault2.Get<string>("key2");

            vault1.Save("key1_2", "anotherValue1");
            vault1.Save("key1_3", "anotherValue2");


            // Assert
            Assert.Equal(valueFromVault1, valueFromVault1);
            Assert.Equal("value2", valueFromVault2);
            Assert.Equal("anotherValue1", vault1.Get<string>("key1_2"));
            Assert.Equal("anotherValue2", vault1.Get<string>("key1_3"));
            Assert.Equal("value2", vault2.Get<string>("key2"));
        }

        [Fact]
        public void DeleteVault_WithExistingVault_RemovesVault()
        {
            // Arrange
            var vaultName = "testVault";
            var vault = _vaultService.GetVault(vaultName);

            // Act
            _vaultService.DeleteVault(vaultName);

            // Assert - Verify that getting the vault again creates a new one (indicating deletion)
            var newVault = _vaultService.GetVault(vaultName);
            var originalVaultId = ((StorageAPI)vault).Vault;
            var newVaultId = ((StorageAPI)newVault).Vault;

            Assert.NotEqual(originalVaultId, newVaultId);
        }

        [Fact]
        public void DeleteVault_WithNonExistingVault_ThrowsKeyNotFoundException()
        {
            // Arrange
            var vaultName = "nonExistingVault";

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => _vaultService.DeleteVault(vaultName));
        }

        [Fact]
        public void DeleteVault_WithEmptyVaultName_ThrowsArgumentException()
        {
            // Arrange
            var vaultName = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _vaultService.DeleteVault(vaultName));
        }

        [Fact]
        public void Manifest_Persistence_WorksAcrossInstances()
        {
            // Arrange
            var vaultName = "persistentVault";
            var firstInstance = CreateVaultServiceWithCustomDirectory();
            var vault = firstInstance.GetVault(vaultName);
            var vaultId = ((StorageAPI)vault).Vault;

            // Act
            var secondInstance = CreateVaultServiceWithCustomDirectory();
            var retrievedVault = secondInstance.GetVault(vaultName);
            var retrievedVaultId = ((StorageAPI)retrievedVault).Vault;

            // Assert
            Assert.Equal(vaultId, retrievedVaultId);
        }
    }
}