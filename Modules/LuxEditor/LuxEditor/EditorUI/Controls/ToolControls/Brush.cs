using LuxEditor.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.System;

namespace LuxEditor.EditorUI.Controls.ToolControls
{
    public class BrushPoint
    {
        public SKPoint NormalizedPos;
        public float Radius;
        public BrushPoint(SKPoint n, float r) { NormalizedPos = n; Radius = r; }
        public SKPoint ToAbs(int w, int h) => new(NormalizedPos.X * w, NormalizedPos.Y * h);
    }

    public partial class BrushToolControl : ATool
    {
        public float BrushSize { get; set; } = 10f;

        private class CustomStroke { public readonly List<BrushPoint> Points = new(); }

        private CustomStroke? _current;
        private readonly Queue<SKPoint> _last = new();
        private SKPoint? _lastMouse;

        private SKBitmap? _maskBmp;
        private SKCanvas? _maskCanv;
        private int _maskW, _maskH, _dispW, _dispH;

        private SKPoint? _rightStart;
        private bool _isRight;
        private bool _subtract;
        private bool _dirty;
        public override event Action? RefreshOverlayTemp;

        public BrushToolControl(BooleanOperationMode bMode) : base(bMode)
        {
        }

        public override ToolType ToolType { get; set; } = ToolType.Brush;
        public override event Action? RefreshAction;
        public override event Action RefreshOperation;

        public override void ResizeCanvas(int w, int h)
        {
            int div = Math.Max(Math.Max(w, h) / 1000, 1);
            int sw = Math.Max(1, w / div);
            int sh = Math.Max(1, h / div);
            if (sw == _maskW && sh == _maskH) return;
            _maskW = sw; _maskH = sh; _dispW = w; _dispH = h;
            var old = _maskBmp;
            _maskBmp = new SKBitmap(sw, sh, SKColorType.Bgra8888, SKAlphaType.Premul);
            _maskCanv = new SKCanvas(_maskBmp);
            _maskCanv.Clear(SKColors.Transparent);
            if (old != null)
            {
                using var p = new SKPaint { FilterQuality = SKFilterQuality.High };
                _maskCanv.DrawBitmap(old, new SKRect(0, 0, sw, sh), p);
                old.Dispose();
            }
            _dirty = false;
            RefreshAction?.Invoke();
        }

        public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            var sk = new SKPoint((float)pos.X, (float)pos.Y);
            _lastMouse = sk;
            _subtract = e.KeyModifiers.HasFlag(VirtualKeyModifiers.Menu);
            if (e.GetCurrentPoint(cvs).Properties.IsLeftButtonPressed)
            {
                _current = new CustomStroke();
                _current.Points.Add(new BrushPoint(Norm(sk), BrushSize));
                _last.Clear();
            }
            else if (e.GetCurrentPoint(cvs).Properties.IsRightButtonPressed)
            {
                _rightStart = sk;
                _isRight = true;
            }
            RefreshAction?.Invoke();
            RefreshOverlayTemp?.Invoke();
        }

        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            var sk = new SKPoint((float)pos.X, (float)pos.Y);
            _lastMouse = sk;
            _subtract = e.KeyModifiers.HasFlag(VirtualKeyModifiers.Menu);
            if (_current != null && e.GetCurrentPoint(cvs).Properties.IsLeftButtonPressed)
            {
                if (_current.Points.Count > 0)
                {
                    var lastPt = _current.Points[^1].ToAbs(_dispW, _dispH);
                    foreach (var p in Interp(lastPt, sk, BrushSize * .25f))
                        _current.Points.Add(new BrushPoint(Norm(Smooth(p)), BrushSize));
                }
                _current.Points.Add(new BrushPoint(Norm(Smooth(sk)), BrushSize));
                if (_current.Points.Count > 30 && _maskCanv != null)
                {
                    foreach (var pt in _current.Points)
                        DrawSoftCircle(_maskCanv, pt.ToAbs(_maskW, _maskH), pt.Radius * _maskW / _dispW, _subtract);
                    _current.Points.Clear();
                    _dirty = true;
                }
            }
            else if (_isRight && _rightStart.HasValue)
            {
                float delta = (float)(pos.X - _rightStart.Value.X);
                BrushSize = Math.Max(1, delta);
            }
            RefreshAction?.Invoke();
            RefreshOverlayTemp?.Invoke();

        }

        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_current != null && _maskCanv != null)
            {
                foreach (var pt in _current.Points)
                    DrawSoftCircle(_maskCanv, pt.ToAbs(_maskW, _maskH), pt.Radius * _maskW / _dispW, _subtract);
                _current = null;
                _dirty = true;
            }
            _isRight = false;
            _rightStart = null;
            RefreshAction?.Invoke();
            RefreshOperation?.Invoke();
            RefreshOverlayTemp?.Invoke();
        }

        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var can = e.Surface.Canvas;

            if (_lastMouse.HasValue)
            {
                var pv = _isRight && _rightStart.HasValue ? _rightStart.Value : _lastMouse.Value;
                DrawPreview(can, pv);
            }
        }

        private void DrawSoftCircle(SKCanvas c, SKPoint center, float r, bool subtract)
        {
            using var p = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(center, r, new[] { SKColors.White, SKColors.White.WithAlpha(100), SKColors.Transparent }, new[] { 0f, .5f, 1f }, SKShaderTileMode.Clamp),
                BlendMode = subtract ? SKBlendMode.DstOut : SKBlendMode.SrcOver
            };
            c.DrawCircle(center, r, p);
        }

        private void DrawPreview(SKCanvas c, SKPoint pos)
        {
            using var p = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = _isRight ? 2 : 1, Color = SKColors.White.WithAlpha(180), IsAntialias = true };
            c.DrawCircle(pos, BrushSize, p);
        }

        private IEnumerable<SKPoint> Interp(SKPoint a, SKPoint b, float step)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y; int n = (int)(MathF.Sqrt(dx * dx + dy * dy) / step);
            for (int i = 1; i <= n; i++) yield return new SKPoint(a.X + dx * i / n, a.Y + dy * i / n);
        }

        private SKPoint Smooth(SKPoint cur)
        {
            _last.Enqueue(cur); while (_last.Count > 4) _last.Dequeue(); float sx = 0, sy = 0; foreach (var p in _last) { sx += p.X; sy += p.Y; }
            return new SKPoint(sx / _last.Count, sy / _last.Count);
        }

        private SKPoint Norm(SKPoint abs) => new(abs.X / _dispW, abs.Y / _dispH);

        public override SKBitmap? GetResult()
        {
            if (_maskBmp == null)
                return null;
            if (_maskBmp.Width == _dispW && _maskBmp.Height == _dispH)
                return _maskBmp;
            return _maskBmp.Resize(new SKImageInfo(_dispW, _dispH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }

        public override ATool Clone()
        {
            var clone = new BrushToolControl(booleanOperationMode)
            {
                ToolType = ToolType,
                Color = this.Color,
                BrushSize = this.BrushSize
            };

            if (_maskBmp != null)
            {
                clone._dispW = _dispW;
                clone._dispH = _dispH;
                clone.ResizeCanvas(_dispW, _dispH);

                using var paint = new SKPaint { FilterQuality = SKFilterQuality.High };
                clone._maskCanv?.DrawBitmap(_maskBmp, new SKRect(0, 0, _maskW, _maskH), paint);
            }

            return clone;
        }

    }
}
