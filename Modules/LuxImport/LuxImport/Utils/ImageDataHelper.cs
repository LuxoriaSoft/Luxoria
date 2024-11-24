using Luxoria.Modules.Models;
using Luxoria.Modules.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;

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
            // Log the start of the image loading process
            Debug.WriteLine($"Attempting to load image from path: {path}");

            // Read the file bytes
            byte[] fileBytes = File.ReadAllBytes(path);

            // Log file size for debugging purposes
            Debug.WriteLine($"Loaded {fileBytes.Length} bytes from {path}");

            if (fileBytes.Length == 0)
            {
                throw new InvalidOperationException($"The file at '{path}' is empty.");
            }

            // Load the image using ImageSharp
            using (MemoryStream memoryStream = new MemoryStream(fileBytes))
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(memoryStream))
                {
                    // Check if the image is valid
                    if (image == null)
                    {
                        throw new InvalidOperationException($"Failed to decode image at path: {path}");
                    }

                    // Log successful decoding for debugging purposes
                    Debug.WriteLine($"Image decoded successfully. Width: {image.Width}, Height: {image.Height}");

                    // Directly access pixel data (in RGBA format) without extra encoding
                    byte[] imageBytes = new byte[image.Width * image.Height * 4]; // RGBA32 = 4 bytes per pixel
                    image.CopyPixelDataTo(imageBytes); // Copy the pixel data directly to the byte array

                    // Create and return an ImageData object
                    return new ImageData(
                        imageBytes,
                        image.Width,
                        image.Height,
                        ext
                    );
                }
            }
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
