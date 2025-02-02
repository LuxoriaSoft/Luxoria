using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Columns;
using LuxImport.Services;

Console.WriteLine("LuxImport Benchmark Program");

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddColumnProvider(DefaultColumnProviders.Instance) // Adds more performance-related columns
    .AddDiagnoser(MemoryDiagnoser.Default) // Tracks memory allocation & GC events
    .AddDiagnoser(ThreadingDiagnoser.Default) // Monitors multi-threaded behavior
    .AddExporter(RPlotExporter.Default)  // Visual performance plots
    .AddExporter(HtmlExporter.Default)   // HTML output
    .AddExporter(MarkdownExporter.GitHub) // Markdown output
    .AddExporter(CsvExporter.Default)    // CSV output
    .AddExporter(JsonExporter.Full)      // JSON output
    .AddExporter(AsciiDocExporter.Default) // AsciiDoc output
    .AddExporter(PlainExporter.Default) // Plain text output
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);

var summary = BenchmarkRunner.Run<ImportServiceBenchmark>(config);

[MemoryDiagnoser]  // Tracks memory allocation & GC events
[ThreadingDiagnoser] // Monitors multi-threaded behavior
[DisassemblyDiagnoser(printSource: true)] // Analyze JIT optimizations
public class ImportServiceBenchmark
{
    private ImportService _importService;

    /// <summary>
    /// Benchmark will run separately for each dataset.
    /// </summary>
    [Params(
        "\\Mac\\Home\\Downloads\\dataset_50",
        "\\Mac\\Home\\Downloads\\dataset_100",
        "\\Mac\\Home\\Downloads\\dataset_200"
    )]
    public string TestCollectionPath { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string testCollectionName = "BenchmarkCollection_" + Path.GetFileName(TestCollectionPath);

        // Ensure the test directory exists
        if (!Directory.Exists(TestCollectionPath))
        {
            Directory.CreateDirectory(TestCollectionPath);
        }

        _importService = new ImportService(testCollectionName, TestCollectionPath);

        Console.WriteLine($"Initialized ImportService for: {TestCollectionPath}");
    }

    [Benchmark]
    [BenchmarkCategory("Initialization")] // Categorizes benchmarks for better analysis
    public bool BenchmarkIsInitialized()
    {
        return _importService.IsInitialized();
    }

    [Benchmark]
    [BenchmarkCategory("Database")] // Categorizes benchmarks
    public void BenchmarkInitializeDatabase()
    {
        _importService.InitializeDatabase();
    }

    [Benchmark]
    [BenchmarkCategory("Indexing")] // Categorizes benchmarks
    public async Task BenchmarkIndexCollectionAsync()
    {
        await _importService.IndexCollectionAsync();
    }

    [Benchmark]
    [BenchmarkCategory("Loading")] // Categorizes benchmarks
    public void BenchmarkLoadAssets()
    {
        _importService.LoadAssets();
    }
}
