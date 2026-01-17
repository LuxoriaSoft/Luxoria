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

        // Check if this comparison has valid data in both sessions
        public bool HasValidTimeComparison => SessionA_AvgMs > 0 && SessionB_AvgMs > 0;
        // For memory, only compare when both have positive values (actual allocations)
        // Negative values mean memory was freed (GC) which makes percentage comparisons meaningless
        public bool HasValidMemoryComparison => SessionA_AvgMemory > 0 && SessionB_AvgMemory > 0;

        // Delta calculations (B - A: negative means B is faster)
        public double DeltaAvgMs => SessionB_AvgMs - SessionA_AvgMs;
        public double DeltaP95Ms => SessionB_P95Ms - SessionA_P95Ms;
        public long DeltaMemory => SessionB_AvgMemory - SessionA_AvgMemory;

        // Percentage improvements (positive = B is better/faster, negative = B is worse/slower)
        // Formula: (A - B) / A * 100
        // If B < A (B is faster), result is positive (improvement)
        // If B > A (B is slower), result is negative (regression)
        public double ImprovementPercent => HasValidTimeComparison
            ? ((SessionA_AvgMs - SessionB_AvgMs) / SessionA_AvgMs) * 100
            : 0;

        public double MemoryImprovementPercent => HasValidMemoryComparison && SessionA_AvgMemory > 0
            ? ((SessionA_AvgMemory - SessionB_AvgMemory) / (double)SessionA_AvgMemory) * 100
            : 0;

        // Status helpers - only valid when both sessions have data
        // IsFaster = true when B is faster than A (DeltaAvgMs < 0 means B - A < 0 means B < A)
        public bool IsFaster => HasValidTimeComparison && DeltaAvgMs < 0;
        public bool IsSlower => HasValidTimeComparison && DeltaAvgMs > 0;
        public bool UsesLessMemory => HasValidMemoryComparison && DeltaMemory < 0;

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

        // Only count operations that have valid data in BOTH sessions
        public List<ComparisonResult> ValidResults => Results.Where(r => r.HasValidTimeComparison).ToList();

        public int TotalOperations => Results.Count;
        public int ValidOperations => ValidResults.Count;

        // Count improved/regressed only for operations with valid data in both sessions
        public int ImprovedCount => ValidResults.Count(r => r.IsFaster);
        public int RegressedCount => ValidResults.Count(r => r.IsSlower);
        public int UnchangedCount => ValidResults.Count(r => !r.IsFaster && !r.IsSlower);

        // Categorized counts (only for valid comparisons)
        public int UXImprovedCount => ValidResults.Count(r => r.IsUXMetric && r.IsFaster);
        public int UXRegressedCount => ValidResults.Count(r => r.IsUXMetric && r.IsSlower);
        public int RenderImprovedCount => ValidResults.Count(r => r.IsRenderMetric && r.IsFaster);
        public int RenderRegressedCount => ValidResults.Count(r => r.IsRenderMetric && r.IsSlower);

        // Total time for all valid operations
        public double TotalTimeA => ValidResults.Sum(r => r.SessionA_AvgMs);
        public double TotalTimeB => ValidResults.Sum(r => r.SessionB_AvgMs);

        // Speed ratio: how many times faster/slower is B compared to A
        // > 1.0 means B is faster (e.g., 2.0 = B is 2x faster)
        // < 1.0 means B is slower (e.g., 0.5 = B is 2x slower)
        // This is symmetric: if A/B gives 2.0, then B/A gives 0.5
        public double SpeedRatio => TotalTimeB > 0 ? TotalTimeA / TotalTimeB : 1.0;

        // Overall improvement as percentage (clamped to reasonable range)
        // Positive = B is faster, Negative = B is slower
        // Capped at +/- 99% to avoid confusing values like -358%
        public double OverallImprovementPercent
        {
            get
            {
                if (TotalTimeA <= 0 || TotalTimeB <= 0) return 0;

                // Use ratio-based calculation
                // If B is faster: (1 - B/A) * 100 -> e.g., B=80, A=100 -> (1 - 0.8) * 100 = +20%
                // If B is slower: (1 - B/A) * 100 -> e.g., B=100, A=80 -> (1 - 1.25) * 100 = -25%
                double ratio = TotalTimeB / TotalTimeA;
                double improvement = (1.0 - ratio) * 100.0;

                // Cap at +/- 95% for display (ratio still available for precise info)
                return Math.Max(-95, Math.Min(95, improvement));
            }
        }

        // Human-readable speed description
        public string SpeedDescription
        {
            get
            {
                if (TotalTimeA <= 0 || TotalTimeB <= 0) return "N/A";

                double ratio = SpeedRatio;
                if (Math.Abs(ratio - 1.0) < 0.01) return "Same speed";

                if (ratio > 1.0)
                    return $"{ratio:F2}x faster";
                else
                    return $"{1.0 / ratio:F2}x slower";
            }
        }

        // Total memory for all valid operations
        public long TotalMemoryA => Results.Where(r => r.HasValidMemoryComparison).Sum(r => r.SessionA_AvgMemory);
        public long TotalMemoryB => Results.Where(r => r.HasValidMemoryComparison).Sum(r => r.SessionB_AvgMemory);

        // Memory ratio
        public double MemoryRatio => TotalMemoryB > 0 ? (double)TotalMemoryA / TotalMemoryB : 1.0;

        // Overall memory improvement (capped)
        public double OverallMemoryImprovementPercent
        {
            get
            {
                if (TotalMemoryA <= 0 || TotalMemoryB <= 0) return 0;
                double ratio = (double)TotalMemoryB / TotalMemoryA;
                double improvement = (1.0 - ratio) * 100.0;
                return Math.Max(-95, Math.Min(95, improvement));
            }
        }
    }
}
