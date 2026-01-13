# LuxEditor Benchmark System - Quick Start Guide (v2.0)

## Overview

The LuxEditor benchmark system provides comprehensive performance tracking for the photo retouching module. All metrics are automatically collected during normal use and can be exported for analysis.

## Files Created

```
LuxEditor/
+-- Services/
|   +-- PerformanceMetrics.cs   # Core metrics tracking service
|   +-- StressTestRunner.cs     # Automated stress testing (EditorSlider-based)
+-- Components/
|   +-- Editor.xaml.cs          # Instrumented RunPipelineAsync()
+-- docs/
    +-- BENCHMARK_REPORT.md     # Complete documentation template
    +-- BENCHMARK_QUICKSTART.md # This file
```

## Quick Usage

### 1. Using the Benchmark UI

1. Open a collection in LuxEditor
2. Select an image
3. Scroll down to the **Benchmark** section in the editor panel
4. Enter an image name (optional - auto-filled from filename)
5. Click **Run Benchmark**
6. Wait for stress tests to complete
7. Results are exported to `Desktop/LuxEditor_Benchmarks/`

### 2. Automatic Metrics Collection

Simply run the application and use the sliders. Metrics are automatically logged to the debug console:

```
[10:30:45.123] [PERF] Pipeline:Total | 45.78ms | Mem: 2.5MB
        +-- ImageName: test_image.jpg
        +-- Width: 1920
        +-- Height: 1080
```

### 3. Export Location

Data is saved to: `Desktop/LuxEditor_Benchmarks/`
- `session_YYYYMMDD_HHMMSS_full.json` - Complete data
- `session_YYYYMMDD_HHMMSS_stats.json` - Statistics only
- `session_YYYYMMDD_HHMMSS_samples.csv` - CSV for analysis

## Tracked Metrics

### Pipeline Metrics
| Phase | Description |
|-------|-------------|
| `Pipeline:Total` | Complete render cycle time |
| `Pipeline:ApplyFilters_preview` | Preview resolution filter application |
| `Pipeline:ApplyFilters_full` | Full resolution filter application |
| `Pipeline:LayerMask_preview` | Layer mask generation (preview) |
| `Pipeline:LayerMask_full` | Layer mask generation (full) |
| `Pipeline:PreviewPass` | Total preview render pass |
| `Pipeline:FullPass` | Total full quality render pass |

### Stress Test Scenarios
| Scenario | Description | Iterations |
|----------|-------------|------------|
| Exposure Full Sweep | Min to max exposure | 20 |
| Contrast Full Sweep | Min to max contrast | 20 |
| Rapid Oscillation | Quick back-and-forth | 50 |
| Temp + Tint Combined | Both at once | 20 |
| All Tone Sliders | Sequential test | 8 |
| Highlights + Shadows | Combined sweep | 20 |
| Maximum Stress | No delay, 100 iterations | 100 |
| Saturation Full Range | Min to max | 20 |
| Vibrance + Dehaze | Combined sweep | 20 |
| Reset All | Return to defaults | 1 |

## Key Performance Indicators

| KPI | Good | Acceptable | Poor |
|-----|------|------------|------|
| Pipeline:Total | <16ms (60fps) | <33ms (30fps) | >100ms |
| Memory/Frame | <1MB | <5MB | >10MB |
| GC Collections | 0 | 1-2 | >5 |

## Color Coding in Console

- **Green**: Excellent (<16.67ms / 60 FPS threshold)
- **Yellow**: Acceptable (<33.33ms / 30 FPS threshold)
- **Orange**: Slow (<100ms)
- **Red**: Very slow (>100ms)

## Programmatic Usage

```csharp
using LuxEditor.Services;

// Get singleton instance
var metrics = PerformanceMetrics.Instance;

// Start new session
metrics.StartNewSession("My test description");

// Get statistics
var stats = metrics.GetStatistics();

// Print summary to console
metrics.PrintSummary();

// Export to files
metrics.ExportToJson();
```

## Comparing Old vs New Version

After running benchmarks on both versions:

1. Copy `_stats.json` files from both versions
2. Compare key metrics:
   - `Pipeline:Total` vs old `ProcessImage:Complete`
   - Memory delta per operation
   - GC collections during stress tests
3. Update `BENCHMARK_REPORT.md` with comparison data

## Architecture Differences

| Aspect | Old Version | New Version |
|--------|-------------|-------------|
| Processing | `ProcessImage()` sync | `RunPipelineAsync()` async |
| Sliders | WinUI `Slider` controls | Custom `EditorSlider` |
| Categories | Flat structure | Categorized (WhiteBalance, Tone, Presence) |
| Layers | Not supported | Full layer system with masks |
| Cancellation | None | `CancellationToken` support |

## Running a Complete Benchmark Session

1. Build LuxEditor in Debug mode
2. Launch the application
3. Load a test collection with various image sizes
4. Select an image
5. Click **Run Benchmark** in the Benchmark panel
6. Wait for all 10 stress tests to complete
7. Analyze data in `Desktop/LuxEditor_Benchmarks/`
8. Compare with old version results

## Next Steps

After collecting benchmark data:
1. Review results in `BENCHMARK_REPORT.md`
2. Identify bottlenecks
3. Compare with old version metrics
4. Document performance differences
5. Plan optimizations if needed
