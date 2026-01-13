# Code Snippets for Benchmark Integration

This file contains all the code snippets to add to existing files in the new Luxoria version.

---

## 1. Editor.xaml.cs

### 1.1 Add Using Statements

Add at the top of the file:
```csharp
using System.Threading.Tasks;
using LuxEditor.Services;
```

### 1.2 Add Private Fields

Add inside the `Editor` class, after existing field declarations:
```csharp
private string _currentImageName = string.Empty;
private bool _isBenchmarkRunning = false;

// Performance tracking
private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
private readonly Stopwatch _sliderDebounceTimer = new();
private int _processImageCallCount = 0;
private int _frameNumber = 0;
private DateTime _lastSliderChange = DateTime.MinValue;

// Track slider values for delta logging
private Dictionary<string, double> _lastSliderValues = new()
{
    { "Exposure", 0 },
    { "Contrast", 0 },
    { "Highlights", 0 },
    { "Shadows", 0 },
    { "Temperature", 0 },
    { "Tint", 0 },
    { "Saturation", 0 }
};
```

### 1.3 Add in Constructor

Add at end of constructor:
```csharp
_metrics.Log("Editor component initialized", "INFO");
```

### 1.4 Modify SetOriginalBitmap

Replace or modify the existing `SetOriginalBitmap` method:
```csharp
/// <summary>
/// Sets the original SKBitmap that we will process.
/// </summary>
public void SetOriginalBitmap(SKBitmap bitmap)
{
    using (_metrics.MeasureOperation("Editor", "SetOriginalBitmap", new Dictionary<string, object>
    {
        { "Width", bitmap?.Width ?? 0 },
        { "Height", bitmap?.Height ?? 0 },
        { "PixelCount", (bitmap?.Width ?? 0) * (bitmap?.Height ?? 0) },
        { "ByteCount", bitmap?.ByteCount ?? 0 }
    }))
    {
        _originalBitmap = bitmap;
        _processImageCallCount = 0;
        _frameNumber = 0;

        if (bitmap != null)
        {
            _metrics.Log($"New image loaded: {bitmap.Width}x{bitmap.Height} ({bitmap.ByteCount / 1024.0 / 1024.0:F2} MB)", "IMAGE");
            _metrics.LogMemorySnapshot("After SetOriginalBitmap");

            // Update UI labels
            if (CurrentImageSizeLabel != null)
            {
                int pixelCount = bitmap.Width * bitmap.Height;
                double sizeMB = bitmap.ByteCount / (1024.0 * 1024.0);
                CurrentImageSizeLabel.Text = $"{bitmap.Width}x{bitmap.Height} ({pixelCount:N0} px, {sizeMB:F2} MB)";
            }
        }
        else
        {
            if (CurrentImageSizeLabel != null)
                CurrentImageSizeLabel.Text = "No image selected";
        }
    }
}
```

### 1.5 Add Slider Change Logger

Add this helper method:
```csharp
/// <summary>
/// Logs slider value changes with timing information.
/// </summary>
private void LogSliderChange(string sliderName, double oldValue, double newValue)
{
    var now = DateTime.Now;
    var timeSinceLastChange = (now - _lastSliderChange).TotalMilliseconds;
    _lastSliderChange = now;

    _metrics.Log($"{sliderName}: {oldValue:F3} → {newValue:F3} | Delta: {newValue - oldValue:F3} | TimeSinceLast: {timeSinceLastChange:F1}ms", "SLIDER");
}
```

### 1.6 Modify Each Slider Handler

For each slider handler (Exposure, Contrast, etc.), add logging. Example for ExposureSlider:
```csharp
private void ExposureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
{
    LogSliderChange("Exposure", e.OldValue / 1000, e.NewValue / 1000);
    if (ExposureValueLabel != null)
        ExposureValueLabel.Text = (e.NewValue / 1000).ToString("F2");
    _lastSliderValues["Exposure"] = e.NewValue / 1000;
    ProcessImage();
}
```

### 1.7 Instrument ProcessImage Method

This is the main instrumentation. Wrap the entire ProcessImage body with timing:

