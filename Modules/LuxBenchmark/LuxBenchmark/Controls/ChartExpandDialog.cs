using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LuxBenchmark.Controls
{
    public class ChartExpandDialog : ContentDialog
    {
        private readonly Border _chartContainer;
        private BaseSkiaChart? _expandedChart;

        public ChartExpandDialog()
        {
            Title = "";
            PrimaryButtonText = "Close";
            DefaultButton = ContentDialogButton.Primary;
            FullSizeDesired = true;

            _chartContainer = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30)),
                CornerRadius = new CornerRadius(8),
                MinWidth = 800,
                MinHeight = 500,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Content = _chartContainer;

            // Style the dialog
            Resources["ContentDialogMaxWidth"] = 1200.0;
            Resources["ContentDialogMaxHeight"] = 800.0;
        }

        public void SetChart(BaseSkiaChart sourceChart)
        {
            // Clone the chart with the same data
            _expandedChart = sourceChart.Clone();

            // Set sizing to fill the container
            _expandedChart.HorizontalAlignment = HorizontalAlignment.Stretch;
            _expandedChart.VerticalAlignment = VerticalAlignment.Stretch;
            _expandedChart.Width = double.NaN;
            _expandedChart.Height = double.NaN;
            _expandedChart.MinWidth = 750;
            _expandedChart.MinHeight = 450;

            _chartContainer.Child = _expandedChart;

            // Set dialog title to chart title
            if (!string.IsNullOrEmpty(sourceChart.Title))
            {
                Title = sourceChart.Title;
            }
        }
    }
}
