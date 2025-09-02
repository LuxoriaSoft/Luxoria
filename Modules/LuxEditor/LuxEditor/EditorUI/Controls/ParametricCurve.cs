using LuxEditor.EditorUI.Controls;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class ParametricCurve : CurveBase
    {
        private readonly ThresholdBar _bar;
        private readonly EditorSlider _high, _light, _dark, _shadow;

        private bool _dragging;
        private double _startY;
        private int _activeRegion;
        private int _hoverRegion = -1;

        private readonly double[] _x = new double[5];
        private readonly byte[] _lut = new byte[256];
        private readonly byte[] _envL = new byte[256];
        private readonly byte[] _envH = new byte[256];

        private DateTime _lastTap;

        public override string SettingKey => "ToneCurve_Parametric";

        private bool _isLayer = false;

        /// <summary>
        /// Initialises the UI and draws the first curve.
        /// </summary>
        public ParametricCurve(bool isLayer)
        {
            _isLayer = isLayer;
            var root = new StackPanel { Spacing = 8 };

            _canvas.Height = 230;
            _canvas.PointerPressed += GridDown;
            _canvas.PointerMoved += GridMove;
            _canvas.PointerReleased += GridUp;
            _canvas.PointerEntered += (s, e) => UpdateHover(e.GetCurrentPoint(_canvas).Position.X);
            _canvas.PointerExited += (s, e) => SetHover(-1);
            root.Children.Add(_canvas);

            _bar = new ThresholdBar();
            _bar.ThresholdChanged += (_, __, ___) => UpdateCurve();
            root.Children.Add(_bar);

            var slidersPanel = new StackPanel { Spacing = 4 };
            _high = CreateSlider("Highlights", 0, slidersPanel);
            _light = CreateSlider("Lights", 1, slidersPanel);
            _dark = CreateSlider("Darks", 2, slidersPanel);
            _shadow = CreateSlider("Shadows", 3, slidersPanel);
            root.Children.Add(slidersPanel);

            Content = root;
            VerticalAlignment = VerticalAlignment.Top;
            Height = double.NaN;

            var img = ImageManager.Instance.SelectedImage;
            if (img != null)
                RefreshCurve(img.Settings);
            else
                UpdateCurve();
        }


        /// <summary>
        /// Creates an <see cref="EditorSlider"/> wired to events.
        /// </summary>
        private EditorSlider CreateSlider(string label, int region, Panel host)
        {
            var slider = new EditorSlider(label, -100, 100, 0, 0, 1f, false)
            {
                OnValueChanged = _ => UpdateCurve(),
                RequestSaveState = () =>
                {
                    Debug.WriteLine("Requesting save state for " + label);
                    
                    ImageManager.Instance.SelectedImage?.SaveState(true);
                }
            };

            var element = slider.GetElement();
            element.PointerEntered += (_, __) => SetHover(region);
            element.PointerExited += (_, __) => SetHover(-1);

            host.Children.Add(element);
            return slider;
        }

        /// <summary>
        /// Handles pointer-down on the grid (drag or double-click reset).
        /// </summary>
        private void GridDown(object sender, PointerRoutedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var pt = e.GetCurrentPoint(_canvas).Position;
            _activeRegion = RegionFromX(pt.X);

            if ((now - _lastTap).TotalMilliseconds < 350)
            {
                GetSlider(_activeRegion).ResetToDefault();
                UpdateCurve();
                _lastTap = DateTime.MinValue;
                return;
            }

            _lastTap = now;
            _dragging = true;
            _startY = pt.Y;
            _canvas.CapturePointer(e.Pointer);
        }

        /// <summary>
        /// Handles pointer-move for dragging and hover tracking.
        /// </summary>
        private void GridMove(object sender, PointerRoutedEventArgs e)
        {
            if (!_dragging)
            {
                UpdateHover(e.GetCurrentPoint(_canvas).Position.X);
                return;
            }

            double y = e.GetCurrentPoint(_canvas).Position.Y;
            double delta = (_startY - y) / _canvas.Height * 200;
            var sldr = GetSlider(_activeRegion);
            sldr.SetValue(Math.Clamp(sldr.GetValue() + (float)delta, -100, 100));

            _startY = y;
            UpdateCurve();
        }

        /// <summary>
        /// Releases the drag operation.
        /// </summary>
        private void GridUp(object sender, PointerRoutedEventArgs e)
        {
            _dragging = false;
            _canvas.ReleasePointerCaptures();   
            ImageManager.Instance.SelectedImage?.SaveState();
        }

        /// <summary>
        /// Returns the slider associated with a region index.
        /// </summary>
        private EditorSlider GetSlider(int region) =>
            region switch { 0 => _high, 1 => _light, 2 => _dark, _ => _shadow };

        /// <summary>
        /// Maps an X coordinate to its region index.
        /// </summary>
        private int RegionFromX(double x)
        {
            double w = _canvas.ActualWidth;
            double t1 = _bar.T1 * w;
            double t2 = _bar.T2 * w;
            double t3 = _bar.T3 * w;
            return x < t1 ? 3 : x < t2 ? 2 : x < t3 ? 1 : 0;
        }

        /// <summary>
        /// Sets the current hover region and invalidates the canvas.
        /// </summary>
        private void SetHover(int region)
        {
            if (_hoverRegion == region) return;
            _hoverRegion = region;
            if (region != -1) RecomputeEnvelope();
            _canvas.Invalidate();
        }

        /// <summary>
        /// Updates hover state from an X coordinate.
        /// </summary>
        private void UpdateHover(double x) => SetHover(RegionFromX(x));


        private void UpdateCurve()
        {
            if (!_isLayer)
            {
                ImageManager.Instance.SelectedImage.Settings[SettingKey + "_Shadow_Value"] = _shadow.GetValue();
                ImageManager.Instance.SelectedImage.Settings[SettingKey + "_Dark_Value"] = _dark.GetValue();
                ImageManager.Instance.SelectedImage.Settings[SettingKey + "_Light_Value"] = _light.GetValue();
                ImageManager.Instance.SelectedImage.Settings[SettingKey + "_High_Value"] = _high.GetValue();

                ImageManager.Instance.SelectedImage.Settings[SettingKey + "_Thresholds"] =
                    new List<float> { _bar.T1, _bar.T2, _bar.T3 };

            }
            BuildLut(
                _lut,
                _shadow.GetValue(),
                _dark.GetValue(),
                _light.GetValue(),
                _high.GetValue()
            );
            if (!_isLayer)
                ImageManager.Instance.SelectedImage.Settings[SettingKey] = GetLut();


            if (_hoverRegion != -1)
                RecomputeEnvelope();

            NotifyCurveChanged();
            _canvas.Invalidate();
        }


        /// <summary>
        /// Calculates the min/max envelope for the current hover region.
        /// </summary>
        private void RecomputeEnvelope()
        {
            if (_hoverRegion == -1) return;

            var s = _shadow.GetValue();
            var d = _dark.GetValue();
            var l = _light.GetValue();
            var h = _high.GetValue();

            BuildLut(_envL,
                     _hoverRegion == 3 ? -100 : s,
                     _hoverRegion == 2 ? -100 : d,
                     _hoverRegion == 1 ? -100 : l,
                     _hoverRegion == 0 ? -100 : h);

            BuildLut(_envH,
                     _hoverRegion == 3 ? 100 : s,
                     _hoverRegion == 2 ? 100 : d,
                     _hoverRegion == 1 ? 100 : l,
                     _hoverRegion == 0 ? 100 : h);
        }

        /// <summary>
        /// Generates a 256-entry LUT for given slider values.
        /// </summary>
        private void BuildLut(byte[] dst, double sVal, double dVal, double lVal, double hVal)
        {
            Span<double> y = stackalloc double[5];
            Span<double> m = stackalloc double[5];
            Span<double> d = stackalloc double[4];

            _x[0] = 0; _x[1] = _bar.T1; _x[2] = _bar.T2; _x[3] = _bar.T3; _x[4] = 1;

            const double kD = 0.25 / 100.0;
            const double kS = 0.18 / 100.0;

            double s = sVal * kS;
            double dk = dVal * kD;
            double lt = lVal * kD;
            double hi = hVal * kD;

            y[0] = 0;
            y[1] = _x[1] + s + 0.5 * dk;
            y[2] = _x[2] + dk + 0.5 * (s + lt);
            y[3] = _x[3] + lt + 0.5 * dk + 0.8 * hi;
            y[4] = 1;
            for (int i = 1; i < 4; i++) y[i] = Math.Clamp(y[i], 0, 1);

            m[4] = 1 + 0.8 * hi;
            for (int i = 0; i < 4; i++) d[i] = (y[i + 1] - y[i]) / (_x[i + 1] - _x[i]);

            m[0] = d[0];
            for (int i = 1; i < 4; i++) m[i] = (d[i - 1] + d[i]) * 0.5;

            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(d[i]) < 1e-9) { m[i] = m[i + 1] = 0; continue; }
                double a = m[i] / d[i];
                double b = m[i + 1] / d[i];
                double len = Double.Hypot(a, b);
                if (len > 3)
                {
                    double t = 3 / len;
                    m[i] = t * a * d[i];
                    m[i + 1] = t * b * d[i];
                }
            }

            for (int i = 0; i < 256; i++)
            {
                double x = i / 255.0;
                int seg = x < _x[1] ? 0 : x < _x[2] ? 1 : x < _x[3] ? 2 : 3;

                double hSeg = _x[seg + 1] - _x[seg];
                double t = (x - _x[seg]) / hSeg;
                double t2 = t * t;
                double t3 = t2 * t;

                double h00 = 2 * t3 - 3 * t2 + 1;
                double h10 = t3 - 2 * t2 + t;
                double h01 = -2 * t3 + 3 * t2;
                double h11 = t3 - t2;

                double yOut = h00 * y[seg] +
                              h10 * hSeg * m[seg] +
                              h01 * y[seg + 1] +
                              h11 * hSeg * m[seg + 1];

                dst[i] = (byte)Math.Round(Math.Clamp(yOut, 0, 1) * 255);
            }
        }

        /// <summary>
        /// Draws grid, curve and hover envelope.
        /// </summary>
        protected override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            int w = e.Info.Width;
            int h = e.Info.Height;
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);

            DrawDiagonal(c, w, h);

            using var grid = new SKPaint { Color = new SKColor(80, 80, 80), StrokeWidth = 1 };
            for (int i = 1; i < 4; i++)
            {
                c.DrawLine(i * w / 4f, 0, i * w / 4f, h, grid);
                c.DrawLine(0, i * h / 4f, w, i * h / 4f, grid);
            }

            using var vline = new SKPaint { Color = new SKColor(100, 100, 100), StrokeWidth = 1 };
            c.DrawLine(_bar.T1 * w, 0, _bar.T1 * w, h, vline);
            c.DrawLine(_bar.T2 * w, 0, _bar.T2 * w, h, vline);
            c.DrawLine(_bar.T3 * w, 0, _bar.T3 * w, h, vline);

            if (_hoverRegion != -1)
            {
                var env = new SKPath();
                env.MoveTo(X(0), Y(_envL[0]));
                for (int i = 1; i < 256; i++) env.LineTo(X(i), Y(_envL[i]));
                for (int i = 255; i >= 0; i--) env.LineTo(X(i), Y(_envH[i]));
                env.Close();

                using var fill = new SKPaint
                {
                    Color = new SKColor(200, 200, 200, 90),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                c.DrawPath(env, fill);
            }

            var curve = new SKPath();
            curve.MoveTo(0, Y(_lut[0]));
            for (int i = 1; i < 256; i++) curve.LineTo(X(i), Y(_lut[i]));

            using var white = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            c.DrawPath(curve, white);
            DrawBorder(c, w, h);

            float X(int i) => i * (w - 1) / 255f;
            float Y(byte v) => h - v / 255f * h;
        }

        /// <summary>
        /// Returns a copy of the current LUT for external use.
        /// </summary>
        /// <returns></returns>
        public override byte[] GetLut()
        {
            var copy = new byte[256];
            Array.Copy(_lut, copy, 256);
            return copy;
        }

        public override void RefreshCurve(Dictionary<string, object> settings)
        {
            if (settings.TryGetValue(SettingKey + "_Shadow_Value", out var sObj) && sObj is float sVal)
                _shadow.SetValue((float)sVal);
            if (settings.TryGetValue(SettingKey + "_Dark_Value", out var dObj) && dObj is float dVal)
                _dark.SetValue((float)dVal);
            if (settings.TryGetValue(SettingKey + "_Light_Value", out var lObj) && lObj is float lVal)
                _light.SetValue((float)lVal);
            if (settings.TryGetValue(SettingKey + "_High_Value", out var hObj) && hObj is float hVal)
                _high.SetValue((float)hVal);

            if (settings.TryGetValue(SettingKey + "_Thresholds", out var tObj))
            {
                if (tObj is List<float> thrF && thrF.Count == 3)
                {
                    _bar.T1 = thrF[0];
                    _bar.T2 = thrF[1];
                    _bar.T3 = thrF[2];
                }
                else if (tObj is List<double> thrD && thrD.Count == 3)
                {
                    _bar.T1 = (float)thrD[0];
                    _bar.T2 = (float)thrD[1];
                    _bar.T3 = (float)thrD[2];
                }
            }

            UpdateCurve();
        }

        public void ResetAllSliders()
        {
            _shadow.ResetToDefault();
            _dark.ResetToDefault();
            _light.ResetToDefault();
            _high.ResetToDefault();
            UpdateCurve();
        }

    }
}
