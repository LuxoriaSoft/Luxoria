using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Interfaces
{
    /// <summary>
    /// Interface for resolving and replacing tags in file names with actual values.
    /// </summary>
    public interface ITagResolver
    {
        /// <summary>
        /// Determines whether the tag can be resolved by this resolver.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>True if the tag can be resolved, otherwise false.</returns>
        bool CanResolve(string tag);

        /// <summary>
        /// Resolves a tag and replaces it with the corresponding value.
        /// </summary>
        /// <param name="tag">The tag to resolve.</param>
        /// <param name="originalName">The original file name.</param>
        /// <param name="metadata">Metadata associated with the image.</param>
        /// <param name="counter">A counter for generating unique names when necessary.</param>
        /// <returns>The resolved value for the tag.</returns>
        string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter);
    }
}
