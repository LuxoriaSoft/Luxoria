using LuxEditor.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;

namespace LuxEditor.EditorUI.Controls.ToolControls
{
    /// <summary>
    /// Circular radial gradient mask tool with editable guides.
    /// 
    /// Interaction
    /// ────────────────────────────────────────────────────────────────────────
    /// • New gradient   : left‑drag on empty space → centre → hard‑edge radius.
    /// • Move gradient  : left‑drag anywhere inside the hard‑edge circle.
    /// • Adjust hard edge : drag inner‑radius handle (east side).
    /// • Adjust feather   : drag outer‑radius handle (east side).
    /// • Delete          : right‑click inside a gradient’s hard‑edge circle.
    ///
    /// The gradient is always a perfect circle (no oval deformation).
    /// </summary>
    public partial class RadialGradientToolControl : ATool
    {
        class RadialGradient
        {
            public SKPoint Center;
            public float Radius;
            public float Feather;
            public RadialGradient(SKPoint c, float r, float f) { Center = c; Radius = r; Feather = f; }
        }

        public bool ShowExistingMask { get; set; } = true;
        readonly List<RadialGradient> _gradients = new();
        int _selected = -1;

        enum DragMode { None, Create, Move, RadiusInner, RadiusOuter }
        DragMode _mode = DragMode.None;
        SKPoint _start;
        float _outerInit;
        const float HANDLE_R = 6f;

        SKBitmap? _maskBmp;
        SKCanvas? _maskCanv;
        int _maskW, _maskH;
        int _dispW, _dispH;

        public RadialGradientToolControl(BooleanOperationMode bMode) : base(bMode) { }

        public override ToolType ToolType { get; set; } = ToolType.RadialGradient;
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
            var canvas = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(canvas).Position;
            var p = new SKPoint((float)pos.X, (float)pos.Y);

            var props = e.GetCurrentPoint(canvas).Properties;
            if (props.IsRightButtonPressed)
            {
                int hitIdx = GradientIndexAtPoint(p);
                if (hitIdx >= 0)
                {
                    _gradients.RemoveAt(hitIdx);
                    _selected = -1;
                    RecomputeMask();
                    RefreshAction?.Invoke();
                    RefreshOverlayTemp?.Invoke();
                }
                return;
            }

            if (!props.IsLeftButtonPressed) return;

            int idx; DragMode hit = HitTest(p, out idx);
            if (hit != DragMode.None)
            {
                _selected = idx;
                _mode = hit;
                _start = p;
                if (hit == DragMode.RadiusOuter) _outerInit = _gradients[idx].Feather;
            }
            else
            {
                _mode = DragMode.Create;
                _start = p;
                _gradients.Add(new RadialGradient(p, 0f, 0.3f));
                _selected = _gradients.Count - 1;
            }

