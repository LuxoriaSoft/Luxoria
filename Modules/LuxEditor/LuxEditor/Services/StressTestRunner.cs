/*
 * StressTestRunner.cs - Standardized Stress Testing for LuxEditor
 *
 * VERSION: 2.0.0
 *
 * IMPORTANT: This file must be identical in both old and new versions for
 * meaningful benchmark comparisons. See BENCHMARK_README.md for documentation.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LuxEditor.EditorUI.Controls;
using Microsoft.UI.Dispatching;

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

        // Slider cache reference (from Editor component)
        private ConcurrentDictionary<string, EditorSlider>? _sliderCache;

        // ═══════════════════════════════════════════════════════════════════
        // SLIDER VALUE RANGES - Must be identical across versions
        // ═══════════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, (float min, float max, float def)> SliderRanges = new()
        {
            ["Exposure"] = (-5f, 5f, 0f),
            ["Contrast"] = (-1f, 1f, 0f),
            ["Highlights"] = (-100f, 100f, 0f),
            ["Shadows"] = (-100f, 100f, 0f),
            ["Whites"] = (-100f, 100f, 0f),
            ["Blacks"] = (-100f, 100f, 0f),
            ["Temperature"] = (2000f, 50000f, 6500f),
            ["Tint"] = (-150f, 150f, 0f),
            ["Vibrance"] = (-100f, 100f, 0f),
            ["Saturation"] = (-100f, 100f, 0f),
            ["Texture"] = (-100f, 100f, 0f),
            ["Dehaze"] = (-100f, 100f, 0f),
        };

        public event Action<string>? OnLogMessage;
        public event Action<BenchmarkResult>? OnScenarioCompleted;
        public event Action<List<BenchmarkResult>>? OnAllScenariosCompleted;

        public StressTestRunner(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        /// <summary>
        /// Sets the slider cache reference from the Editor component.
        /// </summary>
        public void SetSliderCache(ConcurrentDictionary<string, EditorSlider> sliderCache)
        {
            _sliderCache = sliderCache;
        }

        /// <summary>
        /// Runs all standardized benchmark scenarios.
        /// </summary>
        public async Task RunAllScenariosAsync(string imageName = "")
        {
            if (_sliderCache == null)
            {
                Log("[ERROR] Slider cache not set. Call SetSliderCache first.");
                return;
            }

            _results.Clear();
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
                    Description = "Sweep exposure from -5 to +5 in 20 steps. " +
                                 "Measures: Render:Complete, Render:FullPass. " +
                                 "This is the PRIMARY baseline comparison test.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderRanges["Exposure"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Exposure", value);
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
                    Description = "Sweep contrast from -1 to +1 in 20 steps. " +
                                 "Measures contrast processing performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderRanges["Contrast"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Contrast", value);
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
                    Description = "Oscillate exposure between -3 and +3, 50 times with 20ms delay. " +
                                 "Tests render cancellation and UI thread responsiveness.",
                    Iterations = 50,
                    DelayMs = 20,
                    TestAction = (i) =>
                    {
                        SetSliderValue("Exposure", (i % 2 == 0) ? -3f : 3f);
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
                    Description = "Sweep Temperature (2000K-50000K) and Tint (-150 to +150) together. " +
                                 "Tests complex color matrix calculations.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (tempMin, tempMax, _) = SliderRanges["Temperature"];
                        SetSliderValue("Temperature", tempMin + ((tempMax - tempMin) * progress));

                        var (tintMin, tintMax, _) = SliderRanges["Tint"];
                        SetSliderValue("Tint", tintMin + ((tintMax - tintMin) * progress));
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
                    Description = "Sweep Highlights and Shadows together from -100 to +100. " +
                                 "Tests tone mapping performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (hlMin, hlMax, _) = SliderRanges["Highlights"];
                        SetSliderValue("Highlights", hlMin + ((hlMax - hlMin) * progress));

                        var (shMin, shMax, _) = SliderRanges["Shadows"];
                        SetSliderValue("Shadows", shMin + ((shMax - shMin) * progress));
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // SCENARIO 6: Presence Controls
                // Purpose: Measure vibrance/saturation performance
                // ═══════════════════════════════════════════════════════════════
                new BenchmarkScenario
                {
                    Id = BenchmarkOps.TEST_PRESENCE,
                    Name = "Presence Controls Sweep",
                    Description = "Sweep Vibrance and Saturation together from -100 to +100. " +
                                 "Tests color enhancement performance.",
                    Iterations = 20,
                    DelayMs = 100,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (vibMin, vibMax, _) = SliderRanges["Vibrance"];
                        SetSliderValue("Vibrance", vibMin + ((vibMax - vibMin) * progress));

                        var (satMin, satMax, _) = SliderRanges["Saturation"];
                        SetSliderValue("Saturation", satMin + ((satMax - satMin) * progress));
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
                            SetSliderValue("Contrast", 0.5f);
                            SetSliderValue("Highlights", 50f);
                            SetSliderValue("Shadows", -50f);
                            SetSliderValue("Temperature", 7500f);
                            SetSliderValue("Tint", 20f);
                            SetSliderValue("Vibrance", 30f);
                            SetSliderValue("Saturation", 10f);
                        }

                        // Sweep exposure with all other filters active
                        var (min, max, _) = SliderRanges["Exposure"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Exposure", value);
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
                        foreach (var (key, (_, _, def)) in SliderRanges)
                        {
                            SetSliderValue(key, def);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Sets a slider value by key.
        /// </summary>
        private void SetSliderValue(string key, float value)
        {
            if (_sliderCache != null && _sliderCache.TryGetValue(key, out var slider))
            {
                slider.SetValue(value);
            }
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
