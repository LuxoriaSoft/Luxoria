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
        Task IndexCollectionAsync(IProgress<(string, int)>? progress = null);

        /// <summary>
        /// Loads the collection into memory.
        /// </summary>
        Task<ICollection<LuxAsset>> LoadAssetsAsync(IProgress<(string, int)>? progress = null);


        public void UpdateLastUploadId(Guid assetId, string url, Guid collectionid, Guid lastUploadedId);
    }
}
