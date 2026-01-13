/*
  LuxEditor - PhotoViewer Component with Performance Instrumentation

  This file tracks:
  - Image encoding performance (SKBitmap to PNG)
  - WriteableBitmap creation time
  - Memory allocation during image display
  - Pan/zoom interaction latency
*/

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LuxEditor.Services;

namespace LuxEditor.Components
{
    public sealed partial class PhotoViewer : Page
    {
        private bool _isDragging;
        private Windows.Foundation.Point _lastPoint;

        // Performance tracking
        private readonly PerformanceMetrics _metrics = PerformanceMetrics.Instance;
        private int _setImageCallCount = 0;
        private int _panEventCount = 0;
        private DateTime _lastPanTime = DateTime.MinValue;

        public PhotoViewer()
        {
            this.InitializeComponent();
            _metrics.Log("PhotoViewer component initialized", "INFO");
        }

        /// <summary>
        /// Sets the SKBitmap image into the Image control for viewing.
        /// Tracks encoding and display performance.
        /// </summary>
        public void SetImage(SKBitmap bitmap)
        {
            _setImageCallCount++;
            var masterStopwatch = Stopwatch.StartNew();
            var phaseTimings = new Dictionary<string, double>();

            long memoryBefore = GC.GetTotalMemory(false);

            // Phase 1: PNG Encoding
            var encodeStopwatch = Stopwatch.StartNew();
            using (MemoryStream ms = new MemoryStream())
            {
                // Sub-phase: Encode to PNG
                var pngEncodeStopwatch = Stopwatch.StartNew();
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                pngEncodeStopwatch.Stop();
                phaseTimings["PngEncoding"] = pngEncodeStopwatch.Elapsed.TotalMilliseconds;

                // Sub-phase: Stream seek
                var seekStopwatch = Stopwatch.StartNew();
                ms.Seek(0, SeekOrigin.Begin);
                seekStopwatch.Stop();
                phaseTimings["StreamSeek"] = seekStopwatch.Elapsed.TotalMilliseconds;

                long memoryAfterEncode = GC.GetTotalMemory(false);

                // Phase 2: WriteableBitmap creation
                var bitmapCreateStopwatch = Stopwatch.StartNew();
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);
                bitmapCreateStopwatch.Stop();
                phaseTimings["WriteableBitmapCreate"] = bitmapCreateStopwatch.Elapsed.TotalMilliseconds;

                // Phase 3: SetSource (image data transfer)
                var setSourceStopwatch = Stopwatch.StartNew();
                writeableBitmap.SetSource(ms.AsRandomAccessStream());
                setSourceStopwatch.Stop();
                phaseTimings["SetSource"] = setSourceStopwatch.Elapsed.TotalMilliseconds;

                // Phase 4: Assign to UI
                var uiAssignStopwatch = Stopwatch.StartNew();
                DisplayImage.Source = writeableBitmap;
                uiAssignStopwatch.Stop();
                phaseTimings["UIAssignment"] = uiAssignStopwatch.Elapsed.TotalMilliseconds;

                encodeStopwatch.Stop();
                phaseTimings["TotalEncoding"] = encodeStopwatch.Elapsed.TotalMilliseconds;
            }

            masterStopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            // Calculate derived metrics
            int pixelCount = bitmap.Width * bitmap.Height;
            double encodingPixelsPerMs = pixelCount / phaseTimings["PngEncoding"];
            double totalPixelsPerMs = pixelCount / masterStopwatch.Elapsed.TotalMilliseconds;

            // Record comprehensive metric
            _metrics.RecordSample(new MetricSample
            {
                Timestamp = DateTime.UtcNow,
                OperationName = "PhotoViewer",
                Phase = "SetImage",
                DurationMs = masterStopwatch.Elapsed.TotalMilliseconds,
                MemoryBefore = memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryDelta = memoryAfter - memoryBefore,
                Metadata = new Dictionary<string, object>
                {
                    { "CallCount", _setImageCallCount },
                    { "ImageWidth", bitmap.Width },
                    { "ImageHeight", bitmap.Height },
                    { "PixelCount", pixelCount },
                    { "ByteCount", bitmap.ByteCount },
                    { "Timing_PngEncoding_Ms", phaseTimings["PngEncoding"] },
                    { "Timing_WriteableBitmapCreate_Ms", phaseTimings["WriteableBitmapCreate"] },
                    { "Timing_SetSource_Ms", phaseTimings["SetSource"] },
                    { "Timing_UIAssignment_Ms", phaseTimings["UIAssignment"] },
                    { "EncodingPixelsPerMs", encodingPixelsPerMs },
                    { "TotalPixelsPerMs", totalPixelsPerMs }
                }
            });

