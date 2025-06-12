using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;

namespace LuxEditor.EditorUI.Controls
{
    public abstract class CurveBase : UserControl
    {
        protected readonly SKXamlCanvas _canvas;
        protected readonly List<SKPoint> ControlPoints = new();

        public abstract string SettingKey { get; }

        /// <summary>
        /// Initialises the common canvas.
        /// </summary>
        protected CurveBase()
        {
            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;
            Width = 250;
            MinHeight = 250;
        }

        /// <summary>
        /// Raised whenever the curve definition is modified.
        /// </summary>
        public event Action? CurveChanged;

        /// <summary>
        /// Handles the paint surface event to draw the curve and its background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) { }

        /// <summary>
        /// Draws a border around the canvas with a 1px stroke width.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        protected static void DrawBorder(SKCanvas c, int w, int h)
        {
            using var p = new SKPaint
            {
                Color = new SKColor(20, 20, 20),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            c.DrawRect(0.5f, 0.5f, w - 1, h - 1, p);
        }

        /// <summary>
        /// Draws a 45-degree baseline from bottom-left to top-right.
        /// </summary>
        protected static void DrawDiagonal(SKCanvas c, int w, int h)
        {
            using var p = new SKPaint { Color = SKColor.FromHsl(0,0,0,50), StrokeWidth = 1 };
            c.DrawLine(0, h, w, 0, p);
        }

        /// <summary>
        /// Draws a circle handle with black fill and white outline.
        /// </summary>
        protected static void DrawHandle(SKCanvas c, float x, float y, float r = 4)
        {
            using var fill = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var rim = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(x, y, r, fill);
            c.DrawCircle(x, y, r, rim);
        }

        /// <summary>
        /// Notifies subscribers that the curve has changed.
        /// </summary>
        protected void NotifyCurveChanged() => CurveChanged?.Invoke();

        /// <summary>
        /// Returns the LUT (Look-Up Table) for the curve, which is a byte array representing the curve's values.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetLut();

    }
}
