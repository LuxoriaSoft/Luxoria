/*
  LuxEditor - Image Processing Component with Performance Instrumentation

  This file contains comprehensive benchmarking metrics for:
  - Slider response time tracking
  - GPU pass (SkiaSharp filters) performance
  - CPU pass (pixel manipulation) performance
  - Memory allocation tracking
  - Frame timing analysis

  Metrics are exported in JSON/CSV format for dashboard integration.
*/

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LuxEditor.Services;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private SKBitmap _originalBitmap;
        private string _currentImageName = string.Empty;
        private bool _isBenchmarkRunning = false;

        // Performance tracking
        private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
        private readonly Stopwatch _sliderDebounceTimer = new();
        private int _processImageCallCount = 0;
        private int _frameNumber = 0;
        private DateTime _lastSliderChange = DateTime.MinValue;

        // UX Benchmark timing (v3.0.0)
        private readonly Stopwatch _uxTimer = new();
        private const string LUXEDITOR_VERSION = "1.0.0-old";

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

        public event Action<SKBitmap> OnEditorImageUpdated;

        public Editor()
        {
            this.InitializeComponent();

            // Set LuxEditor version for benchmark organization (v3.0.0)
            _metrics.LuxEditorVersion = LUXEDITOR_VERSION;

            _metrics.Log("Editor component initialized", "INFO");
        }

        /// <summary>
        /// Sets the original SKBitmap that we will process.
        /// </summary>
        public void SetOriginalBitmap(SKBitmap bitmap)
        {
            SetOriginalBitmap(bitmap, "Unknown");
        }

        /// <summary>
        /// Sets the original SKBitmap with image name for benchmark tracking.
        /// </summary>
        public void SetOriginalBitmap(SKBitmap bitmap, string imageName)
        {
            using (_metrics.MeasureOperation("Editor", "SetOriginalBitmap", new Dictionary<string, object>
            {
                { "Width", bitmap?.Width ?? 0 },
                { "Height", bitmap?.Height ?? 0 },
                { "PixelCount", (bitmap?.Width ?? 0) * (bitmap?.Height ?? 0) },
                { "ByteCount", bitmap?.ByteCount ?? 0 },
                { "ImageName", imageName }
            }))
            {
                _originalBitmap = bitmap;
                _currentImageName = imageName;
                _processImageCallCount = 0;
                _frameNumber = 0;

                if (bitmap != null)
                {
                    _metrics.Log($"New image loaded: {imageName} - {bitmap.Width}x{bitmap.Height} ({bitmap.ByteCount / 1024.0 / 1024.0:F2} MB)", "IMAGE");
                    _metrics.LogMemorySnapshot("After SetOriginalBitmap");

                    // Update UI labels
                    UpdateImageInfoLabels(bitmap, imageName);
                }
                else
                {
                    // Clear UI labels
                    if (CurrentImageSizeLabel != null)
                        CurrentImageSizeLabel.Text = "No image selected";
                }
            }
        }

        /// <summary>
        /// Updates the benchmark UI with current image information.
        /// </summary>
        private void UpdateImageInfoLabels(SKBitmap bitmap, string imageName)
        {
            if (CurrentImageSizeLabel != null && bitmap != null)
            {
                int pixelCount = bitmap.Width * bitmap.Height;
                double sizeMB = bitmap.ByteCount / (1024.0 * 1024.0);
                CurrentImageSizeLabel.Text = $"{bitmap.Width}x{bitmap.Height} ({pixelCount:N0} px, {sizeMB:F2} MB)";
            }
        }

        // -----------------------
        // Slider Handlers with Instrumentation
        // -----------------------

        private void ExposureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Exposure", e.OldValue / 1000, e.NewValue / 1000);
            if (ExposureValueLabel != null)
                ExposureValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            _lastSliderValues["Exposure"] = e.NewValue / 1000;
            ProcessImage();
        }

        private void ContrastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Contrast", e.OldValue / 1000, e.NewValue / 1000);
            if (ContrastValueLabel != null)
                ContrastValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            _lastSliderValues["Contrast"] = e.NewValue / 1000;
            ProcessImage();
        }

        private void HighlightsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Highlights", e.OldValue / 1000, e.NewValue / 1000);
            if (HighlightsValueLabel != null)
                HighlightsValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            _lastSliderValues["Highlights"] = e.NewValue / 1000;
            ProcessImage();
        }

        private void ShadowsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Shadows", e.OldValue / 1000, e.NewValue / 1000);
            if (ShadowsValueLabel != null)
                ShadowsValueLabel.Text = (e.NewValue / 1000).ToString("F2");
            _lastSliderValues["Shadows"] = e.NewValue / 1000;
            ProcessImage();
        }

        private void TemperatureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Temperature", e.OldValue, e.NewValue);
            if (TemperatureValueLabel != null)
                TemperatureValueLabel.Text = e.NewValue.ToString("F0");
            _lastSliderValues["Temperature"] = e.NewValue;
            ProcessImage();
        }

        private void TintSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Tint", e.OldValue, e.NewValue);
            if (TintValueLabel != null)
                TintValueLabel.Text = e.NewValue.ToString("F0");
            _lastSliderValues["Tint"] = e.NewValue;
            ProcessImage();
        }

        private void SaturationSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // UX Benchmark: Mark input received (v3.0.0)
            _metrics.MarkInputReceived();
            _uxTimer.Restart();

            LogSliderChange("Saturation", e.OldValue, e.NewValue);
            if (SaturationValueLabel != null)
                SaturationValueLabel.Text = e.NewValue.ToString("F0");
            _lastSliderValues["Saturation"] = e.NewValue;
            ProcessImage();
        }

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

        /// <summary>
        /// Applies all adjustments to the original image with comprehensive performance tracking.
        /// </summary>
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

            // 1) Retrieve slider values - Phase: Parameter Collection
            var paramStopwatch = Stopwatch.StartNew();
            float exposure = (float)ExposureSlider.Value / 1000;
            float contrast = (float)ContrastSlider.Value / 1000;
            float highlights = (float)HighlightsSlider.Value / 1000;
            float shadows = (float)ShadowsSlider.Value / 1000;
            float temperature = (float)TemperatureSlider.Value;
            float tint = (float)TintSlider.Value;
            float saturation = (float)SaturationSlider.Value;
            paramStopwatch.Stop();
            phaseTimings["ParameterCollection"] = paramStopwatch.Elapsed.TotalMilliseconds;

            // Add slider values to metadata
            sliderValues["Exposure"] = exposure;
            sliderValues["Contrast"] = contrast;
            sliderValues["Highlights"] = highlights;
            sliderValues["Shadows"] = shadows;
            sliderValues["Temperature"] = temperature;
            sliderValues["Tint"] = tint;
            sliderValues["Saturation"] = saturation;

            // Memory before processing
            long memoryBeforeProcessing = GC.GetTotalMemory(false);

            // 2) First pass with color filters (GPU-accelerated via SkiaSharp)
            SKBitmap firstPassBitmap;
            var gpuPassStopwatch = Stopwatch.StartNew();
            {
                // Sub-phase: Bitmap allocation
                var allocStopwatch = Stopwatch.StartNew();
                firstPassBitmap = new SKBitmap(_originalBitmap.Width, _originalBitmap.Height);
                allocStopwatch.Stop();
                phaseTimings["GPU_BitmapAllocation"] = allocStopwatch.Elapsed.TotalMilliseconds;

                using (var canvas = new SKCanvas(firstPassBitmap))
                {
                    // Sub-phase: Canvas clear
                    var clearStopwatch = Stopwatch.StartNew();
                    canvas.Clear(SKColors.Transparent);
                    clearStopwatch.Stop();
                    phaseTimings["GPU_CanvasClear"] = clearStopwatch.Elapsed.TotalMilliseconds;

                    // Sub-phase: Filter creation - Exposure
                    var exposureFilterStopwatch = Stopwatch.StartNew();
                    float exposureScale = (float)Math.Pow(2, exposure);
                    var exposureFilter = SKColorFilter.CreateLighting(
                        new SKColor(
                            (byte)Math.Min(255, 255 * exposureScale),
                            (byte)Math.Min(255, 255 * exposureScale),
                            (byte)Math.Min(255, 255 * exposureScale)),
                        new SKColor(0, 0, 0));
                    exposureFilterStopwatch.Stop();
                    phaseTimings["GPU_ExposureFilterCreation"] = exposureFilterStopwatch.Elapsed.TotalMilliseconds;

                    // Sub-phase: Filter creation - Contrast
                    var contrastFilterStopwatch = Stopwatch.StartNew();
                    float contrastFactor = 1f + contrast;
                    float translate = 0.5f * (1f - contrastFactor);
                    float[] contrastMatrix =
                    {
                        contrastFactor, 0,             0,             0, translate,
                        0,             contrastFactor, 0,             0, translate,
                        0,             0,             contrastFactor, 0, translate,
                        0,             0,             0,             1, 0
                    };
                    var contrastFilter = SKColorFilter.CreateColorMatrix(contrastMatrix);
                    contrastFilterStopwatch.Stop();
                    phaseTimings["GPU_ContrastFilterCreation"] = contrastFilterStopwatch.Elapsed.TotalMilliseconds;

                    // Sub-phase: Filter creation - Saturation
                    var saturationFilterStopwatch = Stopwatch.StartNew();
                    float saturationFactor = 1f + (saturation / 100f);
                    float lumR = 0.3086f;
                    float lumG = 0.6094f;
                    float lumB = 0.0820f;
                    float oneMinusS = 1f - saturationFactor;
                    float r = (oneMinusS * lumR);
                    float g = (oneMinusS * lumG);
                    float b = (oneMinusS * lumB);
                    float[] saturationMatrix =
                    {
                        r + saturationFactor, g,                     b,                     0, 0,
                        r,                     g + saturationFactor, b,                     0, 0,
                        r,                     g,                     b + saturationFactor, 0, 0,
                        0,                     0,                     0,                     1, 0
                    };
                    var saturationFilter = SKColorFilter.CreateColorMatrix(saturationMatrix);
                    saturationFilterStopwatch.Stop();
                    phaseTimings["GPU_SaturationFilterCreation"] = saturationFilterStopwatch.Elapsed.TotalMilliseconds;

                    // Sub-phase: Filter composition
                    var composeStopwatch = Stopwatch.StartNew();
                    var contrastSaturation = SKColorFilter.CreateCompose(contrastFilter, saturationFilter);
                    var finalFilter = SKColorFilter.CreateCompose(exposureFilter, contrastSaturation);
                    composeStopwatch.Stop();
                    phaseTimings["GPU_FilterComposition"] = composeStopwatch.Elapsed.TotalMilliseconds;

                    // Sub-phase: Canvas draw with filters
                    var drawStopwatch = Stopwatch.StartNew();
                    using (var paint = new SKPaint())
                    {
                        paint.ColorFilter = finalFilter;
                        canvas.DrawBitmap(_originalBitmap, 0, 0, paint);
                    }
                    drawStopwatch.Stop();
                    phaseTimings["GPU_DrawBitmap"] = drawStopwatch.Elapsed.TotalMilliseconds;
                }
            }
            gpuPassStopwatch.Stop();
            phaseTimings["GPU_TotalPass"] = gpuPassStopwatch.Elapsed.TotalMilliseconds;

            // Memory after GPU pass
            long memoryAfterGPU = GC.GetTotalMemory(false);
            sliderValues["MemoryDeltaGPU_Bytes"] = memoryAfterGPU - memoryBeforeProcessing;

            // Record GPU pass as separate metric (standardized name: Render:FullPass)
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = BenchmarkOps.RENDER_FULL,
                Phase = "",
                DurationMs = gpuPassStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = memoryBeforeProcessing,
                MemoryAfter = memoryAfterGPU,
                MemoryDelta = memoryAfterGPU - memoryBeforeProcessing,
                Metadata = new Dictionary<string, object>
                {
                    { "FrameNumber", _frameNumber },
                    { "Exposure", exposure },
                    { "Contrast", contrast },
                    { "Saturation", saturation }
                }
            });

            // 3) Second pass (CPU) for highlights, shadows, temperature, and tint
            var cpuPassStopwatch = Stopwatch.StartNew();
            long memoryBeforeCPU = GC.GetTotalMemory(false);

            // Sub-phase: Final bitmap allocation
            var cpuAllocStopwatch = Stopwatch.StartNew();
            SKBitmap finalBitmap = new SKBitmap(firstPassBitmap.Width, firstPassBitmap.Height);
            cpuAllocStopwatch.Stop();
            phaseTimings["CPU_BitmapAllocation"] = cpuAllocStopwatch.Elapsed.TotalMilliseconds;

            // Sub-phase: Pixel processing
            var pixelProcessStopwatch = Stopwatch.StartNew();
            int pixelCount = 0;
            int highlightPixels = 0;
            int shadowPixels = 0;

            for (int y = 0; y < firstPassBitmap.Height; y++)
            {
                for (int x = 0; x < firstPassBitmap.Width; x++)
                {
                    pixelCount++;
                    uint pixel = (uint)firstPassBitmap.GetPixel(x, y);
                    byte alpha = (byte)((pixel >> 24) & 0xFF);
                    byte red = (byte)((pixel >> 16) & 0xFF);
                    byte green = (byte)((pixel >> 8) & 0xFF);
                    byte blue = (byte)(pixel & 0xFF);

                    float fr = red / 255f;
                    float fg = green / 255f;
                    float fb = blue / 255f;

                    // Highlights
                    float brightness = (fr + fg + fb) / 3f;
                    if (brightness > 0.5f)
                    {
                        highlightPixels++;
                        float factor = (brightness - 0.5f) * 2f;
                        float amount = highlights * factor;
                        fr = fr + amount * (1f - fr);
                        fg = fg + amount * (1f - fg);
                        fb = fb + amount * (1f - fb);
                    }

                    // Shadows
                    brightness = (fr + fg + fb) / 3f;
                    if (brightness < 0.5f)
                    {
                        shadowPixels++;
                        float factor = (0.5f - brightness) * 2f;
                        float amount = shadows * factor;
                        fr = fr + amount * (1f - fr);
                        fg = fg + amount * (1f - fg);
                        fb = fb + amount * (1f - fb);
                    }

                    // Temperature and tint
                    float tempFactor = temperature / 100f;
                    float tintFactor = tint / 100f;

                    // Simple scale for red and blue (temperature)
                    float rScale = 1f + (tempFactor * 0.3f);
                    float bScale = 1f - (tempFactor * 0.3f);
                    fr *= rScale;
                    fb *= bScale;

                    // Simple scale for green (tint)
                    float gScale = 1f + (tintFactor * 0.3f);
                    fg *= gScale;

                    // Optional partial compensation for red/blue with tint
                    float inverseTintScale = 1f - (Math.Abs(tintFactor) * 0.1f);
                    fr *= inverseTintScale;
                    fb *= inverseTintScale;

                    // Clamp
                    fr = Math.Clamp(fr, 0f, 1f);
                    fg = Math.Clamp(fg, 0f, 1f);
                    fb = Math.Clamp(fb, 0f, 1f);

                    // Convert to byte
                    byte nr = (byte)(fr * 255f);
                    byte ng = (byte)(fg * 255f);
                    byte nb = (byte)(fb * 255f);

                    uint newPixel =
                          ((uint)alpha << 24)
                        | ((uint)nr << 16)
                        | ((uint)ng << 8)
                        | (uint)nb;

                    finalBitmap.SetPixel(x, y, newPixel);
                }
            }
            pixelProcessStopwatch.Stop();
            phaseTimings["CPU_PixelProcessing"] = pixelProcessStopwatch.Elapsed.TotalMilliseconds;

            cpuPassStopwatch.Stop();
            phaseTimings["CPU_TotalPass"] = cpuPassStopwatch.Elapsed.TotalMilliseconds;

            // Memory after CPU pass
            long memoryAfterCPU = GC.GetTotalMemory(false);
            sliderValues["MemoryDeltaCPU_Bytes"] = memoryAfterCPU - memoryBeforeCPU;
            sliderValues["HighlightPixels"] = highlightPixels;
            sliderValues["ShadowPixels"] = shadowPixels;
            sliderValues["HighlightRatio"] = (double)highlightPixels / pixelCount;
            sliderValues["ShadowRatio"] = (double)shadowPixels / pixelCount;

            // Calculate pixels per millisecond for CPU pass
            double pixelsPerMs = pixelCount / phaseTimings["CPU_PixelProcessing"];
            sliderValues["PixelsPerMs"] = pixelsPerMs;

            // Record CPU pass as separate metric (standardized name: Render:ApplyFilters)
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = BenchmarkOps.RENDER_FILTERS,
                Phase = "",
                DurationMs = cpuPassStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = memoryBeforeCPU,
                MemoryAfter = memoryAfterCPU,
                MemoryDelta = memoryAfterCPU - memoryBeforeCPU,
                Metadata = new Dictionary<string, object>
                {
                    { "FrameNumber", _frameNumber },
                    { "Highlights", highlights },
                    { "Shadows", shadows },
                    { "Temperature", temperature },
                    { "Tint", tint },
                    { "PixelCount", pixelCount },
                    { "PixelsPerMs", pixelsPerMs }
                }
            });

            // 4) Send final bitmap - Phase: Event dispatch
            var dispatchStopwatch = Stopwatch.StartNew();
            OnEditorImageUpdated?.Invoke(finalBitmap);
            dispatchStopwatch.Stop();
            phaseTimings["EventDispatch"] = dispatchStopwatch.Elapsed.TotalMilliseconds;

            // Master timing complete
            masterStopwatch.Stop();
            phaseTimings["TotalProcessImage"] = masterStopwatch.Elapsed.TotalMilliseconds;

            // Calculate overhead (time not accounted for in sub-phases)
            double accountedTime = phaseTimings["ParameterCollection"] + phaseTimings["GPU_TotalPass"] + phaseTimings["CPU_TotalPass"] + phaseTimings["EventDispatch"];
            phaseTimings["Overhead"] = masterStopwatch.Elapsed.TotalMilliseconds - accountedTime;

            // Add all phase timings to slider values
            foreach (var timing in phaseTimings)
            {
                sliderValues[$"Timing_{timing.Key}_Ms"] = timing.Value;
            }

            // Memory summary
            long totalMemoryAllocated = memoryAfterCPU - memoryBeforeProcessing;
            sliderValues["TotalMemoryAllocated_Bytes"] = totalMemoryAllocated;
            sliderValues["TotalMemoryAllocated_MB"] = totalMemoryAllocated / (1024.0 * 1024.0);

            // Record master ProcessImage metric (standardized name: Render:Complete)
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = BenchmarkOps.RENDER_COMPLETE,
                Phase = "",
                DurationMs = masterStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = memoryBeforeProcessing,
                MemoryAfter = memoryAfterCPU,
                MemoryDelta = totalMemoryAllocated,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                Metadata = sliderValues
            });

            // UX Benchmark: Record UX metrics (v3.0.0)
            // OLD version doesn't have preview pass, so perceived latency = total processing time
            var uxTotalMs = _uxTimer.Elapsed.TotalMilliseconds;
            _metrics.RecordTimeToFirstPaint(uxTotalMs, "FullRender");
            _metrics.RecordPerceivedLatency(uxTotalMs, "NoPreview");
            _metrics.RecordInteractionReady(uxTotalMs, "AfterRender");
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = BenchmarkOps.UX_TOTAL_PROCESSING,
                DurationMs = uxTotalMs
            });

            // Log frame summary
            var fps = 1000.0 / masterStopwatch.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"═══════════════════════════════════════════════════════════════════");
            Debug.WriteLine($"[FRAME {_frameNumber}] ProcessImage Complete");
            Debug.WriteLine($"  Total: {masterStopwatch.Elapsed.TotalMilliseconds:F2}ms | Theoretical FPS: {fps:F1}");
            Debug.WriteLine($"  GPU Pass: {phaseTimings["GPU_TotalPass"]:F2}ms | CPU Pass: {phaseTimings["CPU_TotalPass"]:F2}ms");
            Debug.WriteLine($"  Memory Allocated: {totalMemoryAllocated / 1024.0 / 1024.0:F2} MB");
            Debug.WriteLine($"  Pixels Processed: {pixelCount:N0} | Rate: {pixelsPerMs:F0} px/ms");
            Debug.WriteLine($"═══════════════════════════════════════════════════════════════════");

            // Cleanup first pass bitmap
            firstPassBitmap.Dispose();
        }

        /// <summary>
        /// Exports benchmark data and prints summary.
        /// Called externally to save benchmark results.
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
        /// Runs all automated stress tests and exports results.
        /// </summary>
        public async void RunStressTests()
        {
            if (_originalBitmap == null)
            {
                _metrics.Log("Cannot run stress tests: No image loaded", "ERROR");
                return;
            }

            var runner = GetStressTestRunner();
            runner.OnLogMessage += (msg) => Debug.WriteLine(msg);
            runner.OnAllTestsCompleted += (results) =>
            {
                _metrics.Log($"Stress tests completed: {results.Count} scenarios", "INFO");
            };

            await runner.RunAllTestsAsync();
        }

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

                if (stats.TryGetValue(BenchmarkOps.RENDER_COMPLETE, out var processStats))
                {
                    totalSamples = processStats.SampleCount;
                    avgProcessTime = processStats.AvgMs;
                }

                BenchmarkStatusLabel.Text = $"Benchmark complete!\n" +
                    $"Image: {_currentImageName}\n" +
                    $"Samples: {totalSamples}\n" +
                    $"Avg Render:Complete: {avgProcessTime:F2}ms\n" +
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

        /// <summary>
        /// Gets the current image name.
        /// </summary>
        public string CurrentImageName => _currentImageName;

        /// <summary>
        /// Gets read-only access to the current metrics instance.
        /// </summary>
        public PerformanceMetrics Metrics => _metrics;
    }
}
