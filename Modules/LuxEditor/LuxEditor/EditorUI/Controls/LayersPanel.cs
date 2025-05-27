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

        public LayersPanel()
        {
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
            _removeButton.Click += (s, e) => LayerManager.Instance.RemoveLayer();
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

            LayerManager.Instance.Layers.CollectionChanged += (s, e) => RefreshUI();
            LayerManager.Instance.PropertyChanged += (s, e) =>
            { if (e.PropertyName == nameof(LayerManager.SelectedLayer)) RefreshUI(); };

            RefreshUI();
        }

        private void ShowAddFlyout()
        {
            var fly = new MenuFlyout();
            foreach (BrushType t in Enum.GetValues(typeof(BrushType)))
            {
                var it = new MenuFlyoutItem { Text = t.ToString(), Tag = t };
                it.Click += (s, e) => LayerManager.Instance.AddLayer((BrushType)it.Tag);
                fly.Items.Add(it);
            }
            fly.ShowAt(_addButton);
        }

        private void RefreshUI()
        {
            _itemsHost.Children.Clear();
            _removeButton.IsEnabled = LayerManager.Instance.SelectedLayer != null
                                   && LayerManager.Instance.Layers.IndexOf(LayerManager.Instance.SelectedLayer) > 0;

            for (int i = LayerManager.Instance.Layers.Count - 1; i >= 0; i--)
            {
                var layer = LayerManager.Instance.Layers[i];
                var border = new Border
                {
                    Padding = new Thickness(6),
                    Background = layer == LayerManager.Instance.SelectedLayer
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
                    LayerManager.Instance.SelectedLayer = (Layer)border.Tag;
                };

                _itemsHost.Children.Add(border);
            }
        }
    }
}
