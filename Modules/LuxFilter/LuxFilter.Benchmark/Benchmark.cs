using BenchmarkDotNet.Attributes;
using LuxFilter.Algorithms.ImageQuality;
using LuxFilter.Algorithms.PerceptualMetrics;
using Luxoria.Modules.Models;
using SkiaSharp;

namespace LuxFilter.Benchmark;

[MemoryDiagnoser]
[MarkdownExporter, HtmlExporter, CsvExporter, JsonExporter]
public class FilterServiceBenchmark
{
    private readonly ResolutionAlgo _resolutionAlgo = new();
    private readonly SharpnessAlgo _sharpnessAlgo = new();
    private readonly BrisqueAlgo _brisqueAlgo = new();
    private readonly ImageData _data;

    public FilterServiceBenchmark()
    {
        _data = new(new SKBitmap(1920, 1080), FileExtension.UNKNOWN);
    }

    [Benchmark]
    public double ComputeResolution() => _resolutionAlgo.Compute(_data);

    [Benchmark]
    public double ComputeSharpness() => _sharpnessAlgo.Compute(_data);

    [Benchmark]
    public double ComputeBrisque() => _brisqueAlgo.Compute(_data);
}