```csharp
private void ProcessImage()
{
    if (_originalBitmap == null)
        return;

    _processImageCallCount++;
    _frameNumber++;

    // Master timer for entire ProcessImage operation
    var masterStopwatch = Stopwatch.StartNew();
    var phaseTimings = new Dictionary<string, double>();

    // Collect all slider values for metadata
    var sliderValues = new Dictionary<string, object>
    {
        { "FrameNumber", _frameNumber },
        { "CallCount", _processImageCallCount },
        { "ImageWidth", _originalBitmap.Width },
        { "ImageHeight", _originalBitmap.Height },
        { "TotalPixels", _originalBitmap.Width * _originalBitmap.Height }
    };

    // Memory before processing
    long memoryBeforeProcessing = GC.GetTotalMemory(false);

    // ============================================
    // YOUR EXISTING PROCESSING CODE HERE
    // Add Stopwatch measurements around each phase
    // ============================================

    // Example for GPU pass:
    var gpuPassStopwatch = Stopwatch.StartNew();
    // ... GPU processing code ...
    gpuPassStopwatch.Stop();
    phaseTimings["GPU_TotalPass"] = gpuPassStopwatch.Elapsed.TotalMilliseconds;

    // Example for CPU pass:
    var cpuPassStopwatch = Stopwatch.StartNew();
    // ... CPU pixel processing code ...
    cpuPassStopwatch.Stop();
    phaseTimings["CPU_TotalPass"] = cpuPassStopwatch.Elapsed.TotalMilliseconds;

    // ============================================

    masterStopwatch.Stop();
    long memoryAfterProcessing = GC.GetTotalMemory(false);
    long totalMemoryAllocated = memoryAfterProcessing - memoryBeforeProcessing;

    // Add timing data
    sliderValues["TotalMemoryAllocated_Bytes"] = totalMemoryAllocated;
    sliderValues["TotalMemoryAllocated_MB"] = totalMemoryAllocated / (1024.0 * 1024.0);

    foreach (var timing in phaseTimings)
    {
        sliderValues[$"Timing_{timing.Key}_Ms"] = timing.Value;
    }

    // Record metric
    _metrics.RecordSample(new MetricSample
    {
        Timestamp = DateTime.UtcNow,
        OperationName = "ProcessImage",
        Phase = "Complete",
        DurationMs = masterStopwatch.Elapsed.TotalMilliseconds,
        MemoryBefore = memoryBeforeProcessing,
        MemoryAfter = memoryAfterProcessing,
        MemoryDelta = totalMemoryAllocated,
        Gen0Collections = GC.CollectionCount(0),
        Gen1Collections = GC.CollectionCount(1),
        Gen2Collections = GC.CollectionCount(2),
        Metadata = sliderValues
    });

    // Log frame summary
    var fps = 1000.0 / masterStopwatch.Elapsed.TotalMilliseconds;
    Debug.WriteLine($"[FRAME {_frameNumber}] ProcessImage: {masterStopwatch.Elapsed.TotalMilliseconds:F2}ms | FPS: {fps:F1}");
}
```

### 1.8 Add Benchmark Button Handler

Add this method:
```csharp
/// <summary>
/// Handler for the Run Benchmark button click.
/// </summary>
private async void RunBenchmarkButton_Click(object sender, RoutedEventArgs e)
{
    if (_originalBitmap == null)
    {
        BenchmarkStatusLabel.Text = "Error: No image selected. Please select an image first.";
        return;
    }

    // Get image name from input
    string imageName = ImageNameInput.Text?.Trim() ?? "";
    if (string.IsNullOrEmpty(imageName))
    {
        imageName = $"Image_{_originalBitmap.Width}x{_originalBitmap.Height}";
    }
    _currentImageName = imageName;

    if (_isBenchmarkRunning)
    {
        BenchmarkStatusLabel.Text = "Benchmark already running...";
        return;
    }

    _isBenchmarkRunning = true;

    // Update UI state
    RunBenchmarkButton.IsEnabled = false;
    BenchmarkProgressRing.IsActive = true;
    BenchmarkProgressRing.Visibility = Visibility.Visible;

    // Create session with image name
    string sessionDescription = $"Benchmark: {imageName} ({_originalBitmap.Width}x{_originalBitmap.Height})";
    _metrics.StartNewSession(sessionDescription);

    BenchmarkStatusLabel.Text = $"Running benchmark on: {imageName}...";

    try
    {
        var runner = GetStressTestRunner();

        // Subscribe to log messages for status updates
        int completedScenarios = 0;
        runner.OnTestCompleted += (result) =>
        {
            completedScenarios++;
            DispatcherQueue.TryEnqueue(() =>
            {
                BenchmarkStatusLabel.Text = $"Completed: {result.ScenarioName} ({completedScenarios}/9)";
            });
        };

        runner.OnLogMessage += (msg) => Debug.WriteLine(msg);

        // Run all tests
        await runner.RunAllTestsAsync();

        // Export results
        string exportPath = _metrics.ExportToJson();

        // Update status with completion info
        var stats = _metrics.GetStatistics();
        int totalSamples = 0;
        double avgProcessTime = 0;

        if (stats.TryGetValue("ProcessImage:Complete", out var processStats))
        {
            totalSamples = processStats.SampleCount;
            avgProcessTime = processStats.AvgMs;
        }

        BenchmarkStatusLabel.Text = $"Benchmark complete!\n" +
            $"Image: {_currentImageName}\n" +
            $"Samples: {totalSamples}\n" +
            $"Avg ProcessImage: {avgProcessTime:F2}ms\n" +
            $"Exported to Desktop/LuxEditor_Benchmarks/";

        _metrics.Log($"Benchmark completed for {_currentImageName}", "INFO");
    }
    catch (Exception ex)
    {
        BenchmarkStatusLabel.Text = $"Error: {ex.Message}";
        _metrics.Log($"Benchmark failed: {ex.Message}", "ERROR");
    }
    finally
    {
        // Restore UI state
        _isBenchmarkRunning = false;
        RunBenchmarkButton.IsEnabled = true;
        BenchmarkProgressRing.IsActive = false;
        BenchmarkProgressRing.Visibility = Visibility.Collapsed;
    }
}
```

