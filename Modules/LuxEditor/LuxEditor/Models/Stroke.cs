using System;
using SkiaSharp;

namespace LuxEditor.Models
{
    public enum StrokeMode
    {
        Add,
        Subtract
    }

    public class Stroke
    {
        public SKPath Path { get; }

        public StrokeMode Mode { get; set; }

        public Stroke(SKPath path, StrokeMode mode = StrokeMode.Add)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Mode = mode;
        }
    }
}
