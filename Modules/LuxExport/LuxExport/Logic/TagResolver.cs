using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxExport.Interfaces;

namespace LuxExport.Logic
{
    /// <summary>
    /// Manages a collection of tag resolvers and resolves tags within a given pattern.
    /// </summary>
    public class TagResolverManager
    {
        private readonly List<ITagResolver> _resolvers = new();

        /// <summary>
        /// Initializes the tag resolver manager and adds the supported resolvers.
        /// </summary>
        public TagResolverManager()
        {
            _resolvers.Add(new NameTagResolver());
            _resolvers.Add(new DateTagResolver());
            _resolvers.Add(new CounterTagResolver());
            _resolvers.Add(new MetaTagResolver());
        }

        /// <summary>
        /// Resolves all tags in the given pattern and returns the processed string with replaced values.
        /// </summary>
        /// <param name="pattern">The pattern containing tags to be resolved.</param>
        /// <param name="originalName">The original file name used in some tag resolutions.</param>
        /// <param name="metadata">Metadata associated with the file, used for resolving meta tags.</param>
        /// <param name="counter">A counter used for generating unique values in the pattern.</param>
        /// <returns>A string with all resolved tags replaced by their corresponding values.</returns>
        public string ResolveAll(string pattern, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
        {
            return System.Text.RegularExpressions.Regex.Replace(pattern, @"\{([^\}]+)\}", match =>
            {
                string tag = match.Groups[1].Value;
                foreach (var resolver in _resolvers)
                {
                    if (resolver.CanResolve(tag))
                        return resolver.Resolve(tag, originalName, metadata, counter);
                }
                return $"{{{tag}}}"; // Return the tag as-is if no resolver can handle it
            });
        }
    }

    /// <summary>
    /// Resolves the "name" tag, which is replaced by the file's name without extension.
    /// </summary>
    public class NameTagResolver : ITagResolver
    {
        public bool CanResolve(string tag) => tag == "name";

        public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
        {
            return Path.GetFileNameWithoutExtension(originalName);
        }
    }

    /// <summary>
    /// Resolves the "date" tag, which is replaced by the current date in "yyyy-MM-dd" format.
    /// </summary>
    public class DateTagResolver : ITagResolver
    {
        public bool CanResolve(string tag) => tag == "date";

        public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }

    /// <summary>
    /// Resolves the "counter" tag, which is replaced by the current counter value.
    /// </summary>
    public class CounterTagResolver : ITagResolver
    {
        public bool CanResolve(string tag) => tag == "counter";

        public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
        {
            return counter.ToString();
        }
    }

    /// <summary>
    /// Resolves tags starting with "meta:", which are replaced by the corresponding value in metadata.
    /// </summary>
    public class MetaTagResolver : ITagResolver
    {
        public bool CanResolve(string tag) => tag.StartsWith("meta:");

        public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
        {
            var key = tag.Substring(5); // Extracts the key from "meta:" prefix
            return metadata.TryGetValue(key, out var value) ? value : "unknown";
        }
    }
}
