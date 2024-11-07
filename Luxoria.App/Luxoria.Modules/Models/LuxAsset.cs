using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.Models
{
    /// <summary>
    /// Contains the properties of an asset in the Luxoria application.
    /// Data -> ImageData
    /// ConfigFile -> LuxCfg
    /// </summary>
    public class LuxAsset
    {
        /// <summary>
        /// Gets the unique identifier for the asset.
        /// </summary>
        public Guid Id => Config.Id;

        /// <summary>
        /// Contains the properties of the asset.
        /// </summary>
        public required LuxCfg Config { get; init; }

        /// <summary>
        /// Contains the data of the asset.
        /// </summary>
        public required ImageData Data { get; init; }
    }
}
