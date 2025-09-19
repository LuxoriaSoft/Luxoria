using Microsoft.UI.Xaml;
using System;

namespace LuxEditor.Utils
{
    /// <summary>
    /// Helper class to handle DPI scaling issues when Windows zoom is not at 100%.
    /// This fixes both cursor positioning and canvas sizing problems in LuxEditor when the main app has DPI awareness enabled.
    /// </summary>
    public static class DpiHelper
    {
        /// <summary>
        /// Gets the current DPI scale factor for the given UIElement.
        /// </summary>
        /// <param name="element">The UI element to get DPI for</param>
        /// <returns>DPI scale factor (1.0 = 100%, 1.25 = 125%, etc.)</returns>
        public static double GetDpiScale(UIElement element)
        {
            try
            {
                // Primary method: Use XamlRoot's RasterizationScale for WinUI
                if (element.XamlRoot?.RasterizationScale > 0)
                {
                    return element.XamlRoot.RasterizationScale;
                }

                // Secondary method: Try to get from the element's DispatcherQueue
                if (element.DispatcherQueue != null)
                {
                    // Use a fallback calculation based on system DPI
                    return GetSystemDpiScale();
                }
            }
            catch
            {
                // Continue to fallback
            }

            // If all else fails, assume 100% scaling
            return 1.0;
        }

        /// <summary>
        /// Gets the system DPI scale factor as a fallback method.
        /// </summary>
        /// <returns>System DPI scale factor</returns>
        private static double GetSystemDpiScale()
        {
            try
            {
                // Use Windows API to get system DPI if available
                var hdc = GetDC(IntPtr.Zero);
                if (hdc != IntPtr.Zero)
                {
                    var dpiX = GetDeviceCaps(hdc, 88); // LOGPIXELSX
                    ReleaseDC(IntPtr.Zero, hdc);
                    return dpiX / 96.0; // 96 DPI = 100% scaling
                }
            }
            catch
            {
                // Continue to fallback
            }

            return 1.0;
        }

        // Windows API imports for DPI detection
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        /// <summary>
        /// Adjusts a point position to account for DPI scaling.
        /// This corrects cursor positioning when Windows zoom is not at 100%.
        /// </summary>
        /// <param name="originalPosition">The original position from GetCurrentPoint</param>
        /// <param name="element">The UI element the position is relative to</param>
        /// <returns>DPI-corrected position</returns>
        public static Windows.Foundation.Point AdjustForDpi(Windows.Foundation.Point originalPosition, UIElement element)
        {
            var scale = GetDpiScale(element);
            
            // If scale is 1.0 (100%), no adjustment needed
            if (Math.Abs(scale - 1.0) < 0.001)
            {
                return originalPosition;
            }

            // For coordinate positioning, we need to scale down the position to match the logical coordinate system
            return new Windows.Foundation.Point(
                originalPosition.X / scale,
                originalPosition.Y / scale
            );
        }

        /// <summary>
        /// Gets a DPI-corrected position from a pointer event.
        /// For DpiCanvas (with IgnorePixelScaling = true), coordinates are already correct.
        /// For other elements, applies manual DPI correction.
        /// </summary>
        /// <param name="e">The pointer event args</param>
        /// <param name="relativeTo">The element the position should be relative to</param>
        /// <returns>DPI-corrected position</returns>
        public static Windows.Foundation.Point GetCorrectedPosition(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e, UIElement relativeTo)
        {
            // If it's a DpiCanvas, coordinates are already correct thanks to IgnorePixelScaling = true
            if (relativeTo is DpiCanvas dpiCanvas)
            {
                return dpiCanvas.GetDpiCorrectedPosition(e);
            }

            // For other elements, apply manual DPI correction
            var pointerPoint = e.GetCurrentPoint(relativeTo);
            return AdjustForDpi(pointerPoint.Position, relativeTo);
        }

        /// <summary>
        /// Applies DPI compensation transform to a canvas for proper rendering.
        /// Note: With IgnorePixelScaling = true on DpiCanvas, this transform is usually not needed.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to transform</param>
        /// <param name="element">The UI element to get DPI scale from</param>
        public static void ApplyDpiTransform(SkiaSharp.SKCanvas canvas, UIElement element)
        {
            // With IgnorePixelScaling = true on DpiCanvas, this transformation is generally not necessary
            // Kept for compatibility with other canvas types
            var scale = GetDpiScale(element);

            // If scale is 1.0 (100%), no transform needed
            if (Math.Abs(scale - 1.0) < 0.001)
            {
                return;
            }

            // Apply inverse scale transform to compensate for DPI scaling
            canvas.Scale((float)(1.0 / scale));
        }
    }
}