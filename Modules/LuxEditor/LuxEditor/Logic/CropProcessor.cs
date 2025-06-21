using SkiaSharp;
using LuxEditor.EditorUI.Controls;
using System;

namespace LuxEditor.Logic
{
    /// <summary>Utility that rotates & crops a bitmap.</summary>
    public static class CropProcessor
    {
        public static SKBitmap Apply(SKBitmap src, CropController.CropBox box)
        {
            int w = (int)box.Width;
            int h = (int)box.Height;
            if (w <= 0 || h <= 0) return src.Copy();

            var dst = new SKBitmap(w, h, src.ColorType, src.AlphaType);
            using var surf = SKSurface.Create(new SKImageInfo(w, h));
            var c = surf.Canvas;

            // 1 : on met l’origine sur le coin haut-gauche de la box
            c.Translate(-box.X, -box.Y);

            // 2 : rotation autour du centre *local* (w/2, h/2)
            if (Math.Abs(box.Angle) > 0.001f)
            {
                float cx = box.Width * .5f;
                float cy = box.Height * .5f;
                c.Translate(cx, cy);
                c.RotateDegrees(-box.Angle);
                c.Translate(-cx, -cy);
            }

            c.DrawBitmap(src, 0, 0);
            c.Flush();
            surf.ReadPixels(dst.Info, dst.GetPixels(), dst.RowBytes, 0, 0);
            return dst;
        }


        /// <summary>Return a copy of <paramref name="box"/> scaled by <paramref name="sx"/> / <paramref name="sy"/>.</summary>
        public static CropController.CropBox Scale(CropController.CropBox box, float sx, float sy)
            => new()
            {
                X = box.X * sx,
                Y = box.Y * sy,
                Width = box.Width * sx,
                Height = box.Height * sy,
                Angle = box.Angle
            };
    }
}
