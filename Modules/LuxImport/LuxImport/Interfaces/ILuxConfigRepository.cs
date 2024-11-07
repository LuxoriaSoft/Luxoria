using Luxoria.Modules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
