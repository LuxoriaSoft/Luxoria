using LuxImport.Interfaces;
using LuxImport.Models;
using LuxImport.Repositories;
using LuxImport.Utils;
using Luxoria.Modules.Models;
using Luxoria.Modules.Utils;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Services;
using System.Collections.Concurrent;

namespace LuxImport.Services
{
    public class ImportService : IImportService
    {
        private static string LUXCFG_VERSION = "1.0.0";

        private readonly string _collectionName;
        private readonly string _collectionPath;

        private readonly IManifestRepository _manifestRepository;
        private readonly ILuxConfigRepository _luxCfgRepository;
        private readonly IFileHasherService _fileHasherService;

        // Event declaration for sending progress messages
        public event Action<(string message, int? progress)> ProgressMessageSent;

        // Base progress percent for the import service
        public int BaseProgressPercent { get; set; }

        /// <summary>
        /// Initializes a new instance of the ImportService.
        /// </summary>
        public ImportService(string collectionName, string collectionPath)
        {
            _collectionName = collectionName;
            _collectionPath = collectionPath;

            // Initialize the event with a default handler
            ProgressMessageSent += (message) => { }; // This prevents null reference issues

            _manifestRepository = new ManifestRepository(_collectionName, _collectionPath);
            _fileHasherService = new Sha256Service();


            // Check if the collection path is valid
            if (string.IsNullOrEmpty(_collectionPath))
            {
                throw new ArgumentException("Collection path cannot be null or empty.");
            }

            // Check if the collection path exists
            if (!Directory.Exists(_collectionPath))
            {
                throw new ArgumentException("Collection path does not exist.");
            }

            // Initialize the LuxCfg repository
            _luxCfgRepository = new LuxConfigRepository
            {
                CollectionPath = _collectionPath
            };
        }

        /// <summary>
        /// Verifies if the collection has already been initialized.
        /// </summary>
        public bool IsInitialized()
        {
            // If the collection path is not null or empty return false
            if (string.IsNullOrEmpty(_collectionPath))
            {
                return false;
            }

            // Check if the collection path exists
            if (!Directory.Exists(_collectionPath))
            {
                return false;
            }

            // Check the root directory for the collection path
            if (Directory.Exists(Path.Combine(_collectionPath, ".lux")) &&
                File.Exists(Path.Combine(_collectionPath, ".lux", "manifest.json")))
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
            Directory.CreateDirectory(Path.Combine(_collectionPath, ".lux"));

            // Initialize the manifest file
            Manifest manifest = _manifestRepository.CreateManifest();

            // Save the manifest file
            _manifestRepository.SaveManifest(manifest);
        }

        /// <summary>
        /// Processes to the indexing of the collection.
        /// </summary>
        public async Task IndexCollectionAsync()
        {
            // Ensure the collection is initialized
            if (!IsInitialized())
            {
                throw new InvalidOperationException("Collection is not initialized.");
            }

            // Notify progress: Retrieving the manifest
            ProgressMessageSent?.Invoke(("Retrieving manifest file...", BaseProgressPercent + 5));
            Manifest manifest = _manifestRepository.ReadManifest();

            // Notify progress: Updating indexing files
            ProgressMessageSent?.Invoke(("Updating indexing files...", BaseProgressPercent + 10));

            // Image extensions allowed in the collection, if extension is not in this list, it will be ignored
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".arw", ".raw" };
            // Retrieve all files in the collection
            string[] files = Directory.GetFiles(_collectionPath, "*.*", SearchOption.AllDirectories)
                .Where(file =>
                    !Path.GetFileName(file).StartsWith("._")
                    &&
                    imageExtensions.Contains(Path.GetExtension(file).ToLower())
                )
                .ToArray();
            int totalFiles = files.Length;

            // Handle empty collections early
            if (totalFiles == 0)
            {
                ProgressMessageSent?.Invoke(("No files found to index.", 100));
                return;
            }

            // Calculate progress increment
            const int MaxProgressPercent = 55;
            double progressIncrement = (double)MaxProgressPercent / totalFiles;

            // Process each file
            for (int fcount = 0; fcount < totalFiles; fcount++)
            {
                string file = files[fcount];
                string filename = Path.GetFileName(file);
                string relativePath = file.Replace(_collectionPath, string.Empty);

                // Skip excluded files
                if (filename.Equals("manifest.json", StringComparison.OrdinalIgnoreCase) ||
                    file.Contains(Path.Combine(_collectionPath, ".lux")))
                {
                    continue;
                }

                // Update progress
                int progressPercent = BaseProgressPercent + 10 + (int)Math.Min(MaxProgressPercent, progressIncrement * (fcount + 1));
                ProgressMessageSent?.Invoke(($"Processing file: {filename}... ({fcount + 1}/{totalFiles})", progressPercent));

                // Compute hash and handle assets
                string hash256 = _fileHasherService.ComputeFileHash(file);
                HandleAsset(manifest, filename, relativePath, hash256);
            }

            // Finalize indexing process
            await FinalizeIndexingAsync(manifest, files);

            ProgressMessageSent?.Invoke(("Manifest file saved.", BaseProgressPercent + 10 + MaxProgressPercent + 10));
        }

        /// <summary>
        /// Handles the asset by adding or updating it in the manifest.
        /// </summary>
        private void HandleAsset(Manifest manifest, string filename, string relativePath, string hash256)
        {
            // Check for existing asset in the manifest
            var existingAsset = manifest.Assets
                .FirstOrDefault(asset => asset.FileName == filename && asset.RelativeFilePath == relativePath);

            // Handle new or updated assets
            if (existingAsset == null)
            {
                AddNewAsset(manifest, filename, relativePath, hash256);
            }
            else if (existingAsset.Hash != hash256)
            {
                UpdateExistingAsset(existingAsset, filename, hash256);
            }
        }

