using Luxoria.Core.Helpers;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Luxoria.Core.Services;

/// <summary>
/// Module Installer Helper
/// Ables to fetch an artifact from an URL and, or install a module from zip
/// </summary>
public class ModuleInstaller
{
    /// <summary>
    /// Gets the short architecture name based on the runtime information
    /// </summary>
    /// <returns>Returns the architecture such as x64, x86, arm64</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static string GetShortArch() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.X86 => "x86",
        Architecture.Arm | Architecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException("Unsupported architecture")
    };

    /// <summary>
    /// Installs a module from zip file
    /// </summary>
    /// <param name="moduleName">Module name to be installed, if comes from marketplace LuxMODULENAME</param>
    /// <param name="zipFilePath">Path to the zip file</param>
    /// <exception cref="FileNotFoundException">If zip does not exist</exception>
    public static void InstallFromZip(string moduleName, string zipFilePath)
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

    /// <summary>
    /// Downloads a module from an URL and proceeds to install it
    /// </summary>
    /// <param name="moduleName">Module name</param>
    /// <param name="url">Url to fetch the zip</param>
    /// <exception cref="FileNotFoundException">If fetch failed, throws the error due to missing artifact</exception>
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
                        Debug.WriteLine($"Module downloaded successfully: {fileName}");
                        InstallFromZip(moduleName, fileName);
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
