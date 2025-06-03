
using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.Models;
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
        private SKXamlCanvas _mainCanvas;
        public SKXamlCanvas _overlayCanva;

        private SKImage? _currentGpu;
        private SKBitmap? _currentCpu;

        private EditableImage? _currentImage;
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        public PhotoViewer()
        {
            InitializeComponent();

            _mainCanvas = new SKXamlCanvas();
            _overlayCanva = new SKXamlCanvas();

            CanvasHost.Children.Add(_mainCanvas);
            CanvasHost.Children.Add(_overlayCanva);

            _mainCanvas.PaintSurface += OnPaintSurface;

            ImageManager.Instance.OnSelectionChanged += img =>
            {
                if (img.PreviewBitmap != null) SetImage(img.PreviewBitmap);
                else if (img.EditedBitmap != null) SetImage(img.EditedBitmap);
                else if (img.OriginalBitmap != null) SetImage(img.OriginalBitmap);
            };
        }

        public void SetEditableImage(EditableImage image)
        {
            _currentImage = image;
            image.LayerManager.OnOperationChanged += operationSelected;
        }

        public void operationSelected()
        {
            var tool = _currentImage?.LayerManager?.SelectedLayer?.SelectedOperation?.Tool;
            if (tool == null) return;

            _overlayCanva.PaintSurface += tool.OnPaintSurface!;
            _overlayCanva.PointerPressed += tool.OnPointerPressed;
            _overlayCanva.PointerMoved += tool.OnPointerMoved;
            _overlayCanva.PointerReleased += tool.OnPointerReleased;
            tool.RefreshAction += RefreshAction;

            var bmp = _currentImage?.OriginalBitmap;

                (tool as BrushToolControl)?.ResizeCanvas(bmp.Width, bmp.Height);

        }

        private void RefreshAction()
        {
            _overlayCanva?.Invalidate();
        }

        public void SetImage(SKImage image)
        {
            _currentGpu?.Dispose();
            _currentGpu = image;
            _currentCpu = null;

            _mainCanvas.Width = image.Width;
            _mainCanvas.Height = image.Height;
            _mainCanvas.Invalidate();

            _overlayCanva.Width = image.Width;
            _overlayCanva.Height = image.Height;
            _overlayCanva.Invalidate();

            (_currentImage?.LayerManager?.SelectedLayer?.SelectedOperation?.Tool as BrushToolControl)?.ResizeCanvas(image.Width, image.Height);
        }

        public void SetImage(SKBitmap bitmap)
        {
            _currentCpu = bitmap;
            _currentGpu?.Dispose();
            _currentGpu = null;

            _mainCanvas.Width = bitmap.Width;
            _mainCanvas.Height = bitmap.Height;
            _mainCanvas.Invalidate();

            _overlayCanva.Width = bitmap.Width;
            _overlayCanva.Height = bitmap.Height;
            _overlayCanva.Invalidate();

            (_currentImage?.LayerManager?.SelectedLayer?.SelectedOperation?.Tool as BrushToolControl)?.ResizeCanvas(bitmap.Width, bitmap.Height);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentGpu != null)
                canvas.DrawImage(_currentGpu, 0, 0);
            else if (_currentCpu != null)
                canvas.DrawBitmap(_currentCpu, 0, 0);
        }

        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(ScrollViewerImage).Properties.IsMiddleButtonPressed)
            {
                _isDragging = true;
                _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
                (sender as UIElement)?.CapturePointer(e.Pointer);
            }
            else
            {
                _isDragging = false;
            }
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
