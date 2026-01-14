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
        private ComparisonSummary? _currentComparison;

        public event Action<string>? OperationSelected;

        public DashboardMain()
        {
            InitializeComponent();

            // Create charts programmatically
            _performanceChart = new BarChart
            {
                Title = "Average Response Time",
                Unit = "ms",
                ShowComparison = true
            };
            _performanceChart.ItemClicked += OnChartItemClicked;
            PerformanceChartHost.Content = _performanceChart;

            _memoryChart = new BarChart
            {
                Title = "Memory Usage",
                Unit = "KB",
                ShowComparison = true
            };
            _memoryChart.ItemClicked += OnChartItemClicked;
            MemoryChartHost.Content = _memoryChart;

            UpdateEmptyState();
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

            // Key metrics to show as cards
            var keyMetrics = new[]
            {
                "Pipeline:Total",
                "Pipeline:FullPass",
                "Pipeline:PreviewPass",
                "Pipeline:ApplyFilters_full"
            };

            foreach (var metricKey in keyMetrics)
            {
                var result = _currentComparison.Results.FirstOrDefault(r => r.OperationKey == metricKey);
                if (result == null) continue;

                var card = new MetricCard();
                card.SetData(
                    FormatMetricName(result.OperationName),
                    result.SessionB_AvgMs,
                    "ms",
                    result.SessionA_AvgMs
                );
                card.Clicked += () => OperationSelected?.Invoke(metricKey);
                MetricCardsPanel.Children.Add(card);
            }

            // Overall improvement card
            var overallCard = new MetricCard();
            var overallImprovement = _currentComparison.OverallImprovementPercent;
            overallCard.SetData(
                "Overall Change",
                overallImprovement,
                "%",
                null
            );
            MetricCardsPanel.Children.Add(overallCard);
        }

        private void UpdateMetricCardsSingle()
        {
            MetricCardsPanel.Children.Clear();

            if (_sessionA == null) return;

            foreach (var stat in _sessionA.Statistics.Take(6))
            {
                var card = new MetricCard();
                card.SetData(
                    FormatMetricName(stat.Key),
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

            // Performance chart data
            var perfItems = _currentComparison.Results
                .Where(r => r.SessionA_AvgMs > 0 || r.SessionB_AvgMs > 0)
                .OrderByDescending(r => r.SessionB_AvgMs)
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

            // Memory chart data
            var memItems = _currentComparison.Results
                .Where(r => r.SessionA_AvgMemory > 0 || r.SessionB_AvgMemory > 0)
                .OrderByDescending(r => r.SessionB_AvgMemory)
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
        }

        private void UpdateComparisonTable()
        {
            ComparisonTablePanel.Children.Clear();

            if (_currentComparison == null) return;

            // Header row
            var headerRow = new ComparisonRow();
            headerRow.SetHeader();
            ComparisonTablePanel.Children.Add(headerRow);

            // Data rows
            foreach (var result in _currentComparison.Results.OrderByDescending(r => Math.Abs(r.ImprovementPercent)))
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
                .Replace("Pipeline:", "")
                .Replace("ProcessImage:", "")
                .Replace("PhotoViewer:", "")
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
    }
}
