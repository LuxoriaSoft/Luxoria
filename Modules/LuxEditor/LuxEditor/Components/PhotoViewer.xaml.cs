using CommunityToolkit.WinUI;
using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.EditorUI.Interfaces;
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
using System.Diagnostics;
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

        private Layer? _observedLayer;
        private SKImage? _cachedFusion;
        private SKImage? _cachedOverlay;

        private Action? _overlayTempHandler;
        private Action? _operationRefreshHandler;
        bool _isOperationSelected = false;

        public PhotoViewer()
        {
            InitializeComponent();

            _mainCanvas = new SKXamlCanvas();
            _overlayCanvas = new SKXamlCanvas();
            CanvasHost.Children.Add(_mainCanvas);
            CanvasHost.Children.Add(_overlayCanvas);

            _mainCanvas.PaintSurface += OnPaintSurface;
            _overlayCanvas.PaintSurface += OnOverlayPaintSurface;
            _overlayCanvas.PointerReleased += (_, _) => {
                if (_currentImage == null) return;
                if (_currentImage.LayerManager.SelectedLayer == null) return;
                if (_currentImage.LayerManager.SelectedLayer.SelectedOperation == null) return;
                _currentImage?.SaveState();
            };

            ImageManager.Instance.OnSelectionChanged += img =>
            {
                if (img.PreviewBitmap != null) SetImage(img.PreviewBitmap);
                else if (img.EditedBitmap != null) SetImage(img.EditedBitmap);
                else if (img.OriginalBitmap != null) SetImage(img.OriginalBitmap);
            };
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

        private SKImage? GetImageOps()
        {
            if (_currentImage == null) return null;
            var layer = _currentImage.LayerManager.SelectedLayer;
            if (layer == null) return null;

            SKImage? result = null;

            var currentOp = layer.SelectedOperation;

            foreach (var op in layer.Operations)
            {
                var bm = op.Tool?.GetResult();
                if (bm == null) continue;

                if (op.Mode == BooleanOperationMode.Add)
                {
                    if (result == null)
                    {
                        result = SKImage.FromBitmap(bm);
                    }
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

        /// <summary>
        /// 
        /// </summary>
        private void UpdateFusionCache()
        {
            _cachedFusion?.Dispose();
            _cachedFusion = GetImageOps();
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
            }

            var bmp = _currentImage?.OriginalBitmap;
            if (bmp != null) tool.ResizeCanvas(bmp.Width, bmp.Height);
            tool.OpsFusionned = GetImageOps();
            UpdateFusionCache();

            UpdateOverlayCache();

            RefreshAction();
            _isOperationSelected = true;
        }

        public void LayerSelected()
        {
            UnsubscribeCurrentTool();

            var layer = _currentImage?.LayerManager.SelectedLayer;

            if (layer == null) return;

            if (_observedLayer != null)
                _observedLayer.PropertyChanged -= OnLayerPropertyChanged;
            _observedLayer = layer;
            _observedLayer.PropertyChanged += OnLayerPropertyChanged;

            layer.SelectedOperation?.Tool.OnColorChanged(layer.OverlayColor.ToSKColor());
            RefreshAction();
            _isOperationSelected = false;
        }

        private void OnLayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Layer layer) return;

            switch (e.PropertyName)
            {
                case nameof(Layer.Filters):
                    RefreshAction();
                    break;

                case nameof(Layer.OverlayColor):
                    layer.SelectedOperation?.Tool?.OnColorChanged(layer.OverlayColor.ToSKColor());
                    break;

                case nameof(Layer.Strength):
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

        private void SubscribeTool(ATool tool)
        {
            _currentTool = tool;

            _overlayCanvas.PointerPressed += tool.OnPointerPressed;
            _overlayCanvas.PointerMoved += tool.OnPointerMoved;
            _overlayCanvas.PointerReleased += tool.OnPointerReleased;

            _overlayTempHandler = () =>
            {
                UpdateOverlayCache();
                _overlayCanvas.Invalidate();
            };
            tool.RefreshOverlayTemp += _overlayTempHandler;

            _operationRefreshHandler = () =>
            {
                UpdateFusionCache();
                UpdateOverlayCache();
                _overlayCanvas.Invalidate();
            };
            tool.RefreshOperation += _operationRefreshHandler;

            UpdateFusionCache();
            UpdateOverlayCache();
        }

        private void UnsubscribeCurrentTool()
        {
            if (_currentTool == null) return;

            _overlayCanvas.PointerPressed -= _currentTool.OnPointerPressed;
            _overlayCanvas.PointerMoved -= _currentTool.OnPointerMoved;
            _overlayCanvas.PointerReleased -= _currentTool.OnPointerReleased;

            if (_overlayTempHandler != null)
                _currentTool.RefreshOverlayTemp -= _overlayTempHandler;
            if (_operationRefreshHandler != null)
                _currentTool.RefreshOperation -= _operationRefreshHandler;

            _overlayTempHandler = null;
            _operationRefreshHandler = null;
            _currentTool = null;

            _overlayCanvas.Invalidate();
        }

        private void UpdateOverlayCache()
        {
            if (_currentTool == null || _currentImage == null)
            {
                _cachedOverlay?.Dispose();
                _cachedOverlay = null;
                return;
            }

            int w = (int)_overlayCanvas.Width;
            int h = (int)_overlayCanvas.Height;

            using var surf = SKSurface.Create(new SKImageInfo(w, h));
            var c = surf.Canvas;
            c.Clear(SKColors.Transparent);

            _currentTool.OpsFusionned = GetImageOps();


            var preview = _currentTool.GetResult();
            if (_currentTool.OpsFusionned != null) {
                c.DrawImage(_currentTool.OpsFusionned, new SKRect(0, 0, w, h));
            }

            c.Flush();

            _cachedOverlay?.Dispose();
            _cachedOverlay = surf.Snapshot();
        }

        private void OnOverlayPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentImage == null || _currentImage.LayerManager.SelectedLayer == null || (_currentImage.LayerManager.SelectedLayer.HasActiveFilters() && !_isOperationSelected) )
                return;

            int w = e.Info.Width, h = e.Info.Height;
            var overlayColor =
                _currentImage.LayerManager.SelectedLayer!.OverlayColor.ToSKColor();

            if (_cachedOverlay != null)
            {
                using var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateBlendMode(overlayColor, SKBlendMode.SrcIn)
                };
                canvas.DrawImage(_cachedOverlay, new SKRect(0, 0, w, h), paint);
            }

            if (_currentTool != null)
                _currentTool.OnPaintSurface(sender, e);
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
