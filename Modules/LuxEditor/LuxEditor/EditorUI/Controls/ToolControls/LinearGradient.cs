using LuxEditor.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls.ToolControls
{
    /// <summary>
    /// Linear gradient mask tool with exactly **two** perpendicular guide lines:
    ///   • Line 0 – fully opaque boundary (<c>A</c>).
    ///   • Line 1 – fully transparent boundary (<c>B</c>).
    /// The area between the two lines fades linearly from 100 % to 0 %.
    /// Only one gradient can exist per layer. Creating a new gradient replaces
    /// the previous one.  Users can drag anywhere between the two lines to move
    /// the gradient.
    /// </summary>
    public partial class LinearGradientToolControl : ATool
    {
        class LinearGradient
        {
            public SKPoint A;
            public SKPoint B;
            public LinearGradient(SKPoint a, SKPoint b) { A = a; B = b; }
        }

        LinearGradient? _gradient;
        bool _selected;

        enum DragMode { None, Create, Move, AxisA, AxisB }
        DragMode _mode = DragMode.None;
        SKPoint _start;
        const float HANDLE_R = 6f;

        SKBitmap? _maskBmp;
        SKCanvas? _maskCanv;
        int _maskW, _maskH, _dispW, _dispH;

        public bool ShowExistingMask { get; set; } = true;

        public LinearGradientToolControl(BooleanOperationMode bMode) : base(bMode) { }

        public override ToolType ToolType { get; set; } = ToolType.LinearGradient;
        public override event Action? RefreshAction;
        public override event Action RefreshOperation;
        public override event Action? RefreshOverlayTemp;

        public override void ResizeCanvas(int w, int h)
        {
            int div = Math.Max(Math.Max(w, h) / 1000, 1);
            int sw = Math.Max(1, w / div);
            int sh = Math.Max(1, h / div);
            if (sw == _maskW && sh == _maskH) return;
            _maskW = sw; _maskH = sh; _dispW = w; _dispH = h;
            _maskBmp = new SKBitmap(sw, sh, SKColorType.Bgra8888, SKAlphaType.Premul);
            _maskCanv = new SKCanvas(_maskBmp);
            _maskCanv.Clear(SKColors.Transparent);
            RecomputeMask();
            RefreshAction?.Invoke();
        }

        public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            var p = new SKPoint((float)pos.X, (float)pos.Y);
            var pr = e.GetCurrentPoint(cvs).Properties;

            if (pr.IsRightButtonPressed && _gradient != null && PointInStrip(p, _gradient))
            {
                _gradient = null; _selected = false;
                RecomputeMask();
                RefreshAction?.Invoke(); RefreshOverlayTemp?.Invoke();
                return;
            }
            if (!pr.IsLeftButtonPressed) return;

            if (_gradient == null || !PointInStrip(p, _gradient))
            {
                _gradient = new LinearGradient(p, p);
                _selected = true; _mode = DragMode.Create; _start = p;
            }
            else
            {
                var hit = HitTest(p, _gradient);
                _selected = true; _mode = hit; _start = p;
            }
            RefreshAction?.Invoke(); RefreshOverlayTemp?.Invoke();
        }

        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_mode == DragMode.None || _gradient == null) return;
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            var p = new SKPoint((float)pos.X, (float)pos.Y);
            var g = _gradient;

            switch (_mode)
            {
                case DragMode.Create:
                    g.B = p; break;
                case DragMode.Move:
                    SKPoint d = Sub(p, _start); g.A = Add(g.A, d); g.B = Add(g.B, d); _start = p; break;
                case DragMode.AxisA:
                    g.A = p; break;
                case DragMode.AxisB:
                    g.B = p; break;
            }
            RecomputeMask(); RefreshAction?.Invoke(); RefreshOverlayTemp?.Invoke();
        }

        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _mode = DragMode.None;
            RecomputeMask(); RefreshAction?.Invoke(); RefreshOperation?.Invoke(); RefreshOverlayTemp?.Invoke();
        }

        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas; c.Clear(SKColors.Transparent);
            if (OpsFusionned != null) c.DrawImage(OpsFusionned, 0, 0);
            if (ShowExistingMask && _maskBmp != null)
            {
                using var tint = new SKPaint { ColorFilter = SKColorFilter.CreateBlendMode(Color, SKBlendMode.SrcIn) };
                c.DrawBitmap(_maskBmp, new SKRect(0, 0, _maskW, _maskH), new SKRect(0, 0, _dispW, _dispH), tint);
            }
            if (_gradient != null) DrawGuides(c, _gradient, _selected);
        }

        DragMode HitTest(SKPoint p, LinearGradient g)
        {
            if (Distance(p, g.A) <= HANDLE_R) return DragMode.AxisA;
            if (Distance(p, g.B) <= HANDLE_R) return DragMode.AxisB;
            return PointInStrip(p, g) ? DragMode.Move : DragMode.None;
        }

        bool PointInStrip(SKPoint p, LinearGradient g)
        {
            var v = Sub(g.B, g.A); float len = Length(v); if (len < 1f) return false;
            var vu = Normalize(v); var n = new SKPoint(-vu.Y, vu.X);
            float proj = Dot(Sub(p, g.A), vu);
            if (proj < 0 || proj > len) return false;
            float dist = Math.Abs(Dot(Sub(p, g.A), n));
            return dist <= 15f;
        }

        void RecomputeMask()
        {
            if (_maskCanv == null) return;
            _maskCanv.Clear(SKColors.Transparent);
            if (_gradient != null)
                DrawGradient(_maskCanv, ToMask(_gradient.A), ToMask(_gradient.B), SKColors.White, _maskW, _maskH);
        }

        void DrawGradient(SKCanvas c, SKPoint a, SKPoint b, SKColor col, int w, int h)
        {
            using var paint = new SKPaint
            {
                Shader = SKShader.CreateLinearGradient(a, b, new[] { col.WithAlpha(255), col.WithAlpha(0) }, null, SKShaderTileMode.Clamp),
                BlendMode = SKBlendMode.SrcOver,
                IsAntialias = true
            };
            c.DrawRect(new SKRect(0, 0, w, h), paint);
        }

        void DrawGuides(SKCanvas c, LinearGradient g, bool sel)
        {
            var v = Sub(g.B, g.A); float len = Length(v); if (len < 1f) return;
            var vu = Normalize(v); var n = new SKPoint(-vu.Y, vu.X);
            float ext = Math.Max(_dispW, _dispH) * 1.5f;
            using var guide = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = SKColors.White.WithAlpha(150), IsAntialias = true };
            using var handle = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White.WithAlpha(200), IsAntialias = true };
            float r = sel ? 4f : 3f;
            void DrawPerp(SKPoint basePt)
            {
                var p1 = Sub(basePt, Mul(n, ext)); var p2 = Add(basePt, Mul(n, ext)); c.DrawLine(p1, p2, guide);
            }
            DrawPerp(g.A); DrawPerp(g.B);
            c.DrawCircle(g.A, r, handle); c.DrawCircle(g.B, r, handle);
        }

        static SKPoint Add(SKPoint a, SKPoint b) => new(a.X + b.X, a.Y + b.Y);
        static SKPoint Sub(SKPoint a, SKPoint b) => new(a.X - b.X, a.Y - b.Y);
        static SKPoint Mul(SKPoint p, float s) => new(p.X * s, p.Y * s);
        static float Length(SKPoint v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        static SKPoint Normalize(SKPoint v) { float len = Length(v); return len < 1f ? new SKPoint(0, 0) : new(v.X / len, v.Y / len); }
        static float Dot(SKPoint a, SKPoint b) => a.X * b.X + a.Y * b.Y;
        static float Distance(SKPoint a, SKPoint b) => Length(Sub(a, b));
        SKPoint ToMask(SKPoint p) => new(p.X * _maskW / _dispW, p.Y * _maskH / _dispH);

        public override SKBitmap? GetResult()
        {
            if (_maskBmp == null) return null;
            if (_maskBmp.Width == _dispW && _maskBmp.Height == _dispH) return _maskBmp;
            return _maskBmp.Resize(new SKImageInfo(_dispW, _dispH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }
    }
}
