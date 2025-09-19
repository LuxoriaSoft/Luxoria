using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxEditor.Utils
{
    /// <summary>
    /// A DPI-aware canvas that extends SKXamlCanvas to handle high-DPI displays correctly.
    /// Automatically applies DPI scaling transformations and provides helper methods for position correction.
    /// </summary>
    public class DpiCanvas : SKXamlCanvas
    {
        public DpiCanvas()
        {
            this.IgnorePixelScaling = true;
        }

        /// <summary>
        /// Gets the current DPI scale factor for this canvas.
        /// </summary>
        /// <returns>The DPI scale factor, or 1.0 if unable to determine the scale.</returns>
        private double GetDpiScale()
        {
            try
            {
                return this.XamlRoot?.RasterizationScale ?? 1.0;
            }
            catch
            {
                return 1.0;
            }
        }

        /// <summary>
        /// Gets the DPI-corrected position from a pointer event.
        /// With IgnorePixelScaling = true, coordinates are already correct.
        /// </summary>
        /// <param name="e">The pointer event arguments containing the position information.</param>
        /// <returns>The position from the pointer event.</returns>
        public Windows.Foundation.Point GetDpiCorrectedPosition(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            return e.GetCurrentPoint(this).Position;
        }
        
        /// <summary>
        /// Corrects a SkiaSharp point position to account for DPI scaling when used with overlays.
        /// With IgnorePixelScaling = true, no correction is needed.
        /// </summary>
        /// <param name="position">The original position.</param>
        /// <returns>The position unchanged.</returns>
        public SKPoint CorrectPositionForOverlay(SKPoint position)
        {
            return position;
        }

        /// <summary>
        /// Synchronizes this canvas's layout properties with another DpiCanvas.
        /// Copies width, height, margin, and alignment properties from the source canvas.
        /// </summary>
        /// <param name="otherCanvas">The source canvas to synchronize properties from.</param>
        public void SyncWith(DpiCanvas otherCanvas)
        {
            this.Width = otherCanvas.Width;
            this.Height = otherCanvas.Height;
            this.Margin = otherCanvas.Margin;
            this.HorizontalAlignment = otherCanvas.HorizontalAlignment;
            this.VerticalAlignment = otherCanvas.VerticalAlignment;


            this.UpdateLayout();
        }
    }
}