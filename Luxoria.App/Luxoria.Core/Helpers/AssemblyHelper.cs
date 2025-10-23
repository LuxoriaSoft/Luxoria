using System.Reflection;

namespace Luxoria.Core.Helpers;

public static class AssemblyHelper
{
    /// <summary>
    /// Return the main assembly version
    /// </summary>
    /// <returns>Assembly Version</returns>
    public static Version GetVersion()
        => Assembly.GetEntryAssembly().GetName().Version;

    /// <summary>
    /// Return the main assembly version as X.Y.Z
    /// </summary>
    /// <returns>Assembly Version (XYZ String Format)</returns>
    public static string GetVersionXYZ()
    {
        Version ver = GetVersion();
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }

    /// <summary>
    /// Version Comparator
    /// </summary>
    public static class VersionCompare
    {
        public static bool Compare(Version a, Version b) => a == b;
        public static bool Compare(string a, Version b) => new Version(a) == b;
        public static bool Compare(Version a, string b) => a == new Version(b);
        public static bool Compare(string a, string b) => new Version(a) == new Version(b);
    }
}
