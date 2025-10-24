using LuxEditor.EditorUI.Interfaces;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LuxEditor.EditorUI.Controls;

/// <summary>
/// Interactive histogram control like Lightroom with draggable regions for adjusting exposure, highlights, shadows, whites, and blacks.
/// </summary>
public class HistogramControl : IEditorGroupItem
{
    private readonly UIElement _container;
    private readonly SKXamlCanvas _canvas;
    private readonly ComboBox _modeSelector;

    // Histogram data
    private int[] _redHistogram = new int[256];
    private int[] _greenHistogram = new int[256];
    private int[] _blueHistogram = new int[256];
    private int[] _luminanceHistogram = new int[256];
    private int _maxHistogramValue = 1;

    // Debounce timer for performance
    private DispatcherTimer? _updateDebounceTimer;
    private bool _updatePending = false;

    // Display mode
    public enum HistogramMode
    {
        RGB,
        Luminance,
        Red,
        Green,
        Blue
    }

    private HistogramMode _currentMode = HistogramMode.RGB;

    // Interaction zones (in 0-255 range)
    private const int BLACKS_END = 51;        // 0-51: Blacks
    private const int SHADOWS_END = 102;      // 52-102: Shadows
    private const int MIDTONES_END = 153;     // 103-153: Midtones
    private const int HIGHLIGHTS_END = 204;   // 154-204: Highlights
    // 205-255: Whites

    // Drag state
    private bool _isDragging = false;
    private string _dragZone = "";
    private double _dragStartX = 0;
    private float _dragStartValue = 0;

    // Hover state
    private string _hoveredZone = "";
    private bool _isHovering = false;

    // Callbacks to update sliders
    public Action<string, float>? OnAdjustmentChanged;

    // Current adjustment value (for display during drag)
    private float _currentAdjustmentValue = 0f;

    private const float DRAG_SENSITIVITY_HORIZONTAL = 1.0f; // Horizontal drag sensitivity

