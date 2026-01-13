# LuxEditor Benchmark System - Quick Start Guide

## Overview

The LuxEditor benchmark system provides comprehensive performance tracking for the photo retouching module. All metrics are automatically collected during normal use and can be exported for analysis.

## Files Created

```
LuxEditor/
├── Services/
│   ├── PerformanceMetrics.cs   # Core metrics tracking service
│   └── StressTestRunner.cs     # Automated stress testing
├── Components/
│   ├── Editor.xaml.cs          # Instrumented with detailed metrics
│   ├── PhotoViewer.xaml.cs     # Instrumented for display performance
│   └── CollectionExplorer.xaml.cs  # Instrumented for thumbnail loading
└── docs/
    ├── BENCHMARK_REPORT.md     # Complete documentation template
    └── BENCHMARK_QUICKSTART.md # This file
```

## Quick Usage

### 1. Automatic Metrics Collection

Simply run the application and use the sliders. Metrics are automatically logged to the debug console:

```
[10:30:45.123] [PERF] ProcessImage:Complete | 156.78ms | Mem: 10.0MB
        └─ FrameNumber: 42
        └─ TotalPixels: 540000
```

### 2. Export Benchmark Data

From any code that has access to the Editor component:

```csharp
// Export all collected data
editor.ExportBenchmarkData();
```

Data is saved to: `Desktop/LuxEditor_Benchmarks/`
- `session_YYYYMMDD_HHMMSS_full.json` - Complete data
- `session_YYYYMMDD_HHMMSS_stats.json` - Statistics only
- `session_YYYYMMDD_HHMMSS_samples.csv` - CSV for analysis

### 3. Run Automated Stress Tests

```csharp
// Run all predefined stress test scenarios
editor.RunStressTests();
```

This runs 9 test scenarios:
1. Exposure Full Sweep
2. Contrast Full Sweep
3. Rapid Exposure Oscillation
4. Temperature + Tint Combined
5. All Sliders Sequential
6. Highlights + Shadows Sweep
7. Maximum Stress (No Delay)
8. Saturation Full Range
9. Reset All Sliders

### 4. Start a New Session

```csharp
// Reset and start fresh benchmark session
editor.ResetBenchmark("My test description");
```

### 5. View Summary Report

```csharp
// Print summary to console
editor.Metrics.PrintSummary();
```

Output:
```
╔═══════════════════════════════════════════════════════════════════╗
║                     BENCHMARK SUMMARY REPORT                       ║
╠═══════════════════════════════════════════════════════════════════╣
║  OPERATION                 │ COUNT │  AVG  │  MAX  │  P95  │ MEM  ║
╠════════════════════════════╪═══════╪═══════╪═══════╪═══════╪══════╣
║  ProcessImage:CPU_Pass     │   42  │ 142ms │ 198ms │ 175ms │ 8MB  ║
║  ProcessImage:GPU_Pass     │   42  │  12ms │  18ms │  16ms │ 2MB  ║
╚═══════════════════════════════════════════════════════════════════╝
```

## Tracked Metrics

### ProcessImage Pipeline
| Phase | Description |
|-------|-------------|
| `ProcessImage:Complete` | Total time for one slider adjustment |
| `ProcessImage:GPU_Pass` | SkiaSharp filter application time |
| `ProcessImage:CPU_Pass` | Pixel-by-pixel processing time |
| `ParameterCollection` | Slider value reading time |
| `GPU_BitmapAllocation` | First pass bitmap creation |
| `GPU_FilterComposition` | Filter chain creation |
| `GPU_DrawBitmap` | Canvas draw operation |
| `CPU_BitmapAllocation` | Final bitmap creation |
| `CPU_PixelProcessing` | Actual pixel loop time |
| `EventDispatch` | Event notification time |

### PhotoViewer
| Phase | Description |
|-------|-------------|
| `PhotoViewer:SetImage` | Total display update time |
| `PngEncoding` | SKBitmap to PNG encoding |
| `WriteableBitmapCreate` | WinUI bitmap allocation |
| `SetSource` | Data transfer to UI |
| `PhotoViewer:PanMove` | Pan interaction latency |

### CollectionExplorer
| Phase | Description |
|-------|-------------|
| `CollectionExplorer:SetBitmaps` | Total collection load time |
| `CreateLowResBitmap` | Thumbnail generation |
| `AdjustImageSizes` | UI size recalculation |
| `ImageSelection` | Image tap response time |

## Key Performance Indicators

| KPI | Good | Acceptable | Poor |
|-----|------|------------|------|
| ProcessImage | <16ms (60fps) | <33ms (30fps) | >100ms |
| Memory/Frame | <1MB | <5MB | >10MB |
| Pixels/ms | >100K | >50K | <10K |

## Color Coding in Console

- **Green**: Excellent (<16.67ms / 60 FPS threshold)
- **Yellow**: Acceptable (<33.33ms / 30 FPS threshold)
- **Orange**: Slow (<100ms)
- **Red**: Very slow (>100ms)

## Data Export Formats

### JSON Sample
```json
{
  "timestamp": "2026-01-13T10:30:45.123Z",
  "operationName": "ProcessImage",
  "phase": "Complete",
  "durationMs": 156.78,
  "memoryDelta": 10485760,
  "metadata": {
    "frameNumber": 42,
    "pixelsPerMs": 3443.5
  }
}
```

### CSV Headers
```
Timestamp,OperationName,Phase,DurationMs,MemoryBefore,MemoryAfter,MemoryDelta,Gen0,Gen1,Gen2
```

## Running a Benchmark Test Session

1. Build LuxEditor in Debug mode
2. Launch the application
3. Load a test collection
4. Select an image
5. Perform slider adjustments
6. Run stress tests: `editor.RunStressTests()`
7. Export results: `editor.ExportBenchmarkData()`
8. Analyze data in `Desktop/LuxEditor_Benchmarks/`

## Next Steps

After collecting baseline data:
1. Identify bottlenecks in `BENCHMARK_REPORT.md`
2. Implement optimizations
3. Run same tests on optimized version
4. Compare results in Dashboard section
