using System;
using System.Reflection;

namespace Luxoria.App.Helpers;

public static class AssemblyHelper
{
    /// <summary>
    /// Return the main assembly version
    /// </summary>
    /// <returns>Assembly Version</returns>
    public static Version GetVersion() => Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>
    /// Return the main assembly version as X.Y.Z
    /// </summary>
    /// <returns>Assembly Version (XYZ String Format)</returns>
    public static string GetVersionXYZ()
    {
        Version ver = GetVersion();
        if (ver.Build < 0)
            return $"{ver.Major}.{ver.Minor}";
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }

    /// <summary>
    /// Version Comparator
    /// </summary>
    public static class VersionCompare
    {
        public static string Normalize(string s) => s.Replace("v", "");
        public static bool Compare(Version a, Version b) => a == b;
        public static bool Compare(string a, Version b) => Version.Parse(Normalize(a)) == b;
        public static bool Compare(Version a, string b) => a == Version.Parse(Normalize(b));
        public static bool Compare(string a, string b) => Version.Parse(Normalize(a)) == Version.Parse(Normalize(b));
    }
}
