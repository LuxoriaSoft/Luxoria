using Luxoria.Modules.Models.Events;
using System;

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
        event Action<string> ProgressMessageSent;
    }
}
