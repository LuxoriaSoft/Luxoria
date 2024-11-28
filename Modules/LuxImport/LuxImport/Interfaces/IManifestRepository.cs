using LuxImport.Models;

namespace LuxImport.Interfaces
{
    public interface IManifestRepository
    {
        /// <summary>
        /// Verifies if the collection has already been initialized.
        /// </summary>
        /// <returns>True if the collection has been initialized, false otherwise.</returns>
        bool IsInitialized();

        /// <summary>
        /// Creates the manifest file for the collection.
        /// </summary>
        Manifest CreateManifest();

        /// <summary>
        /// Saves the manifest to a file.
        /// </summary>
        /// <param name="manifest">Manfiest object to be saved</param>
        void SaveManifest(Manifest manifest);

        /// <summary>
        /// Reads the manifest file from the collection.
        /// </summary>
        Manifest ReadManifest();
    }
}
