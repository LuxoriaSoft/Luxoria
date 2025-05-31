using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using LuxEditor.Models;
using LuxEditor.EditorUI.Interfaces;
using LuxEditor.EditorUI.Controls.ToolControls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LuxEditor.Controls.ToolControls
{
    public partial class BrushToolControl : ATool
    {
        private ToolType _toolType = ToolType.Brush;
        public override ToolType ToolType
        {
            get => _toolType;
            set => _toolType = value;
        }

        public override SKXamlCanvas Canvas { get; set; } = new SKXamlCanvas();
        public override SKPath? CurrentPath { get; set; } = null;
        public ObservableCollection<Stroke> Strokes { get; } = new ObservableCollection<Stroke>();

        public BrushToolControl() : base() { }

        public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Pointer Pressed on Brush Tool");
            var p = e.GetCurrentPoint(Canvas).Position;
            CurrentPath = new SKPath();
            CurrentPath.MoveTo((float)p.X, (float)p.Y);
        }

        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentPath != null)
            {
                var p = e.GetCurrentPoint(Canvas).Position;
                CurrentPath.LineTo((float)p.X, (float)p.Y);
                Canvas.Invalidate();
            }
        }

        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentPath != null)
            {
                Strokes.Add(new Stroke(CurrentPath));
                CurrentPath = null;
                Canvas.Invalidate();
            }
        }

        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            Debug.WriteLine("Painting Brush Tool Surface");
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColors.White
            };
            foreach (var st in Strokes)
                canvas.DrawPath(st.Path, paint);

            if (CurrentPath != null)
                canvas.DrawPath(CurrentPath, paint);
        }
    }
}
