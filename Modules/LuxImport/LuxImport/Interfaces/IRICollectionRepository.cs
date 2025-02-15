namespace LuxImport.Interfaces;

public interface IRICollectionRepository
{
    /// <summary>
    /// Updates an existing collection if it exists; otherwise, creates a new one.
    /// </summary>
    /// <param name="collectionName">Collection Name</param>
    /// <param name="collectionPath">Collection Path (from disk)</param>
    void UpdateOrCreate(string collectionName, string collectionPath);

    /// <summary>
    /// Retrieves the Nth latest imported collection.
    /// </summary>
    /// <param name="n">Nth elements to be returned</param>
    /// <returns></returns>
    (string Name, string Path)? GetNthLatestImportedCollection(int n);

    /// <summary>
    /// Retrieves the X latest imported collections.
    /// </summary>
    /// <param name="x">X elements to be returned</param>
    /// <returns>Returns a collection of Name and Path</returns>
    ICollection<(string Name, string Path)> GetXLatestImportedCollections(int x);

    /// <summary>
    /// Flushes the history of imported collections.
    /// </summary>
    void FlushHistory();
}
