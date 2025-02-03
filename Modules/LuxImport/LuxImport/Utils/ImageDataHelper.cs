using Luxoria.Modules.Models;
using Luxoria.Modules.Utils;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SkiaSharp;
using System.Diagnostics;

namespace LuxImport.Utils;

public static class ImageDataHelper
{
    /// <summary>
    /// Load image data from a specified path while preserving EXIF metadata.
    /// </summary>
    /// <param name="path">The path to the image file</param>
    /// <returns>An ImageData object containing the loaded image and its metadata</returns>
    public static ImageData LoadFromPath(string path)
    {
        // Validate path
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException("The specified file does not exist.", path);

        // Extract file extension and validate it
        string extension = Path.GetExtension(path);
        FileExtension ext = FileExtensionHelper.ConvertToEnum(extension);
        if (ext == FileExtension.UNKNOWN)
            throw new NotSupportedException($"File format '{extension}' is not supported.");

        try
        {
            Debug.WriteLine($"Attempting to load image from path: {path}");

            // Read the file bytes
            byte[] fileBytes = File.ReadAllBytes(path);
            Debug.WriteLine($"Loaded {fileBytes.Length} bytes from {path}");

            if (fileBytes.Length == 0)
                throw new InvalidOperationException($"The file at '{path}' is empty.");

            // Read EXIF metadata
            var metadata = ImageMetadataReader.ReadMetadata(path);
            var exifData = ExtractExif(metadata);

            // Load the image using SkiaSharp
            using var stream = new MemoryStream(fileBytes);
            using var codec = SKCodec.Create(stream);
            SKBitmap bitmap = SKBitmap.Decode(codec);

            if (bitmap == null)
                throw new InvalidOperationException($"Failed to load image at '{path}'.");

            // Apply EXIF orientation correction
            bitmap = ApplyExifOrientation(bitmap, codec.EncodedOrigin);

            // Create ImageData object containing both image and EXIF metadata
            return new ImageData(bitmap, ext, exifData);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred while loading the image at '{path}': {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw new InvalidOperationException($"An error occurred while loading the image at '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extracts relevant EXIF metadata from the image.
    /// </summary>
    private static Dictionary<string, string> ExtractExif(IReadOnlyList<MetadataExtractor.Directory> metadata)
    {
        var exifData = new Dictionary<string, string>();

        foreach (var directory in metadata)
        {
            foreach (var tag in directory.Tags)
            {
                exifData[tag.Name] = tag.Description ?? "";
            }
        }

        return exifData;
    }

    /// <summary>
    /// Adjusts the image based on EXIF orientation.
    /// </summary>
    private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        SKBitmap rotatedBitmap;

        switch (origin)
        {
            case SKEncodedOrigin.TopRight: // 2 - Flip Horizontal
                rotatedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.Scale(-1, 1, bitmap.Width / 2f, bitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            case SKEncodedOrigin.BottomRight: // 3 - Rotate 180
                rotatedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.RotateDegrees(180, bitmap.Width / 2f, bitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            case SKEncodedOrigin.BottomLeft: // 4 - Flip Vertical
                rotatedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.Scale(1, -1, bitmap.Width / 2f, bitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            case SKEncodedOrigin.LeftTop: // 5 - Transpose
            case SKEncodedOrigin.RightTop: // 7 - Transverse
                rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.RotateDegrees(90, rotatedBitmap.Width / 2f, rotatedBitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            case SKEncodedOrigin.RightBottom: // 6 - Rotate 90
                rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.RotateDegrees(90, rotatedBitmap.Width / 2f, rotatedBitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            case SKEncodedOrigin.LeftBottom: // 8 - Rotate 270
                rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
                using (var canvas = new SKCanvas(rotatedBitmap))
                {
                    canvas.RotateDegrees(270, rotatedBitmap.Width / 2f, rotatedBitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                bitmap.Dispose();
                return rotatedBitmap;

            default:
                return bitmap;
        }
    }
}
