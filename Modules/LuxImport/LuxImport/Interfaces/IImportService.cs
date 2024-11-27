using Luxoria.Modules.Models;

namespace LuxImport.Interfaces
{
    public interface IImportService
    {
        /// <summary>
        /// Verifies if the collection has already been initialized.
        /// </summary>
        /// <returns>True if the collection has been initialized, false otherwise.</returns>
        bool IsInitialized();

        ///<summary>
        /// Initializes the collection's database.
        ///</summary>
        void InitializeDatabase();

        /// <summary>
        /// Processes to the indexing of the collection.
        /// </summary>
        Task IndexCollectionAsync();

        /// <summary>
        /// Event triggered when a progress message is sent.
        /// </summary>
        event Action<(string message, int? progress)> ProgressMessageSent;

        /// <summary>
        /// Base progress percent for the import service.
        /// </summary>
        int BaseProgressPercent { get; set; }

        /// <summary>
        /// Loads the collection into memory.
        /// </summary>
        ICollection<LuxAsset> LoadAssets();
    }
}
