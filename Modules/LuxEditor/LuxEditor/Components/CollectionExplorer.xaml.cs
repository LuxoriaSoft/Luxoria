using CommunityToolkit.WinUI.Controls;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;

namespace LuxEditor.Components;

public sealed partial class CollectionExplorer : Page
{
    private List<EditableImage> _images = [];
    private ScrollViewer _scrollViewer = new();
    private WrapPanel _imagePanel = new();
    private Border? _selectedBorder = new();
    private MenuFlyout _filterMenuFlyout = new();
    private (string Algorithm, bool Ascending) _filterBy = ("", false);

    public event Action<EditableImage>? OnImageSelected;
    public event Action? ExportRequestedEvent;

    /// <summary>
    /// Constructor for the CollectionExplorer component
    /// </summary>
    public CollectionExplorer()
    {
        InitializeComponent();
        BuildUI();
        SizeChanged += (s, e) => AdjustImageSizes(e.NewSize);
    }

    /// <summary>
    /// Builds the initial UI components for the CollectionExplorer
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

        _filterMenuFlyout = BuildFilterMenuFlyout([]);

        RootGrid.RightTapped += (s, e) =>
        {
            _filterMenuFlyout.ShowAt(_imagePanel, e.GetPosition(_imagePanel));
        };
    }

    /// <summary>
    /// Function handled when a filter menu item is clicked.
    /// </summary>
    private void OnFilterMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item)
        {
            Symbol newSymbol = item.Icon is SymbolIcon icon
                ? icon.Symbol switch
                {
                    Symbol.Sort => Symbol.Up,
                    Symbol.Up => Symbol.Download,
                    Symbol.Download => Symbol.Up,
                    _ => Symbol.Up
                }
                : Symbol.Up;

            bool ascending = newSymbol == Symbol.Up;

            _filterBy = (item.Text, ascending);
            Debug.WriteLine($"Filter by: {_filterBy.Algorithm}, Ordered: {ascending}");

            item.Icon = new SymbolIcon(newSymbol);

            if (item.Parent is MenuFlyoutSubItem subItem)
            {
                foreach (var sibling in subItem.Items.OfType<MenuFlyoutItem>())
                {
                    if (sibling != item)
                        sibling.Icon = new SymbolIcon(Symbol.Sort);
                }
            }

            SetImages(new List<EditableImage>(_images));
        }
    }

    /// <summary>
    /// Returns the Symbol icon based on _filterBy state for the given algorithm name
    /// </summary>
    private SymbolIcon GetSymbolForFilterItem(string algoName)
    {
        if (_filterBy.Algorithm == algoName)
        {
            return new SymbolIcon(_filterBy.Ascending ? Symbol.Up : Symbol.Download);
        }
        return new SymbolIcon(Symbol.Sort);
    }

    /// <summary>
    /// Build the MenuFlyout for filtering images based on available algorithms
    /// </summary>
    private MenuFlyout BuildFilterMenuFlyout(ICollection<string> filterAlgoNames)
    {
        var menuFlyout = new MenuFlyout();

        var filterBy = new MenuFlyoutSubItem
        {
            Text = "Filter by...",
            Icon = new SymbolIcon(Symbol.Filter)
        };

        foreach (var algo in filterAlgoNames)
        {
            var menuItem = new MenuFlyoutItem
            {
                Text = algo,
                Icon = GetSymbolForFilterItem(algo)
            };
            menuItem.Click += OnFilterMenuItemClick;
            filterBy.Items.Add(menuItem);
        }

        menuFlyout.Items.Add(filterBy);
        menuFlyout.Items.Add(new MenuFlyoutSeparator());
        menuFlyout.Items.Add(new RadioMenuFlyoutItem
        {
            Text = "Hide duplicates",
            Icon = new SymbolIcon(Symbol.Copy)
        });
        menuFlyout.Items.Add(new MenuFlyoutSeparator());
        menuFlyout.Items.Add(new MenuFlyoutItem
        {
            Text = "Show All / Reset",
            Icon = new SymbolIcon(Symbol.ViewAll)
        });

        var exportButton = new MenuFlyoutItem
        {
            Text = "Export Collection",
            Icon = new SymbolIcon(Symbol.ViewAll)
        };

        exportButton.Click += (s, e) =>
        {
            Debug.WriteLine("Export Collection clicked.");
            ExportRequestedEvent?.Invoke();
            //var col = ImageManager.Instance.OpenedImages;
            //Debug.WriteLine("Size: " + col.Count);
        };

        menuFlyout.Items.Add(exportButton);


        return menuFlyout;
    }

    /// <summary>
    /// Sets the images to be displayed in the CollectionExplorer
    /// </summary>
    public void SetImages(IList<EditableImage> images)
    {
        if (images == null || images.Count == 0)
        {
            Debug.WriteLine("SetImages: No images provided.");
            return;
        }

        var filterAlgoNames = images[0].FilterData.GetFilteredAlgorithms();
        _filterMenuFlyout = BuildFilterMenuFlyout(filterAlgoNames);

        DispatcherQueue.TryEnqueue(() =>
        {
            _images.Clear();
            _imagePanel.Children.Clear();

            if (!string.IsNullOrEmpty(_filterBy.Algorithm))
            {
                images = images
                    .Where(img => img.FilterData.GetScores().ContainsKey(_filterBy.Algorithm))
                    .ToList();

                images = _filterBy.Ascending
                    ? images.OrderBy(img => img.FilterData.GetScores()[_filterBy.Algorithm]).ToList()
                    : images.OrderByDescending(img => img.FilterData.GetScores()[_filterBy.Algorithm]).ToList();
            }

            _images = images.ToList();

            for (int i = 0; i < _images.Count; i++)
            {
                var image = _images[i];
                var bitmap = image.ThumbnailBitmap ?? image.PreviewBitmap ?? image.OriginalBitmap;

                var border = new Border
                {
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(5),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0))
                };

                var canvas = new SKXamlCanvas { IgnorePixelScaling = true };

                int indexCopy = i;

                canvas.PaintSurface += (s, e) => OnPaintSurface(s, e, indexCopy);
                border.Child = canvas;

                border.PointerEntered += (s, e) => OnHover(border, true);
                border.PointerExited += (s, e) => OnHover(border, false);
                border.Tapped += (s, e) => OnImageTapped(border, indexCopy);

                _imagePanel.Children.Add(border);
            }

            AdjustImageSizes(new Size(ActualWidth, ActualHeight));
        });
    }

    /// <summary>
    /// Adjusts the sizes of the images in the panel based on the new size of the CollectionExplorer
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
                    var bitmap = _images[index].ThumbnailBitmap ?? _images[index].PreviewBitmap ?? _images[index].OriginalBitmap;
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
    /// Handles the paint surface event to render an image onto the provided canvas.
    /// </summary>
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e, int index)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (index >= 0 && index < _images.Count)
        {
            var bitmap = _images[index].ThumbnailBitmap ?? _images[index].PreviewBitmap ?? _images[index].OriginalBitmap;
            float scale = Math.Min((float)e.Info.Width / bitmap.Width, (float)e.Info.Height / bitmap.Height);
            float offsetX = (e.Info.Width - bitmap.Width * scale) / 2;
            float offsetY = (e.Info.Height - bitmap.Height * scale) / 2;

            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
    }

    /// <summary>
    /// Function to handle hover events on the image borders
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
    /// Handles the tap event on an image border to select the image
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