            RefreshAction?.Invoke();
            RefreshOverlayTemp?.Invoke();
        }

        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_mode == DragMode.None || _selected < 0) return;
            var canvas = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(canvas).Position;
            var p = new SKPoint((float)pos.X, (float)pos.Y);
            var g = _gradients[_selected];

            switch (_mode)
            {
                case DragMode.Create:
                    g.Radius = Distance(p, g.Center);
                    break;
                case DragMode.Move:
                    SKPoint d = new(p.X - _start.X, p.Y - _start.Y);
                    g.Center = new SKPoint(g.Center.X + d.X, g.Center.Y + d.Y);
                    _start = p;
                    break;
                case DragMode.RadiusInner:
                    g.Radius = Math.Max(1f, Distance(p, g.Center));
                    break;
                case DragMode.RadiusOuter:
                    float outer = Math.Max(1f, Distance(p, g.Center));
                    g.Feather = Math.Clamp(outer / g.Radius - 1f, 0f, 2f);
                    break;
            }

            RecomputeMask();
            RefreshAction?.Invoke();
            RefreshOverlayTemp?.Invoke();
        }

        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _mode = DragMode.None;
            RecomputeMask();
            RefreshAction?.Invoke();
            RefreshOperation?.Invoke();
            RefreshOverlayTemp?.Invoke();
        }

        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (OpsFusionned != null)
                canvas.DrawImage(OpsFusionned, 0, 0);

            if (ShowExistingMask && _maskBmp != null)
            {
                using var tint = new SKPaint { ColorFilter = SKColorFilter.CreateBlendMode(Color, SKBlendMode.SrcIn) };
                canvas.DrawBitmap(_maskBmp, new SKRect(0, 0, _maskW, _maskH), new SKRect(0, 0, _dispW, _dispH), tint);
            }

            for (int i = 0; i < _gradients.Count; i++)
                DrawGuides(canvas, _gradients[i], i == _selected);
        }

        DragMode HitTest(SKPoint p, out int idx)
        {
            for (int i = _gradients.Count - 1; i >= 0; i--)
            {
                var g = _gradients[i];
                var innerHandle = new SKPoint(g.Center.X + g.Radius, g.Center.Y);
                if (Distance(p, innerHandle) <= HANDLE_R) { idx = i; return DragMode.RadiusInner; }
                var outerHandle = new SKPoint(g.Center.X + g.Radius * (1f + g.Feather), g.Center.Y);
                if (Distance(p, outerHandle) <= HANDLE_R) { idx = i; return DragMode.RadiusOuter; }
                if (Distance(p, g.Center) <= g.Radius) { idx = i; return DragMode.Move; }
            }
            idx = -1; return DragMode.None;
        }

        int GradientIndexAtPoint(SKPoint p)
        {
            for (int i = _gradients.Count - 1; i >= 0; i--)
            {
                if (Distance(p, _gradients[i].Center) <= _gradients[i].Radius)
                    return i;
            }
            return -1;
        }

        void RecomputeMask()
        {
            if (_maskCanv == null) return;
            _maskCanv.Clear(SKColors.Transparent);
            foreach (var g in _gradients)
            {
                var centerMask = ToMask(g.Center);
                float rMask = g.Radius * _maskW / _dispW;
                DrawGradient(_maskCanv, centerMask, rMask, SKColors.White, _maskW, _maskH, g.Feather);
            }
        }

        void DrawGradient(SKCanvas c, SKPoint center, float radius, SKColor col, int w, int h, float feather)
        {
            float outer = radius * (1f + feather);
            var colors = new[] { col.WithAlpha(255), col.WithAlpha(255), col.WithAlpha(0) };
            var positions = new[] { 0f, radius / outer, 1f };

            using var paint = new SKPaint
            {
                Shader = SKShader.CreateRadialGradient(center, outer, colors, positions, SKShaderTileMode.Clamp),
                BlendMode = SKBlendMode.SrcOver,
                IsAntialias = true
            };
            c.DrawRect(new SKRect(0, 0, w, h), paint);
        }

        void DrawGuides(SKCanvas c, RadialGradient g, bool sel)
        {
            float inner = g.Radius;
            float outer = g.Radius * (1f + g.Feather);
            float handleR = sel ? 4f : 3f;

            using var guide = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = SKColors.White.WithAlpha(150), IsAntialias = true };
            using var handle = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.White.WithAlpha(200), IsAntialias = true };

            c.DrawCircle(g.Center, inner, guide);
            c.DrawCircle(g.Center, outer, guide);
            c.DrawCircle(g.Center, handleR, handle);
            c.DrawCircle(new SKPoint(g.Center.X + inner, g.Center.Y), handleR, handle);
            c.DrawCircle(new SKPoint(g.Center.X + outer, g.Center.Y), handleR, handle);
        }

        static float Distance(SKPoint a, SKPoint b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        SKPoint ToMask(SKPoint p) => new(p.X * _maskW / _dispW, p.Y * _maskH / _dispH);

        public override SKBitmap? GetResult()
        {
            if (_maskBmp == null) return null;
            if (_maskBmp.Width == _dispW && _maskBmp.Height == _dispH) return _maskBmp;
            return _maskBmp.Resize(new SKImageInfo(_dispW, _dispH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }

        public override ATool Clone()
        {
            var clone = new RadialGradientToolControl(booleanOperationMode)
            {
                ToolType = ToolType,
                Color = this.Color,
                ShowExistingMask = this.ShowExistingMask
            };

            clone._dispW = _dispW;
            clone._dispH = _dispH;
            clone.ResizeCanvas(_dispW, _dispH);

            foreach (var g in _gradients)
            {
                var copied = new RadialGradient(new SKPoint(g.Center.X, g.Center.Y), g.Radius, g.Feather);
                clone._gradients.Add(copied);
            }

            clone._selected = _selected;
            clone.RecomputeMask();

            return clone;
        }

        public override void LoadMaskBitmap(SKBitmap bmp)
        {
            _dispW = bmp.Width;
            _dispH = bmp.Height;
            ResizeCanvas(_dispW, _dispH);
            _maskBmp?.Dispose();
            _maskBmp = bmp.Copy();
            RefreshAction?.Invoke();
        }

    }
}
