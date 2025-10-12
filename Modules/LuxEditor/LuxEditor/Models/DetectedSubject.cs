using SkiaSharp;
using System;
using System.Drawing;

namespace LuxEditor.Models
{
    /// <summary>
    /// Represents a detected subject/ROI in an image
    /// </summary>
    public class DetectedSubject
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Class ID from the detection model (e.g., "person", "dog", etc.)
        /// </summary>
        public int ClassId { get; set; }

        /// <summary>
        /// Human-readable label for the subject
        /// </summary>
        public string Label { get; set; } = "Unknown";

        /// <summary>
        /// Confidence score (0.0 to 1.0)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Bounding box of the detected subject
        /// </summary>
        public Rectangle BoundingBox { get; set; }

        /// <summary>
        /// Mask bitmap (from GrabCut) - foreground in white, background in black
        /// </summary>
        public SKBitmap? Mask { get; set; }

        /// <summary>
        /// Whether this subject is selected for blur
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether this subject is currently hovered in UI
        /// </summary>
        public bool IsHovered { get; set; }

        /// <summary>
        /// Color used to display the overlay (generated from ID for consistency)
        /// </summary>
        public SKColor OverlayColor { get; init; }

        public DetectedSubject()
        {
            // Generate a deterministic but visually distinct color from the ID
            var hash = Id.GetHashCode();
            var hue = (hash % 360 + 360) % 360; // 0-359
            OverlayColor = SKColor.FromHsv(hue, 80, 90, 128); // Semi-transparent
        }

        /// <summary>
        /// Returns a display string for this subject
        /// </summary>
        public string DisplayName => $"{Label} ({Confidence * 100:F1}%)";
    }
}
