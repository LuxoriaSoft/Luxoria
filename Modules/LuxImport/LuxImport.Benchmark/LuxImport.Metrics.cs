using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;
using LuxImport.Benchmark;

Console.WriteLine("LuxImport Benchmark Program");

/// <summary>
/// Configures BenchmarkDotNet to use various exporters, diagnosers, and performance analysis tools.
/// </summary>
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

/// <summary>
/// Runs the benchmark tests using the configured settings.
/// </summary>
var summary = BenchmarkRunner.Run<ImportServiceBenchmark>(config);

// Prints the summary of the benchmark results
Console.WriteLine(summary);