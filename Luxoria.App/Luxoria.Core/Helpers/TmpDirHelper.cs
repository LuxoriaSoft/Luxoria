using System.Diagnostics;

namespace Luxoria.Core.Helpers;

public sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory()
    {
        string basePath = System.IO.Path.GetTempPath();
        string folder = System.IO.Path.GetRandomFileName();
        Path = System.IO.Path.Combine(basePath, folder);
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch
        {
            Debug.WriteLine($"Failed to delete temporary directory: {Path}");
        }
    }
}