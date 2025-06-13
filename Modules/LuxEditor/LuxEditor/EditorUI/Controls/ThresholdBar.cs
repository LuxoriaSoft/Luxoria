using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class ThresholdBar : UserControl
    {
        private readonly SKXamlCanvas _canvas = new();
        private float _t1 = .25f, _t2 = .50f, _t3 = .75f;
        private int _drag = -1;
        private const float Gap = .10f;
        private const float Edge = .10f;

        /// <summary>
        /// Fires whenever any threshold value changes.
        /// </summary>
        public event Action<float, float, float>? ThresholdChanged;

        /// <summary>
        /// Initialises visuals and input handlers.
        /// </summary>
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

        /// <summary>
        /// Selects the closest threshold to the pointer.
        /// </summary>
        private void Down(object s, PointerRoutedEventArgs e)
        {
            float x = (float)e.GetCurrentPoint(_canvas).Position.X / (float)_canvas.ActualWidth;
            float d1 = MathF.Abs(x - _t1);
            float d2 = MathF.Abs(x - _t2);
            float d3 = MathF.Abs(x - _t3);
            _drag = d1 < d2 && d1 < d3 ? 1 : d2 < d3 ? 2 : 3;
            _canvas.CapturePointer(e.Pointer);
        }

        /// <summary>
        /// Moves the dragged threshold and cascades neighbours when needed.
        /// </summary>
        private void Move(object s, PointerRoutedEventArgs e)
        {
            if (_drag == -1) return;

            float x = (float)e.GetCurrentPoint(_canvas).Position.X / (float)_canvas.ActualWidth;
            x = Math.Clamp(x, 0f, 1f);

            switch (_drag)
            {
                case 1: MoveT1(x); break;
                case 2: MoveT2(x); break;
                case 3: MoveT3(x); break;
            }

            _canvas.Invalidate();
            ThresholdChanged?.Invoke(_t1, _t2, _t3);
        }

        /// <summary>
        /// Releases the current drag.
        /// </summary>
        private void Up(object s, PointerRoutedEventArgs e)
        {
            _drag = -1;
            _canvas.ReleasePointerCaptures();
        }

        /// <summary>
        /// Moves the first threshold (t1) and cascades t2 and t3 if needed.
        /// </summary>
        private void MoveT1(float x)
        {
            _t1 = x;
            if (_t1 < Edge) _t1 = Edge;
            if (_t1 > _t2 - Gap)
            {
                _t2 = _t1 + Gap;
                if (_t2 > _t3 - Gap)
                {
                    _t3 = _t2 + Gap;
                    if (_t3 > 1f - Edge)
                    {
                        _t3 = 1f - Edge;
                        _t2 = _t3 - Gap;
                        _t1 = _t2 - Gap;
                    }
                }
            }
        }

        /// <summary>
        /// Moves the second threshold (t2) and cascades t1 and t3 if needed.
        /// </summary>
        private void MoveT2(float x)
        {
            _t2 = x;
            if (_t2 < _t1 + Gap)
            {
                _t1 = _t2 - Gap;
                if (_t1 < Edge) _t1 = Edge;
            }
            if (_t2 > _t3 - Gap)
            {
                _t3 = _t2 + Gap;
                if (_t3 > 1f - Edge) _t3 = 1f - Edge;
            }
            if (_t2 < _t1 + Gap) _t2 = _t1 + Gap;
            if (_t2 > _t3 - Gap) _t2 = _t3 - Gap;
        }

        /// <summary>
        /// Moves the third threshold (t3) and cascades t1 and t2 if needed.
        /// </summary>
        private void MoveT3(float x)
        {
            _t3 = x;
            if (_t3 > 1f - Edge) _t3 = 1f - Edge;
            if (_t3 < _t2 + Gap)
            {
                _t2 = _t3 - Gap;
                if (_t2 < _t1 + Gap)
                {
                    _t1 = _t2 - Gap;
                    if (_t1 < Edge)
                    {
                        _t1 = Edge;
                        _t2 = _t1 + Gap;
                        _t3 = _t2 + Gap;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the bar and its three thumbs.
        /// </summary>
        private void Paint(object? _, SKPaintSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);

            using var bar = new SKPaint { Color = new SKColor(60, 60, 60) };
            c.DrawRect(0, e.Info.Height / 3f, e.Info.Width, e.Info.Height / 3f, bar);

            using var thumb = new SKPaint { Color = SKColors.White };
            DrawThumb(_t1); DrawThumb(_t2); DrawThumb(_t3);

            void DrawThumb(float t)
            {
                float px = t * e.Info.Width;
                c.DrawCircle(px, e.Info.Height / 2f, 3f, thumb);
            }
        }
    }
}
