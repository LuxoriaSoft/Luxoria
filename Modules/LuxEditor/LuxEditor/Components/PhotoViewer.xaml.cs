using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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

        public PhotoViewer()
        {
            this.InitializeComponent();

            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += OnPaintSurface;
            ScrollViewerImage.Content = _canvas;

            ImageManager.Instance.OnSelectionChanged += image =>
            {
                SetImage(image.PreviewBitmap ?? image.EditedBitmap ?? image.OriginalBitmap);
            };
        }

        public void SetImage(SKBitmap? bitmap)
        {
            if (bitmap == null)
            {
                Debug.WriteLine("No image to display.");
                return;
            }

            Debug.WriteLine("SKXamlCanvas received image.");
            _currentImage = bitmap;
            _canvas.Invalidate();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            if (_currentImage != null)
            {
                var info = e.Info;

                float scale = Math.Min(
                    (float)info.Width / _currentImage.Width,
                    (float)info.Height / _currentImage.Height);

                float offsetX = (info.Width - _currentImage.Width * scale) / 2f;
                float offsetY = (info.Height - _currentImage.Height * scale) / 2f;

                canvas.Translate(offsetX, offsetY);
                canvas.Scale(scale);
                canvas.DrawBitmap(_currentImage, 0, 0);
            }
        }

        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            (sender as UIElement)?.CapturePointer(e.Pointer);
        }

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

        private void ScrollViewerImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }

        private void ScrollViewerImage_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }
    }
}