    public HistogramControl()
    {
        // Create mode selector
        _modeSelector = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 4),
            ItemsSource = new[] { "RGB", "Luminance", "Red", "Green", "Blue" },
            SelectedIndex = 0,
            FontSize = 11
        };
        _modeSelector.SelectionChanged += OnModeChanged;

        // Create canvas for histogram rendering
        _canvas = new SKXamlCanvas
        {
            MinHeight = 1,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Black)
        };
        _canvas.PaintSurface += OnPaintSurface;
        _canvas.SizeChanged += (s, e) => _canvas.Invalidate(); // Redraw when size changes

        // Setup pointer events for interaction
        _canvas.PointerPressed += OnPointerPressed;
        _canvas.PointerMoved += OnPointerMoved;
        _canvas.PointerReleased += OnPointerReleased;
        _canvas.PointerExited += OnPointerExited;
        _canvas.PointerEntered += OnPointerEntered;
        _canvas.ManipulationMode = ManipulationModes.TranslateX; // Horizontal only

        // Create horizontal layout for compact display
        var topRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(12) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        // Title
        var titleBlock = new TextBlock
        {
            Text = "Histogram",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);
        topRow.Children.Add(titleBlock);

        // Mode selector - more compact
        _modeSelector.HorizontalAlignment = HorizontalAlignment.Left;
        _modeSelector.MinWidth = 120;
        Grid.SetColumn(_modeSelector, 2);
        topRow.Children.Add(_modeSelector);

        var contentGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            }
        };

        // Ensure canvas takes all available space
        _canvas.Margin = new Thickness(0);

        Grid.SetRow(topRow, 0);
        Grid.SetRow(_canvas, 1);
        contentGrid.Children.Add(topRow);
        contentGrid.Children.Add(_canvas);

        // Wrapper with background - full width compact style
        _container = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 60, 60, 60)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(12, 8, 12, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = contentGrid
        };

        // Subscribe to image changes
        ImageManager.Instance.OnSelectionChanged += OnImageChanged;

        // Setup debounce timer for performance
        _updateDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150) // Wait 150ms after last request
        };
        _updateDebounceTimer.Tick += (s, e) =>
        {
            _updateDebounceTimer.Stop();
            if (_updatePending)
            {
                _updatePending = false;
                UpdateHistogramImmediate();
            }
        };

        // Initial histogram calculation (immediate, no debounce on first load)
        UpdateHistogramImmediate();
    }

    private void OnModeChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentMode = (HistogramMode)_modeSelector.SelectedIndex;
        // Recalculate max value for new mode
        _maxHistogramValue = CalculateNormalizationMax();
        _canvas.Invalidate();
    }

    private void OnImageChanged(EditableImage? image)
    {
        // When image changes, update immediately (no debounce needed)
        UpdateHistogramImmediate();
    }

    /// <summary>
    /// Request histogram update with debouncing for performance
    /// </summary>
    private void UpdateHistogram()
    {
        _updatePending = true;
        _updateDebounceTimer?.Stop();
        _updateDebounceTimer?.Start();
    }

    /// <summary>
    /// Calculate histogram from current image (immediate, no debounce)
    /// </summary>
    private void UpdateHistogramImmediate()
    {
        var img = ImageManager.Instance.SelectedImage;
        if (img == null)
        {
            // Clear histograms
            Array.Clear(_redHistogram, 0, 256);
            Array.Clear(_greenHistogram, 0, 256);
            Array.Clear(_blueHistogram, 0, 256);
            Array.Clear(_luminanceHistogram, 0, 256);
            _maxHistogramValue = 1;
            _canvas.Invalidate();
            return;
        }

        // Use preview bitmap for better performance (smaller size)
        var bitmap = img.EditedPreviewBitmap ?? img.PreviewBitmap ?? img.EditedBitmap ?? img.OriginalBitmap;
        if (bitmap == null)
        {
            _canvas.Invalidate();
            return;
        }

        // Reset histograms
        Array.Clear(_redHistogram, 0, 256);
        Array.Clear(_greenHistogram, 0, 256);
        Array.Clear(_blueHistogram, 0, 256);
        Array.Clear(_luminanceHistogram, 0, 256);

        // Use direct pixel access for much better performance
        var pixels = bitmap.PeekPixels();
        if (pixels == null)
        {
            _canvas.Invalidate();
            return;
        }

        int width = bitmap.Width;
        int height = bitmap.Height;
        int totalPixels = width * height;

        // Sample every Nth pixel for performance (preview is already smaller)
        int sampleRate = Math.Max(1, totalPixels / 50000);

        IntPtr pixelPtr = pixels.GetPixels();

        for (int i = 0; i < totalPixels; i += sampleRate)
        {
            // Direct memory access - much faster than GetPixel()
            int offset = i * 4; // RGBA = 4 bytes per pixel
            byte b = System.Runtime.InteropServices.Marshal.ReadByte(pixelPtr, offset);
            byte g = System.Runtime.InteropServices.Marshal.ReadByte(pixelPtr, offset + 1);
            byte r = System.Runtime.InteropServices.Marshal.ReadByte(pixelPtr, offset + 2);

            _redHistogram[r]++;
            _greenHistogram[g]++;
            _blueHistogram[b]++;

            // Calculate luminance (ITU-R BT.709)
            int luminance = (int)(0.2126f * r + 0.7152f * g + 0.0722f * b);
            _luminanceHistogram[Math.Clamp(luminance, 0, 255)]++;
        }

        // Apply smoothing to remove jagged edges and create smooth curves
        SmoothHistogram(_redHistogram);
        SmoothHistogram(_greenHistogram);
        SmoothHistogram(_blueHistogram);
        SmoothHistogram(_luminanceHistogram);

        // Find max value for normalization using percentile to avoid extreme peaks
        // This prevents clipped pixels from making the entire histogram flat
        _maxHistogramValue = CalculateNormalizationMax();

        _canvas.Invalidate();
    }

    /// <summary>
    /// Smooth histogram data using Gaussian blur to eliminate jagged edges
    /// This creates professional-looking smooth curves like Lightroom
    /// </summary>
    private void SmoothHistogram(int[] histogram)
    {
        // Create a temporary array to store smoothed values
        float[] temp = new float[256];
        for (int i = 0; i < 256; i++)
            temp[i] = histogram[i];

        // Gaussian kernel for smooth blending
        // Radius 3 with sigma=1.5 gives nice smooth curves
        float[] kernel = { 0.06136f, 0.24477f, 0.38774f, 0.24477f, 0.06136f };
        int radius = 2; // 5-point kernel

        // Apply Gaussian blur (horizontal pass)
        float[] result = new float[256];

        for (int i = 0; i < 256; i++)
        {
            float sum = 0;
            float weightSum = 0;

            for (int j = -radius; j <= radius; j++)
            {
                int index = i + j;
                if (index >= 0 && index < 256)
                {
                    float weight = kernel[j + radius];
                    sum += temp[index] * weight;
                    weightSum += weight;
                }
                else
                {
                    // Edge handling: mirror the edge values
                    int mirroredIndex = index < 0 ? -index : (2 * 255 - index);
                    mirroredIndex = Math.Clamp(mirroredIndex, 0, 255);
                    float weight = kernel[j + radius];
                    sum += temp[mirroredIndex] * weight;
                    weightSum += weight;
                }
            }

            result[i] = sum / weightSum;
        }

        // Apply a second pass for extra smoothness (2D Gaussian effect)
        for (int i = 0; i < 256; i++)
        {
            float sum = 0;
            float weightSum = 0;

            for (int j = -radius; j <= radius; j++)
            {
                int index = i + j;
                if (index >= 0 && index < 256)
                {
                    float weight = kernel[j + radius];
                    sum += result[index] * weight;
                    weightSum += weight;
                }
            }

            histogram[i] = (int)Math.Round(sum / weightSum);
        }
    }

    /// <summary>
    /// Calculate normalization max using 95th percentile to avoid extreme peaks
    /// Only considers non-zero histogram bins to get accurate scaling
    /// </summary>
    private int CalculateNormalizationMax()
    {
        var nonZeroValues = new List<int>();

        // Collect only non-zero values from the relevant histograms
        void AddNonZeroValues(int[] histogram)
        {
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > 0)
                    nonZeroValues.Add(histogram[i]);
            }
        }

        switch (_currentMode)
        {
            case HistogramMode.RGB:
                AddNonZeroValues(_redHistogram);
                AddNonZeroValues(_greenHistogram);
                AddNonZeroValues(_blueHistogram);
                break;
            case HistogramMode.Luminance:
                AddNonZeroValues(_luminanceHistogram);
                break;
            case HistogramMode.Red:
                AddNonZeroValues(_redHistogram);
                break;
            case HistogramMode.Green:
                AddNonZeroValues(_greenHistogram);
                break;
            case HistogramMode.Blue:
                AddNonZeroValues(_blueHistogram);
                break;
        }

        if (nonZeroValues.Count == 0) return 1;

        // Sort and take 95th percentile to ignore extreme peaks
        nonZeroValues.Sort();
        int percentileIndex = Math.Min((int)(nonZeroValues.Count * 0.95), nonZeroValues.Count - 1);
        return Math.Max(nonZeroValues[percentileIndex], 1);
    }

    /// <summary>
    /// Render the histogram
    /// </summary>
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(20, 20, 20));

        var info = e.Info;
        float width = info.Width;
        float height = info.Height;

        if (_maxHistogramValue <= 1)
        {
            // Draw "No image" message
            using var paint = new SKPaint
            {
                Color = new SKColor(120, 120, 120),
                IsAntialias = true,
                TextSize = 14,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("Load an image to see histogram", width / 2, height / 2, paint);
            return;
        }

        // Draw histogram based on mode
        switch (_currentMode)
        {
            case HistogramMode.RGB:
                // Use additive blending for RGB mode to show overlaps as white
                using (var layer = new SKPaint { BlendMode = SKBlendMode.Plus })
                {
                    canvas.SaveLayer(layer);
                    DrawHistogramChannel(canvas, _redHistogram, width, height, new SKColor(255, 0, 0, 255), SKBlendMode.Plus);
                    DrawHistogramChannel(canvas, _greenHistogram, width, height, new SKColor(0, 255, 0, 255), SKBlendMode.Plus);
                    DrawHistogramChannel(canvas, _blueHistogram, width, height, new SKColor(0, 0, 255, 255), SKBlendMode.Plus);
                    canvas.Restore();
                }
                break;

            case HistogramMode.Luminance:
                DrawHistogramChannel(canvas, _luminanceHistogram, width, height, new SKColor(200, 200, 200, 255), SKBlendMode.SrcOver);
                break;

            case HistogramMode.Red:
                DrawHistogramChannel(canvas, _redHistogram, width, height, new SKColor(255, 100, 100, 255), SKBlendMode.SrcOver);
                break;

            case HistogramMode.Green:
                DrawHistogramChannel(canvas, _greenHistogram, width, height, new SKColor(100, 255, 100, 255), SKBlendMode.SrcOver);
                break;

            case HistogramMode.Blue:
                DrawHistogramChannel(canvas, _blueHistogram, width, height, new SKColor(100, 150, 255, 255), SKBlendMode.SrcOver);
                break;
        }

        // Draw zone separators
        DrawZoneSeparators(canvas, width, height);

        // Draw hover highlight if hovering
        if (_isHovering && !string.IsNullOrEmpty(_hoveredZone))
        {
            DrawZoneHighlight(canvas, _hoveredZone, width, height);
        }

        // Draw info bar at bottom (like Lightroom)
        DrawInfoBar(canvas, width, height);
    }

    private void DrawZoneHighlight(SKCanvas canvas, string zone, float width, float height)
    {
        // Highlight the hovered zone
        var (start, end) = GetZoneBounds(zone);
        float startX = (start / 255f) * width;
        float endX = (end / 255f) * width;

        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 30),
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(startX, 0, endX - startX, height, paint);
    }

    private (int start, int end) GetZoneBounds(string zone)
    {
        return zone switch
        {
            "Blacks" => (0, BLACKS_END),
            "Shadows" => (BLACKS_END + 1, SHADOWS_END),
            "Midtones" => (SHADOWS_END + 1, MIDTONES_END),
            "Highlights" => (MIDTONES_END + 1, HIGHLIGHTS_END),
            "Whites" => (HIGHLIGHTS_END + 1, 255),
            _ => (0, 0)
        };
    }

    private void DrawInfoBar(SKCanvas canvas, float width, float height)
    {
        float barY = height - 18;
        float barHeight = 18;

        // Draw background bar
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 100),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, barY, width, barHeight, bgPaint);

        using var textPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            IsAntialias = true,
            TextSize = 11
        };

        // Left side: Zone name when hovering or dragging
        string zoneName = _isDragging ? GetZoneDisplayName(_dragZone) :
                         _isHovering ? GetZoneDisplayName(_hoveredZone) : "";

        if (!string.IsNullOrEmpty(zoneName))
        {
            canvas.DrawText(zoneName, 8, barY + 13, textPaint);
        }

        // Right side: Value when dragging
        if (_isDragging)
        {
            string valueText = _currentAdjustmentValue >= 0 ?
                $"+{_currentAdjustmentValue:0}" :
                $"{_currentAdjustmentValue:0}";

            textPaint.TextAlign = SKTextAlign.Right;
            canvas.DrawText(valueText, width - 8, barY + 13, textPaint);
        }
    }

    private string GetZoneDisplayName(string zone)
    {
        return zone switch
        {
            "Blacks" => "Blacks",
            "Shadows" => "Shadows",
            "Midtones" => "Exposure",
            "Highlights" => "Highlights",
            "Whites" => "Whites",
            _ => ""
        };
    }

    private void DrawHistogramChannel(SKCanvas canvas, int[] histogram, float width, float height, SKColor color, SKBlendMode blendMode)
    {
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = blendMode
        };

        // Create a filled path that covers every pixel - NO GAPS
        using var path = new SKPath();
        path.MoveTo(0, height);

        // Calculate step size based on canvas width
        // Always ensure we have enough points to fill the entire width
        int numPoints = Math.Max(256, (int)Math.Ceiling(width));
        float step = width / (numPoints - 1);

        for (int i = 0; i < numPoints; i++)
        {
            float x = i * step;

            // Map X position to histogram bin
            float binFloat = (x / width) * 255f;
            binFloat = Math.Clamp(binFloat, 0, 255);

            int bin1 = (int)Math.Floor(binFloat);
            int bin2 = Math.Min(bin1 + 1, 255);
            float fraction = binFloat - bin1;

            // Interpolate between bins
            float value1 = histogram[bin1];
            float value2 = histogram[bin2];
            float interpolatedValue = value1 + (value2 - value1) * fraction;

            // Apply gamma curve
            float normalizedValue = 0;
            if (interpolatedValue > 0 && _maxHistogramValue > 1)
            {
                float linearNorm = interpolatedValue / (float)_maxHistogramValue;
                normalizedValue = (float)Math.Pow(linearNorm, 0.4);
            }

            float y = height - (normalizedValue * height * 0.95f);
            path.LineTo(x, y);
        }

        // Close the path at the bottom right to create a filled shape
        path.LineTo(width, height);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    private void DrawZoneSeparators(SKCanvas canvas, float width, float height)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(100, 100, 100, 100),
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0)
        };

        // Draw vertical lines for zones
        float[] separators = {
            BLACKS_END / 255f,
            SHADOWS_END / 255f,
            MIDTONES_END / 255f,
            HIGHLIGHTS_END / 255f
        };

        foreach (var sep in separators)
        {
            float x = sep * width;
            canvas.DrawLine(x, 0, x, height, paint);
        }
    }


    /// <summary>
    /// Determine which zone was clicked based on X position
    /// </summary>
    private string GetZoneFromX(double x, double canvasWidth)
    {
        float normalizedX = (float)(x / canvasWidth);
        int value = (int)(normalizedX * 255);

        if (value <= BLACKS_END) return "Blacks";
        if (value <= SHADOWS_END) return "Shadows";
        if (value <= MIDTONES_END) return "Midtones";
        if (value <= HIGHLIGHTS_END) return "Highlights";
        return "Whites";
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovering = true;
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(_canvas);
        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartX = point.Position.X;
            _dragZone = GetZoneFromX(point.Position.X, _canvas.ActualWidth);

            // Store current slider value for this zone
            _dragStartValue = 0; // We'll get actual value from slider cache
            _currentAdjustmentValue = 0;

            _canvas.CapturePointer(e.Pointer);
            _canvas.Invalidate();
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(_canvas);

        if (_isDragging)
        {
            // Calculate horizontal delta (Lightroom style: left = decrease, right = increase)
            double deltaX = point.Position.X - _dragStartX;
            float adjustment = (float)(deltaX * DRAG_SENSITIVITY_HORIZONTAL);

            // Calculate final value
            float finalValue = _dragStartValue + adjustment;

            // Get the slider key and scale based on zone
            string sliderKey = _dragZone switch
            {
                "Midtones" => "Exposure",
                _ => _dragZone
            };

            // Scale appropriately for each slider type
            float scaledValue = sliderKey switch
            {
                "Exposure" => Math.Clamp(finalValue / 25f, -4f, 4f), // -4 to +4 range
                _ => Math.Clamp(finalValue, -100f, 100f) // -100 to +100 for others
            };

            // Store for display
            _currentAdjustmentValue = sliderKey == "Exposure" ? scaledValue : finalValue;

            // Update the slider
            OnAdjustmentChanged?.Invoke(sliderKey, scaledValue);

            _canvas.Invalidate();
        }
        else if (_isHovering)
        {
            // Update hovered zone for visual feedback
            string newHoveredZone = GetZoneFromX(point.Position.X, _canvas.ActualWidth);
            if (newHoveredZone != _hoveredZone)
            {
                _hoveredZone = newHoveredZone;
                _canvas.Invalidate();
            }
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _canvas.ReleasePointerCapture(e.Pointer);
            _canvas.Invalidate();

            // Save state after adjustment
            ImageManager.Instance.SelectedImage?.SaveState();
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging)
        {
            _isHovering = false;
            _hoveredZone = "";
            _canvas.Invalidate();
        }
    }

    /// <summary>
    /// Reset adjustment tracking (call this when image changes)
    /// </summary>
    public void ResetAdjustments()
    {
        _currentAdjustmentValue = 0f;
        _isDragging = false;
        _isHovering = false;
        _hoveredZone = "";
        _dragZone = "";
    }

    /// <summary>
    /// Refresh the histogram (call this when the image is processed)
    /// Uses debouncing to avoid excessive updates during slider dragging
    /// </summary>
    public void RefreshHistogram()
    {
        UpdateHistogram(); // Will use debounce timer
    }

    public UIElement GetElement() => _container;
}