### 1.9 Add Stress Test Runner Helper

Add these methods:
```csharp
/// <summary>
/// Gets the stress test runner configured with this editor's sliders.
/// </summary>
public StressTestRunner GetStressTestRunner()
{
    var runner = new StressTestRunner(DispatcherQueue);
    runner.SetSliders(
        ExposureSlider,
        ContrastSlider,
        HighlightsSlider,
        ShadowsSlider,
        TemperatureSlider,
        TintSlider,
        SaturationSlider
    );
    return runner;
}

/// <summary>
/// Exports benchmark data and prints summary.
/// </summary>
public void ExportBenchmarkData()
{
    _metrics.PrintSummary();
    _metrics.ExportToJson();
}

/// <summary>
/// Resets benchmark session for a new test run.
/// </summary>
public void ResetBenchmark(string description = "")
{
    _metrics.StartNewSession(description);
    _processImageCallCount = 0;
    _frameNumber = 0;
}

/// <summary>
/// Gets read-only access to the current metrics instance.
/// </summary>
public PerformanceMetrics Metrics => _metrics;
```

---

## 2. PhotoViewer.xaml.cs

### 2.1 Add Using Statement
```csharp
using LuxEditor.Services;
```

### 2.2 Add Field
```csharp
private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
private int _setImageCallCount = 0;
```

### 2.3 Instrument SetImage Method

```csharp
public void SetImage(SKBitmap bitmap)
{
    _setImageCallCount++;
    var masterStopwatch = Stopwatch.StartNew();
    var phaseTimings = new Dictionary<string, double>();

    long memoryBefore = GC.GetTotalMemory(false);

    using (MemoryStream ms = new MemoryStream())
    {
        // Phase 1: PNG Encoding
        var pngEncodeStopwatch = Stopwatch.StartNew();
        bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
        pngEncodeStopwatch.Stop();
        phaseTimings["PngEncoding"] = pngEncodeStopwatch.Elapsed.TotalMilliseconds;

        ms.Seek(0, SeekOrigin.Begin);

        // Phase 2: WriteableBitmap creation
        var bitmapCreateStopwatch = Stopwatch.StartNew();
        WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);
        bitmapCreateStopwatch.Stop();
        phaseTimings["WriteableBitmapCreate"] = bitmapCreateStopwatch.Elapsed.TotalMilliseconds;

        // Phase 3: SetSource
        var setSourceStopwatch = Stopwatch.StartNew();
        writeableBitmap.SetSource(ms.AsRandomAccessStream());
        setSourceStopwatch.Stop();
        phaseTimings["SetSource"] = setSourceStopwatch.Elapsed.TotalMilliseconds;

        DisplayImage.Source = writeableBitmap;
    }

    masterStopwatch.Stop();
    long memoryAfter = GC.GetTotalMemory(false);

    // Record metric
    _metrics.RecordSample(new MetricSample
    {
        Timestamp = DateTime.UtcNow,
        OperationName = "PhotoViewer",
        Phase = "SetImage",
        DurationMs = masterStopwatch.Elapsed.TotalMilliseconds,
        MemoryBefore = memoryBefore,
        MemoryAfter = memoryAfter,
        MemoryDelta = memoryAfter - memoryBefore,
        Metadata = new Dictionary<string, object>
        {
            { "CallCount", _setImageCallCount },
            { "ImageWidth", bitmap.Width },
            { "ImageHeight", bitmap.Height },
            { "Timing_PngEncoding_Ms", phaseTimings["PngEncoding"] },
            { "Timing_SetSource_Ms", phaseTimings["SetSource"] }
        }
    });

    Debug.WriteLine($"[PhotoViewer] SetImage #{_setImageCallCount}: {masterStopwatch.Elapsed.TotalMilliseconds:F2}ms");
}
```

