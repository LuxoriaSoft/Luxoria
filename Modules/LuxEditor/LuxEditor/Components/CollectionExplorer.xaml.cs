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
using Windows.Media.Devices;
using System.Collections.ObjectModel;

namespace LuxEditor;

public sealed partial class CollectionExplorer : Page
{
    private List<SKBitmap> _bitmaps = new();
    private ScrollViewer _scrollViewer;
    private WrapPanel _imagePanel;
    private Border? _selectedBorder;

    public event Action<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>>? OnImageSelected;
    private List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> _originalBitmaps = new();

    public CollectionExplorer()
    {
        InitializeComponent();
        BuildUI();
        SizeChanged += (s, e) => AdjustImageSizes(e.NewSize);
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
                }
                else
                {
                    Debug.WriteLine($"AdjustImageSizes: Index {index} out of range (bitmaps count = {_bitmaps.Count})");
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// Set the bitmaps to display in the collection explorer.
    /// </summary>
    public void SetBitmaps(List<KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>> bitmaps)
    {
        if (bitmaps == null || bitmaps.Count == 0)
        {
            Debug.WriteLine("SetBitmaps: No bitmaps provided.");
            return;
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            _bitmaps.Clear();
            _originalBitmaps.Clear();
            _imagePanel.Children.Clear();

            foreach (var bitmap in bitmaps)
            {

                int previewHeight = 200;
                int previewWidth = (int)((float)bitmap.Key.Width / bitmap.Key.Height * previewHeight);
                var lowResBitmap = CreateLowResBitmap(bitmap.Key, previewWidth, previewHeight);

                previewHeight = int.Min(bitmap.Key.Height, 600);
                previewWidth = (int)((float)bitmap.Key.Width / bitmap.Key.Height * previewHeight);
                var mediumResBitmap = CreateLowResBitmap(bitmap.Key, previewWidth, previewHeight);
                _bitmaps.Add(lowResBitmap);
                _originalBitmaps.Add(new KeyValuePair<SKBitmap, ReadOnlyDictionary<string, string>>(mediumResBitmap, bitmap.Value));

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

            AdjustImageSizes(new Size(ActualWidth, ActualHeight));
        });
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnHover(Border border, bool isHovered)
    {
        if (border != _selectedBorder)
        {
            border.BorderBrush = new SolidColorBrush(isHovered ? Windows.UI.Color.FromArgb(255, 200, 200, 200) : Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnImageTapped(Border border, int index)
    {
        if (index >= _originalBitmaps.Count) return;

        if (_selectedBorder != null)
            _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        _selectedBorder = border;
        _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 3, 169, 244));

        OnImageSelected?.Invoke(_originalBitmaps[index]);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
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
    }

    /// <summary>
    /// 
    /// </summary>
    private SKBitmap CreateLowResBitmap(SKBitmap original, int targetWidth, int targetHeight)
    {
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
        return resizedBitmap;
    }
}
