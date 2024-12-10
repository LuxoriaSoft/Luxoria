using SkiaSharp;
using System;
using System.IO;
using System.Diagnostics;
using Luxoria.Modules.Models;

namespace LuxImport.Utils
{
    public static class ImageDataHelper
    {
        /// <summary>
        /// Load image data from a specified path and convert it into a byte array.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        /// <returns>An ImageData object containing the loaded image's data.</returns>
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

            try
            {
                // Log the start of the image loading process
                Debug.WriteLine($"Attempting to load image from path: {path}");

                // Load the image using SkiaSharp
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    // Decode the image from the stream
                    Debug.WriteLine("1/ Decoding the image using SkiaSharp...");
                    var skData = SKData.Create(stream);
                    Debug.WriteLine("1.1/ SKData created successfully.");
                    var skiaImage = SKBitmap.Decode(skData);  // This will automatically detect the format
                    Debug.WriteLine("2/ Image decoded successfully.");
                    if (skiaImage == null)
                    {
                        throw new InvalidOperationException($"Failed to decode the image at '{path}'.");
                    }
                    Debug.WriteLine($"Image dimensions: {skiaImage.Width}x{skiaImage.Height}");
                    // Convert the image to a byte array (PNG format)
                    using (var memoryStream = new MemoryStream())
                    {
                        Debug.WriteLine("3/ Encoding the image as PNG...");
                        skiaImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(memoryStream);
                        Debug.WriteLine("4/ Image encoded successfully.");
                        Debug.WriteLine($"Image size: {memoryStream.Length} bytes");
                        byte[] imageBytes = memoryStream.ToArray();
                        Debug.WriteLine("5/ Image converted to byte array successfully.");

                        // Return an ImageData object with the necessary data
                        return new ImageData(
                            imageBytes,
                            skiaImage.Width,
                            skiaImage.Height,
                            FileExtension.PNG//,
                            //skiaImage // You can use SKBitmap for further manipulation or display
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
}

