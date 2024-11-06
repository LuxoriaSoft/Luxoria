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
        string CollectionPath { get; set; }

        /// <summary>
        /// Save the LuxCfg model to a file
        /// </summary>
        void Save(LuxCfg model);

    }
}
