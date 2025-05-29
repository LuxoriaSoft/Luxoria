using LuxEditor.Models;
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

        private SKImage? _currentGpu;
        private SKBitmap? _currentCpu;

        private EditableImage? _currentImage;

        private readonly SKXamlCanvas _canvas = new();
        private readonly SKXamlCanvas _overlay = new SKXamlCanvas
        {
            IsHitTestVisible = true,
            Opacity = 0.7
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoViewer"/> class.
        /// </summary>
        public PhotoViewer()
        {
            InitializeComponent();

            _canvas.PaintSurface += OnPaintSurface;

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(_canvas);
            stackPanel.Children.Add(_overlay);

            var viewbox = new Viewbox
            {
                Child = stackPanel,
                Stretch = Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ScrollViewerImage.Content = viewbox;


            ImageManager.Instance.OnSelectionChanged += img =>
            {
                SetImage(img.PreviewBitmap ?? img.EditedBitmap ?? img.OriginalBitmap);
            };
        }

        private SKMatrix GetCurrentTransform()
        {
            return SKMatrix.CreateScaleTranslation(
                (float)ScrollViewerImage.ZoomFactor,
                (float)ScrollViewerImage.ZoomFactor,
                (float)ScrollViewerImage.HorizontalOffset,
                (float)ScrollViewerImage.VerticalOffset
            );
        }

        public void SetEditableImage(EditableImage image)
        {
            _currentImage = image;
        }

        /// <summary>
        /// Sets the image to be displayed in the photo viewer.
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetImage(SKImage image)
        {


            _currentGpu?.Dispose();
            _currentGpu = image;
            _currentCpu = null;

            _canvas.Width = image.Width;
            _canvas.Height = image.Height;
            _canvas.Invalidate();

            _overlay.Width = image.Width;
            _overlay.Height = image.Height;
            _overlay.Invalidate();
        }

        /// <summary>
        /// Sets the image to be displayed in the photo viewer.
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetImage(SKBitmap bitmap)
        {
            _currentCpu = bitmap;
            _currentGpu?.Dispose();
            _currentGpu = null;

            _canvas.Width = bitmap.Width;
            _canvas.Height = bitmap.Height;
            _canvas.Invalidate();
        }

        /// <summary>
        /// Handles the PaintSurface event of the SKXamlCanvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentGpu != null)
                canvas.DrawImage(_currentGpu, 0, 0);
            else if (_currentCpu != null)
                canvas.DrawBitmap(_currentCpu, 0, 0);
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
