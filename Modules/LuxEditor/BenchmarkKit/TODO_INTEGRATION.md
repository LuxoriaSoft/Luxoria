# BenchmarkKit - Integration Guide for New Luxoria Version

## Overview

This kit contains everything needed to add performance benchmarking to LuxEditor. Copy this folder to the new Luxoria version and follow the instructions below.

---

## Contents

```
BenchmarkKit/
├── Services/
│   ├── PerformanceMetrics.cs    # Core metrics tracking service
│   └── StressTestRunner.cs      # Automated stress test runner
├── docs/
│   ├── BENCHMARK_REPORT.md      # Full documentation template
│   └── BENCHMARK_QUICKSTART.md  # Quick start guide
├── TODO_INTEGRATION.md          # THIS FILE - Integration instructions
└── CodeSnippets.md              # Code to add to existing files
```

---

## TODO: Integration Steps

### Step 1: Copy Services

Copy the `Services/` folder to `LuxEditor/Services/` in the new version:
```
BenchmarkKit/Services/PerformanceMetrics.cs  →  LuxEditor/Services/PerformanceMetrics.cs
BenchmarkKit/Services/StressTestRunner.cs    →  LuxEditor/Services/StressTestRunner.cs
```

### Step 2: Copy Documentation

Copy docs to appropriate location:
```
BenchmarkKit/docs/BENCHMARK_REPORT.md      →  LuxEditor/docs/BENCHMARK_REPORT.md
BenchmarkKit/docs/BENCHMARK_QUICKSTART.md  →  LuxEditor/docs/BENCHMARK_QUICKSTART.md
```

### Step 3: Modify Editor.xaml

Add the Benchmark section to the Editor XAML. Add this **after the last Expander** (probably Color Adjustments):

```xml
<!-- BENCHMARK SECTION -->
<Expander Header="Benchmark"
          IsExpanded="False"
          Padding="15"
          Margin="0"
          BorderThickness="0"
          HorizontalAlignment="Stretch"
          HorizontalContentAlignment="Stretch">

    <StackPanel Orientation="Vertical" Spacing="10" Padding="0"
                HorizontalAlignment="Stretch">

        <!-- Image Name Input -->
        <StackPanel Orientation="Vertical" Spacing="2">
            <TextBlock Text="Image Name"
                       Foreground="Gray"
                       FontSize="10"/>
            <TextBox x:Name="ImageNameInput"
                     PlaceholderText="Enter image name..."
                     HorizontalAlignment="Stretch"/>
        </StackPanel>

        <!-- Image Size Info -->
        <TextBlock x:Name="CurrentImageSizeLabel"
                   Text="No image selected"
                   Foreground="Gray"
                   FontSize="10"/>

        <!-- Run Benchmark Button -->
        <Button x:Name="RunBenchmarkButton"
                Content="Run Benchmark"
                Click="RunBenchmarkButton_Click"
                HorizontalAlignment="Stretch"
                Background="#0078D4"
                Foreground="White"/>

        <!-- Status Label -->
        <TextBlock x:Name="BenchmarkStatusLabel"
                   Text="Ready"
                   Foreground="Gray"
                   FontSize="10"
                   TextWrapping="Wrap"/>

        <!-- Progress Ring (hidden by default) -->
        <ProgressRing x:Name="BenchmarkProgressRing"
                      IsActive="False"
                      Width="30"
                      Height="30"
                      Visibility="Collapsed"/>

    </StackPanel>
</Expander>
```

### Step 4: Modify Editor.xaml.cs

See `CodeSnippets.md` for all the code to add to Editor.xaml.cs.

Summary of changes:
1. Add using statements
2. Add private fields for benchmark tracking
3. Modify `SetOriginalBitmap()` to track image info
4. Instrument `ProcessImage()` with metrics
5. Add `RunBenchmarkButton_Click()` handler
6. Add helper methods

### Step 5: Modify PhotoViewer.xaml.cs

See `CodeSnippets.md` for instrumentation code.

Summary:
1. Add using statement for `LuxEditor.Services`
2. Add `PerformanceMetrics` instance
3. Instrument `SetImage()` method with timing

### Step 6: Modify CollectionExplorer.xaml.cs

See `CodeSnippets.md` for instrumentation code.

Summary:
1. Add using statements
2. Add `PerformanceMetrics` instance
3. Instrument `SetBitmaps()` and `CreateLowResBitmap()` methods

### Step 7: Test Build

Build the project and fix any compilation errors. Common issues:
- Missing using statements
- Namespace differences in new version
- UI element name changes

---

## Key Metrics Tracked

| Component | Metrics |
|-----------|---------|
| **Editor.ProcessImage** | GPU pass time, CPU pass time, memory allocation, pixels/ms |
| **PhotoViewer.SetImage** | PNG encoding time, WriteableBitmap creation, total latency |
| **CollectionExplorer** | Thumbnail generation time, memory per thumbnail |

---

## Export Location

Benchmark results are exported to:
```
Desktop/LuxEditor_Benchmarks/
├── session_YYYYMMDD_HHMMSS_full.json   # Complete data
├── session_YYYYMMDD_HHMMSS_stats.json  # Statistics only
└── session_YYYYMMDD_HHMMSS_samples.csv # CSV for analysis
```

---

## Notes for New Version

- If `ProcessImage()` architecture changed significantly, adapt the instrumentation points
- If sliders were renamed or added/removed, update `StressTestRunner.cs`
- The services are standalone and should work with any WinUI 3 app
- Namespace in services is `LuxEditor.Services` - change if needed