---

## 3. CollectionExplorer.xaml.cs

### 3.1 Add Using Statements
```csharp
using System.Linq;
using LuxEditor.Services;
```

### 3.2 Add Fields
```csharp
private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
private int _thumbnailsGenerated = 0;
private long _totalThumbnailMemory = 0;
```

### 3.3 Instrument SetBitmaps Method

Add timing around the bitmap processing loop:
```csharp
public void SetBitmaps(List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> bitmaps)
{
    if (bitmaps == null || bitmaps.Count == 0) return;

    var masterStopwatch = Stopwatch.StartNew();
    long memoryBefore = GC.GetTotalMemory(false);
    _thumbnailsGenerated = 0;
    _totalThumbnailMemory = 0;

    _metrics.Log($"Loading collection with {bitmaps.Count} images", "IMAGE");

    // ... existing processing code ...

    masterStopwatch.Stop();
    long memoryAfter = GC.GetTotalMemory(false);

    _metrics.RecordSample(new MetricSample
    {
        Timestamp = DateTime.UtcNow,
        OperationName = "CollectionExplorer",
        Phase = "SetBitmaps",
        DurationMs = masterStopwatch.Elapsed.TotalMilliseconds,
        MemoryBefore = memoryBefore,
        MemoryAfter = memoryAfter,
        MemoryDelta = memoryAfter - memoryBefore,
        Metadata = new Dictionary<string, object>
        {
            { "ImageCount", bitmaps.Count },
            { "ThumbnailsGenerated", _thumbnailsGenerated },
            { "TotalThumbnailMemory_MB", _totalThumbnailMemory / (1024.0 * 1024.0) }
        }
    });
}
```

### 3.4 Instrument CreateLowResBitmap

```csharp
private SKBitmap CreateLowResBitmap(SKBitmap original, int targetWidth, int targetHeight)
{
    var resizeStopwatch = Stopwatch.StartNew();
    long memoryBefore = GC.GetTotalMemory(false);

    // ... existing resize code ...

    resizeStopwatch.Stop();
    long memoryAfter = GC.GetTotalMemory(false);

    _metrics.RecordSample(new MetricSample
    {
        Timestamp = DateTime.UtcNow,
        OperationName = "CollectionExplorer",
        Phase = "CreateLowResBitmap",
        DurationMs = resizeStopwatch.Elapsed.TotalMilliseconds,
        MemoryBefore = memoryBefore,
        MemoryAfter = memoryAfter,
        MemoryDelta = memoryAfter - memoryBefore,
        Metadata = new Dictionary<string, object>
        {
            { "OriginalSize", $"{original.Width}x{original.Height}" },
            { "TargetSize", $"{targetWidth}x{targetHeight}" }
        }
    });

    return resizedBitmap;
}
```

---

## 4. Adapt StressTestRunner Slider Names

If slider names changed in the new version, update `StressTestRunner.cs`:

```csharp
public void SetSliders(
    Slider exposure,      // Match your slider names
    Slider contrast,
    Slider highlights,
    Slider shadows,
    Slider temperature,
    Slider tint,
    Slider saturation)
{
    _exposureSlider = exposure;
    _contrastSlider = contrast;
    // ... etc
}
```

---

## Quick Checklist

- [ ] Copy `Services/PerformanceMetrics.cs`
- [ ] Copy `Services/StressTestRunner.cs`
- [ ] Copy documentation files
- [ ] Add benchmark UI to Editor.xaml
- [ ] Add using statements to all files
- [ ] Add fields to Editor.xaml.cs
- [ ] Instrument ProcessImage()
- [ ] Add RunBenchmarkButton_Click handler
- [ ] Instrument PhotoViewer.SetImage()
- [ ] Instrument CollectionExplorer.SetBitmaps()
- [ ] Update slider names in StressTestRunner if needed
- [ ] Build and test
