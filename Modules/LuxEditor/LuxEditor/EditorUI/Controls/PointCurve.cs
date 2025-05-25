using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>Luminance point curve rendered as Catmull-Rom spline.</summary>
    public sealed class PointCurve : CurveBase
    {
        private int? _drag;
        private DateTime _tap;
        private const int MaxPts = 16;

        public PointCurve()
        {
            ControlPoints.AddRange(new[] { new SKPoint(0, 1), new SKPoint(1, 0) });
            Content = _canvas;
            _canvas.PointerPressed += Down;
            _canvas.PointerMoved += Move;
            _canvas.PointerReleased += Up;
        }

        private void Down(object s, PointerRoutedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var p = ToCurve(e.GetCurrentPoint(_canvas).Position);
            int hit = Hit(p);
            if (e.GetCurrentPoint(_canvas).Properties.IsRightButtonPressed)
            { if (hit > 0 && hit < ControlPoints.Count - 1) { ControlPoints.RemoveAt(hit); Redraw(); } return; }

            if ((now - _tap).TotalMilliseconds < 350 && hit == -1 && ControlPoints.Count < MaxPts)
            { ControlPoints.Add(p); ControlPoints.Sort((a, b) => a.X.CompareTo(b.X)); Redraw(); }
            else if (hit != -1) { _drag = hit; _canvas.CapturePointer(e.Pointer); }

            _tap = now;
        }
        private void Move(object s, PointerRoutedEventArgs e)
        {
            if (_drag is null) return;
            var p = ToCurve(e.GetCurrentPoint(_canvas).Position);
            var pt = ControlPoints[_drag.Value];
            pt.X = Math.Clamp(p.X, 0, 1);
            pt.Y = Math.Clamp(p.Y, 0, 1);
            if (_drag > 0) pt.X = Math.Max(pt.X, ControlPoints[_drag.Value - 1].X + 0.01f);
            if (_drag < ControlPoints.Count - 1) pt.X = Math.Min(pt.X, ControlPoints[_drag.Value + 1].X - 0.01f);
            ControlPoints[_drag.Value] = pt; Redraw();
        }
        private void Up(object s, PointerRoutedEventArgs e)
        { _drag = null; _canvas.ReleasePointerCaptures(); }

        private void Redraw() { _canvas.Invalidate(); NotifyCurveChanged(); }

        private int Hit(SKPoint p)
        {
            const float h = .04f;
            for (int i = 0; i < ControlPoints.Count; i++)
                if (Dist(p, ControlPoints[i]) < h) return i;
            return -1;
        }
        private static float Dist(SKPoint a, SKPoint b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        private SKPoint ToCurve(Windows.Foundation.Point p) => new((float)(p.X / _canvas.ActualWidth), (float)(1 - p.Y / _canvas.ActualHeight));

        protected override void OnPaintSurface(object? s, SKPaintSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas; c.Clear(SKColors.Transparent);
            using var pathPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
            using var dotPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };

            var pts = ControlPoints.Select(pt => new SKPoint(pt.X * e.Info.Width, (1 - pt.Y) * e.Info.Height)).ToList();
            if (pts.Count < 2) return;

            var path = new SKPath();
            path.MoveTo(pts[0]);

            // Catmull-Rom spline (centripetal alpha = 0.5) → Cubic Bézier segments
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = i == 0 ? pts[i] : pts[i - 1];
                var p1 = pts[i];
                var p2 = pts[i + 1];
                var p3 = i + 2 < pts.Count ? pts[i + 2] : p2;

                var c1 = new SKPoint(p1.X + (p2.X - p0.X) / 6f, p1.Y + (p2.Y - p0.Y) / 6f);
                var c2 = new SKPoint(p2.X - (p3.X - p1.X) / 6f, p2.Y - (p3.Y - p1.Y) / 6f);
                path.CubicTo(c1, c2, p2);
            }
            c.DrawPath(path, pathPaint);
            foreach (var p in pts) c.DrawCircle(p, 4, dotPaint);
        }
    }
}
