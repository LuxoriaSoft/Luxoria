using System.Collections.Generic;
using Luxoria.Modules.Models;

namespace Luxoria.Modules.Models
{
    /// <summary>
    /// Represents a picture with its properties and associated actions and versioning.
    /// </summary>
    public class LuxCfg
    {
        /// <summary>
        /// Gets the configuration version of the Luxoria application.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the name of the picture.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the picture.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the file extension of the picture.
        /// </summary>
        public FileExtension Extension { get; private set; }

        /// <summary>
        /// Gets the full name of the picture, combining the name and extension.
        /// </summary>
        public string FullName => $"{Name}{Extension.ToString().ToLower()}";

        /// <summary>
        /// Gets the list of actions associated with the picture.
        /// </summary>
        public List<LuxAction> Actions { get; private set; }

        /// <summary>
        /// Gets the list of versions associated with the picture.
        /// </summary>
        public List<LuxVersion> Versionning { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Picture"/> class.
        /// </summary>
        /// <param name="version">The configuration version of Luxoria.</param>
        /// <param name="name">The name of the picture.</param>
        /// <param name="description">The description of the picture.</param>
        /// <param name="extension">The file extension of the picture.</param>
        /// <param name="actions">The list of actions associated with the picture.</param>
        /// <param name="versionning">The list of versions associated with the picture.</param>
        public LuxCfg(string version, string name, string description, FileExtension extension, List<LuxAction> actions, List<LuxVersion> versionning)
        {
            Version = version;
            Name = name;
            Description = description;
            Extension = extension;
            Actions = actions ?? new List<LuxAction>();
            Versionning = versionning ?? new List<LuxVersion>();
        }
    }
}
