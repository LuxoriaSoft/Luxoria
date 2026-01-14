/*
 * PerformanceMetrics.cs - Standardized Benchmarking Service for LuxEditor
 *
 * VERSION: 2.0.0
 *
 * IMPORTANT: This file must be identical in both old and new versions for
 * meaningful benchmark comparisons. See BENCHMARK_README.md for documentation.
 *
 * Standard Operation Names:
 * - Render:Complete      - Full render pipeline from slider change to display
 * - Render:PreviewPass   - Preview (low-res) image rendering
 * - Render:FullPass      - Full resolution image rendering
 * - Render:ApplyFilters  - Filter application phase
 * - Render:LayerComposite - Layer compositing phase
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LuxEditor.Services
{
    /// <summary>
    /// Standard operation names for benchmarking.
    /// Use these constants to ensure consistent naming across versions.
    /// </summary>
    public static class BenchmarkOps
    {
        // ═══════════════════════════════════════════════════════════════
        // RENDER OPERATIONS - Core rendering pipeline measurements
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Full render pipeline from parameter change to image displayed.
        /// This is the PRIMARY metric for comparing versions.
        /// </summary>
        public const string RENDER_COMPLETE = "Render:Complete";

        /// <summary>
        /// Preview pass only (low-resolution rendering for responsiveness).
        /// Measures time to show initial feedback to user.
        /// </summary>
        public const string RENDER_PREVIEW = "Render:PreviewPass";

        /// <summary>
        /// Full resolution pass (final quality rendering).
        /// Measures time to render the full-quality image.
        /// </summary>
        public const string RENDER_FULL = "Render:FullPass";

        /// <summary>
        /// Filter application phase within a render pass.
        /// Measures CPU/GPU time for applying all active filters.
        /// </summary>
        public const string RENDER_FILTERS = "Render:ApplyFilters";

        /// <summary>
        /// Layer compositing phase.
        /// Measures time to composite all mask layers.
        /// </summary>
        public const string RENDER_LAYERS = "Render:LayerComposite";

        // ═══════════════════════════════════════════════════════════════
        // TEST SCENARIO MARKERS - Used by StressTestRunner
        // ═══════════════════════════════════════════════════════════════

        public const string TEST_EXPOSURE_SWEEP = "Test:ExposureSweep";
        public const string TEST_CONTRAST_SWEEP = "Test:ContrastSweep";
        public const string TEST_RAPID_MOVEMENT = "Test:RapidMovement";
        public const string TEST_WHITEBALANCE = "Test:WhiteBalance";
        public const string TEST_TONE_CONTROLS = "Test:ToneControls";
        public const string TEST_PRESENCE = "Test:PresenceControls";
        public const string TEST_FULL_STRESS = "Test:FullStress";
        public const string TEST_RESET = "Test:Reset";
    }

    /// <summary>
    /// Represents a single performance measurement sample.
    /// </summary>
    public class MetricSample
    {
        public DateTime Timestamp { get; set; }
        public string OperationName { get; set; } = string.Empty;
        public double DurationMs { get; set; }
        public long MemoryBefore { get; set; }
        public long MemoryAfter { get; set; }
        public long MemoryDelta { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Aggregated statistics for a specific operation.
    /// </summary>
    public class OperationStats
    {
        public string OperationName { get; set; } = string.Empty;
        public int SampleCount { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
        public double AvgMs { get; set; }
        public double MedianMs { get; set; }
        public double P95Ms { get; set; }
        public double P99Ms { get; set; }
        public double StdDevMs { get; set; }
        public long AvgMemoryDelta { get; set; }
        public long MaxMemoryDelta { get; set; }
        public int TotalGCCollections { get; set; }
    }

    /// <summary>
    /// System information for benchmark context.
    /// </summary>
    public class SystemInfo
    {
        public string MachineName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public string ProcessorArchitecture { get; set; } = string.Empty;
        public long TotalMemoryMB { get; set; }
    }

    /// <summary>
    /// Complete benchmark session export format.
    /// </summary>
    public class BenchmarkSession
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Version { get; set; } = "2.0.0";
        public string BenchmarkKitVersion { get; set; } = "2.0.0";
        public string Description { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public SystemInfo SystemInfo { get; set; } = new();
        public List<MetricSample> Samples { get; set; } = new();
        public Dictionary<string, OperationStats> Statistics { get; set; } = new();
    }

    /// <summary>
    /// Scoped timer for measuring operations using 'using' pattern.
    /// Usage: using (_metrics.Measure(BenchmarkOps.RENDER_COMPLETE)) { ... }
    /// </summary>
    public class ScopedTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly PerformanceMetrics _metrics;
        private readonly long _memoryBefore;
        private readonly int _gen0Before;
        private readonly int _gen1Before;
        private readonly int _gen2Before;
        private readonly Dictionary<string, object> _metadata;

        public ScopedTimer(PerformanceMetrics metrics, string operationName, Dictionary<string, object>? metadata = null)
        {
            _metrics = metrics;
            _operationName = operationName;
            _metadata = metadata ?? new Dictionary<string, object>();

            _memoryBefore = GC.GetTotalMemory(false);
            _gen0Before = GC.CollectionCount(0);
            _gen1Before = GC.CollectionCount(1);
            _gen2Before = GC.CollectionCount(2);

            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);

            var sample = new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = _operationName,
                DurationMs = _stopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = _memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryDelta = memoryAfter - _memoryBefore,
                Gen0Collections = GC.CollectionCount(0) - _gen0Before,
                Gen1Collections = GC.CollectionCount(1) - _gen1Before,
                Gen2Collections = GC.CollectionCount(2) - _gen2Before,
                Metadata = _metadata
            };

            _metrics.RecordSample(sample);
        }
    }

    /// <summary>
    /// Main performance metrics tracking service.
    /// Thread-safe singleton for global access.
    /// </summary>
    public class PerformanceMetrics
    {
        private static PerformanceMetrics? _instance;
        private static readonly object _lock = new();

        private readonly ConcurrentBag<MetricSample> _samples = new();
        private readonly ConcurrentDictionary<string, List<double>> _operationTimes = new();
        private readonly Stopwatch _sessionTimer = new();
        private string _sessionId = string.Empty;
        private DateTime _sessionStart;
        private string _imageName = string.Empty;
        private int _imageWidth;
        private int _imageHeight;

        public bool ConsoleLoggingEnabled { get; set; } = true;
        public bool DetailedLoggingEnabled { get; set; } = false;
        public string ExportDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "LuxEditor_Benchmarks");

        public static PerformanceMetrics Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new PerformanceMetrics();
                    }
                }
                return _instance;
            }
        }

        private PerformanceMetrics()
        {
            StartNewSession("Default Session");
        }

        /// <summary>
        /// Sets the current image info for benchmark context.
        /// </summary>
        public void SetImageInfo(string name, int width, int height)
        {
            _imageName = name;
            _imageWidth = width;
            _imageHeight = height;
        }

        /// <summary>
        /// Starts a new benchmark session. Clears all previous data.
        /// </summary>
        public void StartNewSession(string description = "")
        {
            _sessionId = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";
            _sessionStart = DateTime.UtcNow;
            _sessionTimer.Restart();

            while (_samples.TryTake(out _)) { }
            _operationTimes.Clear();

            LogToConsole("═══════════════════════════════════════════════════════════════════", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Session: {_sessionId}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Description: {description}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Image: {_imageName} ({_imageWidth}x{_imageHeight})", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] BenchmarkKit: v2.0.0", ConsoleColor.Cyan);
            LogToConsole("═══════════════════════════════════════════════════════════════════", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Creates a scoped timer for measuring an operation.
        /// Use constants from BenchmarkOps class for operation names.
        /// </summary>
        public ScopedTimer Measure(string operationName, Dictionary<string, object>? metadata = null)
        {
            return new ScopedTimer(this, operationName, metadata);
        }

        /// <summary>
        /// Records a metric sample.
        /// </summary>
        public void RecordSample(MetricSample sample)
        {
            _samples.Add(sample);

            _operationTimes.AddOrUpdate(
                sample.OperationName,
                new List<double> { sample.DurationMs },
                (_, list) => { list.Add(sample.DurationMs); return list; }
            );

            if (ConsoleLoggingEnabled)
            {
                PrintSampleToConsole(sample);
            }
        }

        private void PrintSampleToConsole(MetricSample sample)
        {
            var color = sample.DurationMs switch
            {
                < 16.67 => ConsoleColor.Green,
                < 33.33 => ConsoleColor.Yellow,
                < 100 => ConsoleColor.DarkYellow,
                _ => ConsoleColor.Red
            };

            string timestamp = sample.Timestamp.ToString("HH:mm:ss.fff");
            string duration = $"{sample.DurationMs:F2}ms";
            string memory = FormatBytes(sample.MemoryDelta);

            LogToConsole($"[{timestamp}] ", ConsoleColor.DarkGray, false);
            LogToConsole($"[PERF] ", ConsoleColor.Magenta, false);
            LogToConsole($"{sample.OperationName,-25} ", ConsoleColor.White, false);
            LogToConsole($"| {duration,10} ", color, false);
            LogToConsole($"| Mem: {memory,10}", ConsoleColor.Gray, true);
        }

        public void Log(string message, string category = "INFO")
        {
            if (!ConsoleLoggingEnabled) return;

            var color = category.ToUpper() switch
            {
                "INFO" => ConsoleColor.Cyan,
                "WARN" => ConsoleColor.Yellow,
                "ERROR" => ConsoleColor.Red,
                "TEST" => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            LogToConsole($"[{timestamp}] [{category}] {message}", color);
        }

        public Dictionary<string, OperationStats> GetStatistics()
        {
            var stats = new Dictionary<string, OperationStats>();

            foreach (var kvp in _operationTimes)
            {
                var times = kvp.Value.OrderBy(t => t).ToList();
                if (times.Count == 0) continue;

                var samples = _samples.Where(s => s.OperationName == kvp.Key).ToList();

                stats[kvp.Key] = new OperationStats
                {
                    OperationName = kvp.Key,
                    SampleCount = times.Count,
                    MinMs = times.Min(),
                    MaxMs = times.Max(),
                    AvgMs = times.Average(),
                    MedianMs = times[times.Count / 2],
                    P95Ms = GetPercentile(times, 95),
                    P99Ms = GetPercentile(times, 99),
                    StdDevMs = CalculateStdDev(times),
                    AvgMemoryDelta = samples.Count > 0 ? (long)samples.Average(s => s.MemoryDelta) : 0,
                    MaxMemoryDelta = samples.Count > 0 ? samples.Max(s => s.MemoryDelta) : 0,
                    TotalGCCollections = samples.Sum(s => s.Gen0Collections + s.Gen1Collections + s.Gen2Collections)
                };
            }

            return stats;
        }

        public void PrintSummary()
        {
            var stats = GetStatistics();
            var sessionDuration = _sessionTimer.Elapsed;

            LogToConsole("\n", ConsoleColor.White);
            LogToConsole("╔═══════════════════════════════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
            LogToConsole("║                              BENCHMARK SUMMARY                                        ║", ConsoleColor.Cyan);
            LogToConsole("╠═══════════════════════════════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            LogToConsole($"║  Session: {_sessionId,-74}║", ConsoleColor.Cyan);
            LogToConsole($"║  Image: {_imageName} ({_imageWidth}x{_imageHeight}){new string(' ', Math.Max(0, 74 - _imageName.Length - 20))}║", ConsoleColor.Cyan);
            LogToConsole($"║  Duration: {sessionDuration.TotalSeconds:F1}s | Samples: {_samples.Count,-55}║", ConsoleColor.Cyan);
            LogToConsole("╠═══════════════════════════════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            LogToConsole("║  OPERATION                    │ COUNT │   MIN   │   AVG   │   MAX   │   P95   │ MEM   ║", ConsoleColor.White);
            LogToConsole("╠═══════════════════════════════╪═══════╪═════════╪═════════╪═════════╪═════════╪═══════╣", ConsoleColor.DarkGray);

            foreach (var kvp in stats.OrderBy(s => s.Key))
            {
                var s = kvp.Value;
                var name = s.OperationName.Length > 29 ? s.OperationName[..29] : s.OperationName.PadRight(29);
                var memAvg = FormatBytesShort(s.AvgMemoryDelta);

                var color = s.AvgMs switch
                {
                    < 16.67 => ConsoleColor.Green,
                    < 33.33 => ConsoleColor.Yellow,
                    < 100 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.Red
                };

                LogToConsole($"║  {name} │ {s.SampleCount,5} │ {s.MinMs,6:F1}ms │ ", ConsoleColor.White, false);
                LogToConsole($"{s.AvgMs,6:F1}ms", color, false);
                LogToConsole($" │ {s.MaxMs,6:F1}ms │ {s.P95Ms,6:F1}ms │ {memAvg,5} ║", ConsoleColor.White);
            }

            LogToConsole("╚═══════════════════════════════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        public string ExportToJson()
        {
            var imageFolder = string.IsNullOrEmpty(_imageName) ? "unknown" : SanitizeFileName(_imageName);
            var exportPath = Path.Combine(ExportDirectory, imageFolder);
            Directory.CreateDirectory(exportPath);

            var session = new BenchmarkSession
            {
                SessionId = _sessionId,
                StartTime = _sessionStart,
                EndTime = DateTime.UtcNow,
                Version = "2.0.0",
                BenchmarkKitVersion = "2.0.0",
                Description = "LuxEditor Performance Benchmark",
                ImageName = _imageName,
                ImageWidth = _imageWidth,
                ImageHeight = _imageHeight,
                SystemInfo = new SystemInfo
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    ProcessorArchitecture = Environment.Is64BitProcess ? "x64" : "x86",
                    TotalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024)
                },
                Samples = _samples.ToList(),
                Statistics = GetStatistics()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var fullPath = Path.Combine(exportPath, $"{_sessionId}_full.json");
            File.WriteAllText(fullPath, JsonSerializer.Serialize(session, options));

            var statsPath = Path.Combine(exportPath, $"{_sessionId}_stats.json");
            File.WriteAllText(statsPath, JsonSerializer.Serialize(session.Statistics, options));

            var csvPath = Path.Combine(exportPath, $"{_sessionId}_samples.csv");
            ExportToCsv(csvPath);

            LogToConsole($"\n[EXPORT] Exported to: {exportPath}", ConsoleColor.Green);

            return fullPath;
        }

        private void ExportToCsv(string path)
        {
            var lines = new List<string>
            {
                "Timestamp,OperationName,DurationMs,MemoryDelta,Gen0,Gen1,Gen2"
            };

            foreach (var sample in _samples.OrderBy(s => s.Timestamp))
            {
                lines.Add($"{sample.Timestamp:O},{sample.OperationName},{sample.DurationMs:F3},{sample.MemoryDelta},{sample.Gen0Collections},{sample.Gen1Collections},{sample.Gen2Collections}");
            }

            File.WriteAllLines(path, lines);
        }

        private static double GetPercentile(List<double> sortedValues, int percentile)
        {
            if (sortedValues.Count == 0) return 0;
            int index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Count) - 1;
            return sortedValues[Math.Clamp(index, 0, sortedValues.Count - 1)];
        }

        private static double CalculateStdDev(List<double> values)
        {
            if (values.Count < 2) return 0;
            var avg = values.Average();
            var sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (values.Count - 1));
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = Math.Abs(bytes);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            string sign = bytes < 0 ? "-" : "+";
            return $"{sign}{len:F1}{sizes[order]}";
        }

        private static string FormatBytesShort(long bytes)
        {
            if (Math.Abs(bytes) < 1024) return $"{bytes}B";
            if (Math.Abs(bytes) < 1024 * 1024) return $"{bytes / 1024}K";
            return $"{bytes / (1024 * 1024)}M";
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }

        private void LogToConsole(string message, ConsoleColor color, bool newLine = true)
        {
            Debug.WriteLine(message);
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ResetColor();
        }
    }
}
