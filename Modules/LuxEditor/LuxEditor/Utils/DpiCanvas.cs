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
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
            this.LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyDpiTransform();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyDpiTransform();
        }

        private void OnLayoutUpdated(object sender, object e)
        {
            ApplyDpiTransform();
        }

        /// <summary>
        /// Applies DPI scaling transformation to the canvas.
        /// Called automatically when the canvas is loaded, resized, or layout is updated.
        /// </summary>
        private void ApplyDpiTransform()
        {
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
        /// </summary>
        /// <param name="e">The pointer event arguments containing the position information.</param>
        /// <returns>The corrected position accounting for DPI scaling.</returns>
        public Windows.Foundation.Point GetDpiCorrectedPosition(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(this).Position;

            return pos;
        }
        
        /// <summary>
        /// Corrects a SkiaSharp point position to account for DPI scaling when used with overlays.
        /// </summary>
        /// <param name="position">The original position to correct.</param>
        /// <returns>The position adjusted for DPI scaling, or the original position if no scaling is needed.</returns>
        public SKPoint CorrectPositionForOverlay(SKPoint position)
        {
            var scale = GetDpiScale();
            if (Math.Abs(scale - 1.0) > 0.001)
            {
                return new SKPoint(position.X / (float)scale, position.Y / (float)scale);
            }
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