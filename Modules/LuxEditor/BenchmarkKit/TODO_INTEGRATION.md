# BenchmarkKit v2.0.0 - Integration Guide

## Overview

This kit contains everything needed to add standardized performance benchmarking to LuxEditor.
**IMPORTANT**: Both old and new versions MUST use identical operation names for meaningful comparisons.

---

## Contents

```
BenchmarkKit/
├── Services/
│   ├── PerformanceMetrics.cs    # Core metrics tracking (v2.0.0)
│   └── StressTestRunner.cs      # Standardized test scenarios (v2.0.0)
├── docs/
│   ├── BENCHMARK_README.md      # Full documentation with operation names
│   ├── BENCHMARK_REPORT.md      # Report template
│   └── BENCHMARK_QUICKSTART.md  # Quick start guide
├── TODO_INTEGRATION.md          # THIS FILE
└── CodeSnippets.md              # Code snippets for integration
```

---

## Standard Operation Names (MUST USE THESE)

| Constant | Value | Description |
|----------|-------|-------------|
| `BenchmarkOps.RENDER_COMPLETE` | `Render:Complete` | Full render pipeline (PRIMARY metric) |
| `BenchmarkOps.RENDER_PREVIEW` | `Render:PreviewPass` | Preview pass rendering |
| `BenchmarkOps.RENDER_FULL` | `Render:FullPass` | Full resolution rendering |
| `BenchmarkOps.RENDER_FILTERS` | `Render:ApplyFilters` | Filter application phase |
| `BenchmarkOps.RENDER_LAYERS` | `Render:LayerComposite` | Layer compositing |

---

## Integration Steps

### Step 1: Copy Services

Copy files to `LuxEditor/Services/`:
```
BenchmarkKit/Services/PerformanceMetrics.cs  →  LuxEditor/Services/
BenchmarkKit/Services/StressTestRunner.cs    →  LuxEditor/Services/
```

### Step 2: Update Editor.xaml.cs Measurement Points

Replace any existing measurement calls with standardized names:

```csharp
// At the start of your render pipeline method:
using var pipelineTimer = _perfMetrics.Measure(BenchmarkOps.RENDER_COMPLETE, new Dictionary<string, object>
{
    ["ImageName"] = _currentImageName,
    ["Width"] = _currentImageWidth,
    ["Height"] = _currentImageHeight
});

// For preview pass:
using (_perfMetrics.Measure(BenchmarkOps.RENDER_PREVIEW))
{
    // preview rendering code
}

// For full pass:
using (_perfMetrics.Measure(BenchmarkOps.RENDER_FULL))
{
    // full resolution rendering code
}

// For filter application:
using (_perfMetrics.Measure(BenchmarkOps.RENDER_FILTERS))
{
    // filter application code
}

// For layer compositing:
using (_perfMetrics.Measure(BenchmarkOps.RENDER_LAYERS))
{
    // layer compositing code
}
```

### Step 3: Add Benchmark UI (Optional)

Add benchmark section to Editor.xaml:

```xml
<Expander Header="Benchmark" IsExpanded="False" Padding="15">
    <StackPanel Orientation="Vertical" Spacing="10">
        <TextBox x:Name="ImageNameInput" PlaceholderText="Image name..."/>
        <Button x:Name="RunBenchmarkButton"
                Content="Run Benchmark"
                Click="RunBenchmarkButton_Click"
                Background="#0078D4" Foreground="White"/>
        <TextBlock x:Name="BenchmarkStatusLabel" Text="Ready" Foreground="Gray"/>
    </StackPanel>
</Expander>
```

### Step 4: Add Benchmark Handler

```csharp
private StressTestRunner? _stressTestRunner;

private async void RunBenchmarkButton_Click(object sender, RoutedEventArgs e)
{
    if (_stressTestRunner == null)
    {
        _stressTestRunner = new StressTestRunner(DispatcherQueue);
        _stressTestRunner.SetSliderCache(_sliderCache);
        _stressTestRunner.OnLogMessage += (msg) => Debug.WriteLine(msg);
    }

    var imageName = ImageNameInput.Text;
    if (string.IsNullOrEmpty(imageName)) imageName = "unknown";

    BenchmarkStatusLabel.Text = "Running...";
    RunBenchmarkButton.IsEnabled = false;

    await _stressTestRunner.RunAllScenariosAsync(imageName);

    BenchmarkStatusLabel.Text = "Complete! Check Desktop/LuxEditor_Benchmarks/";
    RunBenchmarkButton.IsEnabled = true;
}
```

---

## Test Scenarios (8 Standardized Tests)

| ID | Name | Iterations | Delay | Purpose |
|----|------|------------|-------|---------|
| `Test:ExposureSweep` | Exposure Full Sweep | 20 | 100ms | Baseline test |
| `Test:ContrastSweep` | Contrast Full Sweep | 20 | 100ms | Contrast performance |
| `Test:RapidMovement` | Rapid Slider Movement | 50 | 20ms | Stress test |
| `Test:WhiteBalance` | White Balance Sweep | 20 | 100ms | Color matrix perf |
| `Test:ToneControls` | Tone Controls Sweep | 20 | 100ms | Tone mapping |
| `Test:PresenceControls` | Presence Controls Sweep | 20 | 100ms | Color enhancement |
| `Test:FullStress` | Full Parameter Stress | 20 | 100ms | All filters active |
| `Test:Reset` | Reset All Sliders | 1 | 0ms | Reset performance |

---

## Export Location

Results are exported to:
```
Desktop/LuxEditor_Benchmarks/{image_name}/
├── session_YYYYMMDD_HHMMSS_full.json   # Complete data
├── session_YYYYMMDD_HHMMSS_stats.json  # Statistics only
└── session_YYYYMMDD_HHMMSS_samples.csv # CSV for analysis
```

---

## Comparing Old vs New Versions

1. Copy this BenchmarkKit to BOTH versions
2. Use the SAME test image
3. Run benchmarks on the SAME machine
4. Compare `Render:Complete` averages between versions
5. Use LuxBenchmark dashboard module for visual comparison

---

## Troubleshooting

**Q: Operations have different names between versions**
A: This BenchmarkKit v2.0.0 standardizes names. Copy these files to both versions.

**Q: StressTestRunner can't find sliders**
A: Ensure `SetSliderCache()` is called with your slider dictionary. Slider keys must match: "Exposure", "Contrast", "Highlights", "Shadows", "Temperature", "Tint", "Vibrance", "Saturation", etc.

**Q: Build errors with EditorSlider**
A: The old version may use different slider types. Adapt `StressTestRunner.SetSliderValue()` to use your slider API.
