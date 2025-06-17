using Luxoria.Core.Helpers;
using Microsoft.VisualBasic.FileIO;
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

    public static async Task InstallFromZip(string moduleName, string zipFilePath)
    {
        string appDir = Path.Combine(AppContext.BaseDirectory);
        string moduleDir = Path.Combine(appDir, "modules");

        using (var tmp = new TempDirectory())
        {
            Debug.WriteLine("Created tmp directory at " + tmp.Path);
            if (File.Exists(zipFilePath))
            {
                Debug.WriteLine($"Installing module from {zipFilePath} to {tmp.Path}");
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, tmp.Path, true);

                    string mPath = Path.Combine(tmp.Path, $"{moduleName}.luxmod");
                    bool isMPathPresent = Directory.Exists(mPath);
                    string gPath = Path.Combine(tmp.Path, moduleName);
                    bool isGPathPresent = Directory.Exists(gPath);

                    Debug.WriteLine($"Is Compiled Core Module Present : Path=[{mPath}] State=[{isMPathPresent}]");
                    Debug.WriteLine($"Is Compiled Graphical Module Present (OPTIONAL) : Path=[{gPath}] State=[{isGPathPresent}]");

                    if (!isMPathPresent) throw new Exception($"Module {mPath} is not present");

                    FileSystem.CopyDirectory(mPath, Path.Combine(moduleDir, $"{moduleName}.mkplinstd"), true);

                    if (isGPathPresent) FileSystem.CopyDirectory(gPath, Path.Combine(appDir, moduleName), true);
                    

                    Debug.WriteLine("Module extracted successfully.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error extracting module: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Zip file does not exist: {zipFilePath}");
                throw new FileNotFoundException("Zip file not found.", zipFilePath);
            }
        }
    }

    public static async Task InstallFromUrlAsync(string moduleName, string url)
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
                    
                    if (File.Exists(fileName))
                    {
                        // Extract the module here if needed
                        Debug.WriteLine($"Module downloaded successfully: {fileName}");
                        await InstallFromZip(moduleName, fileName);
                    }
                    else
                    {
                        Debug.WriteLine("Module file does not exist after download.");
                        throw new FileNotFoundException("Module file not found after download.", fileName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error downloading module: {ex.Message}");
                }
            }
        }
    }
}
