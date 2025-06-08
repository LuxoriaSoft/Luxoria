using CommunityToolkit.WinUI;
using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private readonly SKXamlCanvas _mainCanvas;
        private readonly SKXamlCanvas _overlayCanvas;

        private SKImage? _currentGpu;
        private SKBitmap? _currentCpu;

        private EditableImage? _currentImage;
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        private ATool? _currentTool;
        private CancellationTokenSource? _overlayCts;
        private int _isOverlayRendering;

        private Layer? _observedLayer;
        private EventHandler<SKPaintSurfaceEventArgs>? _overlayClearHandler;

        public PhotoViewer()
        {
            InitializeComponent();

            _mainCanvas = new SKXamlCanvas();
            _overlayCanvas = new SKXamlCanvas();
            CanvasHost.Children.Add(_mainCanvas);
            CanvasHost.Children.Add(_overlayCanvas);

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
            if (_overlayClearHandler != null)
                _overlayCanvas.PaintSurface -= _overlayClearHandler;

            _overlayClearHandler = (s, e) =>
            {
                var c = e.Surface.Canvas;
                c.Clear(SKColors.Transparent);
            };
            _overlayCanvas.PaintSurface += _overlayClearHandler;
            _overlayCanvas.Invalidate();
        }

        public void SetEditableImage(EditableImage image)
        {
            if (_currentImage != null)
            {
                _currentImage.LayerManager.OnOperationChanged -= OperationSelected;
                _currentImage.LayerManager.OnLayerChanged -= LayerSelected;
            }

            _currentImage = image;
            image.LayerManager.OnOperationChanged += OperationSelected;
            image.LayerManager.OnLayerChanged += LayerSelected;
        }

        public void OperationSelected()
        {
            UnsubscribeCurrentTool();

            var layer = _currentImage?.LayerManager?.SelectedLayer;
            var tool = layer?.SelectedOperation?.Tool;
            if (tool == null) return;

            SubscribeTool(tool);

            if (layer != null)
            {
                tool.OnColorChanged(layer.OverlayColor.ToSKColor());
                if (tool is BrushToolControl brush)
                    brush.ShowExistingStrokes = !layer.HasActiveFilters();
            }

            var bmp = _currentImage?.OriginalBitmap;
            if (bmp != null) tool.ResizeCanvas(bmp.Width, bmp.Height);
            tool.OpsFusionned = GetImageOps();
        }

        private SKImage? GetImageOps()
        {
            if (_currentImage == null) return null;
            var layer = _currentImage.LayerManager.SelectedLayer;
            if (layer == null) return null;

            SKImage? result = null;
            foreach (var op in layer.Operations)
            {
                var bm = op.Tool?.GetResult();
                if (bm == null) continue;

                if (op.Mode == BooleanOperationMode.Add)
                {
                    if (result == null) result = SKImage.FromBitmap(bm);
                    else
                    {
                        using var temp = SKImage.FromBitmap(bm);
                        using var surface = SKSurface.Create(new SKImageInfo(result.Width, result.Height));
                        var canvas = surface.Canvas;
                        canvas.DrawImage(result, 0, 0);
                        using var paint = new SKPaint { BlendMode = SKBlendMode.SrcOver };
                        canvas.DrawImage(temp, 0, 0, paint);
                        canvas.Flush();
                        result.Dispose();
                        result = surface.Snapshot();
                    }
                }
                else if (op.Mode == BooleanOperationMode.Subtract)
                {
                    if (result == null) continue;
                    using var temp = SKImage.FromBitmap(bm);
                    using var surface = SKSurface.Create(new SKImageInfo(result.Width, result.Height));
                    var canvas = surface.Canvas;
                    canvas.DrawImage(result, 0, 0);
                    using var paint = new SKPaint { BlendMode = SKBlendMode.DstOut };
                    canvas.DrawImage(temp, 0, 0, paint);
                    canvas.Flush();
                    result.Dispose();
                    result = surface.Snapshot();
                }
            }
            return result;
        }

        private void DebouncedOverlayRefresh()
        {
            _overlayCts?.Cancel();
            _overlayCts = new CancellationTokenSource();
            var token = _overlayCts.Token;

            _ = Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref _isOverlayRendering, 1, 0) != 0)
                    return;

                try
                {
                    await Task.Delay(150, token);
                    if (token.IsCancellationRequested) return;
                    await DispatcherQueue.EnqueueAsync(() => _overlayCanvas.Invalidate());
                }
                catch (OperationCanceledException) { }
                finally
                {
                    Interlocked.Exchange(ref _isOverlayRendering, 0);
                }
            });
        }
        public void LayerSelected()
        {
            UnsubscribeCurrentTool();
            ClearOverlay();

            var layer = _currentImage?.LayerManager.SelectedLayer;
            if (layer == null) return;

            if (_observedLayer != null)
                _observedLayer.PropertyChanged -= OnLayerPropertyChanged;
            _observedLayer = layer;
            _observedLayer.PropertyChanged += OnLayerPropertyChanged;

            if (layer.SelectedOperation?.Tool is BrushToolControl brush)
            {
                brush.ShowExistingStrokes = !layer.HasActiveFilters();
                brush.OnColorChanged(layer.OverlayColor.ToSKColor());
            }
            DebouncedOverlayRefresh();
        }

        private void OnLayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Layer layer) return;

            switch (e.PropertyName)
            {
                case nameof(Layer.Filters):
                    if (layer.SelectedOperation?.Tool is BrushToolControl b)
                        b.ShowExistingStrokes = !layer.HasActiveFilters();
                    DebouncedOverlayRefresh();
                    break;

                case nameof(Layer.OverlayColor):
                    layer.SelectedOperation?.Tool?.OnColorChanged(layer.OverlayColor.ToSKColor());
                    DebouncedOverlayRefresh();
                    break;

                case nameof(Layer.Strength):
                    DebouncedOverlayRefresh();
                    break;
            }
        }

        public void ResetOverlay()
        {
            RefreshAction();
            _currentTool?.ResizeCanvas((int)_mainCanvas.Width,
                                       (int)_mainCanvas.Height);
        }

        private void RefreshAction()
        {
            if (_currentTool == null) return;
            _overlayCanvas.Invalidate();
        }

        public void SetImage(SKImage image)
        {
            _currentGpu?.Dispose();
            _currentGpu = image;
            _currentCpu = null;
            ResizeCanvases(image.Width, image.Height);
        }

        public void SetImage(SKBitmap bitmap)
        {
            _currentCpu = bitmap;
            _currentGpu?.Dispose();
            _currentGpu = null;
            ResizeCanvases(bitmap.Width, bitmap.Height);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            if (_currentGpu != null) canvas.DrawImage(_currentGpu, 0, 0);
            else if (_currentCpu != null) canvas.DrawBitmap(_currentCpu, 0, 0);
        }

        private void ResizeCanvases(int width, int height)
        {
            _mainCanvas.Width = width;
            _mainCanvas.Height = height;
            _overlayCanvas.Width = width;
            _overlayCanvas.Height = height;
            _mainCanvas.Invalidate();
            RefreshAction();
            _currentTool?.ResizeCanvas(width, height);
        }

        private void UnsubscribeCurrentTool()
        {
            if (_currentTool == null) return;
            _overlayCanvas.PaintSurface -= _currentTool.OnPaintSurface!;
            _overlayCanvas.PointerPressed -= _currentTool.OnPointerPressed;
            _overlayCanvas.PointerMoved -= _currentTool.OnPointerMoved;
            _overlayCanvas.PointerReleased -= _currentTool.OnPointerReleased;
            _currentTool.RefreshAction -= RefreshAction;
            ClearOverlay();
        }

        private void SubscribeTool(ATool tool)
        {
            _currentTool = tool;
            _overlayCanvas.PaintSurface += tool.OnPaintSurface!;
            _overlayCanvas.PointerPressed += tool.OnPointerPressed;
            _overlayCanvas.PointerMoved += tool.OnPointerMoved;
            _overlayCanvas.PointerReleased += tool.OnPointerReleased;
            tool.RefreshAction += RefreshAction;
            RefreshAction();
        }

        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = e.GetCurrentPoint(ScrollViewerImage).Properties.IsMiddleButtonPressed;
            if (_isDragging)
            {
                _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
                (sender as UIElement)?.CapturePointer(e.Pointer);
            }
        }

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
