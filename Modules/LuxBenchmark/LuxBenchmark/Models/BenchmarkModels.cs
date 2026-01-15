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
    /// UX-focused summary metrics for quick comparison (v3.0.0).
    /// </summary>
    public class UXSummary
    {
        public double AvgTimeToFirstPaintMs { get; set; }
        public double AvgPerceivedLatencyMs { get; set; }
        public double AvgInteractionReadyMs { get; set; }
        public double AvgTotalProcessingMs { get; set; }
        public double P95TimeToFirstPaintMs { get; set; }
        public double P95PerceivedLatencyMs { get; set; }
        public double FrameConsistencyScore { get; set; }
        public long PeakMemoryBytes { get; set; }
        public long AvgMemoryDeltaBytes { get; set; }
        public int TotalGCCollections { get; set; }
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
        public string BenchmarkKitVersion { get; set; } = string.Empty;
        public string LuxEditorVersion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Image info (from v2.0.0+)
        public string ImageName { get; set; } = string.Empty;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public SystemInfo? SystemInfo { get; set; }
        public List<MetricSample> Samples { get; set; } = new();
        public Dictionary<string, OperationStats> Statistics { get; set; } = new();

        // UX Summary metrics (v3.0.0)
        public UXSummary? UXMetrics { get; set; }

        // Computed properties
        public TimeSpan Duration => EndTime - StartTime;
        public int TotalSamples => Samples.Count;
        public string DisplayName => !string.IsNullOrEmpty(LuxEditorVersion)
            ? $"{LuxEditorVersion} - {(string.IsNullOrEmpty(Description) ? SessionId : Description)}"
            : (string.IsNullOrEmpty(Description) ? SessionId : Description);

        /// <summary>
        /// Gets test scenario names from statistics (keys starting with "Test:")
        /// </summary>
        public List<string> TestScenarios => Statistics.Keys
            .Where(k => k.StartsWith("Test:"))
            .Select(k => k.Replace("Test:", ""))
            .ToList();

        /// <summary>
        /// Gets render operation names from statistics (keys starting with "Render:")
        /// </summary>
        public List<string> RenderOperations => Statistics.Keys
            .Where(k => k.StartsWith("Render:"))
            .ToList();

        /// <summary>
        /// Gets UX operation names from statistics (keys starting with "UX:")
        /// </summary>
        public List<string> UXOperations => Statistics.Keys
            .Where(k => k.StartsWith("UX:"))
            .ToList();

        /// <summary>
        /// Returns true if this session has UX metrics (v3.0.0+)
        /// </summary>
        public bool HasUXMetrics => UXMetrics != null || UXOperations.Count > 0;

        /// <summary>
        /// Image resolution as string (e.g., "4000x3000")
        /// </summary>
        public string ImageResolution => ImageWidth > 0 && ImageHeight > 0
            ? $"{ImageWidth}x{ImageHeight}"
            : "Unknown";

        /// <summary>
        /// Megapixel count
        /// </summary>
        public double Megapixels => ImageWidth > 0 && ImageHeight > 0
            ? (ImageWidth * ImageHeight) / 1_000_000.0
            : 0;
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

        // Percentage improvements (positive = better, negative = worse)
        public double ImprovementPercent => SessionA_AvgMs > 0
            ? ((SessionA_AvgMs - SessionB_AvgMs) / SessionA_AvgMs) * 100
            : 0;

        public double MemoryImprovementPercent => SessionA_AvgMemory > 0
            ? ((SessionA_AvgMemory - SessionB_AvgMemory) / (double)SessionA_AvgMemory) * 100
            : 0;

        // Status helpers
        public bool IsFaster => DeltaAvgMs < 0;
        public bool UsesLessMemory => DeltaMemory < 0;

        // Operation type helpers
        public bool IsUXMetric => OperationKey.StartsWith("UX:");
        public bool IsRenderMetric => OperationKey.StartsWith("Render:");
        public bool IsTestMetric => OperationKey.StartsWith("Test:");
    }

    /// <summary>
    /// UX-focused comparison between two sessions.
    /// </summary>
    public class UXComparison
    {
        // Time to first paint comparison
        public double SessionA_TimeToFirstPaint { get; set; }
        public double SessionB_TimeToFirstPaint { get; set; }
        public double TimeToFirstPaintImprovement => SessionA_TimeToFirstPaint > 0
            ? ((SessionA_TimeToFirstPaint - SessionB_TimeToFirstPaint) / SessionA_TimeToFirstPaint) * 100
            : 0;

        // Perceived latency comparison
        public double SessionA_PerceivedLatency { get; set; }
        public double SessionB_PerceivedLatency { get; set; }
        public double PerceivedLatencyImprovement => SessionA_PerceivedLatency > 0
            ? ((SessionA_PerceivedLatency - SessionB_PerceivedLatency) / SessionA_PerceivedLatency) * 100
            : 0;

        // Frame consistency comparison
        public double SessionA_FrameConsistency { get; set; }
        public double SessionB_FrameConsistency { get; set; }
        public double FrameConsistencyImprovement => SessionB_FrameConsistency - SessionA_FrameConsistency;

        // Memory comparison
        public long SessionA_PeakMemory { get; set; }
        public long SessionB_PeakMemory { get; set; }
        public double PeakMemoryImprovement => SessionA_PeakMemory > 0
            ? ((SessionA_PeakMemory - SessionB_PeakMemory) / (double)SessionA_PeakMemory) * 100
            : 0;

        // GC comparison
        public int SessionA_GCCollections { get; set; }
        public int SessionB_GCCollections { get; set; }
        public double GCCollectionsImprovement => SessionA_GCCollections > 0
            ? ((SessionA_GCCollections - SessionB_GCCollections) / (double)SessionA_GCCollections) * 100
            : 0;

        // Overall UX score (weighted average of improvements)
        public double OverallUXImprovement =>
            (TimeToFirstPaintImprovement * 0.4) +   // Most important for UX
            (PerceivedLatencyImprovement * 0.3) +   // Second most important
            (FrameConsistencyImprovement * 0.2) +   // Smoothness matters
            (PeakMemoryImprovement * 0.1);          // Memory efficiency
    }

    /// <summary>
    /// Summary of a comparison between two sessions.
    /// </summary>
    public class ComparisonSummary
    {
        public BenchmarkSession SessionA { get; set; } = new();
        public BenchmarkSession SessionB { get; set; } = new();
        public List<ComparisonResult> Results { get; set; } = new();

        // UX-focused comparison (v3.0.0+)
        public UXComparison? UXComparison { get; set; }

        public int TotalOperations => Results.Count;
        public int ImprovedCount => Results.FindAll(r => r.IsFaster).Count;
        public int RegressedCount => Results.FindAll(r => !r.IsFaster).Count;

        // Categorized counts
        public int UXImprovedCount => Results.FindAll(r => r.IsUXMetric && r.IsFaster).Count;
        public int UXRegressedCount => Results.FindAll(r => r.IsUXMetric && !r.IsFaster).Count;
        public int RenderImprovedCount => Results.FindAll(r => r.IsRenderMetric && r.IsFaster).Count;
        public int RenderRegressedCount => Results.FindAll(r => r.IsRenderMetric && !r.IsFaster).Count;

        public double OverallImprovementPercent => Results.Count > 0
            ? Results.Average(r => r.ImprovementPercent)
            : 0;

        // UX-weighted improvement (prioritizes UX metrics)
        public double UXWeightedImprovementPercent
        {
            get
            {
                var uxResults = Results.Where(r => r.IsUXMetric).ToList();
                var renderResults = Results.Where(r => r.IsRenderMetric).ToList();

                if (uxResults.Count == 0 && renderResults.Count == 0) return 0;

                // Weight UX metrics more heavily (60% UX, 40% Render)
                var uxAvg = uxResults.Count > 0 ? uxResults.Average(r => r.ImprovementPercent) : 0;
                var renderAvg = renderResults.Count > 0 ? renderResults.Average(r => r.ImprovementPercent) : 0;

                if (uxResults.Count == 0) return renderAvg;
                if (renderResults.Count == 0) return uxAvg;

                return (uxAvg * 0.6) + (renderAvg * 0.4);
            }
        }
    }
}
