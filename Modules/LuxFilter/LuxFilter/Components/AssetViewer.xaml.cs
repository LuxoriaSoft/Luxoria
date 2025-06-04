using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;

namespace LuxFilter.Components;

public sealed partial class AssetViewer : Page
{
    private readonly SKXamlCanvas _canvas = new();
    private SKImage? _gpuImage;
    private SKBitmap? _cpuImage;

    public AssetViewer()
    {
        InitializeComponent();
        _canvas.PaintSurface += OnPaintSurface;
        ImageViewbox.Child = _canvas;
    }

    public void SetImage(SKImage image)
    {
        _gpuImage?.Dispose();
        _gpuImage = image;
        _cpuImage = null;

        _canvas.Width = image.Width;
        _canvas.Height = image.Height;
        _canvas.Invalidate();

        SetInitialZoom(image.Width, image.Height);
    }

    public void SetImage(SKBitmap bitmap)
    {
        _cpuImage = bitmap;
        _gpuImage?.Dispose();
        _gpuImage = null;

        _canvas.Width = bitmap.Width;
        _canvas.Height = bitmap.Height;
        _canvas.Invalidate();

        SetInitialZoom(bitmap.Width, bitmap.Height);
    }

    private void SetInitialZoom(double imageWidth, double imageHeight)
    {
        // Delay setting zoom until layout is ready
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            double viewerWidth = ScrollViewer.ActualWidth;
            double viewerHeight = ScrollViewer.ActualHeight;

            if (viewerWidth <= 0 || viewerHeight <= 0 || imageWidth <= 0 || imageHeight <= 0)
                return;

            double scaleX = viewerWidth / imageWidth;
            double scaleY = viewerHeight / imageHeight;
            float zoom = (float)Math.Min(scaleX, scaleY);

            // Clamp to ScrollViewer's zoom limits
            zoom = Math.Clamp(zoom, ScrollViewer.MinZoomFactor, ScrollViewer.MaxZoomFactor);

            ScrollViewer.ChangeView(null, null, zoom, disableAnimation: true);
        });
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        int canvasWidth = e.Info.Width;
        int canvasHeight = e.Info.Height;

        int imageWidth = _gpuImage?.Width ?? _cpuImage?.Width ?? 0;
        int imageHeight = _gpuImage?.Height ?? _cpuImage?.Height ?? 0;

        if (imageWidth == 0 || imageHeight == 0)
            return;

        float scale = Math.Min((float)canvasWidth / imageWidth, (float)canvasHeight / imageHeight);
        float scaledWidth = imageWidth * scale;
        float scaledHeight = imageHeight * scale;
        float x = (canvasWidth - scaledWidth) / 2f;
        float y = (canvasHeight - scaledHeight) / 2f;

        var destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);

        if (_gpuImage != null)
            canvas.DrawImage(_gpuImage, destRect);
        else if (_cpuImage != null)
            canvas.DrawBitmap(_cpuImage, destRect);
    }
}
