using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls
{
    public class PointCurve : CurveBase
    {
        private const int MaxPts = 16;
        private const float Hit = .04f;

        private int? _drag;
        private DateTime _tap;

        private SKColor _grad0;
        private SKColor _grad1;

        private readonly byte[] _lut = new byte[256];

        public override string SettingKey => "ToneCurve_Point";

        public PointCurve() : this(
            new SKColor(255, 255, 255, 50),
            new SKColor(0, 0, 0, 50))
        { }

        public PointCurve(SKColor gradStart, SKColor gradEnd)
        {
            _grad0 = gradStart;
            _grad1 = gradEnd;

            ControlPoints.AddRange(new[] { new SKPoint(0, 0), new SKPoint(1, 1) });

            _canvas.PointerPressed += Down;
            _canvas.PointerMoved += Move;
            _canvas.PointerReleased += Up;

            BuildLut();
            Content = _canvas;
        }

        public void SetGradient(SKColor a, SKColor b)
        { _grad0 = a; _grad1 = b; _canvas.Invalidate(); }

        private void Down(object s, PointerRoutedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var p = ToCurve(e.GetCurrentPoint(_canvas).Position);
            int hit = HitIndex(p);

            if (e.GetCurrentPoint(_canvas).Properties.IsRightButtonPressed)
            {
                if (hit > 0 && hit < ControlPoints.Count - 1) { ControlPoints.RemoveAt(hit); Redraw(); BuildLut();}
                return;
            }

            if ((now - _tap).TotalMilliseconds < 350)
            {
                if (hit == -1)
                {
                    ControlPoints.Clear();
                    ControlPoints.AddRange(new[] { new SKPoint(0, 0), new SKPoint(1, 1) });
                }
                else
                {
                    if (hit > 0 && hit < ControlPoints.Count - 1)
                        ControlPoints.RemoveAt(hit);
                }
                Redraw();
                BuildLut();
                _tap = now;
                return;
            }

            if (hit == -1 && ControlPoints.Count < MaxPts)
            {
                ControlPoints.Add(p);
                ControlPoints.Sort((a, b) => a.X.CompareTo(b.X));
                _drag = ControlPoints.IndexOf(p);
                _canvas.CapturePointer(e.Pointer);
                Redraw();
                BuildLut();
            }
            else if (hit != -1)
            {
                _drag = hit;
                _canvas.CapturePointer(e.Pointer);
            }

            _tap = now;
        }

        private void Move(object s, PointerRoutedEventArgs e)
        {
            if (_drag is null) return;

            var p = ToCurve(e.GetCurrentPoint(_canvas).Position);
            var pt = ControlPoints[_drag.Value];

            pt.X = Math.Clamp(p.X, 0, 1);
            pt.Y = Math.Clamp(p.Y, 0, 1);

            if (_drag > 0)
                pt.X = Math.Max(pt.X, ControlPoints[_drag.Value - 1].X + 0.01f);
            if (_drag < ControlPoints.Count - 1)
                pt.X = Math.Min(pt.X, ControlPoints[_drag.Value + 1].X - 0.01f);

            ControlPoints[_drag.Value] = pt;
            Redraw();
            BuildLut();
        }

        private void Up(object s, PointerRoutedEventArgs e)
        { _drag = null; _canvas.ReleasePointerCaptures(); }


        private void Redraw() { _canvas.Invalidate(); }

        protected override void OnPaintSurface(object? _, SKPaintSurfaceEventArgs e)
        {
            int w = e.Info.Width, h = e.Info.Height;
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);

            using (var bg = new SKPaint
            {
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0), new SKPoint(w, h),
                    new[] { _grad0, _grad1 }, null, SKShaderTileMode.Clamp)
            })
                c.DrawRect(0, 0, w, h, bg);

            DrawDiagonal(c, w, h);

            // 4×4 grid
            using (var g = new SKPaint { Color = new SKColor(70, 70, 70, 160), StrokeWidth = 1 })
            {
                for (int i = 1; i < 4; i++)
                {
                    float x = i * w / 4f, y = i * h / 4f;
                    c.DrawLine(x, 0, x, h, g);
                    c.DrawLine(0, y, w, y, g);
                }
            }

            // curve itself (white)
            using var pen = new SKPaint { Color = SKColors.White, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
            using var path = new SKPath();

            var first = ControlPoints[0];
            var last = ControlPoints[^1];

            path.MoveTo(0, (1 - first.Y) * h);
            path.LineTo(first.X * w, (1 - first.Y) * h);

            if (ControlPoints.Count == 2)
                path.LineTo(last.X * w, (1 - last.Y) * h);
            else
            {
                var p0 = first;
                for (int i = 0; i < ControlPoints.Count - 1; i++)
                {
                    var p1 = ControlPoints[i];
                    var p2 = ControlPoints[i + 1];
                    var p3 = i + 2 < ControlPoints.Count ? ControlPoints[i + 2] : p2;
                    for (int t = 1; t <= 20; t++)
                    {
                        float s = t / 20f;
                        var q = Catmull(p0, p1, p2, p3, s);
                        path.LineTo(q.X * w, (1 - q.Y) * h);
                    }
                    p0 = p1;
                }
            }

            path.LineTo(w, (1 - last.Y) * h);
            c.DrawPath(path, pen);

            // handles (always on top)
            foreach (var p in ControlPoints)
                DrawHandle(c, p.X * w, (1 - p.Y) * h);

            DrawBorder(c, w, h);
        }

        /// <summary>
        /// Finds the index of the control point that is closest to the given point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private int HitIndex(SKPoint p)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                if (Dist(p, ControlPoints[i]) < Hit) return i;
            return -1;
        }

        /// <summary>
        /// Calculates the distance between two points in 2D space.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static float Dist(SKPoint a, SKPoint b) =>
            MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        /// <summary>
        /// Converts a Windows.Foundation.Point to a normalized SKPoint for the curve.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private SKPoint ToCurve(Windows.Foundation.Point p) =>
            new((float)(p.X / _canvas.ActualWidth), (float)(1 - p.Y / _canvas.ActualHeight));

        /// <summary>
        /// Calculates a point on a Catmull-Rom spline given four control points and a parameter t (0 to 1).
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static SKPoint Catmull(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return new SKPoint(
                0.5f * (2 * p1.X + (-p0.X + p2.X) * t
                        + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2
                        + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3),
                0.5f * (2 * p1.Y + (-p0.Y + p2.Y) * t
                        + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2
                        + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3)
            );
        }


        /// <summary>
        /// Builds the Look-Up Table (LUT) for tone mapping based on the current control points.
        /// </summary>
        private void BuildLut()
        {
            for (int i = 0; i < 256; i++)
            {
                float x = i / 255f;
                _lut[i] = (byte)(Evaluate(x) * 255f + .5f);
            }
            NotifyCurveChanged();
        }

        /// <summary>
        /// Evaluates the curve at a given x-coordinate (0 to 1) using Catmull-Rom splines.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private float Evaluate(float x)
        {
            if (ControlPoints.Count == 2)
                return Lerp(ControlPoints[0], ControlPoints[1], x);

            int i = ControlPoints.FindLastIndex(p => p.X <= x);
            i = Math.Clamp(i, 0, ControlPoints.Count - 2);

            SKPoint p0 = i > 0 ? ControlPoints[i - 1] : ControlPoints[i];
            SKPoint p1 = ControlPoints[i];
            SKPoint p2 = ControlPoints[i + 1];
            SKPoint p3 = i + 2 < ControlPoints.Count ? ControlPoints[i + 2] : p2;

            float t = (x - p1.X) / (p2.X - p1.X);
            return Catmull(p0, p1, p2, p3, t).Y;
        }

        /// <summary>
        /// Linearly interpolates between two points based on the x-coordinate.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static float Lerp(SKPoint a, SKPoint b, float x) => a.Y + (b.Y - a.Y) * ((x - a.X) / (b.X - a.X));

        /// <summary>
        /// Returns a copy of the LUT (Look-Up Table) used for tone mapping.
        /// </summary>
        /// <returns></returns>
        public override byte[] GetLut()
        {
            var copy = new byte[256];
            Array.Copy(_lut, copy, 256);
            return copy;
        }
    }
}
