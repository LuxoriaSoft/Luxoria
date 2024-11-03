using LuxImport.Interfaces;
using LuxImport.Models;
using LuxImport.Repositories;
using System;
using System.IO;

namespace LuxImport.Services
{
    public class ImportService : IImportService
    {
        private readonly string CollectionName;
        private readonly string CollectionPath;

        // Event declaration for sending progress messages
        public event Action<string> ProgressMessageSent;

        /// <summary>
        /// Initializes a new instance of the ImportService.
        /// </summary>
        public ImportService(string collectionName, string collectionPath)
        {
            CollectionName = collectionName;
            CollectionPath = collectionPath;

            // Initialize the event with a default handler
            ProgressMessageSent += message => { }; // This prevents null reference issues


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
        /// Verifies if the collection has already been initialized.
        /// </summary>
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
            if (Directory.Exists(Path.Combine(CollectionPath, ".lux")) &&
                File.Exists(Path.Combine(CollectionPath, ".lux", "manifest.json")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes the collection's database.
        /// </summary>
        public void InitializeDatabase()
        {
            // Check if the collection is already initialized
            if (IsInitialized())
            {
                return;
            }

            // Create the '.lux' folder
            Directory.CreateDirectory(Path.Combine(CollectionPath, ".lux"));

            // Initialize the manifest file
            IManifestRepository manifestRepository = new ManifestRepository(CollectionName, CollectionPath);
            Manifest manifest = manifestRepository.CreateManifest();

            // Save the manifest file
            manifestRepository.SaveManifest(manifest);
        }

        /// <summary>
        /// Processes to the indexing of the collection.
        /// </summary>
        public async Task IndexCollectionAsync()
        {
            if (!IsInitialized())
            {
                throw new InvalidOperationException("Collection is not initialized.");
            }

            ProgressMessageSent?.Invoke("Updating indexing files...");

            string[] files = Directory.GetFiles(CollectionPath, "*.*", SearchOption.AllDirectories);
            int total = files.Length;

            for (int fcount = 0; fcount < total; fcount++)
            {
                string file = files[fcount];
                string filename = Path.GetFileName(file);

                if (filename.Equals("manifest.json", StringComparison.OrdinalIgnoreCase) ||
                    file.Contains(Path.Combine(CollectionPath, ".lux")))
                {
                    continue; // Skip excluded files
                }

                ProgressMessageSent?.Invoke($"Processing file: {filename}... ({fcount + 1}/{total})");
                await Task.Delay(100); // Simulated processing delay
            }
        }
    }
}
