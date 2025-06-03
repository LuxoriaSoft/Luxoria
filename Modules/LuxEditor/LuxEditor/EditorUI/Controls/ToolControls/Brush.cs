using LuxEditor.Models;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace LuxEditor.EditorUI.Controls.ToolControls;

public class BrushPoint
{
    public SKPoint NormalizedPos { get; set; }
    public float Radius { get; set; }

    public BrushPoint(SKPoint normPos, float radius)
    {
        NormalizedPos = normPos;
        Radius = radius;
    }

    public SKPoint ToAbsolute(int width, int height)
        => new SKPoint(NormalizedPos.X * width, NormalizedPos.Y * height);
}


public class CustomStroke
{
    public List<BrushPoint> Points { get; set; } = new();
}
public partial class BrushToolControl : ATool
{
    private ToolType _toolType = ToolType.Brush;
    public override ToolType ToolType { get => _toolType; set => _toolType = value; }
    public override event Action? RefreshAction;

    private CustomStroke? _currentStroke;
    private readonly Queue<SKPoint> _lastPoints = new();
    private SKPoint? _lastMousePos;

    public float BrushSize { get; set; } = 10;

    private SKBitmap? _cacheBitmap;
    private SKCanvas? _cacheCanvas;
    private int _canvasWidth = 0;
    private int _canvasHeight = 0;

    private int _displayWidth = 0;
    private int _displayHeight = 0;

    private SKPoint? _rightClickStartingPoint;
    private bool _isRightClicking = false;

    private int _overlayResolutionDivisor = 1;

    private bool _alreadyIntialized = false;

    public override void ResizeCanvas(int width, int height)
    {
        _overlayResolutionDivisor = Math.Max(Math.Max(width, height) / 1000, 1);
        int scaledWidth = Math.Max(1, width / _overlayResolutionDivisor);
        int scaledHeight = Math.Max(1, height / _overlayResolutionDivisor);

        if (_canvasWidth == scaledWidth && _canvasHeight == scaledHeight) return;

        _canvasWidth = scaledWidth;
        _canvasHeight = scaledHeight;
        _displayWidth = width;
        _displayHeight = height;

        if (!_alreadyIntialized)
        {
            _cacheBitmap?.Dispose();
            _cacheBitmap = new SKBitmap(scaledWidth, scaledHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            _cacheCanvas = new SKCanvas(_cacheBitmap);
            _cacheCanvas.Clear(SKColors.Transparent);
            _alreadyIntialized = true;
        }

        RefreshAction?.Invoke();
    }

    public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var canvas = sender as SKXamlCanvas;
        var pos = e.GetCurrentPoint(canvas).Position;
        var skPos = new SKPoint((float)pos.X, (float)pos.Y);
        _lastMousePos = skPos;

        if (e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
        {
            _currentStroke = new CustomStroke();
            _currentStroke.Points.Add(new BrushPoint(Normalize(skPos), BrushSize));
            _lastPoints.Clear();
        }
        else if (e.GetCurrentPoint(canvas).Properties.IsRightButtonPressed)
        {
            if (!_isRightClicking)
                _rightClickStartingPoint = skPos;
            _isRightClicking = true;
        }

        RefreshAction?.Invoke();
    }

    public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var canvas = sender as SKXamlCanvas;
        var pos = e.GetCurrentPoint(canvas).Position;
        var skPos = new SKPoint((float)pos.X, (float)pos.Y);
        _lastMousePos = skPos;

        if (_currentStroke != null && e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
        {
            if (_currentStroke.Points.Count > 0)
            {
                var last = _currentStroke.Points[^1].ToAbsolute(_displayWidth, _displayHeight);
                foreach (var interp in InterpolatePoints(last, skPos, BrushSize * 0.25f))
                {
                    var smooth = SmoothPosition(interp);
                    _currentStroke.Points.Add(new BrushPoint(Normalize(smooth), BrushSize));
                }
            }

            var finalSmooth = SmoothPosition(skPos);
            _currentStroke.Points.Add(new BrushPoint(Normalize(finalSmooth), BrushSize));

            if (_currentStroke.Points.Count > 30 && _cacheCanvas != null)
            {
                foreach (var point in _currentStroke.Points)
                {
                    var abs = point.ToAbsolute(_canvasWidth, _canvasHeight);
                    DrawSoftCircle(_cacheCanvas, abs, point.Radius * _canvasWidth / _displayWidth, Color);
                }

                _currentStroke.Points.Clear();
            }
        }
        else if (_rightClickStartingPoint != null)
        {
            float deltaX = (float)(pos.X - _rightClickStartingPoint.Value.X);
            BrushSize = Math.Max(1, deltaX);
        }

        RefreshAction?.Invoke();
    }

    public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_currentStroke != null && _cacheCanvas != null)
        {
            foreach (var point in _currentStroke.Points)
            {
                var abs = point.ToAbsolute(_canvasWidth, _canvasHeight);
                DrawSoftCircle(_cacheCanvas, abs, point.Radius * _canvasWidth / _displayWidth, Color);
            }

            _currentStroke = null;
        }
        _isRightClicking = false;

