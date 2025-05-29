using System;
using System.Collections.Specialized;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LuxEditor.Controls
{
    public class LayersPanel : UserControl
    {
        private readonly StackPanel _itemsHost;
        private readonly Button _addButton;
        private readonly Button _removeButton;
        private EditableImage _currentImage;

        public LayersPanel(EditableImage editableImage)
        {
            _currentImage = editableImage ?? throw new ArgumentNullException(nameof(editableImage));

            var root = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Padding = new Thickness(8)
            };

            var btnLine = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4
            };
            _addButton = new Button { Content = "+", Width = 24, Height = 24 };
            _removeButton = new Button { Content = "–", Width = 24, Height = 24 };
            _addButton.Click += (s, e) => ShowAddFlyout();
            _removeButton.Click += (s, e) => _currentImage.LayerManager.RemoveLayer();
            btnLine.Children.Add(_addButton);
            btnLine.Children.Add(_removeButton);
            root.Children.Add(btnLine);

            _itemsHost = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 2
            };
            var scroll = new ScrollViewer
            {
                Content = _itemsHost,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Height = 150
            };
            root.Children.Add(scroll);

            Content = root;

            _currentImage.LayerManager.Layers.CollectionChanged += (s, e) => RefreshUI();
            _currentImage.LayerManager.PropertyChanged += (s, e) =>
            { if (e.PropertyName == nameof(LayerManager.SelectedLayer)) RefreshUI(); };

            RefreshUI();
        }

        private void ShowAddFlyout()
        {
            var fly = new MenuFlyout();
            foreach (BrushType t in Enum.GetValues(typeof(BrushType)))
            {
                var it = new MenuFlyoutItem { Text = t.ToString(), Tag = t };
                it.Click += (s, e) => _currentImage.LayerManager.AddLayer((BrushType)it.Tag);
                fly.Items.Add(it);
            }
            fly.ShowAt(_addButton);
        }

        private void RefreshUI()
        {
            _itemsHost.Children.Clear();
            _removeButton.IsEnabled = _currentImage.LayerManager.SelectedLayer != null
                                   && _currentImage.LayerManager.Layers.IndexOf(_currentImage.LayerManager.SelectedLayer) > 0;

            for (int i = _currentImage.LayerManager.Layers.Count - 1; i >= 0; i--)
            {
                var layer = _currentImage.LayerManager.Layers[i];
                var border = new Border
                {
                    Padding = new Thickness(6),
                    Background = layer == _currentImage.LayerManager.SelectedLayer
                                  ? new SolidColorBrush(Color.FromArgb(255, 70, 70, 70))
                                  : new SolidColorBrush(Colors.Transparent),
                    Tag = layer
                };

                var tb = new TextBlock
                {
                    Text = layer.Name,
                    FontSize = 14
                };
                border.Child = tb;

                border.Tapped += (s, e) =>
                {
                    _currentImage.LayerManager.SelectedLayer = (Layer)border.Tag;
                };

                _itemsHost.Children.Add(border);
            }
        }
    }
}
