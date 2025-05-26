using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using LuxEditor.Models;

namespace LuxEditor.Controls.ToolControls
{
    public class BrushToolControl : UserControl
    {
        private readonly SKXamlCanvas _canvas;
        private readonly MaskOperation _operation;
        private SKPath? _currentPath;

        public BrushToolControl(MaskOperation operation)
        {
            _operation = operation;
            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;
            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerMoved += OnPointerMoved;
            _canvas.PointerReleased += OnPointerReleased;
            Content = _canvas;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var p = e.GetCurrentPoint(_canvas).Position;
            _currentPath = new SKPath();
            _currentPath.MoveTo((float)p.X, (float)p.Y);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_currentPath != null)
            {
                var p = e.GetCurrentPoint(_canvas).Position;
                _currentPath.LineTo((float)p.X, (float)p.Y);
                _canvas.Invalidate();
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_currentPath != null)
            {
                _operation.Strokes.Add(new Stroke(_currentPath, _operation.Mode));
                _currentPath = null;
                _canvas.Invalidate();
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColors.White
            };
            foreach (var st in _operation.Strokes)
                canvas.DrawPath(st.Path, paint);

            if (_currentPath != null)
                canvas.DrawPath(_currentPath, paint);
        }
    }
}