        /// <summary>
        /// Adds a new asset to the manifest.
        /// </summary>
        private void AddNewAsset(Manifest manifest, string filename, string relativePath, string hash256)
        {
            Guid luxCfgId = Guid.NewGuid();
            manifest.Assets.Add(new LuxCfg.AssetInterface
            {
                FileName = filename,
                RelativeFilePath = relativePath,
                Hash = hash256,
                LuxCfgId = luxCfgId
            });

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            var newLuxCfg = new LuxCfg(LUXCFG_VERSION, luxCfgId, fileNameWithoutExtension, filename, string.Empty, FileExtensionHelper.ConvertToEnum(Path.GetExtension(filename)));

            _luxCfgRepository.Save(newLuxCfg);
        }

        /// <summary>
        /// Updates an existing asset in the manifest.
        /// </summary>
        private void UpdateExistingAsset(LuxCfg.AssetInterface existingAsset, string filename, string hash256)
        {
            existingAsset.Hash = hash256;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            var updatedLuxCfg = new LuxCfg(LUXCFG_VERSION, existingAsset.LuxCfgId, fileNameWithoutExtension, filename, string.Empty, FileExtensionHelper.ConvertToEnum(Path.GetExtension(filename)));

            _luxCfgRepository.Save(updatedLuxCfg);
        }

        public void UpdateLastUploadId(Guid assetId, string url, Guid collectionid, Guid lastUploadedId)
        {
            var luxCfgTobeModified = _luxCfgRepository.Load(assetId);

            luxCfgTobeModified.StudioUrl = url;
            luxCfgTobeModified.LastUploadId = lastUploadedId;
            luxCfgTobeModified.CollectionId = collectionid;
            
            _luxCfgRepository.Save(luxCfgTobeModified);
        }

        /// <summary>
        /// Finalizes the indexing process by cleaning up unused assets.
        /// </summary>
        private async Task FinalizeIndexingAsync(Manifest manifest, string[] files)
        {
            // Notify progress: Cleaning up unused assets
            ProgressMessageSent?.Invoke(($"Cleaning up... (base: {manifest.Assets.Count} assets)", BaseProgressPercent + 67));

            // Convert ICollection to List for filtering
            var assetsList = manifest.Assets.ToList();

            // Remove unused assets
            var validFiles = files.Select(file => file.Replace(_collectionPath, string.Empty)).ToHashSet();
            assetsList.RemoveAll(asset => !validFiles.Contains(asset.RelativeFilePath));

            // Clear the original collection and add back the filtered assets
            manifest.Assets.Clear();
            foreach (var asset in assetsList)
            {
                manifest.Assets.Add(asset);
            }

            ProgressMessageSent?.Invoke(($"Cleanup complete. (final: {manifest.Assets.Count} assets)", BaseProgressPercent + 72));
            await Task.Delay(200);

            // Save the updated manifest
            _manifestRepository.SaveManifest(manifest);
        }

        /// <summary>
        /// Loads the collection into memory.
        /// </summary>
        public ICollection<LuxAsset> LoadAssets()
        {
            // Retrieve the manifest file
            Manifest manifest = _manifestRepository.ReadManifest();

            // Use a thread-safe collection to store results
            ConcurrentBag<LuxAsset> concurrentAssets = new ConcurrentBag<LuxAsset>();

            // Run indexication process in parallel
            Parallel.ForEach(manifest.Assets, asset =>
            {
                // Load the LuxCfg model
                LuxCfg? luxCfg = _luxCfgRepository.Load(asset.LuxCfgId);

                // If LuxCfg model is null, throw an exception
                if (luxCfg == null)
                {
                    throw new InvalidOperationException($"LuxCfg model with ID {asset.LuxCfgId} not found.");
                }

                // Load the image data
                ImageData imageData;

                // Ensure the relative path does not start with a directory separator
                string sanitizedRelativePath = asset.RelativeFilePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Construct the full file path
                string filePath = Path.Combine(_collectionPath, sanitizedRelativePath);

                try
                {
                    // Load the image data using the helper
                    imageData = ImageDataHelper.LoadFromPath(filePath);
                }
                catch (Exception ex)
                {
                    // Handle errors and include meaningful context in the exception message
                    throw new InvalidOperationException($"Failed to load image '{asset.FileName}' from path '{filePath}': {ex.Message}", ex);
                }

                // Create a new LuxAsset object
                LuxAsset newAsset = new LuxAsset
                {
                    MetaData = luxCfg,
                    Data = imageData
                };

                // Add the new asset to the list
                concurrentAssets.Add(newAsset);
            });

            // Return the list of assets
            return concurrentAssets.ToList();
        }

        /// <summary>
        /// Checks if the collection is initialized.
        /// If it is, returns true, otherwise false.
        /// </summary>
        /// <param name="collectionPath">Path from disk</param>
        /// <returns>Retruns if the collection is initialized</returns>
        public static bool IsCollectionInitialized(string collectionPath)
        {
            // Check if the collection path exists
            if (!Directory.Exists(collectionPath))
            {
                return false;
            }
            // Check the root directory for the collection path
            if (Directory.Exists(Path.Combine(collectionPath, ".lux")) &&
                File.Exists(Path.Combine(collectionPath, ".lux", "manifest.json")))
            {
                return true;
            }
            return false;
        }
    }
}
