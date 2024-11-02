using LuxImport.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LuxImport.Services
{
    public class ImportService : IImportService
    {
        private readonly string CollectionPath;
        /// <summary>
        /// Initializes a new instance of the ImportService.
        /// </summary>
        public ImportService(Channel<string> @event, string collectionPath)
        {
            CollectionPath = collectionPath;

            // Check if the collection path is valid
            if (string.IsNullOrEmpty(CollectionPath))
            {
                throw new ArgumentException("Collection path cannot be null or empty.");
            }

            // Check if the collection path exists
            if (!Directory.Exists(CollectionPath))
            {
                throw new ArgumentException("Collection path does not exist.");
            }
        }

        /// <summary>
        /// Initializes the ImportService.
        /// </summary>
        private void Initialize()
        {
        }

        /// <summary>
        /// Verifies if the collection has already been initialized.
        /// </summary>
        /// <returns>True if the collection has been initialized, false otherwise.</returns>
        public bool IsInitialized()
        {
            // If the collection path is not null or empty return false
            if (string.IsNullOrEmpty(CollectionPath))
            {
                return false;
            }

            // Check if the collection path exists
            if (!Directory.Exists(CollectionPath))
            {
                return false;
            }

            // Check the root directory for the collection path
            // If there is a folder called '.lux' and in this folder there is a file called 'manifest.json'
            // then the collection is initialized
            if (Directory.Exists(Path.Combine(CollectionPath, ".lux")) &&
                File.Exists(Path.Combine(CollectionPath, ".lux", "manifest.json")))
            {
                return true;
            }

            return false;
        }
    }
}
