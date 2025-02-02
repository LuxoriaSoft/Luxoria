using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Diagnosers;
using LuxImport.Services;
using BenchmarkDotNet.Exporters.Csv;

Console.WriteLine("LuxImport Benchmark Program");
var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddExporter(RPlotExporter.Default)  // Visual performance plots
    .AddExporter(HtmlExporter.Default)   // HTML output
    .AddExporter(MarkdownExporter.GitHub) // Markdown output
    .AddExporter(CsvExporter.Default)    // CSV output
    .AddExporter(JsonExporter.Full)      // JSON output
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);

var summary = BenchmarkRunner.Run<ImportServiceBenchmark>(config);

[MemoryDiagnoser]  // Tracks memory allocation & GC events
[ThreadingDiagnoser] // Monitors multi-threaded behavior
public class ImportServiceBenchmark
{
    private ImportService _importService;

    [GlobalSetup]
    public void Setup()
    {
        string testCollectionName = "BenchmarkCollection";
        string testCollectionPath = "\\Mac\\Home\\Downloads\\test";

        // Ensure the test directory exists
        if (!Directory.Exists(testCollectionPath))
        {
            Directory.CreateDirectory(testCollectionPath);
        }

        _importService = new ImportService(testCollectionName, testCollectionPath);
    }

    [Benchmark]
    public bool BenchmarkIsInitialized()
    {
        return _importService.IsInitialized();
    }

    [Benchmark]
    public void BenchmarkInitializeDatabase()
    {
        _importService.InitializeDatabase();
    }

    [Benchmark]
    public async Task BenchmarkIndexCollectionAsync()
    {
        await _importService.IndexCollectionAsync();
    }

    [Benchmark]
    public void BenchmarkLoadAssets()
    {
        _importService.LoadAssets();
    }
}