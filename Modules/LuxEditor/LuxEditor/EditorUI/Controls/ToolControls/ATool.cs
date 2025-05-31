using LuxEditor.EditorUI.Interfaces;
using LuxEditor.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxEditor.EditorUI.Controls.ToolControls
{
    public abstract class ATool : UserControl, ITool
    {
        public abstract ToolType ToolType { get; set; }
        public abstract SKXamlCanvas Canvas { get; set; }
        public abstract SKPath? CurrentPath { get; set; }
        public ObservableCollection<Stroke> Strokes { get; } = new ObservableCollection<Stroke>();

        public ATool()
        {
            Canvas = new SKXamlCanvas();
            Canvas.PaintSurface += OnPaintSurface;
            Canvas.PointerPressed += OnPointerPressed;
            Canvas.PointerMoved += OnPointerMoved;
            Canvas.PointerReleased += OnPointerReleased;
            Content = Canvas;
        }

        public abstract void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e);
        public abstract void OnPointerMoved(object sender, PointerRoutedEventArgs e);
        public abstract void OnPointerPressed(object sender, PointerRoutedEventArgs e);
        public abstract void OnPointerReleased(object sender, PointerRoutedEventArgs e);
    }
}
