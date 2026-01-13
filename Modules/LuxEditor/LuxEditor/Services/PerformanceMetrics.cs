/*
 * PerformanceMetrics.cs - Professional Benchmarking Service for LuxEditor
 *
 * This service provides comprehensive performance tracking for:
 * - Execution time (latency, response time)
 * - Memory consumption (working set, GC allocations)
 * - CPU utilization metrics
 * - Per-operation breakdown
 *
 * Data is logged to console and exportable to JSON for dashboard integration.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace LuxEditor.Services
{
    /// <summary>
    /// Represents a single performance measurement sample.
    /// </summary>
    public class MetricSample
    {
        public DateTime Timestamp { get; set; }
        public string OperationName { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
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
    /// Complete benchmark session export format.
    /// </summary>
    public class BenchmarkSession
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Version { get; set; } = "2.0.0-newversion";
        public string Description { get; set; } = string.Empty;
        public SystemInfo SystemInfo { get; set; } = new();
        public List<MetricSample> Samples { get; set; } = new();
        public Dictionary<string, OperationStats> Statistics { get; set; } = new();
    }

    /// <summary>
    /// System information for context.
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
    /// Scoped timer for measuring operations using 'using' pattern.
    /// </summary>
    public class ScopedTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly string _phase;
        private readonly PerformanceMetrics _metrics;
        private readonly long _memoryBefore;
        private readonly int _gen0Before;
        private readonly int _gen1Before;
        private readonly int _gen2Before;
        private readonly Dictionary<string, object> _metadata;

        public ScopedTimer(PerformanceMetrics metrics, string operationName, string phase, Dictionary<string, object>? metadata = null)
        {
            _metrics = metrics;
            _operationName = operationName;
            _phase = phase;
            _metadata = metadata ?? new Dictionary<string, object>();

            // Capture initial state
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
                Phase = _phase,
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
    /// Thread-safe and designed for real-time monitoring.
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

        // Configuration
        public bool ConsoleLoggingEnabled { get; set; } = true;
        public bool DetailedLoggingEnabled { get; set; } = true;
        public string ExportDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LuxEditor_Benchmarks");

        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
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
        /// Starts a new benchmark session.
        /// </summary>
        public void StartNewSession(string description = "")
        {
            _sessionId = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";
            _sessionStart = DateTime.UtcNow;
            _sessionTimer.Restart();

            // Clear previous data
            while (_samples.TryTake(out _)) { }
            _operationTimes.Clear();

            LogToConsole("═══════════════════════════════════════════════════════════════════", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] New Session Started: {_sessionId}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Description: {description}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Start Time: {_sessionStart:yyyy-MM-dd HH:mm:ss.fff}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Machine: {Environment.MachineName}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] Processors: {Environment.ProcessorCount}", ConsoleColor.Cyan);
            LogToConsole($"[BENCHMARK] OS: {Environment.OSVersion}", ConsoleColor.Cyan);
            LogToConsole("═══════════════════════════════════════════════════════════════════", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Creates a scoped timer for measuring an operation.
        /// </summary>
        public ScopedTimer MeasureOperation(string operationName, string phase = "default", Dictionary<string, object>? metadata = null)
        {
            return new ScopedTimer(this, operationName, phase, metadata);
        }

        /// <summary>
        /// Records a manual metric sample.
        /// </summary>
        public void RecordSample(MetricSample sample)
        {
            _samples.Add(sample);

            var key = $"{sample.OperationName}:{sample.Phase}";
            _operationTimes.AddOrUpdate(
                key,
                new List<double> { sample.DurationMs },
                (_, list) => { list.Add(sample.DurationMs); return list; }
            );

            if (ConsoleLoggingEnabled)
            {
                PrintSampleToConsole(sample);
            }
        }

        /// <summary>
        /// Prints a sample to the console with color coding.
        /// </summary>
        private void PrintSampleToConsole(MetricSample sample)
        {
            var color = sample.DurationMs switch
            {
                < 16.67 => ConsoleColor.Green,      // Under 60fps threshold
                < 33.33 => ConsoleColor.Yellow,    // Under 30fps threshold
                < 100 => ConsoleColor.DarkYellow,  // Under 10fps threshold
                _ => ConsoleColor.Red              // Very slow
            };

            var memColor = sample.MemoryDelta switch
            {
                < 1024 * 100 => ConsoleColor.Green,      // < 100KB
                < 1024 * 1024 => ConsoleColor.Yellow,    // < 1MB
                < 1024 * 1024 * 10 => ConsoleColor.DarkYellow,  // < 10MB
                _ => ConsoleColor.Red
            };

            string timestamp = sample.Timestamp.ToString("HH:mm:ss.fff");
            string duration = $"{sample.DurationMs:F3}ms";
            string memory = FormatBytes(sample.MemoryDelta);
            string gcInfo = sample.Gen0Collections + sample.Gen1Collections + sample.Gen2Collections > 0
                ? $" | GC: G0={sample.Gen0Collections} G1={sample.Gen1Collections} G2={sample.Gen2Collections}"
                : "";

            LogToConsole($"[{timestamp}] ", ConsoleColor.DarkGray, false);
            LogToConsole($"[PERF] ", ConsoleColor.Magenta, false);
            LogToConsole($"{sample.OperationName}", ConsoleColor.White, false);
            LogToConsole($":{sample.Phase} ", ConsoleColor.DarkGray, false);
            LogToConsole($"| {duration} ", color, false);
            LogToConsole($"| Mem: {memory}", memColor, false);
            LogToConsole($"{gcInfo}", ConsoleColor.DarkCyan, true);

            if (DetailedLoggingEnabled && sample.Metadata.Count > 0)
            {
                foreach (var kv in sample.Metadata)
                {
                    LogToConsole($"        └─ {kv.Key}: {kv.Value}", ConsoleColor.DarkGray);
                }
            }
        }

        /// <summary>
        /// Logs a custom message to the console.
        /// </summary>
        public void Log(string message, string category = "INFO")
        {
            if (!ConsoleLoggingEnabled) return;

            var color = category.ToUpper() switch
            {
                "INFO" => ConsoleColor.Cyan,
                "WARN" => ConsoleColor.Yellow,
                "ERROR" => ConsoleColor.Red,
                "DEBUG" => ConsoleColor.DarkGray,
                "SLIDER" => ConsoleColor.Magenta,
                "IMAGE" => ConsoleColor.Blue,
                "RENDER" => ConsoleColor.Green,
                _ => ConsoleColor.White
            };

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            LogToConsole($"[{timestamp}] [{category}] {message}", color);
        }

        /// <summary>
        /// Gets statistics for all recorded operations.
        /// </summary>
        public Dictionary<string, OperationStats> GetStatistics()
        {
            var stats = new Dictionary<string, OperationStats>();

            foreach (var kvp in _operationTimes)
            {
                var times = kvp.Value.OrderBy(t => t).ToList();
                if (times.Count == 0) continue;

                var samples = _samples.Where(s => $"{s.OperationName}:{s.Phase}" == kvp.Key).ToList();

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
                    AvgMemoryDelta = (long)samples.Average(s => s.MemoryDelta),
                    MaxMemoryDelta = samples.Max(s => s.MemoryDelta),
                    TotalGCCollections = samples.Sum(s => s.Gen0Collections + s.Gen1Collections + s.Gen2Collections)
                };
            }

            return stats;
        }

        /// <summary>
        /// Prints a summary report to the console.
        /// </summary>
        public void PrintSummary()
        {
            var stats = GetStatistics();
            var sessionDuration = _sessionTimer.Elapsed;

            LogToConsole("\n", ConsoleColor.White);
            LogToConsole("╔═══════════════════════════════════════════════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
            LogToConsole("║                              BENCHMARK SUMMARY REPORT                                                  ║", ConsoleColor.Cyan);
            LogToConsole("╠═══════════════════════════════════════════════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            LogToConsole($"║  Session ID: {_sessionId,-82}║", ConsoleColor.Cyan);
            LogToConsole($"║  Duration: {sessionDuration.TotalSeconds:F2}s | Total Samples: {_samples.Count,-60}║", ConsoleColor.Cyan);
            LogToConsole("╠═══════════════════════════════════════════════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            LogToConsole("║  OPERATION                          │ COUNT │   MIN   │   AVG   │   MAX   │   P95   │  MEM AVG │ GCs  ║", ConsoleColor.White);
            LogToConsole("╠═════════════════════════════════════╪═══════╪═════════╪═════════╪═════════╪═════════╪══════════╪══════╣", ConsoleColor.DarkGray);

            foreach (var kvp in stats.OrderByDescending(s => s.Value.AvgMs))
            {
                var s = kvp.Value;
                var name = s.OperationName.Length > 35 ? s.OperationName[..35] : s.OperationName.PadRight(35);
                var memAvg = FormatBytes(s.AvgMemoryDelta).PadLeft(8);

                var color = s.AvgMs switch
                {
                    < 16.67 => ConsoleColor.Green,
                    < 33.33 => ConsoleColor.Yellow,
                    < 100 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.Red
                };

                LogToConsole($"║  {name} │ {s.SampleCount,5} │ {s.MinMs,6:F1}ms │ ", ConsoleColor.White, false);
                LogToConsole($"{s.AvgMs,6:F1}ms", color, false);
                LogToConsole($" │ {s.MaxMs,6:F1}ms │ {s.P95Ms,6:F1}ms │ {memAvg} │ {s.TotalGCCollections,4} ║", ConsoleColor.White);
            }

            LogToConsole("╚═══════════════════════════════════════════════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
            LogToConsole("\n", ConsoleColor.White);
        }

        /// <summary>
        /// Exports all benchmark data to JSON files.
        /// </summary>
        public string ExportToJson()
        {
            Directory.CreateDirectory(ExportDirectory);

            var session = new BenchmarkSession
            {
                SessionId = _sessionId,
                StartTime = _sessionStart,
                EndTime = DateTime.UtcNow,
                Version = "2.0.0-newversion",
                Description = "LuxEditor Performance Benchmark - New Version",
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

            // Export full session
            var fullPath = Path.Combine(ExportDirectory, $"{_sessionId}_full.json");
            File.WriteAllText(fullPath, JsonSerializer.Serialize(session, options));

            // Export statistics only (lighter file)
            var statsPath = Path.Combine(ExportDirectory, $"{_sessionId}_stats.json");
            File.WriteAllText(statsPath, JsonSerializer.Serialize(session.Statistics, options));

            // Export raw samples as CSV for easy analysis
            var csvPath = Path.Combine(ExportDirectory, $"{_sessionId}_samples.csv");
            ExportToCsv(csvPath);

            LogToConsole($"\n[EXPORT] Benchmark data exported to:", ConsoleColor.Green);
            LogToConsole($"  • Full JSON: {fullPath}", ConsoleColor.Gray);
            LogToConsole($"  • Stats JSON: {statsPath}", ConsoleColor.Gray);
            LogToConsole($"  • CSV: {csvPath}", ConsoleColor.Gray);

            return fullPath;
        }

        /// <summary>
        /// Exports samples to CSV format.
        /// </summary>
        private void ExportToCsv(string path)
        {
            var lines = new List<string>
            {
                "Timestamp,OperationName,Phase,DurationMs,MemoryBefore,MemoryAfter,MemoryDelta,Gen0,Gen1,Gen2"
            };

            foreach (var sample in _samples.OrderBy(s => s.Timestamp))
            {
                lines.Add($"{sample.Timestamp:O},{sample.OperationName},{sample.Phase},{sample.DurationMs:F3},{sample.MemoryBefore},{sample.MemoryAfter},{sample.MemoryDelta},{sample.Gen0Collections},{sample.Gen1Collections},{sample.Gen2Collections}");
            }

            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Logs the current memory state.
        /// </summary>
        public void LogMemorySnapshot(string context = "")
        {
            var gcInfo = GC.GetGCMemoryInfo();
            var currentMem = GC.GetTotalMemory(false);

            LogToConsole($"[MEMORY SNAPSHOT] {context}", ConsoleColor.Blue);
            LogToConsole($"  • Total Managed Memory: {FormatBytes(currentMem)}", ConsoleColor.Gray);
            LogToConsole($"  • Heap Size: {FormatBytes(gcInfo.HeapSizeBytes)}", ConsoleColor.Gray);
            LogToConsole($"  • Fragmented: {FormatBytes(gcInfo.FragmentedBytes)}", ConsoleColor.Gray);
            LogToConsole($"  • GC Counts: G0={GC.CollectionCount(0)} G1={GC.CollectionCount(1)} G2={GC.CollectionCount(2)}", ConsoleColor.Gray);
        }

        // Helper methods
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
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:F1}{sizes[order]}";
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
