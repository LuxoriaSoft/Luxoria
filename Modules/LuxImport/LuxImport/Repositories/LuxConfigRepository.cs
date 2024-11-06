using LuxImport.Interfaces;
using Luxoria.Modules.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxImport.Repositories
{
    public class LuxConfigRepository : ILuxConfigRepository
    {
        // The path to the collection of LuxCfg models
        public required string CollectionPath { get; set; }
        private static string _luxRelAssetsPath = ".lux/assets";
        private static string _luxCfgFileExtension = "luxcfg.json";

        /// <summary>
        /// Save the LuxCfg model to a file
        /// </summary>
        public void Save(LuxCfg model)
        {
            // Check if the model is null
            if (model == null) {
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
    }
}
