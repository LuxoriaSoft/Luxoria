using CommunityToolkit.WinUI;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using LuxEditor.Utils;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;


namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private readonly DpiCanvas _mainCanvas;
        private readonly DpiCanvas _overlayCanvas;
        private readonly DpiCanvas _subjectsCanvas;

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
        public event Action? CropChanged;

        private CropController _cropController;
        public CropController CropController => _cropController;

        private readonly DpiCanvas _cropCanvas;
        private bool _isCropMode;

        // Subject detection overlay
        private ICollection<DetectedSubject> _detectedSubjects = new List<DetectedSubject>();
        private DetectedSubject? _hoveredSubject;
        private bool _showSubjectsOverlay = true;
        public event Action<DetectedSubject>? SubjectSelected;
        public bool IsCropMode { get => _isCropMode; set
            {
                _isCropMode = value;
                _cropCanvas.IsHitTestVisible = value;
                _cropCanvas.Invalidate();
                _overlayCanvas.Invalidate();
                if (_isCropMode)
                {
                    OnEnterCropMode();
                }
                else
                {
                    OnExitCropMode();
                }
            }
        }

        public event Action? BeginCropEditing;
        public event Action? EndCropEditing;

        public PhotoViewer()
        {
            InitializeComponent();

            this.Loaded += OnPhotoViewerLoaded;

            _mainCanvas = new DpiCanvas();
            _overlayCanvas = new DpiCanvas();
            _subjectsCanvas = new DpiCanvas { IsHitTestVisible = true };

            CanvasHost.Children.Add(_mainCanvas);
            CanvasHost.Children.Add(_overlayCanvas);
            CanvasHost.Children.Add(_subjectsCanvas);

            _mainCanvas.PaintSurface += OnPaintSurface;
            _overlayCanvas.PaintSurface += OnOverlayPaintSurface;
            _subjectsCanvas.PaintSurface += OnSubjectsPaintSurface;

            _overlayCanvas.PointerReleased += (_, _) => {
                if (_currentImage == null) return;
                if (_currentImage.LayerManager.SelectedLayer == null) return;
                if (_currentImage.LayerManager.SelectedLayer.SelectedOperation == null) return;
                _currentImage?.SaveState();
            };

            // Subject canvas interactions
            _subjectsCanvas.PointerMoved += OnSubjectsPointerMoved;
            _subjectsCanvas.PointerPressed += OnSubjectsPointerPressed;

            ImageManager.Instance.OnSelectionChanged += img =>
            {
                if (img.PreviewBitmap != null) SetImage(img.PreviewBitmap);
                else if (img.EditedBitmap != null) SetImage(img.EditedBitmap);
                else if (img.OriginalBitmap != null) SetImage(img.OriginalBitmap);

                var prev = img.PreviewBitmap ?? img.EditedBitmap ?? img.OriginalBitmap;
                if (prev == null) return;
                SetImage(prev);

                if (_cropController == null || img.OriginalBitmap == null) return;

                DispatcherQueue.TryEnqueue(() => _cropController.ResizeCanvas(img.OriginalBitmap.Width, img.OriginalBitmap.Height));

                _cropController.Box = img.Crop;

                if (_cropCanvas == null) throw new Exception("CropCanvas is null");
                _cropCanvas.Invalidate();
            };

            _cropCanvas = new DpiCanvas { IsHitTestVisible = false };
            CanvasHost.Children.Add(_cropCanvas);

            _cropController = new CropController((float)ActualWidth, (float)ActualHeight);

            _cropController.BoxChanged += () =>
            {
                CropChanged?.Invoke();
                InvalidateCrop();
            };

            _cropCanvas.PointerPressed += (s, e) => HandlePointer(e, _cropController.OnPointerPressed);
            _cropCanvas.PointerMoved += (s, e) =>
            {
                HandlePointer(e, _cropController.OnPointerMoved);

                var pt = DpiHelper.GetCorrectedPosition(e, _cropCanvas);
                _cropController.UpdateHover(pt.X, pt.Y);
            };

            _cropCanvas.PointerReleased += (s, e) => { if (IsCropMode) { _cropController.OnPointerReleased(); InvalidateCrop(); } };

            _cropCanvas.PaintSurface += CropCanvas_PaintSurface;
        }

        private void OnEnterCropMode()
        {
            if (_currentImage != null)
                _cropController.Box = _currentImage.Crop;
            BeginCropEditing?.Invoke();
            _cropCanvas.Visibility = Visibility.Visible;
            InvalidateCrop();
        }

        private void OnExitCropMode()
        {

            if (_currentImage != null)
                _currentImage.Crop = _cropController.Box;
            EndCropEditing?.Invoke();
            _cropCanvas.Visibility = Visibility.Collapsed;
            InvalidateCrop();

            _currentImage?.SaveState();
        }


        private void CropCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!IsCropMode) return;

            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);

            using var dim = new SKPaint { Color = SKColors.Black.WithAlpha(140) };
            var shadow = new SKPath { FillType = SKPathFillType.EvenOdd };
            shadow.AddRect(SKRect.Create(e.Info.Width, e.Info.Height));

            var b = _cropController.Box;

            var inner = new SKPath();
            inner.AddRect(SKRect.Create(b.X, b.Y, b.Width, b.Height));
            inner.Transform(SKMatrix.CreateRotationDegrees(b.Angle,
                                b.X + b.Width * .5f,
                                b.Y + b.Height * .5f));
            shadow.AddPath(inner);

            c.DrawPath(shadow, dim);

            _cropController.Draw(c);
        }


        private void HandlePointer(PointerRoutedEventArgs e, Action<double, double> action)
        {
            if (!IsCropMode) return;
            var pt = DpiHelper.GetCorrectedPosition(e, _cropCanvas);
            action(pt.X, pt.Y);
            InvalidateCrop();
        }

        public void InvalidateCrop() => _cropCanvas.Invalidate();


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
            DispatcherQueue.TryEnqueue(() => _cropController.ResizeCanvas(image.OriginalBitmap.Width, image.OriginalBitmap.Height));

            _cropController.Box = image.Crop;
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
            var image = _currentGpu ?? (_currentCpu != null ? SKImage.FromBitmap(_currentCpu) : null);
            if (image != null)
            {
                _currentTool?.ResizeCanvas(image.Width, image.Height);
            }
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
            // With IgnorePixelScaling = true on DpiCanvas, no DPI adjustment needed for canvas size
            DispatcherQueue.EnqueueAsync(() => {
                _mainCanvas.Width = width;
                _mainCanvas.Height = height;
                _mainCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                _mainCanvas.VerticalAlignment = VerticalAlignment.Center;
            });

            DispatcherQueue.EnqueueAsync(() =>
            {
                _overlayCanvas.Width = width;
                _overlayCanvas.Height = height;
                _overlayCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                _overlayCanvas.VerticalAlignment = VerticalAlignment.Center;
                _overlayCanvas.Margin = _mainCanvas.Margin;
            });

            DispatcherQueue.EnqueueAsync(() =>
            {
                _subjectsCanvas.Width = width;
                _subjectsCanvas.Height = height;
                _subjectsCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                _subjectsCanvas.VerticalAlignment = VerticalAlignment.Center;
                _subjectsCanvas.Margin = _mainCanvas.Margin;
            });

            DispatcherQueue.EnqueueAsync(() =>
            {
                _cropCanvas.Width = width;
                _cropCanvas.Height = height;
                _cropCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                _cropCanvas.VerticalAlignment = VerticalAlignment.Center;
                _cropCanvas.Margin = _mainCanvas.Margin;
            });

            DispatcherQueue.EnqueueAsync(() => {
                _mainCanvas.Invalidate();
                _overlayCanvas.Invalidate();
                _subjectsCanvas.Invalidate();
                _cropCanvas.Invalidate();
            });

            DispatcherQueue.EnqueueAsync(() =>
            {
                _mainCanvas.UpdateLayout();
                _overlayCanvas.UpdateLayout();
                _subjectsCanvas.UpdateLayout();
                _cropCanvas.UpdateLayout();
            });

            DispatcherQueue.EnqueueAsync(RefreshAction);

            DispatcherQueue.EnqueueAsync(() => _currentTool?.ResizeCanvas(width, height));
        }

        private void OnPhotoViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (_currentGpu != null)
            {
                ResizeCanvases(_currentGpu.Width, _currentGpu.Height);
            }
            else if (_currentCpu != null)
            {
                ResizeCanvases(_currentCpu.Width, _currentCpu.Height);
            }
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
            if (IsCropMode) return;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // With IgnorePixelScaling = true on DpiCanvas, no DPI scaling needed

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
                _lastPoint = DpiHelper.GetCorrectedPosition(e, ScrollViewerImage);
                (sender as UIElement)?.CapturePointer(e.Pointer);
            }
        }

        private void ScrollViewerImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;
            var current = DpiHelper.GetCorrectedPosition(e, ScrollViewerImage);
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

        // ======== Subject Detection Overlay Methods ========

        /// <summary>
        /// Sets the detected subjects to be displayed as overlays
        /// </summary>
        public void SetDetectedSubjects(List<DetectedSubject> subjects)
        {
            _detectedSubjects = subjects;
            _subjectsCanvas.Invalidate();
        }

        /// <summary>
        /// Shows or hides the subjects overlay
        /// </summary>
        public void SetSubjectsOverlayVisible(bool visible)
        {
            _showSubjectsOverlay = visible;
            _subjectsCanvas.Invalidate();
        }

        /// <summary>
        /// Clears all detected subjects
        /// </summary>
        public void ClearDetectedSubjects()
        {
            _detectedSubjects.Clear();
            _hoveredSubject = null;
            _subjectsCanvas.Invalidate();
        }

        /// <summary>
        /// Paints the subject detection overlays
        /// </summary>
        private void OnSubjectsPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (!_showSubjectsOverlay || _detectedSubjects.Count == 0)
            {
                return;
            }

            // Calculate scale ratio between displayed image and original image
            // Detection is done on EditedBitmap, but we might be displaying PreviewBitmap
            float scaleX = 1.0f;
            float scaleY = 1.0f;

            if (_currentImage != null)
            {
                // Canvas size matches the displayed bitmap (PreviewBitmap or EditedBitmap)
                // Detection coordinates are based on EditedBitmap
                scaleX = (float)e.Info.Width / _currentImage.EditedBitmap.Width;
                scaleY = (float)e.Info.Height / _currentImage.EditedBitmap.Height;
            }

            foreach (var subject in _detectedSubjects)
            {
                var rect = subject.BoundingBox;

                // Scale coordinates to match displayed image size
                var skRect = new SKRect(
                    rect.X * scaleX,
                    rect.Y * scaleY,
                    (rect.X + rect.Width) * scaleX,
                    (rect.Y + rect.Height) * scaleY
                );

                // Determine color and alpha based on state
                SKColor color;
                byte alpha;

                if (subject.IsSelected)
                {
                    // Selected: darker and more opaque version of overlay color
                    var baseColor = subject.OverlayColor;
                    color = new SKColor(
                        (byte)(baseColor.Red * 0.7),
                        (byte)(baseColor.Green * 0.7),
                        (byte)(baseColor.Blue * 0.7)
                    );
                    alpha = 220;
                }
                else if (subject == _hoveredSubject)
                {
                    // Hovered: bright overlay color
                    color = subject.OverlayColor;
                    alpha = 180;
                }
                else
                {
                    // Normal: semi-transparent overlay
                    color = subject.OverlayColor;
                    alpha = 100;
                }

                // Draw filled rectangle
                using var fillPaint = new SKPaint
                {
                    Color = color.WithAlpha(alpha),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(skRect, fillPaint);

                // Draw border
                using var strokePaint = new SKPaint
                {
                    Color = color.WithAlpha(255),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = subject.IsSelected ? 4 : (subject == _hoveredSubject ? 3 : 2),
                    IsAntialias = true
                };
                canvas.DrawRect(skRect, strokePaint);

                // Draw label
                var labelText = subject.DisplayName;
                using var textPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 16,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                };

                // Background for label
                var textBounds = new SKRect();
                textPaint.MeasureText(labelText, ref textBounds);
                var labelRect = new SKRect(
                    skRect.Left,
                    skRect.Top - 24,
                    skRect.Left + textBounds.Width + 12,
                    skRect.Top
                );

                using var labelBgPaint = new SKPaint
                {
                    Color = color.WithAlpha(220),
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRect(labelRect, labelBgPaint);

                // Draw text
                canvas.DrawText(labelText, skRect.Left + 6, skRect.Top - 6, textPaint);
            }
        }

        /// <summary>
        /// Handles pointer movement over subjects canvas for hover effect
        /// </summary>
        private void OnSubjectsPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_showSubjectsOverlay || _detectedSubjects.Count == 0) return;

            var pt = DpiHelper.GetCorrectedPosition(e, _subjectsCanvas);
            DetectedSubject? newHovered = null;

            // Calculate scale ratio (same as in OnSubjectsPaintSurface)
            float scaleX = 1.0f;
            float scaleY = 1.0f;
            if (_currentImage != null)
            {
                scaleX = (float)_subjectsCanvas.Width / _currentImage.EditedBitmap.Width;
                scaleY = (float)_subjectsCanvas.Height / _currentImage.EditedBitmap.Height;
            }

            // Find subject under pointer
            foreach (var subject in _detectedSubjects)
            {
                var rect = subject.BoundingBox;
                // Apply scale to bounding box coordinates
                float scaledX = rect.X * scaleX;
                float scaledY = rect.Y * scaleY;
                float scaledWidth = rect.Width * scaleX;
                float scaledHeight = rect.Height * scaleY;

                if (pt.X >= scaledX && pt.X <= scaledX + scaledWidth &&
                    pt.Y >= scaledY && pt.Y <= scaledY + scaledHeight)
                {
                    newHovered = subject;
                    break;
                }
            }

            // Update hover state
            if (newHovered != _hoveredSubject)
            {
                if (_hoveredSubject != null)
                    _hoveredSubject.IsHovered = false;

                _hoveredSubject = newHovered;

                if (_hoveredSubject != null)
                    _hoveredSubject.IsHovered = true;

                _subjectsCanvas.Invalidate();
            }
        }

        /// <summary>
        /// Handles pointer press on subjects canvas for selection
        /// </summary>
        private void OnSubjectsPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_showSubjectsOverlay || _hoveredSubject == null) return;

            // Toggle selection
            _hoveredSubject.IsSelected = !_hoveredSubject.IsSelected;

            // Notify editor about selection
            SubjectSelected?.Invoke(_hoveredSubject);

            // Force refresh of overlay to show updated selection state
            DispatcherQueue.TryEnqueue(() => _subjectsCanvas.Invalidate());
        }
    }
}
