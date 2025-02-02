using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using LuxImport.Services;

namespace LuxImport.Benchmark;

/// <summary>
/// Benchmark class for testing the performance of ImportService operations.
/// </summary>
[MemoryDiagnoser]  // Tracks memory allocation & GC events
[ThreadingDiagnoser] // Monitors multi-threaded behavior
[DisassemblyDiagnoser(printSource: true)] // Analyze JIT optimizations
public class ImportServiceBenchmark
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ImportService _importService;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Specifies different dataset paths to be used in benchmarks.
    /// The benchmark will run separately for each dataset.
    /// </summary>
    [Params(
        "\\Mac\\Home\\Downloads\\dataset_50",   // Dataset with 50 images
        "\\Mac\\Home\\Downloads\\dataset_100",  // Dataset with 100 images
        "\\Mac\\Home\\Downloads\\dataset_200"   // Dataset with 200 images
    )]
    public required string TestCollectionPath { get; set; }

    /// <summary>
    /// Sets up the benchmark environment before execution.
    /// Ensures the dataset directory exists and initializes the ImportService.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        string testCollectionName = "BenchmarkCollection_" + Path.GetFileName(TestCollectionPath);

        // Ensure the dataset directory exists
        if (!Directory.Exists(TestCollectionPath))
        {
            Directory.CreateDirectory(TestCollectionPath);
        }

        _importService = new ImportService(testCollectionName, TestCollectionPath);

        Console.WriteLine($"Initialized ImportService for: {TestCollectionPath}");
    }

    /// <summary>
    /// Benchmark test for checking if the collection is already initialized.
    /// Measures the performance of the `IsInitialized()` method.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Initialization")] // Categorizes benchmarks for better analysis
    public bool BenchmarkIsInitialized()
    {
        return _importService.IsInitialized();
    }

    /// <summary>
    /// Benchmark test for initializing the database.
    /// Measures the time taken to execute `InitializeDatabase()`.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Database")] // Categorizes benchmarks
    public void BenchmarkInitializeDatabase()
    {
        _importService.InitializeDatabase();
    }

    /// <summary>
    /// Benchmark test for indexing the image collection asynchronously.
    /// Measures the performance of `IndexCollectionAsync()`.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Indexing")] // Categorizes benchmarks
    public async Task BenchmarkIndexCollectionAsync()
    {
        await _importService.IndexCollectionAsync();
    }

    /// <summary>
    /// Benchmark test for loading assets from the collection.
    /// Measures the performance of `LoadAssets()`.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Loading")] // Categorizes benchmarks
    public void BenchmarkLoadAssets()
    {
        _importService.LoadAssets();
    }
}