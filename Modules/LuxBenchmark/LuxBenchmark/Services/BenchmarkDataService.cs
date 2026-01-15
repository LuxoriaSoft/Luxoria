using LuxBenchmark.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuxBenchmark.Services
{
    /// <summary>
    /// Service for loading and managing benchmark data from JSON files.
    /// </summary>
    public class BenchmarkDataService
    {
        private static BenchmarkDataService? _instance;
        public static BenchmarkDataService Instance => _instance ??= new BenchmarkDataService();

        private readonly List<BenchmarkSession> _loadedSessions = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public IReadOnlyList<BenchmarkSession> LoadedSessions => _loadedSessions.AsReadOnly();

        public event Action? SessionsChanged;
        public event Action<BenchmarkSession>? SessionLoaded;

        private BenchmarkDataService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Gets the default benchmark directory on the desktop.
        /// </summary>
        public string GetDefaultBenchmarkDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "LuxEditor_Benchmarks"
            );
        }

        /// <summary>
        /// Discovers all benchmark session files in a directory.
        /// Supports both flat and version-based folder structures (v3.0.0+).
        /// </summary>
        public List<string> DiscoverSessionFiles(string? directory = null)
        {
            var dir = directory ?? GetDefaultBenchmarkDirectory();
            var files = new List<string>();

            if (!Directory.Exists(dir))
                return files;

            // Look for *_full.json files recursively (supports version-based folders)
            files.AddRange(Directory.GetFiles(dir, "*_full.json", SearchOption.AllDirectories));

            return files.OrderByDescending(f => File.GetLastWriteTime(f)).ToList();
        }

        /// <summary>
        /// Gets all available versions from the benchmark directory.
        /// </summary>
        public List<string> GetAvailableVersions(string? directory = null)
        {
            var dir = directory ?? GetDefaultBenchmarkDirectory();
            var versions = new List<string>();

            if (!Directory.Exists(dir))
                return versions;

            // Get all subdirectories that contain benchmark files
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                var versionName = Path.GetFileName(subDir);
                var hasFiles = Directory.GetFiles(subDir, "*_full.json", SearchOption.AllDirectories).Length > 0;
                if (hasFiles)
                {
                    versions.Add(versionName);
                }
            }

            return versions.OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Gets sessions filtered by version.
        /// </summary>
        public List<BenchmarkSession> GetSessionsByVersion(string version)
        {
            return _loadedSessions
                .Where(s => s.LuxEditorVersion == version)
                .OrderByDescending(s => s.StartTime)
                .ToList();
        }

        /// <summary>
        /// Loads a benchmark session from a JSON file.
        /// </summary>
        public async Task<BenchmarkSession?> LoadSessionAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var session = JsonSerializer.Deserialize<BenchmarkSession>(json, _jsonOptions);

                if (session != null)
                {
                    // Check if already loaded
                    var existing = _loadedSessions.FirstOrDefault(s => s.SessionId == session.SessionId);
                    if (existing != null)
                    {
                        _loadedSessions.Remove(existing);
                    }

                    _loadedSessions.Add(session);
                    SessionLoaded?.Invoke(session);
                    SessionsChanged?.Invoke();
                }

                return session;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load session: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads all sessions from the default directory.
        /// </summary>
        public async Task<int> LoadAllSessionsAsync(string? directory = null)
        {
            var files = DiscoverSessionFiles(directory);
            int loaded = 0;

            foreach (var file in files)
            {
                var session = await LoadSessionAsync(file);
                if (session != null) loaded++;
            }

            return loaded;
        }

        /// <summary>
        /// Compares two sessions and returns detailed comparison results.
        /// Includes UX-focused comparison for v3.0.0+ sessions.
        /// </summary>
        public ComparisonSummary CompareSessions(BenchmarkSession sessionA, BenchmarkSession sessionB)
        {
            var summary = new ComparisonSummary
            {
                SessionA = sessionA,
                SessionB = sessionB
            };

            // Get all unique operation keys from both sessions
            var allKeys = sessionA.Statistics.Keys
                .Union(sessionB.Statistics.Keys)
                .Distinct()
                .OrderBy(k => k);

            foreach (var key in allKeys)
            {
                var result = new ComparisonResult
                {
                    OperationKey = key,
                    OperationName = key
                };

                if (sessionA.Statistics.TryGetValue(key, out var statsA))
                {
                    result.SessionA_AvgMs = statsA.AvgMs;
                    result.SessionA_P95Ms = statsA.P95Ms;
                    result.SessionA_AvgMemory = statsA.AvgMemoryDelta;
                    result.SessionA_Samples = statsA.SampleCount;
                }

                if (sessionB.Statistics.TryGetValue(key, out var statsB))
                {
                    result.SessionB_AvgMs = statsB.AvgMs;
                    result.SessionB_P95Ms = statsB.P95Ms;
                    result.SessionB_AvgMemory = statsB.AvgMemoryDelta;
                    result.SessionB_Samples = statsB.SampleCount;
                }

                summary.Results.Add(result);
            }

            // Build UX comparison if both sessions have UX metrics
            if (sessionA.HasUXMetrics || sessionB.HasUXMetrics)
            {
                summary.UXComparison = BuildUXComparison(sessionA, sessionB);
            }

            return summary;
        }

        /// <summary>
        /// Builds a UX-focused comparison between two sessions.
        /// </summary>
        private UXComparison BuildUXComparison(BenchmarkSession sessionA, BenchmarkSession sessionB)
        {
            var comparison = new UXComparison();

            // Get UX metrics from UXMetrics object or fallback to statistics
            var uxA = sessionA.UXMetrics;
            var uxB = sessionB.UXMetrics;

            // Time to first paint
            comparison.SessionA_TimeToFirstPaint = uxA?.AvgTimeToFirstPaintMs
                ?? GetStatAvgMs(sessionA, "UX:TimeToFirstPaint")
                ?? GetStatAvgMs(sessionA, "Render:PreviewPass")
                ?? 0;

            comparison.SessionB_TimeToFirstPaint = uxB?.AvgTimeToFirstPaintMs
                ?? GetStatAvgMs(sessionB, "UX:TimeToFirstPaint")
                ?? GetStatAvgMs(sessionB, "Render:PreviewPass")
                ?? 0;

            // Perceived latency
            comparison.SessionA_PerceivedLatency = uxA?.AvgPerceivedLatencyMs
                ?? GetStatAvgMs(sessionA, "UX:PerceivedLatency")
                ?? GetStatAvgMs(sessionA, "Render:PreviewPass")
                ?? 0;

            comparison.SessionB_PerceivedLatency = uxB?.AvgPerceivedLatencyMs
                ?? GetStatAvgMs(sessionB, "UX:PerceivedLatency")
                ?? GetStatAvgMs(sessionB, "Render:PreviewPass")
                ?? 0;

            // Frame consistency
            comparison.SessionA_FrameConsistency = uxA?.FrameConsistencyScore ?? 0;
            comparison.SessionB_FrameConsistency = uxB?.FrameConsistencyScore ?? 0;

            // Peak memory
            comparison.SessionA_PeakMemory = uxA?.PeakMemoryBytes ?? 0;
            comparison.SessionB_PeakMemory = uxB?.PeakMemoryBytes ?? 0;

            // GC collections
            comparison.SessionA_GCCollections = uxA?.TotalGCCollections ?? 0;
            comparison.SessionB_GCCollections = uxB?.TotalGCCollections ?? 0;

            return comparison;
        }

        /// <summary>
        /// Gets average milliseconds from statistics for a given operation key.
        /// </summary>
        private double? GetStatAvgMs(BenchmarkSession session, string operationKey)
        {
            if (session.Statistics.TryGetValue(operationKey, out var stats))
            {
                return stats.AvgMs;
            }
            return null;
        }

        /// <summary>
        /// Gets aggregated statistics across all samples for a given operation.
        /// Supports both old format (OperationName:Phase) and new format (just OperationName).
        /// </summary>
        public List<(DateTime Time, double Value)> GetTimeSeriesData(BenchmarkSession session, string operationKey)
        {
            return session.Samples
                .Where(s => s.OperationName == operationKey ||
                           $"{s.OperationName}:{s.Phase}" == operationKey)
                .OrderBy(s => s.Timestamp)
                .Select(s => (s.Timestamp, s.DurationMs))
                .ToList();
        }

        /// <summary>
        /// Gets memory usage time series data for a given operation.
        /// </summary>
        public List<(DateTime Time, long Value)> GetMemoryTimeSeriesData(BenchmarkSession session, string operationKey)
        {
            return session.Samples
                .Where(s => s.OperationName == operationKey ||
                           $"{s.OperationName}:{s.Phase}" == operationKey)
                .OrderBy(s => s.Timestamp)
                .Select(s => (s.Timestamp, s.MemoryDelta))
                .ToList();
        }

        /// <summary>
        /// Clears all loaded sessions.
        /// </summary>
        public void ClearSessions()
        {
            _loadedSessions.Clear();
            SessionsChanged?.Invoke();
        }

        /// <summary>
        /// Removes a specific session.
        /// </summary>
        public void RemoveSession(BenchmarkSession session)
        {
            _loadedSessions.Remove(session);
            SessionsChanged?.Invoke();
        }

        /// <summary>
        /// Formats bytes to a human-readable string.
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = Math.Abs(bytes);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            var sign = bytes < 0 ? "-" : "";
            return $"{sign}{len:F1} {sizes[order]}";
        }

        /// <summary>
        /// Formats milliseconds with color indication.
        /// </summary>
        public static string FormatDuration(double ms)
        {
            return $"{ms:F2} ms";
        }

        /// <summary>
        /// Gets performance rating based on duration.
        /// </summary>
        public static PerformanceRating GetRating(double ms)
        {
            return ms switch
            {
                < 16.67 => PerformanceRating.Excellent,  // 60 FPS
                < 33.33 => PerformanceRating.Good,       // 30 FPS
                < 100 => PerformanceRating.Acceptable,   // 10 FPS
                _ => PerformanceRating.Poor
            };
        }
    }

    public enum PerformanceRating
    {
        Excellent,
        Good,
        Acceptable,
        Poor
    }
}
