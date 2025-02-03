using Luxoria.Modules.Models;
using Luxoria.Modules.Utils;
using SkiaSharp;
using System.Diagnostics;

namespace LuxImport.Utils;

public static class ImageDataHelper
{
    /// <summary>
    /// Load image data from a specified path
    /// </summary>
    /// <param name="path">The path to the image file</param>
    /// <returns>An ImageData object containing the loaded image's data</returns>
    public static ImageData LoadFromPath(string path)
    {
        // Check if the path is valid
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The specified file does not exist.", path);
        }

        // Extract the file extension and validate it
        string extension = Path.GetExtension(path);
        FileExtension ext = FileExtensionHelper.ConvertToEnum(extension);
        if (ext == FileExtension.UNKNOWN)
        {
            throw new NotSupportedException($"File format '{extension}' is not supported.");
        }

        try
        {
            // Read the file bytes
            byte[] fileBytes = File.ReadAllBytes(path);

            if (fileBytes.Length == 0)
            {
                throw new InvalidOperationException($"The file at '{path}' is empty.");
            }

            // Load the image using SkiSharp
            SKBitmap bitmap = SKBitmap.Decode(fileBytes);

            // Check if the image was loaded successfully
            if (bitmap == null)
            {
                throw new InvalidOperationException($"Failed to load image at '{path}'.");
            }

            // Create an ImageData object from the loaded image
            return new ImageData(
                bitmap,
                FileExtensionHelper.ConvertToEnum(extension)
            );
        }
        catch (Exception ex)
        {
            // Log detailed error message and stack trace for troubleshooting
            Debug.WriteLine($"An error occurred while loading the image at '{path}': {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw new InvalidOperationException($"An error occurred while loading the image at '{path}': {ex.Message}", ex);
        }
    }
}
