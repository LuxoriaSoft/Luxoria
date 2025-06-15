using Luxoria.Core.Helpers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Luxoria.Core.Services;

public class ModuleInstaller
{
    public static string GetShortArch() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.X86 => "x86",
        Architecture.Arm | Architecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException("Unsupported architecture")
    };

    public static async Task InstallFromUrlAsync(string url)
    {
        using (var tmp = new TempDirectory())
        {
            Debug.WriteLine("Created tmp directory at " + tmp.Path);
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var fileName = Path.Combine(tmp.Path, "module.zip");
                    await using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    Debug.WriteLine($"Downloaded module to {fileName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error downloading module: {ex.Message}");
                }
            }
        }
    }
}
