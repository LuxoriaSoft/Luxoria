using LuxFilter.Algorithms.Interfaces;
using Luxoria.Modules.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Reflection;

namespace LuxFilter.Algorithms.ColorVisualAesthetics;

public class CLIPAlgo : IFilterAlgorithm, IDisposable
{
    public string Name => "CLIP";
    public string Description => "CLIP AI Model";

    private readonly Lazy<InferenceSession> _session = new(()
        => new InferenceSession(ExtractEmbeddedResource("LuxFilter.Algorithms.Algorithms.ColorVisualAesthetics.CLIPModel.clip_image_encoder.onnx")));
    private readonly float[] _positiveVec;
    private readonly float[] _negativeVec;

    public CLIPAlgo()
    {
        try
        {
            _positiveVec = LoadVector(ExtractEmbeddedResource("LuxFilter.Algorithms.Algorithms.ColorVisualAesthetics.CLIPModel.positive.txt"));
            _negativeVec = LoadVector(ExtractEmbeddedResource("LuxFilter.Algorithms.Algorithms.ColorVisualAesthetics.CLIPModel.negative.txt"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CLIPAlgo: {ex.Message}");
            throw;
        }
    }

    public double Compute(ImageData data)
    {
        try
        {
            string path = SaveBitmapToTempFile(data.Bitmap);
            var tensor = PreprocessImage(path);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("pixel_values", tensor)
            };

            using var results = _session.Value.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();

            double posSim = CosineSimilarity(output, _positiveVec);
            double negSim = CosineSimilarity(output, _negativeVec);

            return (posSim - negSim) * 1000;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error computing CLIP score: {ex.Message}");
            return 0;
        }
    }

    private static string SaveBitmapToTempFile(SKBitmap bitmap)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        using SKFileWStream fileStream = new(tempFile);
        bitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);

        return tempFile;
    }

    private static DenseTensor<float> PreprocessImage(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var resized = bitmap.Resize(new SKImageInfo(224, 224), SKFilterQuality.Medium);

        float[] mean = { 0.48145466f, 0.4578275f, 0.40821073f };
        float[] std = { 0.26862954f, 0.26130258f, 0.27577711f };
        float[] data = new float[3 * 224 * 224];

        for (int y = 0; y < 224; y++)
        {
            for (int x = 0; x < 224; x++)
            {
                var pixel = resized.GetPixel(x, y);
                int i = y * 224 + x;

                data[0 * 224 * 224 + i] = ((pixel.Red / 255f) - mean[0]) / std[0];
                data[1 * 224 * 224 + i] = ((pixel.Green / 255f) - mean[1]) / std[1];
                data[2 * 224 * 224 + i] = ((pixel.Blue / 255f) - mean[2]) / std[2];
            }
        }

        return new DenseTensor<float>(data, new[] { 1, 3, 224, 224 });
    }

    private static float[] LoadVector(string path)
    {
        return File.ReadAllLines(path)
                   .Select(float.Parse)
                   .ToArray();
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    public void Dispose()
    {
        if (_session.IsValueCreated)
        {
            _session.Value.Dispose();
        }
    }

    /// <summary>
    /// Extracts an embedded resource and writes it to a temporary file.
    /// </summary>
    /// <param name="resourceName">The resource name</param>
    /// <returns>Path to the extracted file</returns>
    public static string ExtractEmbeddedResource(string resourceName)
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
}
