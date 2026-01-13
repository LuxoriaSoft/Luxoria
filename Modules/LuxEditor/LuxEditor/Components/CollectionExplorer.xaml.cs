/*
  LuxEditor - CollectionExplorer Component with Performance Instrumentation

  This file tracks:
  - Thumbnail generation performance
  - Bitmap resizing operations
  - Collection loading times
  - Memory usage for thumbnail storage
  - UI rendering performance
*/

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using CommunityToolkit.WinUI.Controls;
using System.Collections.ObjectModel;
using LuxEditor.Services;
using System.Linq;

namespace LuxEditor;

public sealed partial class CollectionExplorer : Page
{
    private List<SKBitmap> _bitmaps = new();
    private ScrollViewer _scrollViewer;
    private WrapPanel _imagePanel;
    private Border? _selectedBorder;

    public event Action<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>>? OnImageSelected;
    private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _originalBitmaps = new();

    // Performance tracking
    private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
    private int _thumbnailsGenerated = 0;
    private long _totalThumbnailMemory = 0;

    public CollectionExplorer()
    {
        InitializeComponent();
        BuildUI();
        SizeChanged += (s, e) => AdjustImageSizes(e.NewSize);
        _metrics.Log("CollectionExplorer component initialized", "INFO");
    }

    /// <summary>
    /// Build UI in code-behind, because xaml doesnt work with external libraries.
    /// </summary>
    private void BuildUI()
    {
        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollMode = ScrollMode.Enabled,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollMode = ScrollMode.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Padding = new Thickness(10)
        };

