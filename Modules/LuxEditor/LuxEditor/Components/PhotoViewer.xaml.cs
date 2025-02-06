using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System.IO;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        public PhotoViewer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the SKBitmap image into the Image control for viewing.
        /// </summary>
        public void SetImage(SKBitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Seek(0, SeekOrigin.Begin);

                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);
                writeableBitmap.SetSource(ms.AsRandomAccessStream());
                DisplayImage.Source = writeableBitmap;
            }
        }

        /// <summary>
        /// Fired when the pointer is pressed down (mouse button down).
        /// Capture the pointer so we receive move events even if the cursor goes outside the control.
        /// </summary>
        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            (sender as UIElement)?.CapturePointer(e.Pointer);
        }

        /// <summary>
        /// Fired as the mouse moves. If we're dragging, update ScrollViewer offsets.
        /// </summary>
        private void ScrollViewerImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;

            var currentPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            double deltaX = currentPoint.X - _lastPoint.X;
            double deltaY = currentPoint.Y - _lastPoint.Y;

            double scrollOffsetX = ScrollViewerImage.HorizontalOffset - deltaX;
            double scrollOffsetY = ScrollViewerImage.VerticalOffset - deltaY;

            // ChangeView(xOffset, yOffset, zoomFactor, disableAnimation)
            ScrollViewerImage.ChangeView(scrollOffsetX, scrollOffsetY, ScrollViewerImage.ZoomFactor, disableAnimation: true);

            _lastPoint = currentPoint;
        }

        /// <summary>
        /// Fired when the pointer is released (mouse button up). Stop dragging.
        /// </summary>
        private void ScrollViewerImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }

        /// <summary>
        /// Fired if the pointer capture is canceled, for example if another control
        /// takes pointer capture. Ensure we stop dragging.
        /// </summary>
        private void ScrollViewerImage_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }
    }
}
