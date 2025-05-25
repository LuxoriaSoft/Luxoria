using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>Horizontal bar with 3 draggable dividers that outputs t1-t2-t3 in [0..1].</summary>
    public sealed class ThresholdBar : UserControl
    {
        private readonly SKXamlCanvas _canvas = new();
        private float _t1 = .25f, _t2 = .50f, _t3 = .75f;
        private int _drag = -1;

        /// <summary>Raised when any threshold changes.</summary>
        public event Action<float, float, float>? ThresholdChanged;

        public ThresholdBar()
        {
            Height = 24;
            Content = _canvas;
            _canvas.PaintSurface += Paint;
            _canvas.PointerPressed += Down;
            _canvas.PointerMoved += Move;
            _canvas.PointerReleased += Up;
        }

        public float T1 { get => _t1; set { _t1 = value; _canvas.Invalidate(); } }
        public float T2 { get => _t2; set { _t2 = value; _canvas.Invalidate(); } }
        public float T3 { get => _t3; set { _t3 = value; _canvas.Invalidate(); } }

        private void Down(object s, PointerRoutedEventArgs e)
        {
            var x = (float)e.GetCurrentPoint(_canvas).Position.X / (float)_canvas.ActualWidth;
            var d1 = MathF.Abs(x - _t1);
            var d2 = MathF.Abs(x - _t2);
            var d3 = MathF.Abs(x - _t3);
            _drag = d1 < d2 && d1 < d3 ? 1 : d2 < d3 ? 2 : 3;
            _canvas.CapturePointer(e.Pointer);
        }
        private void Move(object s, PointerRoutedEventArgs e)
        {
            if (_drag == -1) return;
            float x = (float)e.GetCurrentPoint(_canvas).Position.X / (float)_canvas.ActualWidth;
            x = Math.Clamp(x, 0, 1);
            switch (_drag)
            {
                case 1: _t1 = Math.Clamp(x, 0, _t2 - .05f); break;
                case 2: _t2 = Math.Clamp(x, _t1 + .05f, _t3 - .05f); break;
                case 3: _t3 = Math.Clamp(x, _t2 + .05f, 1); break;
            }
            _canvas.Invalidate();
            ThresholdChanged?.Invoke(_t1, _t2, _t3);
        }
        private void Up(object s, PointerRoutedEventArgs e)
        {
            _drag = -1;
            _canvas.ReleasePointerCaptures();
        }

        private void Paint(object? s, SKPaintSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);
            using var bar = new SKPaint { Color = new SKColor(60, 60, 60) };
            c.DrawRect(0, e.Info.Height / 3, e.Info.Width, e.Info.Height / 3, bar);

            using var thumb = new SKPaint { Color = SKColors.White };
            DrawThumb(_t1); DrawThumb(_t2); DrawThumb(_t3);

            void DrawThumb(float t)
            {
                float x = t * e.Info.Width;
                c.DrawCircle(x, e.Info.Height / 2, 3, thumb);
            }
        }
    }
}
