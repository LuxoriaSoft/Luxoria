using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace LuxImport.Logic.Services;

public interface IOService
{
    public record Source(string DisplayName, string Path)
    {
        [SupportedOSPlatform("windows")]
        public bool HasAccess
        {
            get { return CheckFolderPermission(Path); }
        }
    }

    [SupportedOSPlatform("windows")]
    public static bool CheckFolderPermission(string folderPath)
    {
        DirectoryInfo dirInfo = new(folderPath);
        try
        {
            _ = dirInfo.GetAccessControl(AccessControlSections.All);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IEnumerable<Source> GetDirectories(string path)
        => Directory
            .GetDirectories(path).ToList().Select(d =>
                new Source(Path.GetFileName(d), d));
}
