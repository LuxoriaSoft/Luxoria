using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxBenchmark.Controls
{
    /// <summary>
    /// Bar chart data point.
    /// </summary>
    public class BarChartItem
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public double? CompareValue { get; set; }
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
        public SKColor? CompareColor { get; set; }
        public object? Tag { get; set; }
    }

    /// <summary>
    /// Custom bar chart control using SkiaSharp.
    /// </summary>
    public class BarChart : UserControl
    {
        private readonly SKXamlCanvas _canvas;
        private List<BarChartItem> _items = new();
        private int _hoveredIndex = -1;
        private string _title = string.Empty;
        private string _unit = "ms";
        private bool _showComparison = false;

        public event Action<BarChartItem>? ItemClicked;

        public string Title
        {
            get => _title;
            set { _title = value; _canvas.Invalidate(); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; _canvas.Invalidate(); }
        }

        public bool ShowComparison
        {
            get => _showComparison;
            set { _showComparison = value; _canvas.Invalidate(); }
        }

        public BarChart()
        {
            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;
            _canvas.PointerMoved += OnPointerMoved;
            _canvas.PointerPressed += OnPointerPressed;
            Content = _canvas;
        }

        public void SetData(List<BarChartItem> items)
        {
            _items = items ?? new List<BarChartItem>();
            _canvas.Invalidate();
        }

        private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(_canvas).Position;
            var oldHovered = _hoveredIndex;
            _hoveredIndex = GetItemIndexAtPoint((float)pos.X, (float)pos.Y);
            if (oldHovered != _hoveredIndex)
                _canvas.Invalidate();
        }

        private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_hoveredIndex >= 0 && _hoveredIndex < _items.Count)
            {
                ItemClicked?.Invoke(_items[_hoveredIndex]);
            }
        }

        private int GetItemIndexAtPoint(float x, float y)
        {
            if (_items.Count == 0) return -1;

            float width = (float)_canvas.ActualWidth;
            float height = (float)_canvas.ActualHeight;

            float padding = 60;
            float chartWidth = width - padding * 2;
            float barWidth = chartWidth / _items.Count;

            int index = (int)((x - padding) / barWidth);
            if (index >= 0 && index < _items.Count && y > 40 && y < height - 40)
                return index;
            return -1;
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColor.Parse("#1E1E1E"));

            if (_items.Count == 0)
            {
                DrawEmptyState(canvas, info);
                return;
            }

            float width = info.Width;
            float height = info.Height;
            float padding = 60;
            float titleHeight = 40;
            float labelHeight = 50;

            // Draw title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
            };
            canvas.DrawText(_title, padding, 25, titlePaint);

            // Calculate chart area
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - labelHeight;

            // Find max value
            double maxValue = _items.Max(i => Math.Max(i.Value, i.CompareValue ?? 0));
            if (maxValue <= 0) maxValue = 1;

            // Draw grid lines
            DrawGridLines(canvas, chartLeft, chartTop, chartWidth, chartHeight, maxValue);

            // Draw bars
            float barGroupWidth = chartWidth / _items.Count;
            float barPadding = barGroupWidth * 0.15f;
            float barWidth = _showComparison ? (barGroupWidth - barPadding * 2) / 2 : barGroupWidth - barPadding * 2;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float x = chartLeft + i * barGroupWidth + barPadding;
                bool isHovered = i == _hoveredIndex;

                // Main bar
                float barHeight = (float)(item.Value / maxValue * chartHeight);
                var barRect = new SKRect(
                    x,
                    chartTop + chartHeight - barHeight,
                    x + barWidth,
                    chartTop + chartHeight
                );

                var barColor = item.Color;
                if (isHovered) barColor = barColor.WithAlpha(200);

                using var barPaint = new SKPaint
                {
                    Color = barColor,
                    IsAntialias = true
                };
                canvas.DrawRoundRect(barRect, 4, 4, barPaint);

                // Comparison bar
                if (_showComparison && item.CompareValue.HasValue)
                {
                    float compareHeight = (float)(item.CompareValue.Value / maxValue * chartHeight);
                    var compareRect = new SKRect(
                        x + barWidth + 2,
                        chartTop + chartHeight - compareHeight,
                        x + barWidth * 2 + 2,
                        chartTop + chartHeight
                    );

                    using var comparePaint = new SKPaint
                    {
                        Color = item.CompareColor ?? SKColors.Orange,
                        IsAntialias = true
                    };
                    canvas.DrawRoundRect(compareRect, 4, 4, comparePaint);
                }

                // Value label on hover
                if (isHovered)
                {
                    DrawValueTooltip(canvas, barRect.MidX, barRect.Top - 10, item);
                }

                // X-axis label
                using var labelPaint = new SKPaint
                {
                    Color = isHovered ? SKColors.White : SKColors.Gray,
                    TextSize = 10,
                    IsAntialias = true
                };

                var labelText = TruncateLabel(item.Label, 15);
                var labelWidth = labelPaint.MeasureText(labelText);
                canvas.DrawText(labelText, x + barGroupWidth / 2 - barPadding - labelWidth / 2,
                    chartTop + chartHeight + 20, labelPaint);
            }
        }

        private void DrawGridLines(SKCanvas canvas, float left, float top, float width, float height, double maxValue)
        {
            using var gridPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                StrokeWidth = 1,
                IsAntialias = true
            };

            using var labelPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 10,
                IsAntialias = true
            };

            int gridLines = 5;
            for (int i = 0; i <= gridLines; i++)
            {
                float y = top + height - (height * i / gridLines);
                canvas.DrawLine(left, y, left + width, y, gridPaint);

                double value = maxValue * i / gridLines;
                string label = $"{value:F1} {_unit}";
                canvas.DrawText(label, 5, y + 4, labelPaint);
            }
        }

        private void DrawValueTooltip(SKCanvas canvas, float x, float y, BarChartItem item)
        {
            string text = $"{item.Value:F2} {_unit}";
            if (_showComparison && item.CompareValue.HasValue)
            {
                double delta = item.Value - item.CompareValue.Value;
                string sign = delta >= 0 ? "+" : "";
                text += $" ({sign}{delta:F2})";
            }

            using var bgPaint = new SKPaint { Color = SKColor.Parse("#333333") };
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 12,
                IsAntialias = true
            };

            float textWidth = textPaint.MeasureText(text);
            var bgRect = new SKRect(x - textWidth / 2 - 8, y - 20, x + textWidth / 2 + 8, y);
            canvas.DrawRoundRect(bgRect, 4, 4, bgPaint);
            canvas.DrawText(text, x - textWidth / 2, y - 6, textPaint);
        }

        private void DrawEmptyState(SKCanvas canvas, SKImageInfo info)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("No data available", info.Width / 2, info.Height / 2, paint);
        }

        private string TruncateLabel(string label, int maxLength)
        {
            if (label.Length <= maxLength) return label;
            return label.Substring(0, maxLength - 3) + "...";
        }
    }

    /// <summary>
    /// Metric card for displaying a single metric with comparison.
    /// </summary>
    public class MetricCard : UserControl
    {
        private readonly Border _border;
        private readonly StackPanel _content;
        private readonly TextBlock _titleBlock;
        private readonly TextBlock _valueBlock;
        private readonly TextBlock _deltaBlock;
        private readonly TextBlock _ratingBlock;

        public event Action? Clicked;

        public MetricCard()
        {
            _titleBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 150, 150, 150)),
                Margin = new Thickness(0, 0, 0, 4)
            };

            _valueBlock = new TextBlock
            {
                FontSize = 24,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255))
            };

            _deltaBlock = new TextBlock
            {
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0)
            };

            _ratingBlock = new TextBlock
            {
                FontSize = 10,
                Margin = new Thickness(0, 4, 0, 0)
            };

            _content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children = { _titleBlock, _valueBlock, _deltaBlock, _ratingBlock }
            };

            _border = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 40, 40, 40)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Child = _content
            };

            _border.PointerPressed += (s, e) => Clicked?.Invoke();
            _border.PointerEntered += (s, e) =>
            {
                _border.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 50, 50, 50));
            };
            _border.PointerExited += (s, e) =>
            {
                _border.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 40, 40, 40));
            };

            Content = _border;
        }

        public void SetData(string title, double value, string unit, double? compareValue = null)
        {
            _titleBlock.Text = title;
            _valueBlock.Text = $"{value:F2} {unit}";

            if (compareValue.HasValue)
            {
                double delta = value - compareValue.Value;
                double percent = compareValue.Value > 0
                    ? ((compareValue.Value - value) / compareValue.Value) * 100
                    : 0;

                string sign = delta >= 0 ? "+" : "";
                _deltaBlock.Text = $"{sign}{delta:F2} {unit} ({sign}{percent:F1}%)";

                // Green if improved (faster), red if regressed
                bool improved = delta < 0;
                _deltaBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    improved
                        ? Windows.UI.Color.FromArgb(255, 76, 175, 80)
                        : Windows.UI.Color.FromArgb(255, 244, 67, 54));
            }
            else
            {
                _deltaBlock.Text = string.Empty;
            }

            // Performance rating
            var rating = BenchmarkDataService.GetRating(value);
            _ratingBlock.Text = rating.ToString();
            _ratingBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                rating switch
                {
                    PerformanceRating.Excellent => Windows.UI.Color.FromArgb(255, 76, 175, 80),
                    PerformanceRating.Good => Windows.UI.Color.FromArgb(255, 139, 195, 74),
                    PerformanceRating.Acceptable => Windows.UI.Color.FromArgb(255, 255, 193, 7),
                    _ => Windows.UI.Color.FromArgb(255, 244, 67, 54)
                });
        }
    }

    /// <summary>
    /// Comparison table row for detailed view.
    /// </summary>
    public class ComparisonRow : UserControl
    {
        private readonly Grid _grid;

        public ComparisonRow()
        {
            _grid = new Grid
            {
                Padding = new Thickness(8, 4, 8, 4),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 30, 30, 30))
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Content = _grid;
        }

        public void SetData(ComparisonResult result)
        {
            _grid.Children.Clear();

            AddCell(result.OperationName, 0, SKColors.White);
            AddCell($"{result.SessionA_AvgMs:F2} ms", 1, SKColors.DodgerBlue);
            AddCell($"{result.SessionB_AvgMs:F2} ms", 2, SKColors.Orange);

            // Delta with color
            string deltaText = $"{result.DeltaAvgMs:+0.00;-0.00} ms";
            var deltaColor = result.IsFaster
                ? Windows.UI.Color.FromArgb(255, 76, 175, 80)
                : Windows.UI.Color.FromArgb(255, 244, 67, 54);
            AddCell(deltaText, 3, deltaColor);

            // Improvement percentage
            string improvementText = $"{result.ImprovementPercent:+0.0;-0.0}%";
            AddCell(improvementText, 4, deltaColor);
        }

        public void SetHeader()
        {
            _grid.Children.Clear();
            _grid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 40, 40, 40));

            AddCell("Operation", 0, SKColors.White, true);
            AddCell("Session A", 1, SKColors.DodgerBlue, true);
            AddCell("Session B", 2, SKColors.Orange, true);
            AddCell("Delta", 3, SKColors.White, true);
            AddCell("Change", 4, SKColors.White, true);
        }

        private void AddCell(string text, int column, SKColor color, bool isBold = false)
        {
            AddCell(text, column, Windows.UI.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue), isBold);
        }

        private void AddCell(string text, int column, Windows.UI.Color color, bool isBold = false)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(color),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = isBold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
            };
            Grid.SetColumn(textBlock, column);
            _grid.Children.Add(textBlock);
        }
    }
}
