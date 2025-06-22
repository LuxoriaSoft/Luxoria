using SkiaSharp;
using System;

namespace LuxExport.Logic
{
    /// <summary>
    /// Adds the requested watermark as diagonal repeated banners over an existing bitmap.
    /// </summary>
    public static class WatermarkApplier
    {
        /// <summary>
        /// Returns a new bitmap identical to <paramref name="source"/> with the watermark applied.
        /// </summary>
        public static SKBitmap Apply(SKBitmap source, WatermarkSettings settings)
        {
            if (settings is null || !settings.Enabled)
                return source;

            var result = new SKBitmap(source.Info);
            using var canvas = new SKCanvas(result);
            canvas.DrawBitmap(source, 0, 0);

            canvas.RotateDegrees(settings.Angle);

            if (settings.Type == WatermarkType.Text)
                DrawText(canvas, settings, source);
            else
                DrawLogo(canvas, settings, source);

            return result;
        }

        /// <summary>
        /// Draws repeated text banners.
        /// </summary>
        private static void DrawText(SKCanvas canvas, WatermarkSettings s, SKBitmap src)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(s.FontFamily),
                TextSize = s.FontSize,
                Color = new SKColor(255, 255, 255, s.Opacity),
                FilterQuality = SKFilterQuality.High
            };

        float textWidth = paint.MeasureText(s.Text);
        paint.GetFontMetrics(out var fm);
        float textHeight = fm.Descent - fm.Ascent;
        
        int step = s.Step <= 0
                ? (int)Math.Ceiling(Math.Max(textWidth, textHeight) * 1.3f)
                : Math.Max(s.Step, (int)Math.Ceiling(textWidth * 1.1f));
        
        var diag = (int)Math.Sqrt(src.Width * src.Width + src.Height * src.Height);
            for (int y = -diag; y < diag * 2; y += step)
                    for (int x = -diag; x < diag * 2; x += step)
                        canvas.DrawText(s.Text, x, y, paint);
        }

        /// <summary>
        /// Draws repeated logo banners.
        /// </summary>
        private static void DrawLogo(SKCanvas canvas, WatermarkSettings s, SKBitmap src)
        {
            if (s.Logo == null)
                return;

            using var paint = new SKPaint { FilterQuality = SKFilterQuality.High, Color = new SKColor(255, 255, 255, s.Opacity) };
            var diag = (int)Math.Sqrt(src.Width * src.Width + src.Height * src.Height);
            int step = Math.Max(s.Step, s.Logo.Width);

            for (int y = -diag; y < diag * 2; y += step)
                for (int x = -diag; x < diag * 2; x += step)
                    canvas.DrawBitmap(s.Logo, x, y, paint);
        }
    }
}
