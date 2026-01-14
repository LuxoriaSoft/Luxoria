using LuxBenchmark.Controls;
using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System.Linq;

namespace LuxBenchmark.Components
{
    public sealed partial class MetricDetails : Page
    {
        private BenchmarkSession? _sessionA;
        private BenchmarkSession? _sessionB;
        private string? _selectedOperationKey;
        private readonly BarChart _distributionChart;

        public MetricDetails()
        {
            InitializeComponent();

            _distributionChart = new BarChart
            {
                Title = "Sample Values",
                Unit = "ms"
            };
            DistributionChartHost.Content = _distributionChart;
        }

        public void SetSessions(BenchmarkSession? sessionA, BenchmarkSession? sessionB)
        {
            _sessionA = sessionA;
            _sessionB = sessionB;

            if (_selectedOperationKey != null)
            {
                ShowOperationDetails(_selectedOperationKey);
            }
        }

        public void ShowOperationDetails(string operationKey)
        {
            _selectedOperationKey = operationKey;

            if (_sessionA == null && _sessionB == null)
            {
                ClearDetails();
                return;
            }

            SelectedOperationPanel.Visibility = Visibility.Visible;
            EmptyDetailsText.Visibility = Visibility.Collapsed;

            OperationNameText.Text = FormatOperationName(operationKey);

            // Get stats from both sessions
            OperationStats? statsA = null;
            OperationStats? statsB = null;

            _sessionA?.Statistics.TryGetValue(operationKey, out statsA);
            _sessionB?.Statistics.TryGetValue(operationKey, out statsB);

            // Display Session A stats
            if (statsA != null)
            {
                SessionAAvgText.Text = $"Avg: {statsA.AvgMs:F2} ms";
                SessionAMinMaxText.Text = $"Min: {statsA.MinMs:F2} / Max: {statsA.MaxMs:F2} ms";
                SessionAP95Text.Text = $"P95: {statsA.P95Ms:F2} ms";
                SessionASamplesText.Text = $"Samples: {statsA.SampleCount}";
            }
            else
            {
                SessionAAvgText.Text = "N/A";
                SessionAMinMaxText.Text = "";
                SessionAP95Text.Text = "";
                SessionASamplesText.Text = "";
            }

            // Display Session B stats
            if (statsB != null)
            {
                SessionBAvgText.Text = $"Avg: {statsB.AvgMs:F2} ms";
                SessionBMinMaxText.Text = $"Min: {statsB.MinMs:F2} / Max: {statsB.MaxMs:F2} ms";
                SessionBP95Text.Text = $"P95: {statsB.P95Ms:F2} ms";
                SessionBSamplesText.Text = $"Samples: {statsB.SampleCount}";
            }
            else
            {
                SessionBAvgText.Text = "N/A";
                SessionBMinMaxText.Text = "";
                SessionBP95Text.Text = "";
                SessionBSamplesText.Text = "";
            }

            // Calculate and display improvement
            if (statsA != null && statsB != null)
            {
                double delta = statsB.AvgMs - statsA.AvgMs;
                double improvement = statsA.AvgMs > 0
                    ? ((statsA.AvgMs - statsB.AvgMs) / statsA.AvgMs) * 100
                    : 0;

                bool isFaster = delta < 0;
                string sign = delta >= 0 ? "+" : "";

                ImprovementText.Text = $"{sign}{improvement:F1}%";
                ImprovementText.Foreground = new SolidColorBrush(
                    isFaster
                        ? Windows.UI.Color.FromArgb(255, 76, 175, 80)
                        : Windows.UI.Color.FromArgb(255, 244, 67, 54));

                ImprovementDescText.Text = isFaster
                    ? $"{-delta:F2}ms faster"
                    : $"{delta:F2}ms slower";
            }
            else
            {
                ImprovementText.Text = "—";
                ImprovementText.Foreground = new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 128, 128, 128));
                ImprovementDescText.Text = "Only one session has data";
            }

            // Update distribution chart
            UpdateDistributionChart(operationKey, statsA, statsB);
        }

        private void UpdateDistributionChart(string operationKey, OperationStats? statsA, OperationStats? statsB)
        {
            var items = new System.Collections.Generic.List<BarChartItem>();

            // Show comparison of key metrics
            if (statsA != null || statsB != null)
            {
                items.Add(new BarChartItem
                {
                    Label = "Min",
                    Value = statsB?.MinMs ?? 0,
                    CompareValue = statsA?.MinMs,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue
                });

                items.Add(new BarChartItem
                {
                    Label = "Avg",
                    Value = statsB?.AvgMs ?? 0,
                    CompareValue = statsA?.AvgMs,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue
                });

                items.Add(new BarChartItem
                {
                    Label = "Median",
                    Value = statsB?.MedianMs ?? 0,
                    CompareValue = statsA?.MedianMs,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue
                });

                items.Add(new BarChartItem
                {
                    Label = "P95",
                    Value = statsB?.P95Ms ?? 0,
                    CompareValue = statsA?.P95Ms,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue
                });

                items.Add(new BarChartItem
                {
                    Label = "Max",
                    Value = statsB?.MaxMs ?? 0,
                    CompareValue = statsA?.MaxMs,
                    Color = SKColors.Orange,
                    CompareColor = SKColors.DodgerBlue
                });
            }

            _distributionChart.ShowComparison = statsA != null && statsB != null;
            _distributionChart.SetData(items);
        }

        public void ClearDetails()
        {
            SelectedOperationPanel.Visibility = Visibility.Collapsed;
            EmptyDetailsText.Visibility = Visibility.Visible;
            _distributionChart.SetData(new System.Collections.Generic.List<BarChartItem>());
        }

        private static string FormatOperationName(string key)
        {
            return key
                .Replace("Pipeline:", "Pipeline > ")
                .Replace("ProcessImage:", "ProcessImage > ")
                .Replace("PhotoViewer:", "PhotoViewer > ")
                .Replace("_", " ");
        }
    }
}
