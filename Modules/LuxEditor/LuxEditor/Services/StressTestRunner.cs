/*
 * StressTestRunner.cs - Automated Stress Testing for LuxEditor (New Version)
 *
 * Adapted for the new EditorSlider architecture.
 * Provides automated test scenarios for benchmarking:
 * - Single slider sweep tests
 * - Rapid slider oscillation
 * - Multi-slider combination tests
 * - Memory pressure tests
 *
 * Results are automatically recorded via PerformanceMetrics.
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
    /// Stress test scenario definition.
    /// </summary>
    public class StressTestScenario
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Iterations { get; set; } = 10;
        public int DelayBetweenIterationsMs { get; set; } = 50;
        public Action<int>? TestAction { get; set; }
    }

    /// <summary>
    /// Results from a stress test run.
    /// </summary>
    public class StressTestResult
    {
        public string ScenarioName { get; set; } = string.Empty;
        public int TotalIterations { get; set; }
        public double TotalDurationMs { get; set; }
        public double AvgIterationMs { get; set; }
        public double MinIterationMs { get; set; }
        public double MaxIterationMs { get; set; }
        public long MemoryBefore { get; set; }
        public long MemoryAfter { get; set; }
        public int GCCollections { get; set; }
        public bool Completed { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Automated stress test runner for benchmarking slider performance.
    /// Adapted for the new EditorSlider architecture.
    /// </summary>
    public class StressTestRunner
    {
        private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly List<StressTestResult> _results = new();

        // Slider cache reference (from Editor component)
        private ConcurrentDictionary<string, EditorSlider>? _sliderCache;

        // Slider presets for min/max values
        private static readonly Dictionary<string, (float min, float max, float def)> SliderPresets = new()
        {
            ["Temperature"] = (2000, 50000, 6500),
            ["Tint"] = (-150, 150, 0),
            ["Exposure"] = (-5, 5, 0),
            ["Contrast"] = (-1, 1, 0),
            ["Highlights"] = (-100, 100, 0),
            ["Shadows"] = (-100, 100, 0),
            ["Whites"] = (-100, 100, 0),
            ["Blacks"] = (-100, 100, 0),
            ["Texture"] = (-100, 100, 0),
            ["Dehaze"] = (-100, 100, 0),
            ["Vibrance"] = (-100, 100, 0),
            ["Saturation"] = (-100, 100, 0)
        };

        public event Action<string>? OnLogMessage;
        public event Action<StressTestResult>? OnTestCompleted;
        public event Action<List<StressTestResult>>? OnAllTestsCompleted;

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
        /// Gets a slider's current value.
        /// </summary>
        private float GetSliderValue(string key)
        {
            if (_sliderCache != null && _sliderCache.TryGetValue(key, out var slider))
            {
                return slider.GetValue();
            }
            return 0;
        }

        /// <summary>
        /// Runs all predefined stress test scenarios.
        /// </summary>
        public async Task RunAllTestsAsync()
        {
            if (_sliderCache == null)
            {
                Log("[ERROR] Slider cache not set. Call SetSliderCache first.");
                return;
            }

            _results.Clear();
            _metrics.StartNewSession("Automated Stress Test Suite - New Version");

            Log("═══════════════════════════════════════════════════════════════════");
            Log("[STRESS TEST] Starting Automated Stress Test Suite");
            Log("═══════════════════════════════════════════════════════════════════");

            var scenarios = GetPredefinedScenarios();

            foreach (var scenario in scenarios)
            {
                var result = await RunScenarioAsync(scenario);
                _results.Add(result);
                OnTestCompleted?.Invoke(result);

                // Brief pause between scenarios
                await Task.Delay(500);
            }

            Log("═══════════════════════════════════════════════════════════════════");
            Log("[STRESS TEST] All Tests Completed");
            Log("═══════════════════════════════════════════════════════════════════");

            PrintResultsSummary();
            _metrics.PrintSummary();
            _metrics.ExportToJson();

            OnAllTestsCompleted?.Invoke(_results);
        }

        /// <summary>
        /// Runs a single stress test scenario.
        /// </summary>
        public async Task<StressTestResult> RunScenarioAsync(StressTestScenario scenario)
        {
            Log($"\n[TEST] Starting: {scenario.Name}");
            Log($"[TEST] Description: {scenario.Description}");
            Log($"[TEST] Iterations: {scenario.Iterations}");

            var result = new StressTestResult
            {
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

                    // Delay between iterations
                    if (scenario.DelayBetweenIterationsMs > 0)
                    {
                        await Task.Delay(scenario.DelayBetweenIterationsMs);
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
                Log($"[ERROR] Test failed: {ex.Message}");
            }

            result.MemoryAfter = GC.GetTotalMemory(false);
            int gcAfter = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
            result.GCCollections = gcAfter - gcBefore;

            result.TotalDurationMs = masterStopwatch.Elapsed.TotalMilliseconds;
            if (iterationTimes.Count > 0)
            {
                result.AvgIterationMs = iterationTimes.Average();
                result.MinIterationMs = iterationTimes.Min();
                result.MaxIterationMs = iterationTimes.Max();
            }

            Log($"[TEST] Completed: {scenario.Name}");
            Log($"       Total Time: {result.TotalDurationMs:F2}ms | Avg: {result.AvgIterationMs:F2}ms");
            Log($"       Memory Delta: {(result.MemoryAfter - result.MemoryBefore) / 1024.0 / 1024.0:F2} MB | GC: {result.GCCollections}");

            return result;
        }

        /// <summary>
        /// Gets predefined stress test scenarios adapted for EditorSlider.
        /// </summary>
        private List<StressTestScenario> GetPredefinedScenarios()
        {
            return new List<StressTestScenario>
            {
                // Scenario 1: Exposure Sweep
                new StressTestScenario
                {
                    Name = "Exposure Full Sweep",
                    Description = "Sweep exposure slider from min to max",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderPresets["Exposure"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Exposure", value);
                    }
                },

                // Scenario 2: Contrast Sweep
                new StressTestScenario
                {
                    Name = "Contrast Full Sweep",
                    Description = "Sweep contrast slider from min to max",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderPresets["Contrast"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Contrast", value);
                    }
                },

                // Scenario 3: Rapid Oscillation
                new StressTestScenario
                {
                    Name = "Rapid Exposure Oscillation",
                    Description = "Rapidly toggle exposure between two values",
                    Iterations = 50,
                    DelayBetweenIterationsMs = 10,
                    TestAction = (i) =>
                    {
                        SetSliderValue("Exposure", (i % 2 == 0) ? -3f : 3f);
                    }
                },

                // Scenario 4: Temperature + Tint Combined
                new StressTestScenario
                {
                    Name = "Temperature + Tint Combined",
                    Description = "Adjust both temperature and tint simultaneously",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (tempMin, tempMax, _) = SliderPresets["Temperature"];
                        SetSliderValue("Temperature", tempMin + ((tempMax - tempMin) * progress));

                        var (tintMin, tintMax, _) = SliderPresets["Tint"];
                        SetSliderValue("Tint", tintMin + ((tintMax - tintMin) * progress));
                    }
                },

                // Scenario 5: All Tone Sliders Sequential
                new StressTestScenario
                {
                    Name = "All Tone Sliders Sequential",
                    Description = "Adjust all tone sliders in sequence",
                    Iterations = 8,
                    DelayBetweenIterationsMs = 50,
                    TestAction = (i) =>
                    {
                        string[] sliderKeys = { "Exposure", "Contrast", "Highlights", "Shadows", "Whites", "Blacks", "Vibrance", "Saturation" };
                        if (i < sliderKeys.Length)
                        {
                            var key = sliderKeys[i];
                            var (min, max, _) = SliderPresets[key];
                            SetSliderValue(key, max * 0.5f); // Set to 50% of max
                        }
                    }
                },

                // Scenario 6: Highlights + Shadows
                new StressTestScenario
                {
                    Name = "Highlights + Shadows Sweep",
                    Description = "Sweep highlights and shadows together",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (hlMin, hlMax, _) = SliderPresets["Highlights"];
                        SetSliderValue("Highlights", hlMin + ((hlMax - hlMin) * progress));

                        var (shMin, shMax, _) = SliderPresets["Shadows"];
                        SetSliderValue("Shadows", shMin + ((shMax - shMin) * progress));
                    }
                },

                // Scenario 7: Maximum Stress (No Delay)
                new StressTestScenario
                {
                    Name = "Maximum Stress - No Delay",
                    Description = "Rapid changes with no delay between iterations",
                    Iterations = 100,
                    DelayBetweenIterationsMs = 0,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderPresets["Exposure"];
                        float value = (float)(Math.Sin(i * 0.2) * (max - min) / 2);
                        SetSliderValue("Exposure", value);
                    }
                },

                // Scenario 8: Saturation Sweep
                new StressTestScenario
                {
                    Name = "Saturation Full Range",
                    Description = "Sweep saturation from -100 to +100",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        var (min, max, _) = SliderPresets["Saturation"];
                        float value = min + ((max - min) * i / 19.0f);
                        SetSliderValue("Saturation", value);
                    }
                },

                // Scenario 9: Vibrance + Dehaze
                new StressTestScenario
                {
                    Name = "Vibrance + Dehaze Sweep",
                    Description = "Adjust vibrance and dehaze together",
                    Iterations = 20,
                    DelayBetweenIterationsMs = 30,
                    TestAction = (i) =>
                    {
                        float progress = i / 19.0f;

                        var (vibMin, vibMax, _) = SliderPresets["Vibrance"];
                        SetSliderValue("Vibrance", vibMin + ((vibMax - vibMin) * progress));

                        var (dhMin, dhMax, _) = SliderPresets["Dehaze"];
                        SetSliderValue("Dehaze", dhMin + ((dhMax - dhMin) * progress));
                    }
                },

                // Scenario 10: Reset All
                new StressTestScenario
                {
                    Name = "Reset All Sliders",
                    Description = "Reset all sliders to default values",
                    Iterations = 1,
                    DelayBetweenIterationsMs = 0,
                    TestAction = (i) =>
                    {
                        foreach (var (key, (_, _, def)) in SliderPresets)
                        {
                            SetSliderValue(key, def);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Prints summary of all test results.
        /// </summary>
        private void PrintResultsSummary()
        {
            Log("\n╔═══════════════════════════════════════════════════════════════════════════════╗");
            Log("║                         STRESS TEST RESULTS SUMMARY                           ║");
            Log("╠═══════════════════════════════════════════════════════════════════════════════╣");
            Log("║  SCENARIO                          │ ITERS │ AVG(ms) │ MAX(ms) │ MEM Δ │ GCs ║");
            Log("╠════════════════════════════════════╪═══════╪═════════╪═════════╪═══════╪═════╣");

            foreach (var result in _results)
            {
                var name = result.ScenarioName.Length > 34
                    ? result.ScenarioName[..34]
                    : result.ScenarioName.PadRight(34);
                var memDelta = (result.MemoryAfter - result.MemoryBefore) / (1024.0 * 1024.0);

                var status = result.Completed ? "" : " [FAIL]";
                Log($"║  {name} │ {result.TotalIterations,5} │ {result.AvgIterationMs,6:F1}ms │ {result.MaxIterationMs,6:F1}ms │ {memDelta,4:F1}MB │ {result.GCCollections,3} ║{status}");
            }

            Log("╚═══════════════════════════════════════════════════════════════════════════════╝");
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
            OnLogMessage?.Invoke(message);
        }
    }
}
