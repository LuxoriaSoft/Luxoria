using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Diagnostics;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        private readonly SKXamlCanvas _canvas;
        private SKBitmap? _currentImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoViewer"/> class.
        /// </summary>
        public PhotoViewer()
        {
            this.InitializeComponent();

            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;

            var viewbox = new Viewbox
            {
                Child = _canvas,
                Stretch = Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            ScrollViewerImage.Content = viewbox;


            ImageManager.Instance.OnSelectionChanged += image =>
            {
                SetImage(image.PreviewBitmap ?? image.EditedBitmap ?? image.OriginalBitmap);
            };
        }

        /// <summary>
        /// Sets the image to be displayed in the photo viewer.
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetImage(SKBitmap? bitmap)
        {
            if (bitmap == null)
            {
                Debug.WriteLine("No image to display.");
                return;
            }

            Debug.WriteLine("SKXamlCanvas received image.");
            _currentImage = bitmap;

            _canvas.Width = bitmap.Width;
            _canvas.Height = bitmap.Height;

            _canvas.Invalidate();
        }

        /// <summary>
        /// Handles the PaintSurface event of the SKXamlCanvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentImage != null)
            {
                canvas.DrawBitmap(_currentImage, 0, 0);
            }
        }

        /// <summary>
        /// Handles the PointerPressed event of the ScrollViewerImage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            (sender as UIElement)?.CapturePointer(e.Pointer);
        }

        /// <summary>
        /// Handles the PointerMoved event of the ScrollViewerImage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;

            var currentPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            double deltaX = currentPoint.X - _lastPoint.X;
            double deltaY = currentPoint.Y - _lastPoint.Y;

            ScrollViewerImage.ChangeView(
                ScrollViewerImage.HorizontalOffset - deltaX,
                ScrollViewerImage.VerticalOffset - deltaY,
                ScrollViewerImage.ZoomFactor,
                disableAnimation: true);

            _lastPoint = currentPoint;
        }

        /// <summary>
        /// Handles the PointerReleased event of the ScrollViewerImage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }

        /// <summary>
        /// Handles the PointerCanceled event of the ScrollViewerImage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewerImage_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }
    }
}
