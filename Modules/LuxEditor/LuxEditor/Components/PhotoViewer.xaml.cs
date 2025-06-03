using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private readonly SKXamlCanvas _mainCanvas;
        public readonly SKXamlCanvas _overlayCanva;

        private SKImage? _currentGpu;
        private SKBitmap? _currentCpu;

        private EditableImage? _currentImage;
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        private ATool? _currentTool;

        /// <summary>
        /// Initializes the viewer and sets up canvas layers.
        /// </summary>
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

        private void ClearOverlay()
        {
            _overlayCanva.PaintSurface += (sender, e) =>
            {
                var canvas = e.Surface.Canvas;
                canvas.Clear(SKColors.Transparent);
            };
            _overlayCanva.Invalidate();
        }

        /// <summary>
        /// Connects this viewer to the given editable image.
        /// </summary>
        public void SetEditableImage(EditableImage image)
        {
            _currentImage = image;
            image.LayerManager.OnOperationChanged += OperationSelected;
            image.LayerManager.OnLayerChanged += LayerSelected;
        }

        /// <summary>
        /// Handles switching to a new operation/tool on the active layer.
        /// </summary>
        public void OperationSelected()
        {
            UnsubscribeCurrentTool();
            var tool = _currentImage?.LayerManager?.SelectedLayer?.SelectedOperation?.Tool;
            if (tool == null) return;

            SubscribeTool(tool);

            var bmp = _currentImage?.OriginalBitmap;
            if (bmp != null) tool.ResizeCanvas(bmp.Width, bmp.Height);
        }

        public void LayerSelected()
        {
            UnsubscribeCurrentTool();
            ClearOverlay();
            if (_currentImage?.LayerManager.SelectedLayer == null) return;
            SKImage? resultImage = null;
            foreach (var op in _currentImage.LayerManager.SelectedLayer.Operations)
            {
                if (op == null) continue;
                if (op.Tool == null) continue;
                var bm = op.Tool.GetResult();

                if (bm == null) continue;

                if (op.Mode == BooleanOperationMode.Add)
                {
                    if (resultImage == null)
                    {
                        resultImage = SKImage.FromBitmap(bm);
                    }
                    else
                    {
                        using var temp = SKImage.FromBitmap(bm);
                        using var paint = new SKPaint
                        {
                            BlendMode = SKBlendMode.SrcOver
                        };
                        using var surface = SKSurface.Create(new SKImageInfo(resultImage.Width, resultImage.Height));
                        var canvas = surface.Canvas;
                        canvas.DrawImage(resultImage, 0, 0);
                        canvas.DrawImage(temp, 0, 0, paint);
                        canvas.Flush();
                        resultImage.Dispose();
                        resultImage = surface.Snapshot();
                    }
                }
                else if (op.Mode == BooleanOperationMode.Subtract)
                {
                    if (resultImage == null)
                    {
                        resultImage = SKImage.FromBitmap(bm);
                    }
                    else
                    {
                        using var temp = SKImage.FromBitmap(bm);
                        using var paint = new SKPaint
                        {
                            BlendMode = SKBlendMode.DstOut
                        };
                        using var surface = SKSurface.Create(new SKImageInfo(resultImage.Width, resultImage.Height));
                        var canvas = surface.Canvas;
                        canvas.DrawImage(resultImage, 0, 0);
                        canvas.DrawImage(temp, 0, 0, paint);
                        canvas.Flush();
                        resultImage.Dispose();
                        resultImage = surface.Snapshot();
                    }
                }
            }
            _overlayCanva.PaintSurface += (sender, args) =>
            {
                if (resultImage == null) return;
                var canvas = args.Surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.DrawImage(resultImage, 0, 0);
            };
            _overlayCanva.Invalidate();
        }

        public void ResetOverlay()
        {
            RefreshAction();
            UnsubscribeCurrentTool();
            _currentTool?.ResizeCanvas((int)_mainCanvas.Width, (int)_mainCanvas.Height);
        }

        /// <summary>
        /// Forces a repaint of the overlay.
        /// </summary>
        private void RefreshAction() => _overlayCanva.Invalidate();

        /// <summary>
        /// Sets a new GPU-backed image.
        /// </summary>
        public void SetImage(SKImage image)
        {
            _currentGpu?.Dispose();
            _currentGpu = image;
            _currentCpu = null;

            ResizeCanvases(image.Width, image.Height);
        }

        /// <summary>
        /// Sets a new CPU-backed bitmap.
        /// </summary>
        public void SetImage(SKBitmap bitmap)
        {
            _currentCpu = bitmap;
            _currentGpu?.Dispose();
            _currentGpu = null;

            ResizeCanvases(bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// Repaints the main canvas with the current bitmap/image.
        /// </summary>
        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentGpu != null) canvas.DrawImage(_currentGpu, 0, 0);
            else if (_currentCpu != null) canvas.DrawBitmap(_currentCpu, 0, 0);
        }

        /// <summary>
        /// Handles middle-mouse drag panning.
        /// </summary>
        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = e.GetCurrentPoint(ScrollViewerImage).Properties.IsMiddleButtonPressed;
            if (_isDragging)
            {
                _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
                (sender as UIElement)?.CapturePointer(e.Pointer);
            }
        }

        /// <summary>
        /// Updates scroll position while dragging.
        /// </summary>
        private void ScrollViewerImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;

            var current = e.GetCurrentPoint(ScrollViewerImage).Position;
            ScrollViewerImage.ChangeView(
                ScrollViewerImage.HorizontalOffset - (current.X - _lastPoint.X),
                ScrollViewerImage.VerticalOffset - (current.Y - _lastPoint.Y),
                ScrollViewerImage.ZoomFactor,
                true);

            _lastPoint = current;
        }

        /// <summary>
        /// Releases the drag state.
        /// </summary>
        private void ScrollViewerImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }

        /// <summary>
        /// Cancels the drag state.
        /// </summary>
        private void ScrollViewerImage_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }

        /// <summary>
        /// Adjusts both canvases to a new size and notifies the active tool.
        /// </summary>
        private void ResizeCanvases(int width, int height)
        {
            _mainCanvas.Width = width;
            _mainCanvas.Height = height;
            _overlayCanva.Width = width;
            _overlayCanva.Height = height;

            _mainCanvas.Invalidate();
            RefreshAction();

            _currentTool?.ResizeCanvas(width, height);
        }

        /// <summary>
        /// Removes event subscriptions for the current tool.
        /// </summary>
        private void UnsubscribeCurrentTool()
        {
            if (_currentTool == null) return;

            _overlayCanva.PaintSurface -= _currentTool.OnPaintSurface!;
            _overlayCanva.PointerPressed -= _currentTool.OnPointerPressed;
            _overlayCanva.PointerMoved -= _currentTool.OnPointerMoved;
            _overlayCanva.PointerReleased -= _currentTool.OnPointerReleased;
            _currentTool.RefreshAction -= RefreshAction;
            ClearOverlay();
        }



        /// <summary>
        /// Adds event subscriptions for the specified tool.
        /// </summary>
        private void SubscribeTool(ATool tool)
        {
            _currentTool = tool;

            _overlayCanva.PaintSurface += tool.OnPaintSurface!;
            _overlayCanva.PointerPressed += tool.OnPointerPressed;
            _overlayCanva.PointerMoved += tool.OnPointerMoved;
            _overlayCanva.PointerReleased += tool.OnPointerReleased;
            tool.RefreshAction += RefreshAction;
            RefreshAction();
        }
    }
}
