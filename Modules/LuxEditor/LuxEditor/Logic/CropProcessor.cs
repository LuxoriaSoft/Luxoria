using SkiaSharp;
using LuxEditor.EditorUI.Controls;
using System;
using System.Diagnostics;

namespace LuxEditor.Logic
{
    /// <summary>Utility that rotates and crops a bitmap exactly on the CropBox area.</summary>
    public static class CropProcessor
    {
        /// <summary>
        /// Return a new bitmap containing only the pixels inside <paramref name="box"/>,
        /// correctly deskewed by its rotation angle.
        /// </summary>
        public static SKBitmap Apply(SKBitmap src, CropController.CropBox box)
        {
            int w = (int)MathF.Round(box.Width);
            int h = (int)MathF.Round(box.Height);
            if (w <= 0 || h <= 0) return src.Copy();

            var info = new SKImageInfo(w, h, src.ColorType, src.AlphaType, src.ColorSpace);
            var dst = new SKBitmap(info);

            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            float cxSrc = box.X + box.Width * 0.5f;
            float cySrc = box.Y + box.Height * 0.5f;

            canvas.Translate(w * 0.5f, h * 0.5f);

            if (Math.Abs(box.Angle) > 0.001f)
                canvas.RotateDegrees(-box.Angle);

            canvas.Translate(-cxSrc, -cySrc);

            canvas.DrawBitmap(src, 0, 0);
            canvas.Flush();

            surface.ReadPixels(dst.Info, dst.GetPixels(), dst.RowBytes, 0, 0);

            Debug.WriteLine("Box: X:" + box.X + " Y: " + box.Y + " Width: " + box.Width + " Height: " + box.Height + " Angle: " + box.Angle);

            return dst;
        }

        /// <summary>
        /// Return a copy of <paramref name="box"/> scaled by <paramref name="sx"/> and <paramref name="sy"/>.
        /// </summary>
        //public static CropController.CropBox Scale(CropController.CropBox box, float sx, float sy) => new()
        //{
        //    X = box.X * sx,
        //    Y = box.Y * sy,
        //    Width = box.Width * sx,
        //    Height = box.Height * sy,
        //    Angle = box.Angle
        //};
    }
}
