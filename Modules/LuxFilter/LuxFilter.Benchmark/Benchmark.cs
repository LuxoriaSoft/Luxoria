using BenchmarkDotNet.Attributes;
using LuxFilter.Algorithms.ImageQuality;
using LuxFilter.Algorithms.PerceptualMetrics;
using SkiaSharp;

namespace LuxFilter.Benchmark;

[MemoryDiagnoser]
[MarkdownExporter, HtmlExporter, CsvExporter, JsonExporter]
public class FilterServiceBenchmark
{
    private readonly ResolutionAlgo _resolutionAlgo = new();
    private readonly SharpnessAlgo _sharpnessAlgo = new();
    private readonly BrisqueAlgo _brisqueAlgo = new();
    private readonly SKBitmap _bitmap;

    public FilterServiceBenchmark()
    {
        _bitmap = new SKBitmap(1920, 1080);
    }

    [Benchmark]
    public double ComputeResolution() => _resolutionAlgo.Compute(_bitmap, _bitmap.Height, _bitmap.Width);

    [Benchmark]
    public double ComputeSharpness() => _sharpnessAlgo.Compute(_bitmap, _bitmap.Height, _bitmap.Width);

    [Benchmark]
    public double ComputeBrisque() => _brisqueAlgo.Compute(_bitmap, _bitmap.Height, _bitmap.Width);
}
