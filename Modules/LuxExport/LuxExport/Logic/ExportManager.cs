using System;
using System.IO;
using System.Threading.Tasks;

public enum ExportMode
{
    HardDrive,
    Web
}

public class ExportManager
{
    public ExportMode Mode { get; set; } = ExportMode.HardDrive;
    public string ExportPath { get; set; } = "";
    public event Action<string>? ExportCompleted;

    public async Task<bool> ExportAsync(string fileName, byte[] fileData)
    {
        if (Mode == ExportMode.HardDrive)
        {
            return await ExportToHardDriveAsync(fileName, fileData);
        }
        else
        {
            return await ExportToWebAsync(fileName, fileData);
        }
    }

    private async Task<bool> ExportToHardDriveAsync(string fileName, byte[] fileData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ExportPath))
                throw new InvalidOperationException("Export path is not set.");

            string fullPath = Path.Combine(ExportPath, fileName);
            await File.WriteAllBytesAsync(fullPath, fileData);
            ExportCompleted?.Invoke($"Exported successfully to {fullPath}");
            return true;
        }
        catch (Exception ex)
        {
            ExportCompleted?.Invoke($"Export failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ExportToWebAsync(string fileName, byte[] fileData)
    {
        await Task.Delay(1000);
        ExportCompleted?.Invoke($"Exported {fileName} to the web successfully.");
        return true;
    }
}
