using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace LuxBenchmark.Controls
{
    #region Data Classes

    public class BarChartItem
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public double? CompareValue { get; set; }
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
        public SKColor? CompareColor { get; set; }
        public object? Tag { get; set; }
    }

    public class PieChartSlice
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
    }

    public class LineChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<double> Values { get; set; } = new();
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
    }

    public class BoxPlotItem
    {
        public string Label { get; set; } = string.Empty;
        public double Min { get; set; }
        public double Q1 { get; set; }
        public double Median { get; set; }
        public double Q3 { get; set; }
        public double Max { get; set; }
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
    }

    public class ScatterPoint
    {
        public string Label { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public object? Tag { get; set; }
    }

    public class ConfidenceBandSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<ConfidenceBandPoint> Points { get; set; } = new();
        public SKColor Color { get; set; } = SKColors.DodgerBlue;
    }

    public class ConfidenceBandPoint
    {
        public double Mean { get; set; }
        public double Upper { get; set; }
        public double Lower { get; set; }
    }

    public class WaterfallItem
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public bool IsIncrease { get; set; } = true;
    }

    #endregion

    #region Base Chart Class

    public abstract class BaseSkiaChart : UserControl
    {
        protected readonly Image _image;
        protected readonly Border _border;
        protected string _title = string.Empty;
        protected string _unit = "ms";
        private int _lastWidth = 0;
        private int _lastHeight = 0;

        public string Title
        {
            get => _title;
            set { _title = value; Redraw(); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; Redraw(); }
        }

        protected BaseSkiaChart()
        {
            _image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };

            _border = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 30, 30, 30)),
                Child = _image,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _border.SizeChanged += OnSizeChanged;
            _border.Loaded += (s, e) => Redraw();

            Content = _border;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int newWidth = (int)e.NewSize.Width;
            int newHeight = (int)e.NewSize.Height;

            if (newWidth > 0 && newHeight > 0 && (newWidth != _lastWidth || newHeight != _lastHeight))
            {
                _lastWidth = newWidth;
                _lastHeight = newHeight;
                Redraw();
            }
        }

        protected abstract void OnDraw(SKCanvas canvas, int width, int height);

        public void Invalidate() => Redraw();

        protected void Redraw()
        {
            int width = _lastWidth > 0 ? _lastWidth : (int)_border.ActualWidth;
            int height = _lastHeight > 0 ? _lastHeight : (int)_border.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                width = 400;
                height = 250;
            }

            try
            {
                var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using var surface = SKSurface.Create(info);
                var canvas = surface.Canvas;

                canvas.Clear(SKColor.Parse("#1E1E1E"));
                OnDraw(canvas, width, height);

                using var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                surface.ReadPixels(info, skBitmap.GetPixels(), width * 4, 0, 0);

                var bitmap = new WriteableBitmap(width, height);
                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    var pixels = skBitmap.GetPixelSpan();
                    stream.Write(pixels.ToArray(), 0, pixels.Length);
                }
                bitmap.Invalidate();

                _image.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart render error: {ex.Message}");
            }
        }

        protected void DrawTitle(SKCanvas canvas, int width)
        {
            if (string.IsNullOrEmpty(_title)) return;
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
            };
            canvas.DrawText(_title, 16, 24, paint);
        }

        protected void DrawEmptyState(SKCanvas canvas, int width, int height)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("No data", width / 2f, height / 2f, paint);
        }

        protected static string TruncateLabel(string label, int maxLength)
        {
            if (label.Length <= maxLength) return label;
            return label.Substring(0, maxLength - 2) + "..";
        }
    }

    #endregion

    #region Bar Chart

    public class BarChart : BaseSkiaChart
    {
        private List<BarChartItem> _items = new();
        private bool _showComparison = false;

        #pragma warning disable CS0067
        public event Action<BarChartItem>? ItemClicked;
        #pragma warning restore CS0067

        public bool ShowComparison
        {
            get => _showComparison;
            set { _showComparison = value; Invalidate(); }
        }

        public void SetData(List<BarChartItem> items)
        {
            _items = items ?? new List<BarChartItem>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_items.Count == 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float labelHeight = 60;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - labelHeight;

            double maxValue = _items.Max(i => Math.Max(i.Value, i.CompareValue ?? 0));
            if (maxValue <= 0) maxValue = 1;

            // Grid lines
            DrawGridLines(canvas, chartLeft, chartTop, chartWidth, chartHeight, maxValue);

            // Bars
            float barGroupWidth = chartWidth / _items.Count;
            float barPadding = barGroupWidth * 0.15f;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float x = chartLeft + i * barGroupWidth + barPadding;

                if (_showComparison && item.CompareValue.HasValue)
                {
                    float singleBarWidth = (barGroupWidth - barPadding * 2) / 2 - 2;

                    // Compare bar (Session A)
                    float compareHeight = (float)(item.CompareValue.Value / maxValue * chartHeight);
                    var compareRect = new SKRect(x, chartTop + chartHeight - compareHeight, x + singleBarWidth, chartTop + chartHeight);
                    using var comparePaint = new SKPaint { Color = SKColors.DodgerBlue, IsAntialias = true };
                    canvas.DrawRoundRect(compareRect, 4, 4, comparePaint);

                    // Value bar (Session B)
                    float valueHeight = (float)(item.Value / maxValue * chartHeight);
                    var valueRect = new SKRect(x + singleBarWidth + 4, chartTop + chartHeight - valueHeight, x + singleBarWidth * 2 + 4, chartTop + chartHeight);
                    using var valuePaint = new SKPaint { Color = SKColors.Orange, IsAntialias = true };
                    canvas.DrawRoundRect(valueRect, 4, 4, valuePaint);
                }
                else
                {
                    float barWidth = barGroupWidth - barPadding * 2;
                    float barHeight = (float)(item.Value / maxValue * chartHeight);
                    var barRect = new SKRect(x, chartTop + chartHeight - barHeight, x + barWidth, chartTop + chartHeight);
                    using var barPaint = new SKPaint { Color = item.Color, IsAntialias = true };
                    canvas.DrawRoundRect(barRect, 4, 4, barPaint);
                }

                // Label
                using var labelPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 10,
                    IsAntialias = true
                };
                var label = TruncateLabel(item.Label, 12);
                var labelWidth = labelPaint.MeasureText(label);
                canvas.DrawText(label, x + (barGroupWidth - barPadding * 2) / 2 - labelWidth / 2, chartTop + chartHeight + 20, labelPaint);
            }
        }

        private void DrawGridLines(SKCanvas canvas, float left, float top, float chartWidth, float chartHeight, double maxValue)
        {
            using var gridPaint = new SKPaint { Color = SKColor.Parse("#333333"), StrokeWidth = 1 };
            using var labelPaint = new SKPaint { Color = SKColors.Gray, TextSize = 10, IsAntialias = true };

            for (int i = 0; i <= 5; i++)
            {
                float y = top + chartHeight - (chartHeight * i / 5);
                canvas.DrawLine(left, y, left + chartWidth, y, gridPaint);
                double value = maxValue * i / 5;
                canvas.DrawText($"{value:F1}", 5, y + 4, labelPaint);
            }
        }
    }

    #endregion

    #region Pie Chart

    public class PieChart : BaseSkiaChart
    {
        private List<PieChartSlice> _slices = new();

        public void SetData(List<PieChartSlice> slices)
        {
            _slices = slices ?? new List<PieChartSlice>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_slices.Count == 0 || _slices.Sum(s => s.Value) <= 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float centerX = width * 0.4f;
            float centerY = height / 2f + 10;
            float radius = Math.Min(width * 0.35f, height * 0.4f);
            float innerRadius = radius * 0.5f;

            double total = _slices.Sum(s => s.Value);
            float startAngle = -90;

            foreach (var slice in _slices)
            {
                float sweepAngle = (float)(slice.Value / total * 360);

                using var path = new SKPath();
                path.MoveTo(centerX + innerRadius * (float)Math.Cos(startAngle * Math.PI / 180),
                           centerY + innerRadius * (float)Math.Sin(startAngle * Math.PI / 180));
                path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
                          startAngle, sweepAngle, false);
                path.ArcTo(new SKRect(centerX - innerRadius, centerY - innerRadius, centerX + innerRadius, centerY + innerRadius),
                          startAngle + sweepAngle, -sweepAngle, false);
                path.Close();

                using var paint = new SKPaint { Color = slice.Color, IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawPath(path, paint);

                startAngle += sweepAngle;
            }

            // Legend
            float legendX = width * 0.7f;
            float legendY = 50;
            using var legendPaint = new SKPaint { Color = SKColors.White, TextSize = 11, IsAntialias = true };

            foreach (var slice in _slices)
            {
                using var colorPaint = new SKPaint { Color = slice.Color, IsAntialias = true };
                canvas.DrawRect(legendX, legendY - 8, 12, 12, colorPaint);

                double pct = slice.Value / total * 100;
                canvas.DrawText($"{slice.Label} ({pct:F1}%)", legendX + 18, legendY, legendPaint);
                legendY += 20;
            }
        }
    }

    #endregion

    #region Line Chart

    public class LineChart : BaseSkiaChart
    {
        private List<LineChartSeries> _series = new();
        private string _xAxisLabel = "Sample";

        public string XAxisLabel
        {
            get => _xAxisLabel;
            set { _xAxisLabel = value; Invalidate(); }
        }

        public void SetData(List<LineChartSeries> series)
        {
            _series = series ?? new List<LineChartSeries>();
            Invalidate();
        }

        public void Clear() { _series.Clear(); Invalidate(); }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_series.Count == 0 || _series.All(s => s.Values.Count == 0))
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float legendHeight = 30;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - legendHeight - 20;

            double minValue = _series.SelectMany(s => s.Values).Min();
            double maxValue = _series.SelectMany(s => s.Values).Max();
            if (maxValue <= minValue) maxValue = minValue + 1;
            int maxPoints = _series.Max(s => s.Values.Count);

            // Grid
            using var gridPaint = new SKPaint { Color = SKColor.Parse("#333333"), StrokeWidth = 1 };
            for (int i = 0; i <= 5; i++)
            {
                float y = chartTop + chartHeight - (chartHeight * i / 5);
                canvas.DrawLine(chartLeft, y, chartLeft + chartWidth, y, gridPaint);
            }

            // Series
            foreach (var series in _series)
            {
                if (series.Values.Count < 2) continue;

                using var linePaint = new SKPaint
                {
                    Color = series.Color,
                    StrokeWidth = 2,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                var path = new SKPath();
                for (int i = 0; i < series.Values.Count; i++)
                {
                    float x = chartLeft + (i / (float)(maxPoints - 1)) * chartWidth;
                    float y = chartTop + chartHeight - (float)((series.Values[i] - minValue) / (maxValue - minValue) * chartHeight);
                    if (i == 0) path.MoveTo(x, y);
                    else path.LineTo(x, y);
                }
                canvas.DrawPath(path, linePaint);
            }

            // Legend
            float legendX = chartLeft;
            float legendY = height - 15;
            using var legendPaint = new SKPaint { Color = SKColors.White, TextSize = 11, IsAntialias = true };
            foreach (var series in _series)
            {
                using var colorPaint = new SKPaint { Color = series.Color };
                canvas.DrawRect(legendX, legendY - 8, 12, 3, colorPaint);
                canvas.DrawText(series.Name, legendX + 16, legendY, legendPaint);
                legendX += legendPaint.MeasureText(series.Name) + 30;
            }
        }
    }

    #endregion

    #region Histogram

    public class Histogram : BaseSkiaChart
    {
        private List<double> _values = new();
        private int _binCount = 20;

        public int BinCount
        {
            get => _binCount;
            set { _binCount = Math.Max(5, Math.Min(50, value)); Invalidate(); }
        }

        public void SetData(List<double> values)
        {
            _values = values ?? new List<double>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_values.Count == 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - 50;

            double minValue = _values.Min();
            double maxValue = _values.Max();
            if (maxValue <= minValue) maxValue = minValue + 1;

            double binWidth = (maxValue - minValue) / _binCount;
            var binCounts = new int[_binCount];
            foreach (var value in _values)
            {
                int binIndex = Math.Min((int)((value - minValue) / binWidth), _binCount - 1);
                binCounts[binIndex]++;
            }

            int maxBinCount = binCounts.Max();
            if (maxBinCount == 0) maxBinCount = 1;

            float barWidthPx = chartWidth / _binCount;
            for (int i = 0; i < _binCount; i++)
            {
                float barHeight = (float)binCounts[i] / maxBinCount * chartHeight;
                float x = chartLeft + i * barWidthPx;
                var rect = new SKRect(x + 1, chartTop + chartHeight - barHeight, x + barWidthPx - 1, chartTop + chartHeight);
                using var paint = new SKPaint { Color = SKColor.Parse("#2196F3"), IsAntialias = true };
                canvas.DrawRect(rect, paint);
            }

            // Stats overlay
            using var statsPaint = new SKPaint { Color = SKColors.White, TextSize = 10, IsAntialias = true };
            double mean = _values.Average();
            double stdDev = Math.Sqrt(_values.Average(v => Math.Pow(v - mean, 2)));
            canvas.DrawText($"n={_values.Count}  μ={mean:F2}  σ={stdDev:F2}", width - 150, 25, statsPaint);
        }
    }

    #endregion

    #region Box Plot

    public class BoxPlot : BaseSkiaChart
    {
        private List<BoxPlotItem> _items = new();

        public void SetData(List<BoxPlotItem> items)
        {
            _items = items ?? new List<BoxPlotItem>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_items.Count == 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - 50;

            double minValue = _items.Min(i => i.Min);
            double maxValue = _items.Max(i => i.Max);
            if (maxValue <= minValue) maxValue = minValue + 1;

            float itemWidth = chartWidth / _items.Count;
            float boxWidth = itemWidth * 0.5f;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float centerX = chartLeft + i * itemWidth + itemWidth / 2;

                float yMin = chartTop + chartHeight - (float)((item.Min - minValue) / (maxValue - minValue) * chartHeight);
                float yQ1 = chartTop + chartHeight - (float)((item.Q1 - minValue) / (maxValue - minValue) * chartHeight);
                float yMedian = chartTop + chartHeight - (float)((item.Median - minValue) / (maxValue - minValue) * chartHeight);
                float yQ3 = chartTop + chartHeight - (float)((item.Q3 - minValue) / (maxValue - minValue) * chartHeight);
                float yMax = chartTop + chartHeight - (float)((item.Max - minValue) / (maxValue - minValue) * chartHeight);

                using var whiskerPaint = new SKPaint { Color = item.Color, StrokeWidth = 2, IsAntialias = true };

                // Whiskers
                canvas.DrawLine(centerX, yMin, centerX, yQ1, whiskerPaint);
                canvas.DrawLine(centerX, yQ3, centerX, yMax, whiskerPaint);
                canvas.DrawLine(centerX - boxWidth * 0.3f, yMin, centerX + boxWidth * 0.3f, yMin, whiskerPaint);
                canvas.DrawLine(centerX - boxWidth * 0.3f, yMax, centerX + boxWidth * 0.3f, yMax, whiskerPaint);

                // Box
                var boxRect = new SKRect(centerX - boxWidth / 2, yQ3, centerX + boxWidth / 2, yQ1);
                using var boxPaint = new SKPaint { Color = item.Color.WithAlpha(150), IsAntialias = true };
                canvas.DrawRect(boxRect, boxPaint);
                canvas.DrawRect(boxRect, whiskerPaint);

                // Median
                using var medianPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 3, IsAntialias = true };
                canvas.DrawLine(centerX - boxWidth / 2, yMedian, centerX + boxWidth / 2, yMedian, medianPaint);

                // Label
                using var labelPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9, IsAntialias = true };
                var label = TruncateLabel(item.Label, 10);
                canvas.DrawText(label, centerX - labelPaint.MeasureText(label) / 2, chartTop + chartHeight + 15, labelPaint);
            }
        }
    }

    #endregion

    #region Scatter Plot

    public class ScatterPlot : BaseSkiaChart
    {
        private List<ScatterPoint> _points = new();
        private string _xLabel = "Session A";
        private string _yLabel = "Session B";

        public string XLabel { get => _xLabel; set { _xLabel = value; Invalidate(); } }
        public string YLabel { get => _yLabel; set { _yLabel = value; Invalidate(); } }

        public void SetData(List<ScatterPoint> points)
        {
            _points = points ?? new List<ScatterPoint>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_points.Count == 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 60;
            float titleHeight = 40;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - 50;

            double minX = _points.Min(p => p.X);
            double maxX = _points.Max(p => p.X);
            double minY = _points.Min(p => p.Y);
            double maxY = _points.Max(p => p.Y);
            if (maxX <= minX) maxX = minX + 1;
            if (maxY <= minY) maxY = minY + 1;

            // Diagonal line (y = x)
            using var diagPaint = new SKPaint
            {
                Color = SKColor.Parse("#555555"),
                StrokeWidth = 1,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            };
            double diagMin = Math.Max(minX, minY);
            double diagMax = Math.Min(maxX, maxY);
            if (diagMax > diagMin)
            {
                float x1 = chartLeft + (float)((diagMin - minX) / (maxX - minX) * chartWidth);
                float y1 = chartTop + chartHeight - (float)((diagMin - minY) / (maxY - minY) * chartHeight);
                float x2 = chartLeft + (float)((diagMax - minX) / (maxX - minX) * chartWidth);
                float y2 = chartTop + chartHeight - (float)((diagMax - minY) / (maxY - minY) * chartHeight);
                canvas.DrawLine(x1, y1, x2, y2, diagPaint);
            }

            // Points
            foreach (var point in _points)
            {
                float x = chartLeft + (float)((point.X - minX) / (maxX - minX) * chartWidth);
                float y = chartTop + chartHeight - (float)((point.Y - minY) / (maxY - minY) * chartHeight);
                var color = point.Y < point.X ? SKColor.Parse("#4CAF50") : SKColor.Parse("#F44336");
                using var pointPaint = new SKPaint { Color = color, IsAntialias = true };
                canvas.DrawCircle(x, y, 6, pointPaint);
            }
        }
    }

    #endregion

    #region Confidence Band Chart

    public class ConfidenceBandChart : BaseSkiaChart
    {
        private List<ConfidenceBandSeries> _series = new();

        public void SetData(List<ConfidenceBandSeries> series)
        {
            _series = series ?? new List<ConfidenceBandSeries>();
            Invalidate();
        }

        public void Clear() { _series.Clear(); Invalidate(); }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_series.Count == 0 || _series.All(s => s.Points.Count == 0))
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - 50;

            double minValue = _series.SelectMany(s => s.Points.Select(p => p.Lower)).Min();
            double maxValue = _series.SelectMany(s => s.Points.Select(p => p.Upper)).Max();
            if (maxValue <= minValue) { minValue = 0; maxValue = 1; }
            int maxPoints = _series.Max(s => s.Points.Count);

            foreach (var series in _series)
            {
                if (series.Points.Count < 2) continue;

                // Band
                var bandPath = new SKPath();
                for (int i = 0; i < series.Points.Count; i++)
                {
                    float x = chartLeft + (i / (float)(maxPoints - 1)) * chartWidth;
                    float yUpper = chartTop + chartHeight - (float)((series.Points[i].Upper - minValue) / (maxValue - minValue) * chartHeight);
                    if (i == 0) bandPath.MoveTo(x, yUpper);
                    else bandPath.LineTo(x, yUpper);
                }
                for (int i = series.Points.Count - 1; i >= 0; i--)
                {
                    float x = chartLeft + (i / (float)(maxPoints - 1)) * chartWidth;
                    float yLower = chartTop + chartHeight - (float)((series.Points[i].Lower - minValue) / (maxValue - minValue) * chartHeight);
                    bandPath.LineTo(x, yLower);
                }
                bandPath.Close();

                using var bandPaint = new SKPaint { Color = series.Color.WithAlpha(50), IsAntialias = true };
                canvas.DrawPath(bandPath, bandPaint);

                // Mean line
                var meanPath = new SKPath();
                for (int i = 0; i < series.Points.Count; i++)
                {
                    float x = chartLeft + (i / (float)(maxPoints - 1)) * chartWidth;
                    float y = chartTop + chartHeight - (float)((series.Points[i].Mean - minValue) / (maxValue - minValue) * chartHeight);
                    if (i == 0) meanPath.MoveTo(x, y);
                    else meanPath.LineTo(x, y);
                }
                using var linePaint = new SKPaint { Color = series.Color, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
                canvas.DrawPath(meanPath, linePaint);
            }
        }
    }

    #endregion

    #region Waterfall Chart

    public class WaterfallChart : BaseSkiaChart
    {
        private List<WaterfallItem> _items = new();

        public void SetData(List<WaterfallItem> items)
        {
            _items = items ?? new List<WaterfallItem>();
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            if (_items.Count == 0)
            {
                DrawEmptyState(canvas, width, height);
                return;
            }

            float padding = 50;
            float titleHeight = 40;
            float chartLeft = padding;
            float chartTop = titleHeight;
            float chartWidth = width - padding * 2;
            float chartHeight = height - titleHeight - 60;

            double total = _items.Sum(i => i.Value);
            double maxValue = Math.Max(total, _items.Max(i => i.Value)) * 1.1;

            float barWidthPx = chartWidth / (_items.Count + 1) * 0.7f;
            float barGroupWidth = chartWidth / (_items.Count + 1);

            double cumulative = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float x = chartLeft + i * barGroupWidth + (barGroupWidth - barWidthPx) / 2;
                float yStart = chartTop + chartHeight - (float)(cumulative / maxValue * chartHeight);
                float yEnd = chartTop + chartHeight - (float)((cumulative + item.Value) / maxValue * chartHeight);

                var rect = new SKRect(x, yEnd, x + barWidthPx, yStart);
                using var paint = new SKPaint { Color = SKColor.Parse("#2196F3"), IsAntialias = true };
                canvas.DrawRoundRect(rect, 4, 4, paint);

                cumulative += item.Value;

                using var labelPaint = new SKPaint { Color = SKColors.Gray, TextSize = 9, IsAntialias = true };
                var label = TruncateLabel(item.Label, 8);
                canvas.DrawText(label, x + barWidthPx / 2 - labelPaint.MeasureText(label) / 2, chartTop + chartHeight + 15, labelPaint);
            }

            // Total bar
            float totalX = chartLeft + _items.Count * barGroupWidth + (barGroupWidth - barWidthPx) / 2;
            float totalHeight = (float)(total / maxValue * chartHeight);
            var totalRect = new SKRect(totalX, chartTop + chartHeight - totalHeight, totalX + barWidthPx, chartTop + chartHeight);
            using var totalPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            canvas.DrawRoundRect(totalRect, 4, 4, totalPaint);

            using var totalLabelPaint = new SKPaint { Color = SKColors.White, TextSize = 10, IsAntialias = true, Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) };
            canvas.DrawText("Total", totalX + barWidthPx / 2 - totalLabelPaint.MeasureText("Total") / 2, chartTop + chartHeight + 15, totalLabelPaint);
        }
    }

    #endregion

    #region Gauge Chart

    public class GaugeChart : BaseSkiaChart
    {
        private double _value;
        private double _minValue;
        private double _maxValue = 100;
        private string _label = string.Empty;

        public double MinValue { get => _minValue; set { _minValue = value; Invalidate(); } }
        public double MaxValue { get => _maxValue; set { _maxValue = value; Invalidate(); } }
        public string Label { get => _label; set { _label = value; Invalidate(); } }

        public void SetValue(double value, string? label = null)
        {
            _value = value;
            if (label != null) _label = label;
            Invalidate();
        }

        protected override void OnDraw(SKCanvas canvas, int width, int height)
        {
            DrawTitle(canvas, width);

            float centerX = width / 2f;
            float centerY = height / 2f + 20;
            float radius = Math.Min(width, height) / 2f - 40;
            float arcWidth = 20;

            // Background arc
            using var bgPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                StrokeWidth = arcWidth,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round
            };
            var arcRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            canvas.DrawArc(arcRect, 135, 270, false, bgPaint);

            // Value arc
            double normalizedValue = Math.Max(0, Math.Min(1, (_value - _minValue) / (_maxValue - _minValue)));
            var valueColor = GetColorForValue(normalizedValue);
            using var valuePaint = new SKPaint
            {
                Color = valueColor,
                StrokeWidth = arcWidth,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round
            };
            canvas.DrawArc(arcRect, 135, (float)(normalizedValue * 270), false, valuePaint);

            // Value text
            using var valueLabelPaint = new SKPaint
            {
                Color = valueColor,
                TextSize = 24,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold),
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText($"{_value:F1} {_unit}", centerX, centerY + 10, valueLabelPaint);

            if (!string.IsNullOrEmpty(_label))
            {
                using var labelPaint = new SKPaint
                {
                    Color = valueColor,
                    TextSize = 14,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText(_label, centerX, centerY + 30, labelPaint);
            }
        }

        private SKColor GetColorForValue(double normalizedValue)
        {
            if (normalizedValue <= 0.25) return SKColor.Parse("#4CAF50");
            if (normalizedValue <= 0.5) return SKColor.Parse("#8BC34A");
            if (normalizedValue <= 0.75) return SKColor.Parse("#FFC107");
            return SKColor.Parse("#F44336");
        }
    }

    #endregion

    #region Metric Card

    public class MetricCard : UserControl
    {
        private readonly Border _border;
        private readonly TextBlock _titleBlock;
        private readonly TextBlock _valueBlock;
        private readonly TextBlock _deltaBlock;
        private readonly TextBlock _ratingBlock;

        public event Action? Clicked;

        public MetricCard()
        {
            _titleBlock = new TextBlock { FontSize = 12, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150)) };
            _valueBlock = new TextBlock { FontSize = 24, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) };
            _deltaBlock = new TextBlock { FontSize = 12, Margin = new Thickness(0, 4, 0, 0) };
            _ratingBlock = new TextBlock { FontSize = 10, Margin = new Thickness(0, 4, 0, 0) };

            var content = new StackPanel { Orientation = Orientation.Vertical, Children = { _titleBlock, _valueBlock, _deltaBlock, _ratingBlock } };
            _border = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 40, 40, 40)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Child = content
            };
            _border.PointerPressed += (s, e) => Clicked?.Invoke();
            Content = _border;
        }

        public void SetData(string title, double value, string unit, double? compareValue = null)
        {
            _titleBlock.Text = title;
            _valueBlock.Text = $"{value:F2} {unit}";

            if (compareValue.HasValue)
            {
                double delta = value - compareValue.Value;
                double percent = compareValue.Value > 0 ? ((compareValue.Value - value) / compareValue.Value) * 100 : 0;
                _deltaBlock.Text = $"{(delta >= 0 ? "+" : "")}{delta:F2} {unit} ({percent:F1}%)";
                _deltaBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(delta < 0 ? Windows.UI.Color.FromArgb(255, 76, 175, 80) : Windows.UI.Color.FromArgb(255, 244, 67, 54));
            }
            else _deltaBlock.Text = string.Empty;

            var rating = BenchmarkDataService.GetRating(value);
            _ratingBlock.Text = rating.ToString();
            _ratingBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(rating switch
            {
                PerformanceRating.Excellent => Windows.UI.Color.FromArgb(255, 76, 175, 80),
                PerformanceRating.Good => Windows.UI.Color.FromArgb(255, 139, 195, 74),
                PerformanceRating.Acceptable => Windows.UI.Color.FromArgb(255, 255, 193, 7),
                _ => Windows.UI.Color.FromArgb(255, 244, 67, 54)
            });
        }
    }

    #endregion

    #region Comparison Row

    public class ComparisonRow : UserControl
    {
        private readonly Grid _grid;

        public ComparisonRow()
        {
            _grid = new Grid { Padding = new Thickness(8, 4, 8, 4), Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30)) };
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
            var deltaColor = result.IsFaster ? Windows.UI.Color.FromArgb(255, 76, 175, 80) : Windows.UI.Color.FromArgb(255, 244, 67, 54);
            AddCell($"{result.DeltaAvgMs:+0.00;-0.00} ms", 3, deltaColor);
            AddCell($"{result.ImprovementPercent:+0.0;-0.0}%", 4, deltaColor);
        }

        public void SetHeader()
        {
            _grid.Children.Clear();
            _grid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 40, 40, 40));
            AddCell("Operation", 0, SKColors.White, true);
            AddCell("Session A", 1, SKColors.DodgerBlue, true);
            AddCell("Session B", 2, SKColors.Orange, true);
            AddCell("Delta", 3, SKColors.White, true);
            AddCell("Change", 4, SKColors.White, true);
        }

        private void AddCell(string text, int column, SKColor color, bool isBold = false) =>
            AddCell(text, column, Windows.UI.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue), isBold);

        private void AddCell(string text, int column, Windows.UI.Color color, bool isBold = false)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(color),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = isBold ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
            };
            Grid.SetColumn(tb, column);
            _grid.Children.Add(tb);
        }
    }

    #endregion
}
