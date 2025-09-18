using Luxoria.Core.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace Luxoria.Core.Services;

/// <summary>
/// Vault System
/// Used to manage vaults and their contents
/// A Vault can be allocated to a module, which can then store and retrieve items from it
/// </summary>
public class VaultService : IVaultService
{
    private const string SectionName = "Luxoria.Core/Vaults";
    private readonly string _luxoriaDir;
    private readonly string _vaultsDir;
    private readonly string _manifestFilePath;
    private readonly ILoggerService _logger;

    private readonly Dictionary<string, Guid> _vaults = new();

    /// <summary>
    /// Condtructor for the VaultService
    /// </summary>
    /// <param name="logger"></param>
    public VaultService(ILoggerService logger)
    {
        logger.Log("Initializing Vault System...", SectionName, LogLevel.Info);

        _luxoriaDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Luxoria");
        Directory.CreateDirectory(_luxoriaDir);

        _vaultsDir = Path.Combine(_luxoriaDir, "IntlSys", "Vaults");
        Directory.CreateDirectory(_vaultsDir);

        _manifestFilePath = Path.Combine(_vaultsDir, "manifest.json");
        if (!File.Exists(_manifestFilePath))
        {
            logger.Log($"Creating manifest at {_manifestFilePath}", SectionName, LogLevel.Info);
            SaveVaultsToManifest();
        }

        LoadVaultsFromManifest();
        logger.Log("Vault System initialized successfully.", SectionName, LogLevel.Info);
        _logger = logger;
    }

    /// <summary>
    /// Saves the vaults to the manifest file when the service is disposed
    /// </summary>
    ~VaultService()
    {
        SaveVaultsToManifest();
    }

    /// <summary>
    /// Saves vaults to the manifest file
    /// </summary>
    private void SaveVaultsToManifest()
    {
        var json = JsonConvert.SerializeObject(_vaults);
        File.WriteAllText(_manifestFilePath, json);
    }

    /// <summary>
    /// Loads vaults from the manifest file
    /// </summary>
    private void LoadVaultsFromManifest()
    {
        if (!File.Exists(_manifestFilePath)) return;

        var json = File.ReadAllText(_manifestFilePath);
        var loaded = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(json);

        _vaults.Clear();
        if (loaded != null)
            foreach (var kv in loaded)
                _vaults[kv.Key] = kv.Value;
    }

    /// <summary>
    /// Gets the vault interface as StorageAPI by its name
    /// </summary>
    /// <param name="vaultName">Vault Name</param>
    /// <returns>API used to manage the vault</returns>
    /// <exception cref="ArgumentException">If vault cannot be found</exception>
    public IStorageAPI GetVault(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
            throw new ArgumentException("Vault name cannot be null or empty.", nameof(vaultName));

        if (!_vaults.TryGetValue(vaultName, out var vaultId))
        {
            vaultId = Guid.NewGuid();
            _vaults[vaultName] = vaultId;
            SaveVaultsToManifest();
        }

        Directory.CreateDirectory(Path.Combine(_vaultsDir, vaultId.ToString()));
        return new StorageAPI(_logger, _vaultsDir, vaultId);
    }

    /// <summary>
    /// Gets all vaults in a read-only collection
    /// </summary>
    /// <returns>Returns a collection which contains every vault</returns>
    public ICollection<(string, Guid)> GetVaults() =>
        new ReadOnlyCollection<(string, Guid)>(
            _vaults.Select(kv => (kv.Key, kv.Value)).ToList()
        );

    /// <summary>
    /// Deletes a vault by its name
    /// </summary>
    /// <param name="vaultName">Vault name</param>
    /// <exception cref="ArgumentException">If vault name is null or empty</exception>
    /// <exception cref="KeyNotFoundException">If vault cannot be found</exception>
    public void DeleteVault(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
            throw new ArgumentException("Vault name cannot be null or empty.", nameof(vaultName));

        if (!_vaults.TryGetValue(vaultName, out var vaultId))
            throw new KeyNotFoundException($"Vault '{vaultName}' does not exist.");

        _vaults.Remove(vaultName);
        SaveVaultsToManifest();

        var path = Path.Combine(_vaultsDir, vaultId.ToString());
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
}
