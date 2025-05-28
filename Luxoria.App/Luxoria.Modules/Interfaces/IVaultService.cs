namespace Luxoria.Modules.Interfaces;

public interface IVaultService
{
    /// <summary>
    /// Get the vault by its name.
    /// </summary>
    /// <param name="vaultName"></param>
    /// <returns>Returns the IStorageAPI instance for the specified vault.</returns>
    IStorageAPI GetVault(string vaultName);

    /// <summary>
    /// Get all vaults
    /// </summary>
    /// <returns>A read-only collection of vault names</returns>
    ICollection<(string, Guid)> GetVaults();

    /// <summary>
    /// Delete a vault by its name.
    /// </summary>
    /// <param name="vaultName">Name of the vault to be deleted</param>
    void DeleteVault(string vaultName);
}
