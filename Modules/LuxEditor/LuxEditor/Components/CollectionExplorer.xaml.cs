using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.Collections.Generic;
using System.Diagnostics;

namespace LuxEditor;

public sealed partial class CollectionExplorer : Page
{
    private List<SKBitmap> _bitmaps = new(); // Store bitmaps dynamically
    private List<SKXamlCanvas> _canvases = new(); // Keep track of SKXamlCanvas instances

    public CollectionExplorer()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Sets a new collection of SKBitmap images and updates the UI.
    /// </summary>
    public void SetBitmaps(List<SKBitmap> bitmaps)
    {
        Debug.WriteLine($"SetBitmaps: {bitmaps.Count} bitmaps");

        // Run UI updates on the main thread
        DispatcherQueue.TryEnqueue(() =>
        {
            // Store the new bitmaps
            _bitmaps = bitmaps;

            // Clear previous canvases
            CanvasContainer.Children.Clear();
            _canvases.Clear();

            // Add new canvases based on the provided bitmaps
            for (int i = 0; i < _bitmaps.Count; i++)
            {
                var skiaCanvas = InitializeSkiaCanvas(i);
                skiaCanvas.Width = 200;
                skiaCanvas.Height = 150;
                //skiaCanvas.Margin = new Thickness(10, 0, 10, 0);

                _canvases.Add(skiaCanvas);
                CanvasContainer.Children.Add(skiaCanvas);

                // Force an initial draw
                skiaCanvas.Invalidate();
            }
        });
    }

    private SKXamlCanvas InitializeSkiaCanvas(int index)
    {
        var skiaCanvas = new SKXamlCanvas
        {
            IgnorePixelScaling = true,
            Width = 200,
            Height = 150
        };

        skiaCanvas.PaintSurface += (sender, e) => OnPaintSurface(sender, e, index);
        return skiaCanvas;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        if (index < _bitmaps.Count)
        {
            var bitmap = _bitmaps[index];

            // Scale the bitmap to fit the canvas
            var scaleX = (float)e.Info.Width / bitmap.Width;
            var scaleY = (float)e.Info.Height / bitmap.Height;

            canvas.Scale(scaleX, scaleY);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
    }
}
