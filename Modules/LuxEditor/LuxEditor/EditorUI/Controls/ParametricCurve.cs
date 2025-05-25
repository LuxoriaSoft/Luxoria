using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>
    /// Parametric tone‑curve control reproducing Lightroom behaviour.
    /// Four region sliders (Shadows, Darks, Lights, Highlights) + three threshold handles.
    /// Generates a 256‑entry LUT and draws the preview from that same LUT, ensuring
    /// 1‑to‑1 fidelity with the processing pipeline.
    /// </summary>
    public sealed class ParametricCurve : CurveBase
    {
        private readonly ThresholdBar _bar;
        private readonly Slider _high, _light, _dark, _shadow;

        private bool _dragging;
        private double _startY;
        private int _activeRegion;

        private readonly double[] _x = new double[5];
        private readonly double[] _y = new double[5];
        private double _lastSlope = 1.0;
        private byte[] _lut256 = new byte[256];

        private readonly double[] _m = new double[5];
        private readonly double[] _d = new double[4];

        public ParametricCurve()
        {
            var root = new StackPanel { Spacing = 8 };

            _canvas.Height = 230;
            _canvas.PointerPressed += GridDown;
            _canvas.PointerMoved += GridMove;
            _canvas.PointerReleased += GridUp;
            root.Children.Add(_canvas);

            _bar = new ThresholdBar();
            root.Children.Add(_bar);

            var col = new StackPanel { Spacing = 4 };
            _high = RegionSlider("Highlights", col);
            _light = RegionSlider("Lights", col);
            _dark = RegionSlider("Darks", col);
            _shadow = RegionSlider("Shadows", col);
            root.Children.Add(col);

            Content = root;
            VerticalAlignment = VerticalAlignment.Top;
            Height = double.NaN;

            _bar.ThresholdChanged += (_, __, ___) => { RebuildAnchors(); Redraw(); };
            _high.ValueChanged += (_, __) => { RebuildAnchors(); Redraw(); };
            _light.ValueChanged += (_, __) => { RebuildAnchors(); Redraw(); };
            _dark.ValueChanged += (_, __) => { RebuildAnchors(); Redraw(); };
            _shadow.ValueChanged += (_, __) => { RebuildAnchors(); Redraw(); };

            RebuildAnchors();
            Redraw();
        }

        private static Slider RegionSlider(string label, Panel host)
        {
            var s = new Slider
            {
                Minimum = -100,
                Maximum = 100,
                StepFrequency = 1,
                MinWidth = 180,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var stack = new StackPanel { Spacing = 2 };
            stack.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255,255,255,255))
            });
            stack.Children.Add(s);
            host.Children.Add(stack);
            return s;
        }

        private void GridDown(object sender, PointerRoutedEventArgs e)
        {
            _dragging = true;
            _startY = e.GetCurrentPoint(_canvas).Position.Y;
            _activeRegion = RegionFromX(e.GetCurrentPoint(_canvas).Position.X);
            _canvas.CapturePointer(e.Pointer);
        }
        private void GridMove(object sender, PointerRoutedEventArgs e)
        {
            if (!_dragging) return;
            double y = e.GetCurrentPoint(_canvas).Position.Y;
            double delta = (_startY - y) / _canvas.Height * 200;
            ApplyDelta(delta);
            _startY = y;
        }
        private void GridUp(object sender, PointerRoutedEventArgs e)
        { _dragging = false; _canvas.ReleasePointerCaptures(); }

        private int RegionFromX(double x)
        {
            double w = _canvas.ActualWidth;
            double t1 = _bar.T1 * w, t2 = _bar.T2 * w, t3 = _bar.T3 * w;
            return x < t1 ? 3 : x < t2 ? 2 : x < t3 ? 1 : 0;
        }
        private void ApplyDelta(double d)
        {
            Slider s = _activeRegion switch { 0 => _high, 1 => _light, 2 => _dark, _ => _shadow };
            s.Value = Math.Clamp(s.Value + d, s.Minimum, s.Maximum);
        }

        private void Redraw() => _canvas.Invalidate();

        private void RebuildAnchors()
        {
            _x[0] = 0;
            _x[1] = _bar.T1;
            _x[2] = _bar.T2;
            _x[3] = _bar.T3;
            _x[4] = 1;

            const double kD = 0.25 / 100.0;
            const double kS = 0.18 / 100.0;

            double s = _shadow.Value * kS;
            double d = _dark.Value * kD;
            double l = _light.Value * kD;
            double h = _high.Value * kD;

            _y[0] = 0;
            _y[1] = _x[1] + s + 0.5 * d;
            _y[2] = _x[2] + d + 0.5 * (s + l);
            _y[3] = _x[3] + l + 0.5 * d + 0.8 * h;
            _y[4] = 1;

            for (int i = 1; i < 4; i++)
                _y[i] = Math.Clamp(_y[i], 0, 1);

            _lastSlope = 1 + 0.8 * h;

            BuildPreviewLUT();
            ComputeSlopes();
            NotifyCurveChanged();
        }

        private void BuildPreviewLUT()
        {
            for (int i = 0; i < 256; i++)
            {
                double x = i / 255.0;
                double y = SplineY(x);
                _lut256[i] = (byte)Math.Round(Math.Clamp(y, 0, 1) * 255);
            }
        }

        private void ComputeSlopes()
        {
            for (int i = 0; i < 4; i++)
                _d[i] = (_y[i + 1] - _y[i]) / (_x[i + 1] - _x[i]);

            _m[0] = _d[0];
            _m[4] = _lastSlope;
            for (int i = 1; i < 4; i++)
                _m[i] = (_d[i - 1] + _d[i]) * 0.5;

            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(_d[i]) < 1e-9)
                { _m[i] = _m[i + 1] = 0; continue; }

                double a = _m[i] / _d[i];
                double b = _m[i + 1] / _d[i];
                double h = Double.Hypot(a, b);
                if (h > 3)
                {
                    double t = 3 / h;
                    _m[i] = t * a * _d[i];
                    _m[i + 1] = t * b * _d[i];
                }
            }
        }

        private double SplineY(double x)
        {
            int i = x < _x[1] ? 0 : x < _x[2] ? 1 : x < _x[3] ? 2 : 3;

            double x0 = _x[i];
            double x1 = _x[i + 1];
            double h = x1 - x0;
            double t = (x - x0) / h;
            double t2 = t * t;
            double t3 = t2 * t;

            double h00 = 2 * t3 - 3 * t2 + 1;
            double h10 = t3 - 2 * t2 + t;
            double h01 = -2 * t3 + 3 * t2;
            double h11 = t3 - t2;

            return h00 * _y[i] +
                   h10 * h * _m[i] +
                   h01 * _y[i + 1] +
                   h11 * h * _m[i + 1];
        }

        protected override void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            int w = e.Info.Width;
            int h = e.Info.Height;
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var grid = new SKPaint { Color = new SKColor(80, 80, 80), StrokeWidth = 1 };
            for (int i = 1; i < 4; i++)
            {
                canvas.DrawLine(i * w / 4, 0, i * w / 4, h, grid);
                canvas.DrawLine(0, i * h / 4, w, i * h / 4, grid);
            }

            using var vline = new SKPaint { Color = new SKColor(100, 100, 100), StrokeWidth = 1 };
            canvas.DrawLine(_bar.T1 * w, 0, _bar.T1 * w, h, vline);
            canvas.DrawLine(_bar.T2 * w, 0, _bar.T2 * w, h, vline);
            canvas.DrawLine(_bar.T3 * w, 0, _bar.T3 * w, h, vline);

            var path = new SKPath();
            path.MoveTo(0, h - _lut256[0] / 255f * h);
            for (int i = 1; i < 256; i++)
            {
                float x = i * (w - 1) / 255f;
                float y = h - _lut256[i] / 255f * h;
                path.LineTo(x, y);
            }

            using var white = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            canvas.DrawPath(path, white);
        }
    }
}
