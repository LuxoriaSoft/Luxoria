using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxBenchmark.Models
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
        public Dictionary<string, object>? Metadata { get; set; }
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
    /// Complete benchmark session data.
    /// </summary>
    public class BenchmarkSession
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SystemInfo? SystemInfo { get; set; }
        public List<MetricSample> Samples { get; set; } = new();
        public Dictionary<string, OperationStats> Statistics { get; set; } = new();

        // Computed properties
        public TimeSpan Duration => EndTime - StartTime;
        public int TotalSamples => Samples.Count;
        public string DisplayName => string.IsNullOrEmpty(Description) ? SessionId : Description;
    }

    /// <summary>
    /// Result of comparing two benchmark sessions.
    /// </summary>
    public class ComparisonResult
    {
        public string OperationKey { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;

        // Session A (baseline/old)
        public double SessionA_AvgMs { get; set; }
        public double SessionA_P95Ms { get; set; }
        public long SessionA_AvgMemory { get; set; }
        public int SessionA_Samples { get; set; }

        // Session B (new)
        public double SessionB_AvgMs { get; set; }
        public double SessionB_P95Ms { get; set; }
        public long SessionB_AvgMemory { get; set; }
        public int SessionB_Samples { get; set; }

        // Delta calculations
        public double DeltaAvgMs => SessionB_AvgMs - SessionA_AvgMs;
        public double DeltaP95Ms => SessionB_P95Ms - SessionA_P95Ms;
        public long DeltaMemory => SessionB_AvgMemory - SessionA_AvgMemory;

        // Percentage improvements (negative = faster/better)
        public double ImprovementPercent => SessionA_AvgMs > 0
            ? ((SessionA_AvgMs - SessionB_AvgMs) / SessionA_AvgMs) * 100
            : 0;

        public double MemoryImprovementPercent => SessionA_AvgMemory > 0
            ? ((SessionA_AvgMemory - SessionB_AvgMemory) / (double)SessionA_AvgMemory) * 100
            : 0;

        // Status helpers
        public bool IsFaster => DeltaAvgMs < 0;
        public bool UsesLessMemory => DeltaMemory < 0;
    }

    /// <summary>
    /// Summary of a comparison between two sessions.
    /// </summary>
    public class ComparisonSummary
    {
        public BenchmarkSession SessionA { get; set; } = new();
        public BenchmarkSession SessionB { get; set; } = new();
        public List<ComparisonResult> Results { get; set; } = new();

        public int TotalOperations => Results.Count;
        public int ImprovedCount => Results.FindAll(r => r.IsFaster).Count;
        public int RegressedCount => Results.FindAll(r => !r.IsFaster).Count;

        public double OverallImprovementPercent => Results.Count > 0
            ? Results.Average(r => r.ImprovementPercent)
            : 0;
    }
}
