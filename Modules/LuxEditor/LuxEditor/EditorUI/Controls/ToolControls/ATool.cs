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
        public abstract event Action RefreshAction;
        public abstract event Action RefreshOperation;
        public abstract event Action? RefreshOverlayTemp;

        public SKColor Color { get; set; }
        public BooleanOperationMode booleanOperationMode { get; set; }

        public SKImage? OpsFusionned;

        public ATool(BooleanOperationMode bMode)
        {
            this.booleanOperationMode = bMode;
        }

        public abstract void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e);
        public abstract void OnPointerMoved(object sender, PointerRoutedEventArgs e);
        public abstract void OnPointerPressed(object sender, PointerRoutedEventArgs e);
        public abstract void OnPointerReleased(object sender, PointerRoutedEventArgs e);
        public abstract void ResizeCanvas(int width, int height);
        public abstract SKBitmap? GetResult();
        public void OnColorChanged(SKColor newColor)
        {
            Color = newColor;
        }
        public abstract ATool Clone();
        public abstract void LoadMaskBitmap(SKBitmap bmp);
    }
}
