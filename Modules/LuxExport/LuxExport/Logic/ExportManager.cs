using System;
using System.IO;
using System.Threading.Tasks;

namespace LuxExport.Logic
{
    /// <summary>
    /// Enum representing the mode of export: either to the hard drive or to the web.
    /// </summary>
    public enum ExportMode
    {
        HardDrive,
        We
    }

    /// <summary>
    /// Manages the export process, supporting different export modes (hard drive or web).
    /// </summary>
    public class ExportManager
    {
        /// <summary>
        /// Gets or sets the export mode. Default is HardDrive.
        /// </summary>
        public ExportMode Mode { get; set; } = ExportMode.HardDrive;

        /// <summary>
        /// Gets or sets the path where files should be exported on the hard drive.
        /// </summary>
        public string ExportPath { get; set; } = "";

        /// <summary>
        /// Event triggered when the export is completed.
        /// </summary>
        public event Action<string>? ExportCompleted;

        /// <summary>
        /// Starts the export process asynchronously, based on the selected export mode.
        /// </summary>
        /// <param name="fileName">The name of the file to be exported.</param>
        /// <param name="fileData">The byte data of the file to be exported.</param>
        /// <returns>A task that represents the asynchronous operation, with a result indicating success or failure.</returns>
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

        /// <summary>
        /// Exports the file to the hard drive asynchronously.
        /// </summary>
        /// <param name="fileName">The name of the file to be exported.</param>
        /// <param name="fileData">The byte data of the file to be exported.</param>
        /// <returns>A task that represents the asynchronous operation, with a result indicating success or failure.</returns>
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

        /// <summary>
        /// Exports the file to the web asynchronously (simulated with a delay for now).
        /// </summary>
        /// <param name="fileName">The name of the file to be exported.</param>
        /// <param name="fileData">The byte data of the file to be exported.</param>
        /// <returns>A task that represents the asynchronous operation, with a result indicating success or failure.</returns>
        private async Task<bool> ExportToWebAsync(string fileName, byte[] fileData)
        {
            await Task.Delay(1000);

            ExportCompleted?.Invoke($"Exported {fileName} to the web successfully.");
            return true;
        }
    }
}
