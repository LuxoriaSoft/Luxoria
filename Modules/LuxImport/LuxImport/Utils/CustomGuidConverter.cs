using Newtonsoft.Json;
using System.Diagnostics;

namespace LuxImport.Utils;

/// <summary>
/// Custom converter for Guid values
/// </summary>
public class CustomGuidConverter : JsonConverter<Guid>
{
    /// <summary>
    /// Read a Guid value from JSON
    /// </summary>
    /// <returns>
    /// Returns the parsed Guid value if successful, otherwise Guid.Empty
    /// </returns>
    public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string? value = reader.Value?.ToString();
            if (Guid.TryParse(value, out Guid result))
            {
                return result;  // Return the parsed Guid
            }
            else
            {
                Debug.WriteLine($"Invalid Guid: {value}");
            }
        }

        return Guid.Empty; // Return Guid.Empty if parsing fails
    }

    /// <summary>
    /// Write a Guid value to JSON
    /// </summary>
    public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());  // Serialize the Guid as a string
    }
}
