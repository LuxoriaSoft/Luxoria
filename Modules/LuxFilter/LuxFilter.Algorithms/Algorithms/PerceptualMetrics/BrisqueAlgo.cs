using LuxFilter.Algorithms.Interfaces;
using Luxoria.Algorithm.BrisqueScore;
using Luxoria.Modules.Models;
using SkiaSharp;
using System.Reflection;

namespace LuxFilter.Algorithms.PerceptualMetrics;

public class BrisqueAlgo : IFilterAlgorithm, IDisposable
{
    /// <summary>
    /// Get the algorithm name
    /// </summary>
    public string Name => "Brisque";

    /// <summary>
    /// Get the algorithm description
    /// </summary>
    public string Description => "Brisque algorithm";

    private readonly BrisqueInterop _brisque;
    private readonly string _modelPath;
    private readonly string _rangePath;

    /// <summary>
    /// Constructor - Initializes BrisqueInterop with embedded YAML files.
    /// </summary>
    public BrisqueAlgo()
    {
        try
        {
            _modelPath = ExtractEmbeddedResource("LuxFilter.Algorithms.Algorithms.PerceptualMetrics.brisque_model_live.yml");
            _rangePath = ExtractEmbeddedResource("LuxFilter.Algorithms.Algorithms.PerceptualMetrics.brisque_range_live.yml");

            _brisque = new BrisqueInterop(_modelPath, _rangePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error initializing Brisque: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Compute the Brisque score of the image.
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns>Returns the computed score of the algorithm</returns>
    public double Compute(ImageData data)
    {
        try
        {
            string imagePath = SaveBitmapToTempFile(data.Bitmap);
            return _brisque.ComputeScore(imagePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error computing Brisque score: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Saves an SKBitmap to a temporary file as a PNG.
    /// </summary>
    /// <param name="bitmap">The SKBitmap to save</param>
    /// <returns>The file path of the saved image</returns>
    private string SaveBitmapToTempFile(SKBitmap bitmap)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        using SKFileWStream fileStream = new SKFileWStream(tempFile);
        bitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);

        return tempFile;
    }

    /// <summary>
    /// Extracts an embedded resource and writes it to a temporary file.
    /// </summary>
    /// <param name="resourceName">The resource name</param>
    /// <returns>Path to the extracted file</returns>
    private string ExtractEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource {resourceName} not found");

        string tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(resourceName));
        using FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);

        return tempFile;
    }

    /// <summary>
    /// Properly disposes of the BrisqueInterop instance.
    /// </summary>
    public void Dispose()
    {
        _brisque?.Dispose();
    }
}