        _rightClickStartingPoint = null;
        RefreshAction?.Invoke();
    }

    public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (OpsFusionned != null)
        {
            canvas.DrawImage(OpsFusionned, 0, 0);
        }

        if (_cacheBitmap != null)
        {
            var src = new SKRect(0, 0, _canvasWidth, _canvasHeight);
            var dest = new SKRect(0, 0, _displayWidth, _displayHeight);
            canvas.DrawBitmap(_cacheBitmap, src, dest);
        }

        if (_currentStroke != null)
        {
            foreach (var point in _currentStroke.Points)
            {
                var abs = point.ToAbsolute(_displayWidth, _displayHeight);
                DrawSoftCircle(canvas, abs, point.Radius, Color);
            }
        }

        if (_lastMousePos != null)
        {
            if (_isRightClicking && _rightClickStartingPoint != null)
            {
                DrawBrushPreview(canvas, _rightClickStartingPoint.Value);
            } else
            {
                DrawBrushPreview(canvas, _lastMousePos.Value);
            }
        }
    }

    private void DrawSoftCircle(SKCanvas canvas, SKPoint center, float radius, SKColor color)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateRadialGradient(
                center,
                radius,
                new[] { color.WithAlpha(255), color.WithAlpha(100), color.WithAlpha(0) },
                new[] { 0f, 0.5f, 1f },
                SKShaderTileMode.Clamp)
        };

        canvas.DrawCircle(center, radius, paint);
    }

    private void DrawBrushPreview(SKCanvas canvas, SKPoint pos)
    {
        using var previewPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _isRightClicking ? 2 : 1,
            Color = SKColors.White.WithAlpha(180),
            IsAntialias = true
        };

        canvas.DrawCircle(pos, BrushSize, previewPaint);
    }

    private IEnumerable<SKPoint> InterpolatePoints(SKPoint from, SKPoint to, float spacing)
    {
        float dx = to.X - from.X;
        float dy = to.Y - from.Y;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        int steps = (int)(distance / spacing);

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            yield return new SKPoint(from.X + t * dx, from.Y + t * dy);
        }
    }

    private SKPoint SmoothPosition(SKPoint current)
    {
        _lastPoints.Enqueue(current);
        while (_lastPoints.Count > 4)
            _lastPoints.Dequeue();

        float sumX = 0, sumY = 0;
        foreach (var p in _lastPoints)
        {
            sumX += p.X;
            sumY += p.Y;
        }

        return new SKPoint(sumX / _lastPoints.Count, sumY / _lastPoints.Count);
    }

    private SKPoint Normalize(SKPoint absolute)
        => new SKPoint(absolute.X / _displayWidth, absolute.Y / _displayHeight);

    public override SKBitmap? GetResult()
    {
        return _cacheBitmap?.Resize(new SKImageInfo(_displayWidth, _displayHeight), SKSamplingOptions.Default);
    }
}
