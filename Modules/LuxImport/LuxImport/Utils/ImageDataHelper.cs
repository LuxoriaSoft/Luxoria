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

            // Get orientation from EXIF metadata
            // Apply EXIF orientation correction
            bitmap = ApplyExifOrientation(bitmap, GetExifOrientation(metadata));

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
    /// Extracts EXIF orientation from the metadata.
    /// </summary>
    private static SKEncodedOrigin GetExifOrientation(IReadOnlyList<MetadataExtractor.Directory> metadata)
    {
        foreach (var directory in metadata)
        {
            if (directory is ExifIfd0Directory exifDirectory)
            {
                if (exifDirectory.TryGetUInt16(ExifDirectoryBase.TagOrientation, out ushort orientation))
                {
                    return orientation switch
                    {
                        1 => SKEncodedOrigin.TopLeft,      // Normal (No Rotation)
                        2 => SKEncodedOrigin.TopRight,     // Flip Horizontal
                        3 => SKEncodedOrigin.BottomRight,  // Rotate 180
                        4 => SKEncodedOrigin.BottomLeft,   // Flip Vertical
                        5 => SKEncodedOrigin.LeftTop,      // Transpose (Rotate 90 + Flip)
                        6 => SKEncodedOrigin.RightBottom,  // Rotate 90
                        7 => SKEncodedOrigin.RightTop,     // Transverse (Rotate 270 + Flip)
                        8 => SKEncodedOrigin.LeftBottom,   // Rotate 270
                        _ => SKEncodedOrigin.TopLeft       // Default to No Rotation
                    };
                }
            }
        }

        return SKEncodedOrigin.TopLeft; // Default if orientation not found
    }


    /// <summary>
    /// Adjusts the image based on EXIF orientation.
    /// </summary>
    private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        if (origin == SKEncodedOrigin.TopLeft)
            return bitmap; // No transformation needed

        // Determine new dimensions based on rotation
        bool swapDimensions = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightBottom
                                        or SKEncodedOrigin.LeftBottom or SKEncodedOrigin.RightTop;

        int newWidth = swapDimensions ? bitmap.Height : bitmap.Width;
        int newHeight = swapDimensions ? bitmap.Width : bitmap.Height;

        SKBitmap transformedBitmap = new SKBitmap(newWidth, newHeight);

        using (SKCanvas canvas = new SKCanvas(transformedBitmap))
        {
            // Clear canvas to avoid black images
            canvas.Clear(SKColors.Transparent);

            SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High };

            switch (origin)
            {
                case SKEncodedOrigin.TopLeft:
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.TopRight: // Flip Horizontal
                    canvas.Scale(-1, 1);
                    canvas.Translate(-bitmap.Width, 0);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.BottomRight: // Rotate 180
                    canvas.RotateDegrees(180, bitmap.Width / 2f, bitmap.Height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.BottomLeft: // Flip Vertical
                    canvas.Scale(1, -1);
                    canvas.Translate(0, -bitmap.Height);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.LeftTop: // Rotate 90 CW
                    canvas.Translate(bitmap.Height, 0);
                    canvas.RotateDegrees(90);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.RightBottom: // Rotate 90 CCW
                    canvas.Translate(0, bitmap.Width);
                    canvas.RotateDegrees(-90);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.RightTop: // Rotate 270 CW + Flip Horizontal
                    canvas.Scale(-1, 1);
                    canvas.Translate(-bitmap.Height, 0);
                    canvas.RotateDegrees(270);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;

                case SKEncodedOrigin.LeftBottom: // Rotate 270 CW
                    canvas.Translate(0, bitmap.Width);
                    canvas.RotateDegrees(270);
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                    break;
            }
        }

        return transformedBitmap;
    }
}
