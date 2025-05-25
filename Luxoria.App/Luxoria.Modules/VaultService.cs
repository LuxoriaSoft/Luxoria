using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System.Collections.ObjectModel;

namespace Luxoria.Modules;

/// <summary>
/// Vault System
/// Used to manage vaults and their contents
/// A Vault can be allocated to a module, which can then store and retrieve items from it
/// </summary>
public class VaultService : IVaultService, IStorageAPI
{
    /// <summary>
    /// Section Name
    /// </summary>
    private const string _sectionName = "Luxoria.Modules/Vaults";

    /// <summary>
    /// Path to the Luxoria AppData directory
    /// </summary>
    private readonly string _luxoriaDir;

    /// <summary
    /// Path to the vaults directory
    /// </summary>
    private readonly string _vaultsDir;

    /// <summary>
    /// Path to the manifest file for the vault system
    /// </summary>
    private readonly string _manifestFilePath;

    /// <summary>
    /// Dictionnary to store vaults by their unique identifier
    /// </summary>
    private readonly Dictionary<string, Guid> _vaults = [];

    /// <summary>
    /// Selected Vault
    /// </summary>
    public Guid Vault { get; private set; } = Guid.Empty;


    /// <summary>
    /// Constructor
    /// </summary>
    public VaultService(ILoggerService _logger)
    {
        _logger.Log("Initializing Vault System...", _sectionName, LogLevel.Info);
        // AppData path for Luxoria
        _luxoriaDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Luxoria");
        // If the directory does not exist, create it
        if (!Directory.Exists(_luxoriaDir))
        {
            _logger.Log($"Creating Luxoria directory at {_luxoriaDir}", _sectionName, LogLevel.Info);
            Directory.CreateDirectory(_luxoriaDir);
        }

        // Vaults path ./_luxoriaDir/IntlSys/Vaults
        _vaultsDir = Path.Combine(_luxoriaDir, "IntlSys", "Vaults");
        // If the directory does not exist, create it
        if (!Directory.Exists(_vaultsDir))
        {
            _logger.Log($"Creating Vaults directory at {_vaultsDir}", _sectionName, LogLevel.Info);
            Directory.CreateDirectory(_vaultsDir);
        }

        _manifestFilePath = Path.Combine(_vaultsDir, "manifest.json");
        // If the manifest file does not exist, create it by serializing the _vaults dictionary to JSON
        if (!File.Exists(_manifestFilePath))
        {
            _logger.Log($"Creating manifest file at {_manifestFilePath}", _sectionName, LogLevel.Info);
            SaveVaultsToManifest();
        }

        // Load existing vaults from the manifest file
        LoadVaultsFromManifest();


        _logger.Log("Vault System initialized successfully.", _sectionName, LogLevel.Info);
    }

    /// <summary>
    /// When the VaultService is released, save the vaults to the manifest file
    /// </summary>
    ~VaultService()
    {
        // Save the vaults to the manifest file
        SaveVaultsToManifest();
    }

    /// <summary>
    /// Saves the current vaults to the manifest file
    /// </summary>
    private void SaveVaultsToManifest()
    {
        // Serialize the _vaults dictionary to JSON and save it to the manifest file
        var json = System.Text.Json.JsonSerializer.Serialize(_vaults);
        File.WriteAllText(_manifestFilePath, json);
    }

    /// <summary>
    /// Load vaults from the manifest file
    /// </summary>
    private void LoadVaultsFromManifest()
    {
        if (File.Exists(_manifestFilePath))
        {
            var json = File.ReadAllText(_manifestFilePath);
            _vaults.Clear();
            var loadedVaults = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Guid>>(json);
            if (loadedVaults != null)
            {
                foreach (var vault in loadedVaults)
                {
                    _vaults[vault.Key] = vault.Value;
                }
            }
        }
    }

    // ONLY ACCESSIBLE THROUGH THE IVAULTSERVICE INTERFACE
    /// <summary>
    /// Get a vault by using its name
    /// </summary>
    public IStorageAPI GetVault(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
        {
            throw new ArgumentException("Vault name cannot be null or empty.", nameof(vaultName));
        }
        // Check if the vault exists
        if (_vaults.TryGetValue(vaultName, out Guid vaultId))
        {
            Vault = vaultId;
            return this;
        }
        // If the vault does not exist, create a new one
        var newVaultId = Guid.NewGuid();
        _vaults[vaultName] = newVaultId;

        Vault = newVaultId;
        return this;
    }

    /// <summary>
    /// Get all vaults
    /// </summary>
    /// <returns>A read-only collection of vault names</returns>
    public ICollection<(string, Guid)> GetVaults() =>
        new ReadOnlyCollection<(string, Guid)>(_vaults.Select(v => (v.Key, v.Value)).ToList());

    /// <summary>
    /// Delete a vault by its name
    /// </summary>
    public void DeleteVault(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
        {
            throw new ArgumentException("Vault name cannot be null or empty.", nameof(vaultName));
        }
        // Check if the vault exists
        if (_vaults.Remove(vaultName))
        {
            SaveVaultsToManifest();
        }
        else
        {
            throw new KeyNotFoundException($"Vault '{vaultName}' does not exist.");
        }
    }

    // ONLY ACCESSIBLE THROUGH THE ISTORAGEAPI INTERFACE
}
