namespace Luxoria.Modules.Interfaces;

public interface IStorageAPI
{
    /// <summary>
    /// Save an object to the current vault using a key
    /// </summary>
    /// <param name="key">Key under which the object will be stored</param>
    /// <param name="value">Value (object) to be stored/updated</param>
    /// <exception cref="ArgumentException">If one of the params is null</exception>
    /// <exception cref="InvalidOperationException">If a vault has NOT been selected</exception>
    void Save(string key, object value);

    /// <summary>
    /// Retrieve an object from the current vault using a key
    /// </summary>
    /// <param name="key">Key used to retreived the object</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The key is null or empty</exception>
    /// <exception cref="InvalidOperationException">If a vault has NOT been selected</exception>
    /// <exception cref="FileNotFoundException">If key is NOT attached to an object</exception>
    object Get(string key);

    /// <summary>
    /// Retrieve all objects in the current vault
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the vault has NOT been selected</exception>
    /// <exception cref="DirectoryNotFoundException">If the vault does NOT exist</exception>
    ICollection<Guid> GetObjects();

}
