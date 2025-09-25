using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Luxoria.Modules.Services;

public class StorageAPI : IStorageAPI
{
    private const string SectionName = "Luxoria.Core/Vaults";
    public readonly Guid Vault;
    private readonly string _vaultsDir;
    private readonly ILoggerService _logger;

    public StorageAPI(
        ILoggerService logger,
        string vaultsDir,
        Guid vaultId)
    {
        _logger = logger;
        _vaultsDir = vaultsDir;
        Vault = vaultId;
    }

    /// <summary>
    /// Standarizes the input by removing unwanted characters
    /// </summary>
    /// <param name="input">Input to be standarised</param>
    /// <returns>Returns the standarised input</returns>
    public static string StandarizeInput(string input)
    {
        Regex r = new("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        return r.Replace(input, String.Empty);
    }

    /// <summary>
    /// Saves the content to the current vault using a key
    /// </summary>
    /// <param name="key">Saves as the key name</param>
    /// <param name="value">Value to be saved</param>
    public void Save(string key, object value)
        => SaveInternal(StandarizeInput(key), null, value);

    /// <summary>
    /// Saves the content to the current vault using a key with an expiration date
    /// </summary>
    /// <param name="key">Saves as the key name</param>
    /// <param name="goodUntil">Expiration data from retreival</param>
    /// <param name="value">Value to be saved</param>
    public void Save(string key, DateTime goodUntil, object value)
        => SaveInternal(StandarizeInput(key), goodUntil, value);

    /// <summary>
    /// Internal method to save the content to the current vault using a key
    /// </summary>
    private void SaveInternal<T>(string key, DateTime? goodUntil, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (Vault == Guid.Empty)
            throw new InvalidOperationException("No vault is currently selected.");

        _logger.Log($"Saving key=[{key}]", SectionName);

        var folder = Path.Combine(_vaultsDir, Vault.ToString());
        Directory.CreateDirectory(folder);

        var dataPath = Path.Combine(folder, key);
        var payload = value is string s ? s : JsonConvert.SerializeObject(value);
        File.WriteAllText(dataPath, payload);

        if (goodUntil.HasValue)
        {
            var ttlPath = Path.Combine(folder, $"{key}_cache.txt");
            File.WriteAllText(ttlPath, goodUntil.Value.ToString("o"));
        }
    }

    /// <summary>
    /// Checks whether the artifact exists in the current vault
    /// </summary>
    /// <param name="key">Artifact key</param>
    /// <returns>Returns where it exists</returns>
    public bool Contains(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (Vault == Guid.Empty)
            throw new InvalidOperationException("No vault is currently selected.");

        key = StandarizeInput(key);
        _logger.Log($"Contains key=[{key}]", SectionName);
        var folder = Path.Combine(_vaultsDir, Vault.ToString());
        var ttlPath = Path.Combine(folder, $"{key}_cache.txt");
        var dataPath = Path.Combine(folder, key);

        if (File.Exists(ttlPath))
        {
            if (DateTime.TryParse(File.ReadAllText(ttlPath), out var expire)
                && expire >= DateTime.UtcNow)
            {
                return true;
            }
            File.Delete(ttlPath);
            return false;
        }

        return File.Exists(dataPath);
    }

    /// <summary>
    /// Gets the content from the current vault using a key
    /// </summary>
    public T Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (Vault == Guid.Empty)
            throw new InvalidOperationException("No vault is currently selected.");

        key = StandarizeInput(key);
        _logger.Log($"Retreiving key=[{key}]", SectionName);
        var folder = Path.Combine(_vaultsDir, Vault.ToString());
        var ttlPath = Path.Combine(folder, $"{key}_cache.txt");
        var dataPath = Path.Combine(folder, key);

        if (File.Exists(ttlPath) && File.Exists(dataPath))
        {
            if (DateTime.TryParse(File.ReadAllText(ttlPath), out var expire)
                && expire >= DateTime.UtcNow)
            {
                return ConvertTo<T>(File.ReadAllText(dataPath));
            }
            File.Delete(ttlPath);
        }

        if (File.Exists(dataPath))
        {
            return ConvertTo<T>(File.ReadAllText(dataPath));
        }

        throw new FileNotFoundException($"Key '{key}' not found in the current vault.");
    }

    /// <summary>
    /// Internal method to convert the content described as string to a specific object
    /// </summary>
    private static T ConvertTo<T>(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));

        if (typeof(T) == typeof(string))
            return (T)(object)text!;

        var targetType = typeof(T);

        if (targetType.IsValueType || targetType.IsEnum)
        {
            try
            {
                return (T)Convert.ChangeType(text, targetType, System.Globalization.CultureInfo.InvariantCulture)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to convert '{text}' to {targetType.Name}.", ex);
            }
        }

        try
        {
            var obj = JsonConvert.DeserializeObject<T>(text);
            if (obj is null)
                throw new JsonSerializationException(
                    $"JsonConvert returned null when deserializing to {targetType.FullName}.");

            return obj;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error deserializing payload to {targetType.FullName}: {ex.Message}\nPayload: {text}",
                ex);
        }
    }

    /// <summary>
    /// Gets all objects in the current vault
    /// </summary>
    public ICollection<Guid> GetObjects()
    {
        if (Vault == Guid.Empty)
            throw new InvalidOperationException("No vault is currently selected.");

        var folder = Path.Combine(_vaultsDir, Vault.ToString());
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Vault '{Vault}' does not exist.");

        return Directory.GetFiles(folder)
            .Select(Path.GetFileName)
            .Where(name => !name.EndsWith("_cache.txt", StringComparison.OrdinalIgnoreCase))
            .Select(Guid.Parse)
            .ToList();
    }
}