            // Log frame update
            var fps = 1000.0 / masterStopwatch.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"[PhotoViewer] SetImage #{_setImageCallCount}");
            Debug.WriteLine($"  Total: {masterStopwatch.Elapsed.TotalMilliseconds:F2}ms | FPS: {fps:F1}");
            Debug.WriteLine($"  PNG Encode: {phaseTimings["PngEncoding"]:F2}ms | SetSource: {phaseTimings["SetSource"]:F2}ms");
            Debug.WriteLine($"  Memory Delta: {(memoryAfter - memoryBefore) / 1024.0 / 1024.0:F2} MB");
        }

        /// <summary>
        /// Fired when the pointer is pressed down (mouse button down).
        /// Capture the pointer so we receive move events even if the cursor goes outside the control.
        /// </summary>
        private void ScrollViewerImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _lastPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            (sender as UIElement)?.CapturePointer(e.Pointer);

            _metrics.Log($"Pan started at ({_lastPoint.X:F0}, {_lastPoint.Y:F0})", "DEBUG");
        }

        /// <summary>
        /// Fired as the mouse moves. If we're dragging, update ScrollViewer offsets.
        /// Tracks pan latency for responsiveness analysis.
        /// </summary>
        private void ScrollViewerImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;

            var moveStopwatch = Stopwatch.StartNew();
            _panEventCount++;

            var now = DateTime.Now;
            var timeSinceLastPan = (now - _lastPanTime).TotalMilliseconds;
            _lastPanTime = now;

            var currentPoint = e.GetCurrentPoint(ScrollViewerImage).Position;
            double deltaX = currentPoint.X - _lastPoint.X;
            double deltaY = currentPoint.Y - _lastPoint.Y;

            double scrollOffsetX = ScrollViewerImage.HorizontalOffset - deltaX;
            double scrollOffsetY = ScrollViewerImage.VerticalOffset - deltaY;

            // ChangeView(xOffset, yOffset, zoomFactor, disableAnimation)
            ScrollViewerImage.ChangeView(scrollOffsetX, scrollOffsetY, ScrollViewerImage.ZoomFactor, disableAnimation: true);

            _lastPoint = currentPoint;
            moveStopwatch.Stop();

            // Only log every 10th pan event to avoid flooding
            if (_panEventCount % 10 == 0)
            {
                _metrics.RecordSample(new MetricSample
                {
                    Timestamp = DateTime.UtcNow,
                    OperationName = "PhotoViewer",
                    Phase = "PanMove",
                    DurationMs = moveStopwatch.Elapsed.TotalMilliseconds,
                    MemoryBefore = 0,
                    MemoryAfter = 0,
                    MemoryDelta = 0,
                    Metadata = new Dictionary<string, object>
                    {
                        { "PanEventCount", _panEventCount },
                        { "DeltaX", deltaX },
                        { "DeltaY", deltaY },
                        { "TimeSinceLastPan_Ms", timeSinceLastPan },
                        { "ZoomFactor", ScrollViewerImage.ZoomFactor }
                    }
                });
            }
        }

        /// <summary>
        /// Fired when the pointer is released (mouse button up). Stop dragging.
        /// </summary>
        private void ScrollViewerImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();

            _metrics.Log($"Pan ended after {_panEventCount} move events", "DEBUG");
            _panEventCount = 0;
        }

        /// <summary>
        /// Fired if the pointer capture is canceled, for example if another control
        /// takes pointer capture. Ensure we stop dragging.
        /// </summary>
        private void ScrollViewerImage_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement)?.ReleasePointerCaptures();
        }
    }
}
