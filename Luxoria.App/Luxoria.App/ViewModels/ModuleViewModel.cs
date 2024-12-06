using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.App.ViewModels
{
    /// <summary>
    /// Represents a ViewModel wrapper for <see cref="IModule"/> to enable UI binding.
    /// </summary>
    public class ModuleViewModel
    {
        /// <summary>
        /// Gets the wrapped module instance.
        /// </summary>
        public IModule Module { get; }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name => Module.Name;

        /// <summary>
        /// Gets the version of the module.
        /// </summary>
        public string Version => Module.Version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleViewModel"/> class.
        /// </summary>
        /// <param name="module">The module to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when the module is null.</exception>
        public ModuleViewModel(IModule module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }
    }
}
