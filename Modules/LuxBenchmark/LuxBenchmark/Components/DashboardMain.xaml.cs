using LuxBenchmark.Controls;
using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxBenchmark.Components
{
    public sealed partial class DashboardMain : Page
    {
        private BenchmarkSession? _sessionA;
        private BenchmarkSession? _sessionB;
        private readonly BarChart _performanceChart;
        private readonly BarChart _memoryChart;
        private readonly PieChart _timeDistributionChart;
        private readonly GaugeChart _primaryGauge;
        private readonly Histogram _histogramChart;
        private readonly BoxPlot _boxPlotChart;
        private readonly ConfidenceBandChart _confidenceBandChart;
        private readonly ScatterPlot _scatterPlot;
        private readonly WaterfallChart _waterfallChart;
        private ComparisonSummary? _currentComparison;

        // Color palette for charts
        private static readonly SKColor[] ChartColors = new[]
        {
            SKColor.Parse("#4CAF50"), // Green
            SKColor.Parse("#2196F3"), // Blue
            SKColor.Parse("#FF9800"), // Orange
            SKColor.Parse("#E91E63"), // Pink
            SKColor.Parse("#9C27B0"), // Purple
            SKColor.Parse("#00BCD4"), // Cyan
            SKColor.Parse("#FFEB3B"), // Yellow
            SKColor.Parse("#795548"), // Brown
        };

        public event Action<string>? OperationSelected;

        public DashboardMain()
        {
            InitializeComponent();

            // Create bar chart for performance
            _performanceChart = new BarChart
            {
                Title = "Average Response Time (ms)",
                Unit = "ms",
                ShowComparison = true
            };
            _performanceChart.ItemClicked += OnChartItemClicked;
            _performanceChart.ChartClicked += OnChartClicked;
            PerformanceChartHost.Content = _performanceChart;

            // Create bar chart for memory
            _memoryChart = new BarChart
            {
                Title = "Memory Usage (KB)",
                Unit = "KB",
                ShowComparison = true
            };
            _memoryChart.ItemClicked += OnChartItemClicked;
            _memoryChart.ChartClicked += OnChartClicked;
            MemoryChartHost.Content = _memoryChart;

            // Create pie chart for improvement breakdown
            _timeDistributionChart = new PieChart
            {
                Title = "Performance Change Distribution"
            };
            _timeDistributionChart.ChartClicked += OnChartClicked;
            TimeDistributionChartHost.Content = _timeDistributionChart;

            // Create primary gauge chart (shows improvement %)
            _primaryGauge = new GaugeChart
            {
                Title = "Overall Improvement",
                Unit = "%",
                MinValue = -50,
                MaxValue = 50
            };
            _primaryGauge.ChartClicked += OnChartClicked;
            PrimaryGaugeHost.Content = _primaryGauge;

            // Create histogram chart for comparing distributions
            _histogramChart = new Histogram
            {
                Title = "Sample Distribution (A vs B)",
                Unit = "ms",
                BinCount = 15
            };
            _histogramChart.ChartClicked += OnChartClicked;
            HistogramChartHost.Content = _histogramChart;

            // Create box plot chart
            _boxPlotChart = new BoxPlot
            {
                Title = "Statistical Distribution by Operation",
                Unit = "ms"
            };
            _boxPlotChart.ChartClicked += OnChartClicked;
            BoxPlotChartHost.Content = _boxPlotChart;

            // Create confidence band chart
            _confidenceBandChart = new ConfidenceBandChart
            {
                Title = "Performance Over Time (with variance)",
                Unit = "ms"
            };
            _confidenceBandChart.ChartClicked += OnChartClicked;
            ConfidenceBandChartHost.Content = _confidenceBandChart;

            // Create scatter plot
            _scatterPlot = new ScatterPlot
            {
                Title = "Session Comparison (A vs B)",
                XLabel = "Session A (ms)",
                YLabel = "Session B (ms)",
                Unit = "ms"
            };
            _scatterPlot.ChartClicked += OnChartClicked;
            ScatterPlotHost.Content = _scatterPlot;

            // Create waterfall chart for delta comparison
            _waterfallChart = new WaterfallChart
            {
                Title = "Performance Delta by Operation",
                Unit = "ms",
                ShowAsDelta = true
            };
            _waterfallChart.ChartClicked += OnChartClicked;
            WaterfallChartHost.Content = _waterfallChart;

            UpdateEmptyState();
        }

        private async void OnChartClicked(BaseSkiaChart chart)
        {
            var dialog = new ChartExpandDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.SetChart(chart);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Sets the sessions to compare.
        /// </summary>
        public void SetSessions(BenchmarkSession? sessionA, BenchmarkSession? sessionB)
        {
            _sessionA = sessionA;
            _sessionB = sessionB;

            UpdateEmptyState();
            UpdateComparison();
        }

        /// <summary>
        /// Sets only the primary session (no comparison).
        /// </summary>
        public void SetSession(BenchmarkSession session)
        {
            _sessionA = session;
            _sessionB = null;

            UpdateEmptyState();
            UpdateSingleSession();
        }

        private void UpdateEmptyState()
        {
            bool hasData = _sessionA != null || _sessionB != null;
            EmptyStatePanel.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateComparison()
        {
            if (_sessionA == null || _sessionB == null)
            {
                if (_sessionA != null)
                    UpdateSingleSession();
                return;
            }

            // Update status text
            ComparisonStatusText.Text = $"Comparing: {_sessionA.DisplayName} vs {_sessionB.DisplayName}";

            // Calculate comparison
            _currentComparison = BenchmarkDataService.Instance.CompareSessions(_sessionA, _sessionB);

            // Update metric cards
            UpdateMetricCards();

            // Update charts
            UpdateCharts();

            // Update comparison table
            UpdateComparisonTable();
        }

        private void UpdateSingleSession()
        {
            if (_sessionA == null) return;

            ComparisonStatusText.Text = $"Viewing: {_sessionA.DisplayName}";

            // Update metric cards for single session
            UpdateMetricCardsSingle();

            // Update charts for single session
            UpdateChartsSingle();

            // Clear comparison table
            ComparisonTablePanel.Children.Clear();
            var infoRow = new TextBlock
            {
                Text = "Load a second session to compare",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                Margin = new Thickness(16)
            };
            ComparisonTablePanel.Children.Add(infoRow);
        }

        private void UpdateMetricCards()
        {
            MetricCardsPanel.Children.Clear();

            if (_currentComparison == null) return;

            // Key metrics to show as cards (try new names first, then old names)
            var keyMetricPriority = new[]
            {
                // New standardized names (v2.0.0+)
                ("Render:Complete", "Render Complete"),
                ("Render:FullPass", "Full Pass"),
                ("Render:PreviewPass", "Preview Pass"),
                ("Render:ApplyFilters", "Apply Filters"),
                // Old names (v1.x)
                ("Pipeline:Total", "Pipeline Total"),
                ("Pipeline:FullPass", "Full Pass"),
                ("Pipeline:PreviewPass", "Preview Pass"),
                ("ProcessImage:Complete", "Process Image"),
            };

            int cardsAdded = 0;
            foreach (var (metricKey, displayName) in keyMetricPriority)
            {
                if (cardsAdded >= 3) break; // Max 3 time cards

                var result = _currentComparison.Results.FirstOrDefault(r => r.OperationKey == metricKey);
                if (result == null) continue;

                var card = new MetricCard();
                card.SetData(
                    displayName,
                    result.SessionB_AvgMs,
                    "ms",
                    result.SessionA_AvgMs
                );
                card.Clicked += () => OperationSelected?.Invoke(metricKey);
                MetricCardsPanel.Children.Add(card);
                cardsAdded++;
            }

            // Memory comparison card
            var totalMemoryA = _currentComparison.Results.Sum(r => r.SessionA_AvgMemory);
            var totalMemoryB = _currentComparison.Results.Sum(r => r.SessionB_AvgMemory);
            if (totalMemoryA > 0 || totalMemoryB > 0)
            {
                var memoryCard = new MetricCard();
                memoryCard.SetData(
                    "Total Memory Δ",
                    totalMemoryB / 1024.0, // KB
                    "KB",
                    totalMemoryA / 1024.0
                );
                MetricCardsPanel.Children.Add(memoryCard);
            }

            // Overall improvement card
            var overallCard = new MetricCard();
            var overallImprovement = _currentComparison.OverallImprovementPercent;
            overallCard.SetData(
                "Time Improvement",
                overallImprovement,
                "%",
                null
            );
            MetricCardsPanel.Children.Add(overallCard);

            // Memory improvement card (calculated on totals)
            var memImprovementCard = _currentComparison.OverallMemoryImprovementPercent;
            if (Math.Abs(memImprovementCard) > 0.01)
            {
                var memImpCard = new MetricCard();
                memImpCard.SetData(
                    "Memory Improvement",
                    memImprovementCard,
                    "%",
                    null
                );
                MetricCardsPanel.Children.Add(memImpCard);
            }
        }

        private void UpdateMetricCardsSingle()
        {
            MetricCardsPanel.Children.Clear();

            if (_sessionA == null) return;

            // Prioritize Render operations, then show Test operations
            var prioritizedStats = _sessionA.Statistics
                .OrderByDescending(s => s.Key.StartsWith("Render:"))
                .ThenByDescending(s => s.Key.StartsWith("Test:"))
                .Take(6);

            foreach (var stat in prioritizedStats)
            {
                var card = new MetricCard();
                var displayName = stat.Key.StartsWith("Test:")
                    ? $"Test: {FormatMetricName(stat.Key)}"
                    : FormatMetricName(stat.Key);

                card.SetData(
                    displayName,
                    stat.Value.AvgMs,
                    "ms"
                );
                card.Clicked += () => OperationSelected?.Invoke(stat.Key);
                MetricCardsPanel.Children.Add(card);
            }
        }

        private void UpdateCharts()
        {
            if (_currentComparison == null) return;

            // Performance chart data - show all results but prefer valid comparisons
            var perfItems = _currentComparison.Results
                .Where(r => r.HasValidTimeComparison)
                .OrderByDescending(r => Math.Max(r.SessionA_AvgMs, r.SessionB_AvgMs))
                .Take(10)
                .Select(r => new BarChartItem
                {
                    Label = FormatMetricName(r.OperationKey),
                    Value = r.SessionB_AvgMs,
                    CompareValue = r.SessionA_AvgMs,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue,
                    Tag = r.OperationKey
                })
                .ToList();

            _performanceChart.SetData(perfItems);

            // Memory chart data - only show valid memory comparisons
            var memItems = _currentComparison.Results
                .Where(r => r.HasValidMemoryComparison)
                .OrderByDescending(r => Math.Max(r.SessionA_AvgMemory, r.SessionB_AvgMemory))
                .Take(10)
                .Select(r => new BarChartItem
                {
                    Label = FormatMetricName(r.OperationKey),
                    Value = r.SessionB_AvgMemory / 1024.0, // Convert to KB
                    CompareValue = r.SessionA_AvgMemory / 1024.0,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue,
                    Tag = r.OperationKey
                })
                .ToList();

            _memoryChart.SetData(memItems);

            // Improvement breakdown pie chart
            UpdateImprovementBreakdownChart();

            // Update new charts
            UpdateGaugeChart();
            UpdateHistogramChart();
            UpdateBoxPlotChart();
            UpdateConfidenceBandChart();
            UpdateScatterPlot();
            UpdateWaterfallChart();
        }

        private void UpdateChartsSingle()
        {
            if (_sessionA == null) return;

            // Performance chart data
            var perfItems = _sessionA.Statistics
                .OrderByDescending(s => s.Value.AvgMs)
                .Take(10)
                .Select(s => new BarChartItem
                {
                    Label = FormatMetricName(s.Key),
                    Value = s.Value.AvgMs,
                    Color = GetColorForRating(BenchmarkDataService.GetRating(s.Value.AvgMs)),
                    Tag = s.Key
                })
                .ToList();

            _performanceChart.ShowComparison = false;
            _performanceChart.SetData(perfItems);

            // Memory chart data
            var memItems = _sessionA.Statistics
                .Where(s => s.Value.AvgMemoryDelta > 0)
                .OrderByDescending(s => s.Value.AvgMemoryDelta)
                .Take(10)
                .Select(s => new BarChartItem
                {
                    Label = FormatMetricName(s.Key),
                    Value = s.Value.AvgMemoryDelta / 1024.0,
                    Color = SKColors.Purple,
                    Tag = s.Key
                })
                .ToList();

            _memoryChart.ShowComparison = false;
            _memoryChart.SetData(memItems);

            // Time distribution pie chart
            UpdateTimeDistributionChart(_sessionA);

            // Update new charts (single session mode)
            UpdateGaugeChartSingle();
            UpdateHistogramChartSingle();
            UpdateBoxPlotChartSingle();
            UpdateConfidenceBandChartSingle();
            UpdateScatterPlotSingle();
            UpdateWaterfallChartSingle();
        }

        private void UpdateImprovementBreakdownChart()
        {
            if (_currentComparison == null)
            {
                _timeDistributionChart.SetData(new List<PieChartSlice>());
                return;
            }

            // Calculate total time saved vs time lost (only for valid comparisons)
            double totalImproved = 0;
            double totalRegressed = 0;
            int improvedCount = 0;
            int regressedCount = 0;

            foreach (var result in _currentComparison.ValidResults)
            {
                // ImprovementPercent > 0 means B is faster (improvement)
                // ImprovementPercent < 0 means B is slower (regression)
                if (result.IsFaster)
                {
                    totalImproved += Math.Abs(result.DeltaAvgMs);
                    improvedCount++;
                }
                else if (result.IsSlower)
                {
                    totalRegressed += Math.Abs(result.DeltaAvgMs);
                    regressedCount++;
                }
            }

            var slices = new List<PieChartSlice>();

            if (totalImproved > 0)
            {
                slices.Add(new PieChartSlice
                {
                    Label = $"Faster ({improvedCount})",
                    Value = totalImproved,
                    Color = SKColor.Parse("#4CAF50") // Green
                });
            }

            if (totalRegressed > 0)
            {
                slices.Add(new PieChartSlice
                {
                    Label = $"Slower ({regressedCount})",
                    Value = totalRegressed,
                    Color = SKColor.Parse("#F44336") // Red
                });
            }

            if (slices.Count == 0)
            {
                slices.Add(new PieChartSlice
                {
                    Label = "No change",
                    Value = 1,
                    Color = SKColors.Gray
                });
            }

            _timeDistributionChart.Title = "Performance Change Distribution";
            _timeDistributionChart.SetData(slices);
        }

        private void UpdateTimeDistributionChart(BenchmarkSession? session)
        {
            if (session == null)
            {
                _timeDistributionChart.SetData(new List<PieChartSlice>());
                return;
            }

            // Get top operations by time for pie chart
            var slices = session.Statistics
                .Where(s => s.Value.AvgMs > 0)
                .OrderByDescending(s => s.Value.AvgMs)
                .Take(8)
                .Select((s, index) => new PieChartSlice
                {
                    Label = FormatMetricName(s.Key),
                    Value = s.Value.AvgMs,
                    Color = ChartColors[index % ChartColors.Length]
                })
                .ToList();

            _timeDistributionChart.Title = "Time Distribution by Operation";
            _timeDistributionChart.SetData(slices);
        }

        private void UpdateComparisonTable()
        {
            ComparisonTablePanel.Children.Clear();

            if (_currentComparison == null) return;

            // Header row
            var headerRow = new ComparisonRow();
            headerRow.SetHeader();
            ComparisonTablePanel.Children.Add(headerRow);

            // Data rows - show valid comparisons first, sorted by absolute improvement
            var sortedResults = _currentComparison.Results
                .OrderByDescending(r => r.HasValidTimeComparison)
                .ThenByDescending(r => Math.Abs(r.ImprovementPercent));

            foreach (var result in sortedResults)
            {
                var row = new ComparisonRow();
                row.SetData(result);
                ComparisonTablePanel.Children.Add(row);
            }
        }

        private void OnChartItemClicked(BarChartItem item)
        {
            if (item.Tag is string operationKey)
            {
                OperationSelected?.Invoke(operationKey);
            }
        }

        private static string FormatMetricName(string key)
        {
            // Remove common prefixes for cleaner display
            return key
                .Replace("Render:", "")
                .Replace("Pipeline:", "")
                .Replace("ProcessImage:", "")
                .Replace("PhotoViewer:", "")
                .Replace("Test:", "")
                .Replace("_", " ");
        }

        private static SKColor GetColorForRating(PerformanceRating rating)
        {
            return rating switch
            {
                PerformanceRating.Excellent => SKColor.Parse("#4CAF50"),
                PerformanceRating.Good => SKColor.Parse("#8BC34A"),
                PerformanceRating.Acceptable => SKColor.Parse("#FFC107"),
                _ => SKColor.Parse("#F44336")
            };
        }

        #region New Chart Update Methods

        private void UpdateGaugeChart()
        {
            if (_currentComparison == null) return;

            // Show overall improvement percentage
            // Positive = B is faster than A (improvement)
            // Negative = B is slower than A (regression)
            double improvement = _currentComparison.OverallImprovementPercent;

            _primaryGauge.Title = "Overall Improvement";
            _primaryGauge.Unit = "%";
            _primaryGauge.MinValue = -50;
            _primaryGauge.MaxValue = 50;

            string label = improvement > 0 ? "Faster" : improvement < 0 ? "Slower" : "Same";
            _primaryGauge.SetValue(improvement, label);
        }

        private void UpdateGaugeChartSingle()
        {
            if (_sessionA == null) return;

            // In single session mode, show render performance
            var renderComplete = _sessionA.Statistics
                .FirstOrDefault(s => s.Key == "Render:Complete" || s.Key == "Pipeline:Total");

            if (renderComplete.Value != null)
            {
                double avgMs = renderComplete.Value.AvgMs;
                _primaryGauge.Title = "Render Performance";
                _primaryGauge.Unit = "ms";
                _primaryGauge.MinValue = 0;
                _primaryGauge.MaxValue = Math.Max(100, avgMs * 2);
                _primaryGauge.SetValue(avgMs, BenchmarkDataService.GetRating(avgMs).ToString());
            }
        }

        private void UpdateHistogramChart()
        {
            if (_sessionA == null && _sessionB == null) return;

            // Get all render samples for histogram from both sessions
            var samplesA = _sessionA?.Samples
                .Where(s => s.OperationName.StartsWith("Render") || s.OperationName == "Pipeline")
                .Select(s => s.DurationMs)
                .ToList() ?? new List<double>();

            var samplesB = _sessionB?.Samples
                .Where(s => s.OperationName.StartsWith("Render") || s.OperationName == "Pipeline")
                .Select(s => s.DurationMs)
                .ToList() ?? new List<double>();

            if (samplesA.Count > 0 || samplesB.Count > 0)
            {
                _histogramChart.Title = "Sample Distribution (A vs B)";
                _histogramChart.SetComparisonData(samplesA, samplesB);
            }
        }

        private void UpdateHistogramChartSingle()
        {
            if (_sessionA == null) return;

            var samples = _sessionA.Samples
                .Where(s => s.OperationName.StartsWith("Render") || s.OperationName == "Pipeline")
                .Select(s => s.DurationMs)
                .ToList();

            if (samples.Count > 0)
            {
                _histogramChart.Title = "Sample Distribution";
                _histogramChart.SetData(samples);
            }
        }

        private void UpdateBoxPlotChart()
        {
            if (_sessionA == null || _sessionB == null) return;

            var boxPlotItems = new List<BoxPlotItem>();

            // Get operations present in both sessions
            var commonOps = _sessionA.Statistics.Keys
                .Intersect(_sessionB.Statistics.Keys)
                .Where(k => k.StartsWith("Render:") || k.StartsWith("Pipeline:"))
                .Take(6);

            int colorIndex = 0;
            foreach (var op in commonOps)
            {
                var statsA = _sessionA.Statistics[op];
                var statsB = _sessionB.Statistics[op];

                // Add Session A box
                boxPlotItems.Add(new BoxPlotItem
                {
                    Label = $"A:{FormatMetricName(op)}",
                    Min = statsA.MinMs,
                    Q1 = statsA.AvgMs - statsA.StdDevMs * 0.675, // Approximate Q1
                    Median = statsA.MedianMs,
                    Q3 = statsA.AvgMs + statsA.StdDevMs * 0.675, // Approximate Q3
                    Max = statsA.MaxMs,
                    Color = SKColors.DodgerBlue
                });

                // Add Session B box
                boxPlotItems.Add(new BoxPlotItem
                {
                    Label = $"B:{FormatMetricName(op)}",
                    Min = statsB.MinMs,
                    Q1 = statsB.AvgMs - statsB.StdDevMs * 0.675,
                    Median = statsB.MedianMs,
                    Q3 = statsB.AvgMs + statsB.StdDevMs * 0.675,
                    Max = statsB.MaxMs,
                    Color = SKColors.Orange
                });

                colorIndex++;
            }

            _boxPlotChart.SetData(boxPlotItems);
        }

        private void UpdateBoxPlotChartSingle()
        {
            if (_sessionA == null) return;

            var boxPlotItems = _sessionA.Statistics
                .Where(s => s.Key.StartsWith("Render:") || s.Key.StartsWith("Pipeline:"))
                .OrderByDescending(s => s.Value.AvgMs)
                .Take(8)
                .Select((s, index) => new BoxPlotItem
                {
                    Label = FormatMetricName(s.Key),
                    Min = s.Value.MinMs,
                    Q1 = s.Value.AvgMs - s.Value.StdDevMs * 0.675,
                    Median = s.Value.MedianMs,
                    Q3 = s.Value.AvgMs + s.Value.StdDevMs * 0.675,
                    Max = s.Value.MaxMs,
                    Color = ChartColors[index % ChartColors.Length]
                })
                .ToList();

            _boxPlotChart.SetData(boxPlotItems);
        }

        private void UpdateConfidenceBandChart()
        {
            if (_sessionA == null && _sessionB == null)
            {
                _confidenceBandChart.Clear();
                return;
            }

            var series = new List<ConfidenceBandSeries>();

            // Find operation to track
            string? trackOp = FindPrimaryOperation(_sessionB ?? _sessionA);
            if (trackOp == null) return;

            // Session A confidence band
            if (_sessionA != null)
            {
                var seriesA = CreateConfidenceBandSeries(_sessionA, trackOp, "Session A", SKColors.DodgerBlue);
                if (seriesA != null) series.Add(seriesA);
            }

            // Session B confidence band
            if (_sessionB != null)
            {
                var seriesB = CreateConfidenceBandSeries(_sessionB, trackOp, "Session B", SKColors.Orange);
                if (seriesB != null) series.Add(seriesB);
            }

            _confidenceBandChart.Title = $"Performance Variance ({FormatMetricName(trackOp)})";
            _confidenceBandChart.SetData(series);
        }

        private void UpdateConfidenceBandChartSingle()
        {
            if (_sessionA == null)
            {
                _confidenceBandChart.Clear();
                return;
            }

            string? trackOp = FindPrimaryOperation(_sessionA);
            if (trackOp == null) return;

            var series = CreateConfidenceBandSeries(_sessionA, trackOp, "Samples", SKColors.DodgerBlue);
            if (series != null)
            {
                _confidenceBandChart.Title = $"Performance Variance ({FormatMetricName(trackOp)})";
                _confidenceBandChart.SetData(new List<ConfidenceBandSeries> { series });
            }
        }

        private ConfidenceBandSeries? CreateConfidenceBandSeries(BenchmarkSession session, string operation, string name, SKColor color)
        {
            var samples = session.Samples
                .Where(s => $"{s.OperationName}:{s.Phase}" == operation || s.OperationName == operation)
                .Select(s => s.DurationMs)
                .ToList();

            if (samples.Count < 5) return null;

            // Create rolling window statistics
            int windowSize = Math.Max(3, samples.Count / 10);
            var points = new List<ConfidenceBandPoint>();

            for (int i = 0; i < samples.Count - windowSize; i += Math.Max(1, windowSize / 2))
            {
                var window = samples.Skip(i).Take(windowSize).ToList();
                double mean = window.Average();
                double stdDev = Math.Sqrt(window.Average(v => Math.Pow(v - mean, 2)));

                points.Add(new ConfidenceBandPoint
                {
                    Mean = mean,
                    Upper = mean + stdDev,
                    Lower = Math.Max(0, mean - stdDev)
                });
            }

            return new ConfidenceBandSeries
            {
                Name = name,
                Points = points,
                Color = color
            };
        }

        private void UpdateScatterPlot()
        {
            if (_sessionA == null || _sessionB == null || _currentComparison == null)
            {
                _scatterPlot.SetData(new List<ScatterPoint>());
                return;
            }

            // Only plot points with valid data in both sessions
            var points = _currentComparison.ValidResults
                .Select(r => new ScatterPoint
                {
                    Label = FormatMetricName(r.OperationKey),
                    X = r.SessionA_AvgMs,
                    Y = r.SessionB_AvgMs,
                    Tag = r.OperationKey
                })
                .ToList();

            _scatterPlot.XLabel = $"Session A: {_sessionA.DisplayName}";
            _scatterPlot.YLabel = $"Session B: {_sessionB.DisplayName}";
            _scatterPlot.SetData(points);
        }

        private void UpdateScatterPlotSingle()
        {
            // Scatter plot requires two sessions for comparison
            _scatterPlot.Title = "Session Comparison (requires 2 sessions)";
            _scatterPlot.SetData(new List<ScatterPoint>());
        }

        private void UpdateWaterfallChart()
        {
            if (_currentComparison == null) return;

            // Show delta (improvement/regression) per operation - only valid comparisons
            // Value > 0 means B is faster (green), Value < 0 means B is slower (red)
            var deltaItems = _currentComparison.ValidResults
                .OrderByDescending(r => Math.Abs(r.DeltaAvgMs))
                .Take(10)
                .Select(r => new WaterfallItem
                {
                    Label = FormatMetricName(r.OperationKey),
                    // Value = A - B: positive when B is faster (improvement)
                    Value = r.SessionA_AvgMs - r.SessionB_AvgMs,
                    IsIncrease = r.IsFaster
                })
                .ToList();

            _waterfallChart.Title = "Performance Delta by Operation (A→B)";
            _waterfallChart.ShowAsDelta = true;
            _waterfallChart.SetData(deltaItems);
        }

        private void UpdateWaterfallChartSingle()
        {
            if (_sessionA == null) return;

            var renderOps = _sessionA.Statistics
                .Where(s => s.Key.StartsWith("Render:") && s.Key != "Render:Complete")
                .OrderByDescending(s => s.Value.AvgMs)
                .Take(8)
                .Select(s => new WaterfallItem
                {
                    Label = FormatMetricName(s.Key),
                    Value = s.Value.AvgMs,
                    IsIncrease = true
                })
                .ToList();

            _waterfallChart.Title = "Time Breakdown by Operation";
            _waterfallChart.ShowAsDelta = false;
            _waterfallChart.SetData(renderOps);
        }

        private string? FindPrimaryOperation(BenchmarkSession? session)
        {
            if (session == null) return null;

            var priorityOps = new[] { "Render:Complete", "Render:FullPass", "Pipeline:Total" };

            foreach (var op in priorityOps)
            {
                if (session.Samples.Any(s => $"{s.OperationName}:{s.Phase}" == op || s.OperationName == op))
                    return op;
            }

            return session.Samples.FirstOrDefault()?.OperationName;
        }

        #endregion
    }
}
