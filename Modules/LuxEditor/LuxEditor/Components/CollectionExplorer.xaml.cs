using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;

namespace LuxEditor;

public sealed partial class CollectionExplorer : Page
{
    private List<SKBitmap> _bitmaps = new();
    private ScrollViewer _scrollViewer;
    private StackPanel _imagePanel;

    public CollectionExplorer()
    {
        InitializeComponent();
        BuildUI();
    }

    /// <summary>
    /// Crée dynamiquement l'interface utilisateur avec un ScrollViewer et un StackPanel.
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

        _imagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        _scrollViewer.Content = _imagePanel;
        RootGrid.Children.Add(_scrollViewer);
    }

    /// <summary>
    /// Charge et affiche une liste d'images dans le carrousel.
    /// </summary>
    public void SetBitmaps(List<SKBitmap> bitmaps)
    {
        if (bitmaps == null || bitmaps.Count == 0)
        {
            Debug.WriteLine("SetBitmaps: No bitmaps provided.");
            return;
        }

        Debug.WriteLine($"SetBitmaps: {bitmaps.Count} bitmaps");

        DispatcherQueue.TryEnqueue(() =>
        {
            _bitmaps = bitmaps;
            _imagePanel.Children.Clear();

            foreach (var bitmap in _bitmaps)
            {
                var border = new Border
                {
                    Width = 300,
                    Height = 200,
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5),
                };

                var skiaCanvas = new SKXamlCanvas
                {
                    IgnorePixelScaling = true,
                    Width = 300,
                    Height = 200
                };
                skiaCanvas.PaintSurface += (sender, e) => OnPaintSurface(sender, e, _bitmaps.IndexOf(bitmap));
                skiaCanvas.Invalidate();

                border.Child = skiaCanvas;
                _imagePanel.Children.Add(border);
            }
        });
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        if (index < _bitmaps.Count)
        {
            var bitmap = _bitmaps[index];
            Debug.WriteLine($"Rendering image {index} with size {bitmap.Width}x{bitmap.Height}");

            float scale = System.Math.Min((float)e.Info.Width / bitmap.Width, (float)e.Info.Height / bitmap.Height);
            float offsetX = (e.Info.Width - bitmap.Width * scale) / 2;
            float offsetY = (e.Info.Height - bitmap.Height * scale) / 2;

            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
    }
}
