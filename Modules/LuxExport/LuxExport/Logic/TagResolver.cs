using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxExport.Interfaces;

namespace LuxExport.Logic;
public class TagResolverManager
{
    private readonly List<ITagResolver> _resolvers = new();

    public TagResolverManager()
    {
        _resolvers.Add(new NameTagResolver());
        _resolvers.Add(new DateTagResolver());
        _resolvers.Add(new CounterTagResolver());
        _resolvers.Add(new MetaTagResolver());
    }

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
            return $"{{{tag}}}";
        });
    }
}


public class NameTagResolver : ITagResolver
{
    public bool CanResolve(string tag) => tag == "name";

    public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
    {
        return Path.GetFileNameWithoutExtension(originalName);
    }
}

public class DateTagResolver : ITagResolver
{
    public bool CanResolve(string tag) => tag == "date";

    public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}

public class CounterTagResolver : ITagResolver
{
    public bool CanResolve(string tag) => tag == "counter";

    public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
    {
        return counter.ToString();
    }
}

public class MetaTagResolver : ITagResolver
{
    public bool CanResolve(string tag) => tag.StartsWith("meta:");

    public string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter)
    {
        var key = tag.Substring(5);
        return metadata.TryGetValue(key, out var value) ? value : "unknown";
    }
}
