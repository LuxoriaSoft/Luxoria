using LuxEditor.Models;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls.ToolControls
{
    public class RadialGradientToolControl : ATool
    {
        public bool ShowExistingMask { get; set; } = true;
        private SKPoint? _center;
        private SKPoint? _edge;
        private bool _drag;
        private SKBitmap? _maskBmp;
        private SKCanvas? _maskCanv;
        private int _maskW, _maskH, _dispW, _dispH;
        public override ToolType ToolType { get; set; } = ToolType.RadialGradient;
        public override event Action? RefreshAction;

        public override void ResizeCanvas(int w, int h)
        {
            int div = Math.Max(Math.Max(w, h) / 1000, 1);
            int sw = Math.Max(1, w / div);
            int sh = Math.Max(1, h / div);
            if (sw == _maskW && sh == _maskH) return;
            _maskW = sw; _maskH = sh;
            _dispW = w; _dispH = h;
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
            RefreshAction?.Invoke();
        }

        public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            var sk = new SKPoint((float)pos.X, (float)pos.Y);
            if (e.GetCurrentPoint(cvs).Properties.IsLeftButtonPressed)
            {
                _center = sk;
                _edge = sk;
                _drag = true;
            }
            RefreshAction?.Invoke();
        }

        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_drag) return;
            var cvs = (SKXamlCanvas)sender;
            var pos = e.GetCurrentPoint(cvs).Position;
            _edge = new SKPoint((float)pos.X, (float)pos.Y);
            RefreshAction?.Invoke();
        }

        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_drag && _center.HasValue && _edge.HasValue && _maskCanv != null)
            {
                _maskCanv.Clear(SKColors.Transparent);
                DrawGradient(_maskCanv,
                             ToMask(_center.Value),
                             DistanceToMask(_center.Value, _edge.Value),
                             SKColors.White,
                             _maskW,
                             _maskH);
            }
            _drag = false;
            RefreshAction?.Invoke();
        }

        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var can = e.Surface.Canvas;
            can.Clear(SKColors.Transparent);
            if (OpsFusionned != null) can.DrawImage(OpsFusionned, 0, 0);
            if (ShowExistingMask && _maskBmp != null)
            {
                using var p = new SKPaint { ColorFilter = SKColorFilter.CreateBlendMode(Color, SKBlendMode.SrcIn) };
                can.DrawBitmap(_maskBmp, new SKRect(0, 0, _maskW, _maskH), new SKRect(0, 0, _dispW, _dispH), p);
            }
            if (_drag && _center.HasValue && _edge.HasValue)
            {
                DrawGradient(can, _center.Value, Distance(_center.Value, _edge.Value), Color, _dispW, _dispH);
            }
        }

        private void DrawGradient(SKCanvas c, SKPoint center, float radius, SKColor col, int w, int h)
        {
            using var p = new SKPaint
            {
                Shader = SKShader.CreateRadialGradient(
                    center,
                    radius,
                    new[] { col.WithAlpha(255), col.WithAlpha(0) },
                    null,
                    SKShaderTileMode.Clamp)
            };
            c.DrawRect(new SKRect(0, 0, w, h), p);
        }

        private SKPoint ToMask(SKPoint p) => new(p.X * _maskW / _dispW, p.Y * _maskH / _dispH);

        private float Distance(SKPoint a, SKPoint b) => (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

        private float DistanceToMask(SKPoint a, SKPoint b) => Distance(ToMask(a), ToMask(b));

        public override SKBitmap? GetResult()
        {
            if (_maskBmp == null) return null;
            if (_maskBmp.Width == _dispW && _maskBmp.Height == _dispH) return _maskBmp;
            return _maskBmp.Resize(new SKImageInfo(_dispW, _dispH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }
    }
}
