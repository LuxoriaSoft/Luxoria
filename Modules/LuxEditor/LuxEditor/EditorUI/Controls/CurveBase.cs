using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>Abstract drawing surface for every curve control.</summary>
    public abstract class CurveBase : UserControl
    {
        protected readonly SKXamlCanvas _canvas;
        protected readonly List<SKPoint> ControlPoints = new();

        /// <summary>Initialises the common canvas.</summary>
        protected CurveBase()
        {
            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;
            MinWidth = 250;
            MinHeight = 250;
        }

        /// <summary>Raised whenever the curve definition is modified.</summary>
        public event Action? CurveChanged;

        protected virtual void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) { }

        protected void NotifyCurveChanged() => CurveChanged?.Invoke();
    }
}
