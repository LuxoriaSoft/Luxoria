# LuxEditor BenchmarkKit

## Overview

This BenchmarkKit provides standardized performance measurement for LuxEditor.
It enables meaningful comparisons between different versions of the application.

**IMPORTANT**: When comparing benchmarks between versions, both versions MUST use
the same operation names and test scenarios defined in this document.

---

## Operation Naming Convention

All operations follow this format: `Category:Operation`

### Categories

| Category | Description |
|----------|-------------|
| `Render` | Image rendering pipeline operations |
| `Filter` | Individual filter application |
| `UI` | User interface response times |
| `Memory` | Memory-related operations |

### Standard Operation Names

These names MUST be used consistently across all versions:

| Operation Name | Description | What It Measures |
|----------------|-------------|------------------|
| `Render:Complete` | Full render pipeline | Total time from slider change to image displayed |
| `Render:PreviewPass` | Preview image rendering | Time to render low-res preview |
| `Render:FullPass` | Full resolution rendering | Time to render full-res image |
| `Render:ApplyFilters` | Filter application phase | Time to apply all active filters |
| `Render:LayerComposite` | Layer compositing | Time to composite all layers |
| `Filter:Exposure` | Exposure filter | Individual filter timing |
| `Filter:Contrast` | Contrast filter | Individual filter timing |
| `Filter:WhiteBalance` | White balance filter | Individual filter timing |
| `Filter:ToneCurve` | Tone curve filter | Individual filter timing |
| `Filter:Blur` | Blur filter | Individual filter timing |
| `UI:SliderResponse` | Slider interaction | Time from user input to pipeline start |

---

## Test Scenarios

Each scenario tests a specific aspect of performance. Run ALL scenarios for a complete benchmark.

### Scenario 1: Single Slider Sweep (Exposure)
- **ID**: `Test:ExposureSweep`
- **Purpose**: Measure baseline rendering performance with single parameter change
- **Method**: Move Exposure slider from -5 to +5 in 20 steps
- **Delay**: 100ms between steps (allows render to complete)
- **Measures**: `Render:Complete`, `Render:FullPass`
- **Expected samples**: 20

### Scenario 2: Single Slider Sweep (Contrast)
- **ID**: `Test:ContrastSweep`
- **Purpose**: Measure contrast filter performance
- **Method**: Move Contrast slider from -1 to +1 in 20 steps
- **Delay**: 100ms between steps
- **Measures**: `Render:Complete`, `Render:FullPass`
- **Expected samples**: 20

### Scenario 3: Rapid Slider Movement
- **ID**: `Test:RapidMovement`
- **Purpose**: Stress test with rapid parameter changes
- **Method**: Oscillate Exposure between -3 and +3, 50 iterations
- **Delay**: 20ms between changes
- **Measures**: `Render:Complete`, cancellation handling
- **Expected samples**: Variable (many renders will be cancelled)

### Scenario 4: White Balance Adjustment
- **ID**: `Test:WhiteBalance`
- **Purpose**: Measure white balance performance (complex calculation)
- **Method**: Sweep Temperature (2000K-50000K) and Tint (-150 to +150) together
- **Delay**: 100ms between steps
- **Measures**: `Render:Complete`, `Filter:WhiteBalance`
- **Expected samples**: 20

### Scenario 5: Tone Controls Combined
- **ID**: `Test:ToneControls`
- **Purpose**: Measure multiple simultaneous parameter changes
- **Method**: Adjust Highlights and Shadows together from -100 to +100
- **Delay**: 100ms between steps
- **Measures**: `Render:Complete` with multiple active filters
- **Expected samples**: 20

### Scenario 6: Presence Controls
- **ID**: `Test:PresenceControls`
- **Purpose**: Measure texture/dehaze/vibrance performance
- **Method**: Sweep Vibrance and Saturation together
- **Delay**: 100ms between steps
- **Measures**: `Render:Complete`, color processing
- **Expected samples**: 20

### Scenario 7: Full Parameter Stress
- **ID**: `Test:FullStress`
- **Purpose**: Maximum stress with all parameters active
- **Method**: Set all sliders to non-default values, then sweep Exposure
- **Delay**: 100ms between steps
- **Measures**: `Render:Complete` with full filter stack
- **Expected samples**: 20

### Scenario 8: Reset to Default
- **ID**: `Test:Reset`
- **Purpose**: Measure reset/clear performance
- **Method**: Reset all sliders to default
- **Delay**: None
- **Measures**: `Render:Complete` with cleared filters
- **Expected samples**: 1

---

## How to Run Benchmarks

1. Load an image in LuxEditor
2. Open the Benchmark panel in the Editor
3. Enter a descriptive name for the image (e.g., "portrait_24mp")
4. Click "Run Benchmark"
5. Wait for all tests to complete
6. Results are exported to `Desktop/LuxEditor_Benchmarks/[image_name]/`

---

## Output Files

Each benchmark session creates:

| File | Description |
|------|-------------|
| `session_YYYYMMDD_HHMMSS_full.json` | Complete data with all samples |
| `session_YYYYMMDD_HHMMSS_stats.json` | Aggregated statistics only |
| `session_YYYYMMDD_HHMMSS_samples.csv` | Raw samples in CSV format |

---

## Comparing Results

When comparing two versions:

1. Use the **same image** (same resolution, same file)
2. Use the **same hardware** (same machine)
3. Run benchmarks in **similar conditions** (no other heavy apps running)
4. Compare matching operation names (e.g., `Render:Complete` vs `Render:Complete`)

Key metrics to compare:
- **AvgMs**: Average time (lower is better)
- **P95Ms**: 95th percentile (stability indicator)
- **MaxMs**: Worst case (spike detection)
- **MemoryDelta**: Memory consumption

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2024-01 | Standardized naming convention |
| 1.0.0 | 2024-01 | Initial benchmark system |

---

## Troubleshooting

**Q: Why are operation names different between versions?**
A: Ensure both versions use this BenchmarkKit. Copy `PerformanceMetrics.cs` and
`StressTestRunner.cs` to the old version and update measurement points in
`Editor.xaml.cs` to use the standard operation names.

**Q: Why do I get fewer samples than expected?**
A: Rapid tests may cancel pending renders. This is expected behavior.
Focus on `Render:Complete` samples that actually finished.

**Q: How do I add a new test scenario?**
A: Add it to `StressTestRunner.GetTestScenarios()` and document it here.
Use a unique `Test:` prefix for the scenario ID.
