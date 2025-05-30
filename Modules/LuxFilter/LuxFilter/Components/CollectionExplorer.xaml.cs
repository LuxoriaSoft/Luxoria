using CommunityToolkit.WinUI.Controls;
using Luxoria.Modules.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;

namespace LuxFilter.Components;

/// <summary>
/// Displays a scrollable panel of preview images.
/// </summary>
public sealed partial class CollectionExplorer : Page
{
    private List<LuxAsset> _images = new();
    private ScrollViewer _scrollViewer;
    private WrapPanel _imagePanel;
    private Border? _selectedBorder;

    public event Action<LuxAsset>? OnImageSelected;

    /// <summary>
    /// Initializes the CollectionExplorer page and builds the UI.
    /// </summary>
    public CollectionExplorer()
    {
        InitializeComponent();
        BuildUI();
        SizeChanged += (s, e) => AdjustImageSizes(e.NewSize);
    }

    /// <summary>
    /// Builds the scrollable layout to hold preview thumbnails.
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
            HorizontalSpacing = 5
        };

        _scrollViewer.Content = _imagePanel;
        RootGrid.Children.Add(_scrollViewer);
    }

    /// <summary>
    /// Replaces the current list of images with a new one and updates the layout.
    /// </summary>
    public void SetImages(ICollection<LuxAsset> images)
    {
        if (images == null || images.Count == 0)
        {
            Debug.WriteLine("SetImages: No images provided.");
            return;
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            _images.Clear();
            _imagePanel.Children.Clear();

            foreach (var image in images)
            {
                _images.Add(image);
                int index = _images.Count - 1;

                var border = new Border
                {
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0))
                };

                var canvas = new SKXamlCanvas
                {
                    IgnorePixelScaling = true
                };

                canvas.PaintSurface += (sender, e) => OnPaintSurface(sender, e, index);
                border.Child = canvas;

                border.PointerEntered += (s, e) => OnHover(border, true);
                border.PointerExited += (s, e) => OnHover(border, false);
                border.Tapped += (s, e) => OnImageTapped(border, index);

                _imagePanel.Children.Add(border);
            }

            AdjustImageSizes(new Size(ActualWidth, ActualHeight));
        });
    }

    /// <summary>
    /// Adjusts thumbnail sizes dynamically based on control size.
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
                if (index < _images.Count)
                {
                    var bitmap = _images[index].Data.Bitmap;
                    double scale = availableHeight / bitmap.Height;
                    double width = bitmap.Width * scale;

                    border.Width = width;
                    border.Height = availableHeight;
                    canvas.Width = width;
                    canvas.Height = availableHeight;
                    canvas.Invalidate();
                }
            }
        }
    }

    /// <summary>
    /// Draws the bitmap preview into the canvas.
    /// </summary>
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (index >= 0 && index < _images.Count)
        {
            var bitmap = _images[index].Data.Bitmap;
            float scale = Math.Min((float)e.Info.Width / bitmap.Width, (float)e.Info.Height / bitmap.Height);
            float offsetX = (e.Info.Width - bitmap.Width * scale) / 2;
            float offsetY = (e.Info.Height - bitmap.Height * scale) / 2;

            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
    }

    /// <summary>
    /// Highlights a thumbnail when hovered.
    /// </summary>
    private void OnHover(Border border, bool isHovered)
    {
        if (border != _selectedBorder)
        {
            border.BorderBrush = new SolidColorBrush(isHovered
                ? Windows.UI.Color.FromArgb(255, 200, 200, 200)
                : Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
    }

    /// <summary>
    /// Triggers when a user selects a thumbnail.
    /// </summary>
    private void OnImageTapped(Border border, int index)
    {
        if (index >= _images.Count) return;

        if (_selectedBorder != null)
            _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        _selectedBorder = border;
        _selectedBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 3, 169, 244));

        OnImageSelected?.Invoke(_images[index]);
    }
}