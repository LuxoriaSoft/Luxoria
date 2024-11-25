using LuxImport.Interfaces;
using LuxImport.Utils;
using Luxoria.Modules.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LuxImport.Repositories
{
    public class LuxConfigRepository : ILuxConfigRepository
    {
        // The path to the collection of LuxCfg models
        public required string CollectionPath { get; init; }
        private static string _luxRelAssetsPath = ".lux/assets";
        private static string _luxCfgFileExtension = "luxcfg.json";

        /// <summary>
        /// Save the LuxCfg model to a file
        /// </summary>
        public void Save(LuxCfg model)
        {
            // Check if the model is null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Check if the assets directory exists
            if (!Directory.Exists($"{CollectionPath}/{_luxRelAssetsPath}"))
            {
                Directory.CreateDirectory($"{CollectionPath}/{_luxRelAssetsPath}");
            }

            // Save the model to a file called 'model.LuxCfgId'
            File.WriteAllText($"{CollectionPath}/{_luxRelAssetsPath}/{model.Id}.{_luxCfgFileExtension}", JsonConvert.SerializeObject(model));

            // Log the save operation
            Debug.WriteLine($"Saved LuxCfg model with ID {model.Id}");
        }

        /// <summary>
        /// Load the LuxCfg model from a file, retrieving it by its ID
        /// </summary>
        public LuxCfg? Load(Guid id)
        {
            // Check if the assets directory exists
            if (!Directory.Exists($"{CollectionPath}/{_luxRelAssetsPath}"))
            {
                throw new DirectoryNotFoundException($"Assets directory not found at {CollectionPath}/{_luxRelAssetsPath}");
            }

            // Check if the file exists
            if (!File.Exists($"{CollectionPath}/{_luxRelAssetsPath}/{id}.{_luxCfgFileExtension}"))
            {
                throw new FileNotFoundException($"LuxCfg file not found at {CollectionPath}/{_luxRelAssetsPath}/{id}.{_luxCfgFileExtension}");
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomLuxCfgResolver(),
                Converters = { new CustomGuidConverter() }
            };

            // Load the model from the file
            var model = JsonConvert.DeserializeObject<LuxCfg>(File.ReadAllText($"{CollectionPath}/{_luxRelAssetsPath}/{id}.{_luxCfgFileExtension}"), jsonSettings);
            if (model == null)
            {
                throw new InvalidOperationException($"Deserialization of LuxCfg model with ID {id} returned null.");
            }

            return model;
        }
    }
}
