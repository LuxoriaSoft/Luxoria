using LuxImport.Interfaces;
using LuxImport.Models;
using Newtonsoft.Json;

namespace LuxImport.Repositories
{
    public class ManifestRepository : IManifestRepository
    {
        private readonly string CollectionName;
        private readonly string CollectionPath;
        private readonly string ManifestPath;

        /// <summary>
        /// Initializes a new instance of the ManifestRepository.
        /// </summary>
        public ManifestRepository(string collectionName, string collectionPath)
        {
            CollectionName = collectionName;
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

            // Assign the manifest path
            ManifestPath = Path.Combine(CollectionPath, ".lux", "manifest.json");
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
            if (!File.Exists(ManifestPath))
            {
                return false;
            }

            // Check the json format of the manifest file
            return true;
        }

        /// <summary>
        /// Creates the manifest file for the collection.
        /// </summary>
        public Manifest CreateManifest()
        {
            var manifest = new Manifest(
                CollectionName,
                CollectionPath,
                "1.0.0",
                new Manifest.LuxoriaInfo("1.0.0")
            );

            return manifest;
        }

        /// <summary>
        /// Saves the manifest to a file.
        /// </summary>
        /// <param name="manifest">Manfiest object to be saved</param>
        public void SaveManifest(Manifest manifest)
        {
            // Serialize the manifest object to JSON
            string json = JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);

            // Define the file path where you want to save the manifest
            string filePath = Path.Combine(ManifestPath);

            // Save the JSON to a file
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Reads the manifest file from the collection.
        /// </summary>
        public Manifest ReadManifest()
        {
            // Ensure the manifest file exists before attempting to read
            if (!File.Exists(ManifestPath))
            {
                throw new FileNotFoundException("The manifest file was not found.", ManifestPath);
            }

            // Read the JSON content from the manifest file
            string json = File.ReadAllText(ManifestPath);

            // Deserialize the JSON to a Manifest object
            Manifest? manifest = JsonConvert.DeserializeObject<Manifest>(json);

            // Check if deserialization was successful
            if (manifest == null)
            {
                throw new InvalidOperationException("Failed to deserialize the manifest JSON.");
            }

            return manifest;
        }
    }
}
