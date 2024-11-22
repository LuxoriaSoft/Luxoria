using LuxImport.Interfaces;
using LuxImport.Models;
using LuxImport.Repositories;
using Luxoria.Modules.Models;
using Luxoria.Modules.Utils;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Services;
using System;
using System.Diagnostics;
using System.IO;

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
            if (!IsInitialized())
            {
                throw new InvalidOperationException("Collection is not initialized.");
            }

            // Initial progress for manifest retrieval
            ProgressMessageSent?.Invoke(("Retrieving manifest file...", BaseProgressPercent + 5));
            Manifest manifest = _manifestRepository.ReadManifest();
            await Task.Delay(100);

            // Progress for updating indexing files
            ProgressMessageSent?.Invoke(("Updating indexing files...", BaseProgressPercent + 10));
            string[] files = Directory.GetFiles(_collectionPath, "*.*", SearchOption.AllDirectories);
            int total = files.Length;

            // Check if there are files to process
            if (total == 0)
            {
                ProgressMessageSent?.Invoke(("No files found to index.", 100));
                return; // Exit early if no files to process
            }

            // Set max progress percent to 55%
            const int MaxProgressPercent = 55;
            double progressIncrement = (double)MaxProgressPercent / total;

            for (int fcount = 0; fcount < total; fcount++)
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

                // Update the current progress percentage, ensuring it does not exceed MaxProgressPercent%
                int progressPercent = BaseProgressPercent + 10 + (int)Math.Min(MaxProgressPercent, progressIncrement * (fcount + 1));
                ProgressMessageSent?.Invoke(($"Processing file: {filename}... ({fcount + 1}/{total})", progressPercent));

                // Get sha256 hash of the file
                string hash256 = _fileHasherService.ComputeFileHash(file);

                // Check if file (asset) already exists in the manifest
                LuxCfg.AssetInterface? existingAsset = manifest.Assets
                    .FirstOrDefault(asset =>
                        asset.FileName == filename
                        &&
                        asset.RelativeFilePath == relativePath
                );

                // If the asset does not exist, add it to the manifest
                if (existingAsset == null)
                {
                    // Generate a new LuxCfg ID
                    Guid luxCfgId = Guid.NewGuid();
                    manifest.Assets.Add(new LuxCfg.AssetInterface
                    {
                        FileName = filename,
                        RelativeFilePath = relativePath,
                        Hash = hash256,
                        LuxCfgId = luxCfgId
                    });

                    // Create a new LuxCfg model
                    LuxCfg newLuxCfg = new LuxCfg(LUXCFG_VERSION, luxCfgId, filename, String.Empty, FileExtensionHelper.ConvertToEnum(Path.GetExtension(filename)));

                    // Save the LuxCfg model
                    _luxCfgRepository.Save(newLuxCfg);
                }
                // Check if the asset exists but the hash is different
                else if (existingAsset.Hash != hash256)
                {
                    // Update the hash of the existing asset
                    existingAsset.Hash = hash256;

                    // Create a new LuxCfg model
                    LuxCfg newLuxCfg = new LuxCfg(LUXCFG_VERSION, existingAsset.LuxCfgId, filename, String.Empty, FileExtensionHelper.ConvertToEnum(Path.GetExtension(filename)));

                    // Save the LuxCfg model
                    _luxCfgRepository.Save(newLuxCfg);
                }

                await Task.Delay(25);
            }

            await Task.Delay(300);
            ProgressMessageSent?.Invoke(("Indexing complete.", BaseProgressPercent + 10 + MaxProgressPercent));
            await Task.Delay(250);

            ProgressMessageSent?.Invoke(($"Cleaning up... (base : {manifest.Assets.Count} assets)", BaseProgressPercent + 10 + MaxProgressPercent + 5));
            await Task.Delay(250);

            // Remove unused assets from the manifest
            var assetsList = manifest.Assets.ToList(); // Convert to a List
            assetsList.RemoveAll(asset => !files.Any(file => asset.RelativeFilePath == file.Replace(_collectionPath, string.Empty)));

            // Clear the original collection and add back the filtered assets
            manifest.Assets.Clear();
            foreach (var asset in assetsList)
            {
                manifest.Assets.Add(asset);
            }

            ProgressMessageSent?.Invoke(($"Cleanup complete. (final : {manifest.Assets.Count} assets)", BaseProgressPercent + 10 + MaxProgressPercent + 7));
            await Task.Delay(250);

            // Saving manifest file
            ProgressMessageSent?.Invoke(("Saving manifest file...", BaseProgressPercent + 10 + MaxProgressPercent + 8));
            _manifestRepository.SaveManifest(manifest);
            await Task.Delay(250);

            ProgressMessageSent?.Invoke(("Manifest file saved.", BaseProgressPercent + 10 + MaxProgressPercent + 10));
        }

        /// <summary>
        /// Loads the collection into memory.
        /// </summary>
        public ICollection<LuxAsset>  LoadAssets()
        {
            List<LuxAsset> assets = new List<LuxAsset>();

            // Retrieve the manifest file
            Manifest manifest = _manifestRepository.ReadManifest();

            // Iterate through the assets in the manifest
            foreach (var asset in manifest.Assets)
            {
                // Load the LuxCfg model
                LuxCfg? luxCfg = _luxCfgRepository.Load(asset.LuxCfgId);

                // If LuxCfg model is null, throw an exception
                if (luxCfg == null)
                {
                    throw new InvalidOperationException($"LuxCfg model with ID {asset.LuxCfgId} not found.");
                }

                // Create a new LuxAsset object
                LuxAsset newAsset = new LuxAsset
                {
                    Config = luxCfg,
                    Data = new ImageData(new byte[500], 1920, 1080, "")
                };

                // Add the new asset to the list
                assets.Add(newAsset);
            }

            // Return the list of assets
            return assets;
        }
    }
}
