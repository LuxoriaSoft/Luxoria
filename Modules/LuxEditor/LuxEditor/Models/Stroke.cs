using System;
using SkiaSharp;

namespace LuxEditor.Models
{
    public class Stroke
    {
        public SKPath Path { get; set; }

        public Stroke(SKPath path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }
}
