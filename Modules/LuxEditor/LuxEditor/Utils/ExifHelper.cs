using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LuxEditor.Utils;

public static class ExifHelper
{
    /// <summary>
    /// Prints all EXIF metadata in a clean, recursive-style view for debugging.
    /// </summary>
    public static void DebugPrintMetadata(IReadOnlyDictionary<string, string> metadata, string context = "EXIF")
    {
        Debug.WriteLine($"--- {context} Metadata Dump ---");

        foreach (var kvp in metadata)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key)) continue;

            string value = kvp.Value ?? "null";

            if (value.Contains(","))
            {
                var parts = value.Split(',');
                Debug.WriteLine($"{kvp.Key} = [");

                for (int i = 0; i < parts.Length; i++)
                {
                    Debug.WriteLine($"    [{i}] {parts[i].Trim()}");
                }

                Debug.WriteLine($"]");
            }
            else
            {
                Debug.WriteLine($"{kvp.Key} = {value}");
            }
        }

        Debug.WriteLine("--- END ---");
    }

    /// <summary>
    /// Tries to parse a float value from metadata by key, returns null if missing or invalid.
    /// </summary>
    public static float? GetFloat(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var raw) && float.TryParse(raw, out float parsed))
            return parsed;

        return null;
    }

    /// <summary>
    /// Tries to parse an array of floats from a comma-separated string in metadata.
    /// </summary>
    public static float[]? GetFloatArray(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var raw))
            return null;

        var parts = raw.Split(',');
        var result = new List<float>();

        foreach (var part in parts)
        {
            if (float.TryParse(part.Trim(), out float val))
                result.Add(val);
        }

        return result.Count > 0 ? result.ToArray() : null;
    }

    /// <summary>
    /// Tries to get the raw white balance in Kelvin from metadata.
    /// </summary>
    /// <param name="meta"></param>
    /// <returns></returns>
    public static float? TryGetRawWhiteBalanceKelvin(IReadOnlyDictionary<string, string> meta)
    {
        if (meta.TryGetValue("Unknown tag (0x7010)", out var raw))
        {
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4 &&
                float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[3], out float b) &&
                b != 0)
            {
                float ratio = r / b;
                float kelvin = 6500f * (1f / ratio);
                return Math.Clamp(kelvin, 2000, 50000);
            }
        }

        return null;
    }

}