        _imagePanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalSpacing = 5,
        };

        _scrollViewer.Content = _imagePanel;
        RootGrid.Children.Add(_scrollViewer);
    }

    /// <summary>
    /// Adjusts the size of the images based on the new size of the control.
    /// </summary>
    private void AdjustImageSizes(Size newSize)
    {
        if (_imagePanel.Children.Count == 0) return;

        var adjustStopwatch = Stopwatch.StartNew();
        int adjustedCount = 0;

        double availableHeight = newSize.Height * 0.8;

        foreach (var child in _imagePanel.Children)
        {
            if (child is Border border && border.Child is SKXamlCanvas canvas)
            {
                int index = _imagePanel.Children.IndexOf(border);
                if (index < _bitmaps.Count)
                {
                    var bitmap = _bitmaps[index];
                    double scale = availableHeight / bitmap.Height;
                    double width = bitmap.Width * scale;

                    border.Width = width;
                    border.Height = availableHeight;
                    canvas.Width = width;
                    canvas.Height = availableHeight;
                    canvas.Invalidate();
                    adjustedCount++;
                }
                else
                {
                    Debug.WriteLine($"AdjustImageSizes: Index {index} out of range (bitmaps count = {_bitmaps.Count})");
                    continue;
                }
            }
        }

        adjustStopwatch.Stop();

        if (adjustedCount > 0)
        {
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = "CollectionExplorer",
                Phase = "AdjustImageSizes",
                DurationMs = adjustStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = 0,
                MemoryAfter = 0,
                MemoryDelta = 0,
                Metadata = new Dictionary<string, object>
                {
                    { "ImagesAdjusted", adjustedCount },
                    { "NewWidth", newSize.Width },
                    { "NewHeight", newSize.Height },
                    { "AvailableHeight", availableHeight }
                }
            });
        }
    }

    /// <summary>
    /// Set the bitmaps to display in the collection explorer.
    /// Tracks collection loading performance.
    /// </summary>
    public void SetBitmaps(List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> bitmaps)
    {
        if (bitmaps == null || bitmaps.Count == 0)
        {
            Debug.WriteLine("SetBitmaps: No bitmaps provided.");
            return;
        }

        var masterStopwatch = Stopwatch.StartNew();
        long memoryBefore = GC.GetTotalMemory(false);
        _thumbnailsGenerated = 0;
        _totalThumbnailMemory = 0;

        _metrics.Log($"Loading collection with {bitmaps.Count} images", "IMAGE");

        DispatcherQueue.TryEnqueue(() =>
        {
            var dispatchStopwatch = Stopwatch.StartNew();
            var phaseTimings = new Dictionary<string, double>();

            // Phase 1: Clear existing data
            var clearStopwatch = Stopwatch.StartNew();
            _bitmaps.Clear();
            _originalBitmaps.Clear();
            _imagePanel.Children.Clear();
            clearStopwatch.Stop();
            phaseTimings["ClearExisting"] = clearStopwatch.Elapsed.TotalMilliseconds;

            // Phase 2: Process each bitmap
            var processStopwatch = Stopwatch.StartNew();
            var thumbnailTimings = new List<double>();
            var mediumResTimings = new List<double>();

            foreach (var bitmap in bitmaps)
            {
                // Generate low-res thumbnail (200px height)
                var lowResStopwatch = Stopwatch.StartNew();
                int previewHeight = 200;
                int previewWidth = (int)((float)bitmap.Key.Width / bitmap.Key.Height * previewHeight);
                var lowResBitmap = CreateLowResBitmap(bitmap.Key, previewWidth, previewHeight);
                lowResStopwatch.Stop();
                thumbnailTimings.Add(lowResStopwatch.Elapsed.TotalMilliseconds);

                // Generate medium-res working image (up to 600px height)
                var mediumResStopwatch = Stopwatch.StartNew();
                previewHeight = int.Min(bitmap.Key.Height, 600);
                previewWidth = (int)((float)bitmap.Key.Width / bitmap.Key.Height * previewHeight);
                var mediumResBitmap = CreateLowResBitmap(bitmap.Key, previewWidth, previewHeight);
                mediumResStopwatch.Stop();
                mediumResTimings.Add(mediumResStopwatch.Elapsed.TotalMilliseconds);

                _bitmaps.Add(lowResBitmap);
                _originalBitmaps.Add(new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(mediumResBitmap, bitmap.Value));

                _thumbnailsGenerated++;
                _totalThumbnailMemory += lowResBitmap.ByteCount + mediumResBitmap.ByteCount;

                // Create UI elements
                var border = new Border
                {
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0))
                };

                var skiaCanvas = new SKXamlCanvas
                {
                    IgnorePixelScaling = true
                };
                int index = _bitmaps.Count - 1;
                skiaCanvas.PaintSurface += (sender, e) => OnPaintSurface(sender, e, index);

                border.Child = skiaCanvas;
                border.PointerEntered += (s, e) => OnHover(border, true);
                border.PointerExited += (s, e) => OnHover(border, false);
                border.Tapped += (s, e) => OnImageTapped(border, index);

                _imagePanel.Children.Add(border);
            }
            processStopwatch.Stop();
            phaseTimings["ProcessBitmaps"] = processStopwatch.Elapsed.TotalMilliseconds;

            // Phase 3: Adjust sizes
            var adjustStopwatch = Stopwatch.StartNew();
            AdjustImageSizes(new Size(ActualWidth, ActualHeight));
            adjustStopwatch.Stop();
            phaseTimings["AdjustSizes"] = adjustStopwatch.Elapsed.TotalMilliseconds;

            dispatchStopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            // Calculate statistics
            double avgThumbnailTime = thumbnailTimings.Count > 0 ? thumbnailTimings.Average() : 0;
            double maxThumbnailTime = thumbnailTimings.Count > 0 ? thumbnailTimings.Max() : 0;
            double avgMediumResTime = mediumResTimings.Count > 0 ? mediumResTimings.Average() : 0;

            // Record comprehensive metric
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = "CollectionExplorer",
                Phase = "SetBitmaps",
                DurationMs = dispatchStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryDelta = memoryAfter - memoryBefore,
                Metadata = new Dictionary<string, object>
                {
                    { "ImageCount", bitmaps.Count },
                    { "ThumbnailsGenerated", _thumbnailsGenerated },
                    { "TotalThumbnailMemory_MB", _totalThumbnailMemory / (1024.0 * 1024.0) },
                    { "Timing_ClearExisting_Ms", phaseTimings["ClearExisting"] },
                    { "Timing_ProcessBitmaps_Ms", phaseTimings["ProcessBitmaps"] },
                    { "Timing_AdjustSizes_Ms", phaseTimings["AdjustSizes"] },
                    { "AvgThumbnailTime_Ms", avgThumbnailTime },
                    { "MaxThumbnailTime_Ms", maxThumbnailTime },
                    { "AvgMediumResTime_Ms", avgMediumResTime }
                }
            });

            // Log summary
            Debug.WriteLine($"═══════════════════════════════════════════════════════════════════");
            Debug.WriteLine($"[CollectionExplorer] SetBitmaps Complete");
            Debug.WriteLine($"  Total: {dispatchStopwatch.Elapsed.TotalMilliseconds:F2}ms | Images: {bitmaps.Count}");
            Debug.WriteLine($"  Avg Thumbnail: {avgThumbnailTime:F2}ms | Max: {maxThumbnailTime:F2}ms");
            Debug.WriteLine($"  Thumbnail Memory: {_totalThumbnailMemory / 1024.0 / 1024.0:F2} MB");
            Debug.WriteLine($"  Total Memory Delta: {(memoryAfter - memoryBefore) / 1024.0 / 1024.0:F2} MB");
            Debug.WriteLine($"═══════════════════════════════════════════════════════════════════");
        });
    }

    /// <summary>
    /// Handles hover effects on thumbnail borders.
    /// </summary>
    private void OnHover(Border border, bool isHovered)
    {
        if (border != _selectedBorder)
        {
            border.BorderBrush = new SolidColorBrush(isHovered ? Windows.UI.Color.FromArgb(255, 200, 200, 200) : Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
    }

    /// <summary>
    /// Handles image selection with performance tracking.
    /// </summary>
    private void OnImageTapped(Border border, int index)
    {
        if (index >= _originalBitmaps.Count) return;

        var selectStopwatch = Stopwatch.StartNew();

        if (_selectedBorder != null)
            _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        _selectedBorder = border;
        _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 3, 169, 244));

        var selectedBitmap = _originalBitmaps[index];
        OnImageSelected?.Invoke(selectedBitmap);

        selectStopwatch.Stop();

        _metrics.RecordSample(new MetricSample
        {
            Timestamp = DateTime.UtcNow,
            OperationName = "CollectionExplorer",
            Phase = "ImageSelection",
            DurationMs = selectStopwatch.Elapsed.TotalMilliseconds,
            MemoryBefore = 0,
            MemoryAfter = 0,
            MemoryDelta = 0,
            Metadata = new Dictionary<string, object>
            {
                { "SelectedIndex", index },
                { "ImageWidth", selectedBitmap.Key.Width },
                { "ImageHeight", selectedBitmap.Key.Height },
                { "ImagePixels", selectedBitmap.Key.Width * selectedBitmap.Key.Height }
            }
        });

        _metrics.Log($"Image selected: index={index}, size={selectedBitmap.Key.Width}x{selectedBitmap.Key.Height}", "IMAGE");
    }

    /// <summary>
    /// Paints the thumbnail surface.
    /// </summary>
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
        var paintStopwatch = Stopwatch.StartNew();

        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (index < _bitmaps.Count && index >= 0)
        {
            var bitmap = _bitmaps[index];
            float scale = Math.Min((float)e.Info.Width / bitmap.Width, (float)e.Info.Height / bitmap.Height);
            float offsetX = (e.Info.Width - bitmap.Width * scale) / 2;
            float offsetY = (e.Info.Height - bitmap.Height * scale) / 2;
            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);
            canvas.DrawBitmap(bitmap, 0, 0);
        }

        paintStopwatch.Stop();

        // Only log slow paint operations (> 1ms)
        if (paintStopwatch.Elapsed.TotalMilliseconds > 1.0)
        {
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = "CollectionExplorer",
                Phase = "PaintSurface",
                DurationMs = paintStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = 0,
                MemoryAfter = 0,
                MemoryDelta = 0,
                Metadata = new Dictionary<string, object>
                {
                    { "ThumbnailIndex", index },
                    { "SurfaceWidth", e.Info.Width },
                    { "SurfaceHeight", e.Info.Height }
                }
            });
        }
    }

    /// <summary>
    /// Creates a low-resolution bitmap for thumbnail display.
    /// Tracks resizing performance.
    /// </summary>
    private SKBitmap CreateLowResBitmap(SKBitmap original, int targetWidth, int targetHeight)
    {
        var resizeStopwatch = Stopwatch.StartNew();
        long memoryBefore = GC.GetTotalMemory(false);

        var resizedBitmap = new SKBitmap(targetWidth, targetHeight);
        using (var surface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight)))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
            canvas.DrawImage(SKImage.FromBitmap(original), new SKRect(0, 0, targetWidth, targetHeight), sampling);
            canvas.Flush();
            var image = surface.Snapshot();
            image.ReadPixels(resizedBitmap.Info, resizedBitmap.GetPixels(), resizedBitmap.RowBytes, 0, 0);
        }

        resizeStopwatch.Stop();
        long memoryAfter = GC.GetTotalMemory(false);

        // Calculate compression ratio
        double compressionRatio = (double)(original.Width * original.Height) / (targetWidth * targetHeight);
        double pixelsPerMs = (targetWidth * targetHeight) / resizeStopwatch.Elapsed.TotalMilliseconds;

        // Record resize operation
        _metrics.RecordSample(new MetricSample
        {
            Timestamp = DateTime.UtcNow,
            OperationName = "CollectionExplorer",
            Phase = "CreateLowResBitmap",
            DurationMs = resizeStopwatch.Elapsed.TotalMilliseconds,
            MemoryBefore = memoryBefore,
            MemoryAfter = memoryAfter,
            MemoryDelta = memoryAfter - memoryBefore,
            Metadata = new Dictionary<string, object>
            {
                { "OriginalWidth", original.Width },
                { "OriginalHeight", original.Height },
                { "TargetWidth", targetWidth },
                { "TargetHeight", targetHeight },
                { "CompressionRatio", compressionRatio },
                { "OutputBytes", resizedBitmap.ByteCount },
                { "PixelsPerMs", pixelsPerMs }
            }
        });

        return resizedBitmap;
    }
}
