using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace LuxImport.Utils;

/// <summary>
/// Custom resolver for the LuxCfg model
/// </summary>
public class CustomLuxCfgResolver : DefaultContractResolver
{
    /// <summary>
    /// Create a resolver for specific properties
    /// </summary>
    /// <param name="member">Member to resolve</param>
    /// <param name="memberSerialization">Serialization type</param>
    /// <returns></returns>
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        var prop = base.CreateProperty(member, memberSerialization);

        // Target the "Id" property specifically
        if (prop.PropertyName == "Id")
        {
            prop.Writable = true; // Ensure the property can be written to
        }

        return prop;
    }
}
