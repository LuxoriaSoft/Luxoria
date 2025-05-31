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
using Windows.UI;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private SKXamlCanvas _mainCanvas;
        private SKXamlCanvas _currentOverlay;
        private Grid _canvasHost;

        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        private SKImage? _currentGpu;
        private SKBitmap? _currentCpu;

        private EditableImage? _currentImage;

        public PhotoViewer()
        {
            InitializeComponent();

            _mainCanvas = new SKXamlCanvas();
            _currentOverlay = new SKXamlCanvas
            {
                IsHitTestVisible = false,
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromArgb(150, 255,0,0))
            };


            // Utilise le CanvasHost défini en XAML
            CanvasHost.Children.Add(_mainCanvas);
            CanvasHost.Children.Add(_currentOverlay);

            _mainCanvas.PaintSurface += OnPaintSurface;

            ImageManager.Instance.OnSelectionChanged += img =>
            {
                if (img.PreviewBitmap != null) SetImage(img.PreviewBitmap);
                else if (img.EditedBitmap != null) SetImage(img.EditedBitmap);
                else if (img.OriginalBitmap != null) SetImage(img.OriginalBitmap);
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

        /// <summary>
        /// Register the EditableImage and subscribe to its OperationChanged event.
        /// </summary>
        public void SetEditableImage(EditableImage image)
        {
            _currentImage = image;
            image.LayerManager.OnOperationChanged += operationSelected;
        }

        public void operationSelected()
        {
            if (_currentImage == null || _currentImage.LayerManager == null || _currentImage.LayerManager.SelectedLayer == null || _currentImage.LayerManager.SelectedLayer.SelectedOperation == null) return;

            Debug.WriteLine("Operation Selected: " + _currentImage.LayerManager.SelectedLayer.SelectedOperation.Tool.ToolType);

            var toolCanvas = _currentImage
                                .LayerManager
                                .SelectedLayer
                                .SelectedOperation
                                .Tool
                                .Canvas as SKXamlCanvas;

            if (toolCanvas == null) return;

            _currentOverlay.Background = toolCanvas.Background;
            _currentOverlay.Opacity = 0.7;
            _currentOverlay.IsHitTestVisible = true;
            _currentOverlay.Width = _mainCanvas.Width;
            _currentOverlay.Height = _mainCanvas.Height;
            _currentOverlay.Invalidate();
            _mainCanvas.Invalidate();
        }


        /// <summary>
        /// Set the SKImage (GPU) to render on the main canvas.
        /// </summary>
        public void SetImage(SKImage image)
        {
            _currentGpu?.Dispose();
            _currentGpu = image;
            _currentCpu = null;

            _mainCanvas.Width = image.Width;
            _mainCanvas.Height = image.Height;
            _mainCanvas.Invalidate();

            _currentOverlay.Width = image.Width;
            _currentOverlay.Height = image.Height;
            _currentOverlay.Invalidate();
        }

        /// <summary>
        /// Set the SKBitmap (CPU) to render on the main canvas.
        /// </summary>
        public void SetImage(SKBitmap bitmap)
        {
            _currentCpu = bitmap;
            _currentGpu?.Dispose();
            _currentGpu = null;

            _mainCanvas.Width = bitmap.Width;
            _mainCanvas.Height = bitmap.Height;
            _mainCanvas.Invalidate();

            _currentOverlay.Width = bitmap.Width;
            _currentOverlay.Height = bitmap.Height;
            _currentOverlay.Invalidate();
        }

        /// <summary>
        /// PaintSurface event handler for the main SKXamlCanvas.
        /// </summary>
        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentGpu != null)
            {
                canvas.DrawImage(_currentGpu, 0, 0);
            }
            else if (_currentCpu != null)
            {
                canvas.DrawBitmap(_currentCpu, 0, 0);
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
