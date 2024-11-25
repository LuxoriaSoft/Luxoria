using Luxoria.Modules.Models;

namespace LuxImport.Interfaces
{
    public interface ILuxConfigRepository
    {
        // The path to the collection of LuxCfg models
        string CollectionPath { get; init; }

        /// <summary>
        /// Save the LuxCfg model to a file
        /// </summary>
        void Save(LuxCfg model);

        /// <summary>
        /// Load the LuxCfg model from a file, retrieving it by its ID
        /// </summary>
        LuxCfg? Load(Guid id);
    }
}
