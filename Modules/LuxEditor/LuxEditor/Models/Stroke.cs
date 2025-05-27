using System;
using SkiaSharp;

namespace LuxEditor.Models
{
    public enum BooleanOperationMode
    {
        Add,
        Subtract
    }

    public class Stroke
    {
        public SKPath Path { get; }

        public BooleanOperationMode Mode { get; set; }

        public Stroke(SKPath path, BooleanOperationMode mode = BooleanOperationMode.Add)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Mode = mode;
        }
    }
}
