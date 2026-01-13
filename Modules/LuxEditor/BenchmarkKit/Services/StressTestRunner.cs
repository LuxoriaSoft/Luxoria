/*
 * StressTestRunner.cs - Automated Stress Testing for LuxEditor
 *
 * Provides automated test scenarios for benchmarking:
 * - Single slider sweep tests
 * - Rapid slider oscillation
 * - Multi-slider combination tests
 * - Memory pressure tests
 *
 * Results are automatically recorded via PerformanceMetrics.
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
    /// </summary>
    public class StressTestRunner
    {
        private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly List<StressTestResult> _results = new();

        // Slider references (set via SetSliders method)
        private Slider? _exposureSlider;
        private Slider? _contrastSlider;
        private Slider? _highlightsSlider;
        private Slider? _shadowsSlider;
        private Slider? _temperatureSlider;
        private Slider? _tintSlider;
        private Slider? _saturationSlider;

        public event Action<string>? OnLogMessage;
        public event Action<StressTestResult>? OnTestCompleted;
        public event Action<List<StressTestResult>>? OnAllTestsCompleted;

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
        /// Runs all predefined stress test scenarios.
        /// </summary>
        public async Task RunAllTestsAsync()
        {
            _results.Clear();
            _metrics.StartNewSession("Automated Stress Test Suite");

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
        /// Gets predefined stress test scenarios.
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
                        if (_exposureSlider is Slider slider)
                        {
                            double value = slider.Minimum +
                                ((slider.Maximum - slider.Minimum) * i / 19.0);
                            slider.Value = value;
                        }
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
                        if (_contrastSlider is Slider slider)
                        {
                            double value = slider.Minimum +
                                ((slider.Maximum - slider.Minimum) * i / 19.0);
                            slider.Value = value;
                        }
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
                        if (_exposureSlider is Slider slider)
                        {
                            slider.Value = (i % 2 == 0) ? -500 : 500;
                        }
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
                        double progress = i / 19.0;
                        if (_temperatureSlider is Slider tempSlider)
                        {
                            tempSlider.Value = -100 + (200 * progress);
                        }
                        if (_tintSlider is Slider tintSlider)
                        {
                            tintSlider.Value = -100 + (200 * progress);
                        }
                    }
                },

                // Scenario 5: All Sliders Sequential
                new StressTestScenario
                {
                    Name = "All Sliders Sequential",
                    Description = "Adjust all 7 sliders in sequence",
                    Iterations = 7,
                    DelayBetweenIterationsMs = 50,
                    TestAction = (i) =>
                    {
                        switch (i)
                        {
                            case 0:
                                if (_exposureSlider is Slider s0) s0.Value = 500;
                                break;
                            case 1:
                                if (_contrastSlider is Slider s1) s1.Value = 500;
                                break;
                            case 2:
                                if (_highlightsSlider is Slider s2) s2.Value = 500;
                                break;
                            case 3:
                                if (_shadowsSlider is Slider s3) s3.Value = 500;
                                break;
                            case 4:
                                if (_temperatureSlider is Slider s4) s4.Value = 50;
                                break;
                            case 5:
                                if (_tintSlider is Slider s5) s5.Value = 50;
                                break;
                            case 6:
                                if (_saturationSlider is Slider s6) s6.Value = 50;
                                break;
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
                        double progress = i / 19.0;
                        if (_highlightsSlider is Slider hlSlider)
                        {
                            hlSlider.Value = -1000 + (2000 * progress);
                        }
                        if (_shadowsSlider is Slider shSlider)
                        {
                            shSlider.Value = -1000 + (2000 * progress);
                        }
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
                        if (_exposureSlider is Slider slider)
                        {
                            slider.Value = Math.Sin(i * 0.2) * 500;
                        }
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
                        if (_saturationSlider is Slider slider)
                        {
                            slider.Value = -100 + (200 * i / 19.0);
                        }
                    }
                },

                // Scenario 9: Reset All
                new StressTestScenario
                {
                    Name = "Reset All Sliders",
                    Description = "Reset all sliders to zero",
                    Iterations = 1,
                    DelayBetweenIterationsMs = 0,
                    TestAction = (i) =>
                    {
                        if (_exposureSlider is Slider s0) s0.Value = 0;
                        if (_contrastSlider is Slider s1) s1.Value = 0;
                        if (_highlightsSlider is Slider s2) s2.Value = 0;
                        if (_shadowsSlider is Slider s3) s3.Value = 0;
                        if (_temperatureSlider is Slider s4) s4.Value = 0;
                        if (_tintSlider is Slider s5) s5.Value = 0;
                        if (_saturationSlider is Slider s6) s6.Value = 0;
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
