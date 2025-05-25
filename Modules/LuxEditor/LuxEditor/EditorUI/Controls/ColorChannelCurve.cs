using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>Single RGB-channel point curve (placeholder).</summary>
    public sealed class ColorChannelCurve : CurveBase
    {
        private readonly SKColor _col;

        /// <summary>Creates a channel-specific curve.</summary>
        public ColorChannelCurve(SKColor col)
        {
            _col = col;
            Content = _canvas;
        }

        protected override void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);
            using var p = new SKPaint { Color = _col, StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
            c.DrawLine(0, e.Info.Height, e.Info.Width, 0, p);
        }
    }
}
