using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace LuxBenchmark.Components
{
    public sealed partial class SummaryBar : Page
    {
        private readonly BenchmarkDataService _dataService;
        private ComparisonSummary? _currentComparison;

        public SummaryBar()
        {
            InitializeComponent();
            _dataService = BenchmarkDataService.Instance;
            _dataService.SessionsChanged += UpdateSessionsCount;

            UpdateSessionsCount();
        }

        public void SetComparison(ComparisonSummary? comparison)
        {
            _currentComparison = comparison;
            UpdateComparisonStats();
        }

        public void SetSessions(BenchmarkSession? sessionA, BenchmarkSession? sessionB)
        {
            if (sessionA != null && sessionB != null)
            {
                _currentComparison = _dataService.CompareSessions(sessionA, sessionB);
                UpdateComparisonStats();

                // Update total samples
                var totalSamples = sessionA.TotalSamples + sessionB.TotalSamples;
                TotalSamplesText.Text = $"{totalSamples:N0} samples";

                SetStatus("Comparing", Windows.UI.Color.FromArgb(255, 255, 193, 7));
            }
            else if (sessionA != null || sessionB != null)
            {
                var session = sessionA ?? sessionB;
                _currentComparison = null;
                ClearComparisonStats();

                TotalSamplesText.Text = $"{session!.TotalSamples:N0} samples";
                SetStatus("Single session", Windows.UI.Color.FromArgb(255, 0, 120, 212));
            }
            else
            {
                _currentComparison = null;
                ClearComparisonStats();
                TotalSamplesText.Text = "0 samples";
                SetStatus("Ready", Windows.UI.Color.FromArgb(255, 76, 175, 80));
            }
        }

        private void UpdateSessionsCount()
        {
            var count = _dataService.LoadedSessions.Count();
            SessionsCountText.Text = $"{count} session{(count != 1 ? "s" : "")}";
        }

        private void UpdateComparisonStats()
        {
            if (_currentComparison == null)
            {
                ClearComparisonStats();
                return;
            }

            // Count faster/slower/unchanged operations
            int faster = 0, slower = 0, unchanged = 0;

            foreach (var result in _currentComparison.Results)
            {
                // Use 1% threshold for "unchanged"
                if (Math.Abs(result.ImprovementPercent) < 1.0)
                    unchanged++;
                else if (result.ImprovementPercent > 0)
                    faster++;
                else
                    slower++;
            }

            FasterCountText.Text = faster.ToString();
            SlowerCountText.Text = slower.ToString();
            UnchangedCountText.Text = unchanged.ToString();

            // Overall improvement
            var overall = _currentComparison.OverallImprovementPercent;
            string sign = overall >= 0 ? "+" : "";
            OverallImprovementText.Text = $"{sign}{overall:F1}%";

            // Color based on improvement
            var color = overall > 0
                ? Windows.UI.Color.FromArgb(255, 76, 175, 80)   // Green for improvement
                : overall < 0
                    ? Windows.UI.Color.FromArgb(255, 244, 67, 54)  // Red for regression
                    : Windows.UI.Color.FromArgb(255, 128, 128, 128); // Gray for no change

            OverallImprovementText.Foreground = new SolidColorBrush(color);

            // Update status
            if (faster > slower)
                SetStatus("Performance improved", Windows.UI.Color.FromArgb(255, 76, 175, 80));
            else if (slower > faster)
                SetStatus("Performance regressed", Windows.UI.Color.FromArgb(255, 244, 67, 54));
            else
                SetStatus("Performance similar", Windows.UI.Color.FromArgb(255, 255, 193, 7));
        }

        private void ClearComparisonStats()
        {
            FasterCountText.Text = "0";
            SlowerCountText.Text = "0";
            UnchangedCountText.Text = "0";
            OverallImprovementText.Text = "—";
            OverallImprovementText.Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 128, 128, 128));
        }

        private void SetStatus(string text, Windows.UI.Color indicatorColor)
        {
            StatusText.Text = text;
            StatusIndicator.Fill = new SolidColorBrush(indicatorColor);
        }
    }
}
