/*
 * StressTestRunner.cs - Standardized Stress Testing for LuxEditor
 *
 * VERSION: 2.0.0
 *
 * IMPORTANT: This file must use identical test scenarios in both old and new versions
 * for meaningful benchmark comparisons. See BENCHMARK_README.md for documentation.
 *
 * Test Scenarios (must be identical across versions):
 * 1. ExposureSweep    - Single slider sweep (baseline test)
 * 2. ContrastSweep    - Contrast slider sweep
 * 3. RapidMovement    - Rapid oscillation stress test
 * 4. WhiteBalance     - Temperature + Tint combined
 * 5. ToneControls     - Highlights + Shadows combined
 * 6. PresenceControls - Vibrance + Saturation combined
 * 7. FullStress       - All parameters active
 * 8. Reset            - Reset to defaults
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace LuxEditor.Services
{
    /// <summary>
    /// Test scenario definition with documentation.
    /// </summary>
    public class BenchmarkScenario
    {
        /// <summary>Unique scenario ID (e.g., "Test:ExposureSweep")</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Human-readable name</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>What this test measures</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Number of iterations to run</summary>
        public int Iterations { get; set; } = 20;

        /// <summary>Delay between iterations in ms (allows render to complete)</summary>
        public int DelayMs { get; set; } = 100;

        /// <summary>The test action to execute on each iteration</summary>
        public Action<int>? TestAction { get; set; }
    }

    /// <summary>
    /// Results from a benchmark scenario run.
    /// </summary>
    public class BenchmarkResult
    {
        public string ScenarioId { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public int TotalIterations { get; set; }
        public double TotalDurationMs { get; set; }
        public double AvgIterationMs { get; set; }
        public double MinIterationMs { get; set; }
        public double MaxIterationMs { get; set; }
        public long MemoryBefore { get; set; }
        public long MemoryAfter { get; set; }
        public long MemoryDelta { get; set; }
        public int GCCollections { get; set; }
        public bool Completed { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Standardized stress test runner for benchmarking LuxEditor performance.
    /// Runs identical test scenarios in both old and new versions.
    /// </summary>
    public class StressTestRunner
    {
        private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly List<BenchmarkResult> _results = new();

        // Slider references (set via SetSliders method)
        private Slider? _exposureSlider;
        private Slider? _contrastSlider;
        private Slider? _highlightsSlider;
        private Slider? _shadowsSlider;
        private Slider? _temperatureSlider;
        private Slider? _tintSlider;
        private Slider? _saturationSlider;

        // ═══════════════════════════════════════════════════════════════════
        // SLIDER VALUE RANGES - Must be identical across versions
        // These are the LOGICAL values, not the raw slider values
        // ═══════════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, (double min, double max, double def)> SliderRanges = new()
        {
            // Note: Old version uses slider values * 1000 for some sliders
            ["Exposure"] = (-5000, 5000, 0),      // Slider range (divide by 1000 for actual)
            ["Contrast"] = (-1000, 1000, 0),      // Slider range (divide by 1000 for actual)
            ["Highlights"] = (-1000, 1000, 0),    // Slider range (divide by 1000 for actual)
            ["Shadows"] = (-1000, 1000, 0),       // Slider range (divide by 1000 for actual)
            ["Temperature"] = (-100, 100, 0),     // Direct value
            ["Tint"] = (-100, 100, 0),            // Direct value
            ["Saturation"] = (-100, 100, 0),      // Direct value
        };

        public event Action<string>? OnLogMessage;
        public event Action<BenchmarkResult>? OnScenarioCompleted;
        public event Action<List<BenchmarkResult>>? OnAllScenariosCompleted;

        // Backward-compatible event names
        public event Action<BenchmarkResult>? OnTestCompleted
        {
            add => OnScenarioCompleted += value;
            remove => OnScenarioCompleted -= value;
        }
        public event Action<List<BenchmarkResult>>? OnAllTestsCompleted
        {
            add => OnAllScenariosCompleted += value;
            remove => OnAllScenariosCompleted -= value;
        }

        public StressTestRunner(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        /// <summary>
        /// Sets the slider references for stress testing.
        /// </summary>
        public void SetSliders(
            Slider exposure,
            Slider contrast,
            Slider highlights,
            Slider shadows,
            Slider temperature,
            Slider tint,
            Slider saturation)
        {
            _exposureSlider = exposure;
            _contrastSlider = contrast;
            _highlightsSlider = highlights;
            _shadowsSlider = shadows;
            _temperatureSlider = temperature;
            _tintSlider = tint;
            _saturationSlider = saturation;
        }

        /// <summary>
        /// Runs all standardized benchmark scenarios.
        /// </summary>
        public async Task RunAllScenariosAsync(string imageName = "")
        {
            if (_exposureSlider == null)
            {
                Log("[ERROR] Sliders not set. Call SetSliders first.");
                return;
            }

            _results.Clear();
            _metrics.SetImageInfo(imageName, 0, 0);
            _metrics.StartNewSession($"Benchmark Suite - {imageName}");

            Log("═══════════════════════════════════════════════════════════════════");
            Log("[BENCHMARK] Starting Standardized Benchmark Suite v2.0.0");
            Log("[BENCHMARK] See BENCHMARK_README.md for test documentation");
            Log("═══════════════════════════════════════════════════════════════════");

            var scenarios = GetStandardizedScenarios();

            foreach (var scenario in scenarios)
            {
                Log($"\n[SCENARIO] {scenario.Name}");
                Log($"[SCENARIO] {scenario.Description}");
                Log($"[SCENARIO] Iterations: {scenario.Iterations}, Delay: {scenario.DelayMs}ms");

                var result = await RunScenarioAsync(scenario);
                _results.Add(result);
                OnScenarioCompleted?.Invoke(result);

                // Pause between scenarios to stabilize
                await Task.Delay(500);
            }

            Log("\n═══════════════════════════════════════════════════════════════════");
            Log("[BENCHMARK] All Scenarios Completed");
            Log("═══════════════════════════════════════════════════════════════════");

            PrintResultsSummary();
            _metrics.PrintSummary();
            _metrics.ExportToJson();

            OnAllScenariosCompleted?.Invoke(_results);
        }

        /// <summary>
        /// Backward-compatible method name.
        /// </summary>
        public async Task RunAllTestsAsync()
        {
            await RunAllScenariosAsync();
        }

        /// <summary>
        /// Runs a single benchmark scenario.
        /// </summary>
        public async Task<BenchmarkResult> RunScenarioAsync(BenchmarkScenario scenario)
        {
            var result = new BenchmarkResult
            {
                ScenarioId = scenario.Id,
                ScenarioName = scenario.Name,
                TotalIterations = scenario.Iterations
            };

            var iterationTimes = new List<double>();
            var masterStopwatch = Stopwatch.StartNew();

            result.MemoryBefore = GC.GetTotalMemory(false);
            int gcBefore = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);

            try
            {
                for (int i = 0; i < scenario.Iterations; i++)
                {
                    var iterationStopwatch = Stopwatch.StartNew();

                    // Execute test action on UI thread
                    var tcs = new TaskCompletionSource<bool>();
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            scenario.TestAction?.Invoke(i);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    });

                    await tcs.Task;

                    iterationStopwatch.Stop();
                    iterationTimes.Add(iterationStopwatch.Elapsed.TotalMilliseconds);

                    // Delay between iterations (allows render to complete)
                    if (scenario.DelayMs > 0)
                    {
                        await Task.Delay(scenario.DelayMs);
                    }
                }

                masterStopwatch.Stop();
                result.Completed = true;
            }
            catch (Exception ex)
            {
                masterStopwatch.Stop();
                result.Completed = false;
                result.ErrorMessage = ex.Message;
                Log($"[ERROR] Scenario failed: {ex.Message}");
            }

            result.MemoryAfter = GC.GetTotalMemory(false);
            result.MemoryDelta = result.MemoryAfter - result.MemoryBefore;
            int gcAfter = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
            result.GCCollections = gcAfter - gcBefore;

            result.TotalDurationMs = masterStopwatch.Elapsed.TotalMilliseconds;
            if (iterationTimes.Count > 0)
            {
                result.AvgIterationMs = iterationTimes.Average();
                result.MinIterationMs = iterationTimes.Min();
                result.MaxIterationMs = iterationTimes.Max();
            }

            Log($"[RESULT] {scenario.Name}: Avg={result.AvgIterationMs:F2}ms, " +
                $"Min={result.MinIterationMs:F2}ms, Max={result.MaxIterationMs:F2}ms, " +
                $"Memory={FormatBytes(result.MemoryDelta)}");

            return result;
        }

        /// <summary>
        /// Gets the standardized benchmark scenarios.
        /// IMPORTANT: These scenarios MUST be identical in old and new versions.
        /// </summary>
        private List<BenchmarkScenario> GetStandardizedScenarios()
        {
            return new List<BenchmarkScenario>
            {
                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 1: Exposure Sweep (Baseline Test)
                // Purpose: Measure baseline rendering performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_EXPOSURE_SWEEP,
                    Name = "Exposure Full Sweep",
                    Description = "Sweep exposure from min to max in 20 steps. " +
                                 "Measures: Render:Complete. " +
                                 "This is the PRIMARY baseline comparison test.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        if (_exposureSlider != null)
                        {
                            double value = _exposureSlider.Minimum +
                                ((_exposureSlider.Maximum - _exposureSlider.Minimum) * i / 19.0);
                            _exposureSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 2: Contrast Sweep
                // Purpose: Measure contrast filter performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_CONTRAST_SWEEP,
                    Name = "Contrast Full Sweep",
                    Description = "Sweep contrast from min to max in 20 steps. " +
                                 "Measures contrast processing performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        if (_contrastSlider != null)
                        {
                            double value = _contrastSlider.Minimum +
                                ((_contrastSlider.Maximum - _contrastSlider.Minimum) * i / 19.0);
                            _contrastSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 3: Rapid Movement (Stress Test)
                // Purpose: Test cancellation handling and UI responsiveness
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_RAPID_MOVEMENT,
                    Name = "Rapid Slider Movement",
                    Description = "Oscillate exposure between extremes, 50 times with 20ms delay. " +
                                 "Tests render cancellation and UI thread responsiveness.",
                    Iterations = 50,
                    DelayMs = 20,
                    TestAction = (i) =>
                    {
                        if (_exposureSlider != null)
                        {
                            // Oscillate between -3000 and +3000 (scaled values)
                            _exposureSlider.Value = (i % 2 == 0) ? -3000 : 3000;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 4: White Balance
                // Purpose: Measure color temperature calculation performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_WHITEBALANCE,
                    Name = "White Balance Sweep",
                    Description = "Sweep Temperature and Tint together from min to max. " +
                                 "Tests complex color matrix calculations.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        double progress = i / 19.0;

                        if (_temperatureSlider != null)
                        {
                            double value = _temperatureSlider.Minimum +
                                ((_temperatureSlider.Maximum - _temperatureSlider.Minimum) * progress);
                            _temperatureSlider.Value = value;
                        }

                        if (_tintSlider != null)
                        {
                            double value = _tintSlider.Minimum +
                                ((_tintSlider.Maximum - _tintSlider.Minimum) * progress);
                            _tintSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 5: Tone Controls
                // Purpose: Measure highlight/shadow recovery performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_TONE_CONTROLS,
                    Name = "Tone Controls Sweep",
                    Description = "Sweep Highlights and Shadows together from min to max. " +
                                 "Tests tone mapping performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        double progress = i / 19.0;

                        if (_highlightsSlider != null)
                        {
                            double value = _highlightsSlider.Minimum +
                                ((_highlightsSlider.Maximum - _highlightsSlider.Minimum) * progress);
                            _highlightsSlider.Value = value;
                        }

                        if (_shadowsSlider != null)
                        {
                            double value = _shadowsSlider.Minimum +
                                ((_shadowsSlider.Maximum - _shadowsSlider.Minimum) * progress);
                            _shadowsSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 6: Presence Controls
                // Purpose: Measure saturation performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_PRESENCE,
                    Name = "Presence Controls Sweep",
                    Description = "Sweep Saturation from min to max. " +
                                 "Tests color enhancement performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        double progress = i / 19.0;

                        if (_saturationSlider != null)
                        {
                            double value = _saturationSlider.Minimum +
                                ((_saturationSlider.Maximum - _saturationSlider.Minimum) * progress);
                            _saturationSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 7: Full Stress Test
                // Purpose: Measure performance with all filters active
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_FULL_STRESS,
                    Name = "Full Parameter Stress",
                    Description = "Set all sliders to non-default values, then sweep exposure. " +
                                 "Tests full filter stack performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        // On first iteration, set all sliders to mid-range values
                        if (i == 0)
                        {
                            if (_contrastSlider != null) _contrastSlider.Value = 500;
                            if (_highlightsSlider != null) _highlightsSlider.Value = 500;
                            if (_shadowsSlider != null) _shadowsSlider.Value = -500;
                            if (_temperatureSlider != null) _temperatureSlider.Value = 25;
                            if (_tintSlider != null) _tintSlider.Value = 20;
                            if (_saturationSlider != null) _saturationSlider.Value = 30;
                        }

                        // Sweep exposure with all other filters active
                        if (_exposureSlider != null)
                        {
                            double value = _exposureSlider.Minimum +
                                ((_exposureSlider.Maximum - _exposureSlider.Minimum) * i / 19.0);
                            _exposureSlider.Value = value;
                        }
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 8: Reset to Default
                // Purpose: Measure reset/clear performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_RESET,
                    Name = "Reset All Sliders",
                    Description = "Reset all sliders to their default values. " +
                                 "Tests parameter reset performance.",
                    Iterations = 1,
                    DelayMs = 0,
                    TestAction = (i) =>
                    {
                        if (_exposureSlider != null) _exposureSlider.Value = 0;
                        if (_contrastSlider != null) _contrastSlider.Value = 0;
                        if (_highlightsSlider != null) _highlightsSlider.Value = 0;
                        if (_shadowsSlider != null) _shadowsSlider.Value = 0;
                        if (_temperatureSlider != null) _temperatureSlider.Value = 0;
                        if (_tintSlider != null) _tintSlider.Value = 0;
                        if (_saturationSlider != null) _saturationSlider.Value = 0;
                    }
                }
            };
        }

        /// <summary>
        /// Prints a summary of all benchmark results.
        /// </summary>
        private void PrintResultsSummary()
        {
            Log("\n╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Log("║                         BENCHMARK RESULTS SUMMARY                                     ║");
            Log("╠═══════════════════════════════════════════════════════════════════════════════════════╣");
            Log("║  SCENARIO                          │ ITERS │ AVG(ms) │ MIN(ms) │ MAX(ms) │ MEM Δ     ║");
            Log("╠════════════════════════════════════╪═══════╪═════════╪═════════╪═════════╪═══════════╣");

            foreach (var result in _results)
            {
                var name = result.ScenarioName.Length > 34
                    ? result.ScenarioName[..34]
                    : result.ScenarioName.PadRight(34);
                var memDelta = FormatBytes(result.MemoryDelta).PadLeft(9);
                var status = result.Completed ? "" : " [FAIL]";

                Log($"║  {name} │ {result.TotalIterations,5} │ {result.AvgIterationMs,6:F1}ms │ " +
                    $"{result.MinIterationMs,6:F1}ms │ {result.MaxIterationMs,6:F1}ms │ {memDelta} ║{status}");
            }

            Log("╚═══════════════════════════════════════════════════════════════════════════════════════╝");
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = Math.Abs(bytes);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            string sign = bytes >= 0 ? "+" : "-";
            return $"{sign}{len:F1}{sizes[order]}";
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
            OnLogMessage?.Invoke(message);
        }
    }
}
