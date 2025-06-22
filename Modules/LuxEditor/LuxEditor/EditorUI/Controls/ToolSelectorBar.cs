using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LuxEditor.Controls
{
    /// <summary>
    /// Tab-style selector with three options: Edit, Crop and Layers.
    /// </summary>
    public class ToolSelectorBar : StackPanel
    {
        private readonly Button _overallButton;
        private readonly Button _cropButton;
        private readonly Button _layersButton;
        private int _selectedIndex;

        public event EventHandler<int> SelectionChanged;

        public ToolSelectorBar()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 8;

            _overallButton = CreateTabButton("Edit", 0);
            _cropButton = CreateTabButton("Crop", 1);
            _layersButton = CreateTabButton("Layers", 2);

            Children.Add(_overallButton);
            Children.Add(_cropButton);
            Children.Add(_layersButton);

            SelectedIndex = 0;
        }

        private Button CreateTabButton(string text, int index)
        {
            var btn = new Button
            {
                Content = text,
                Tag = index,
                Padding = new Thickness(12, 6, 12, 6),
                MinWidth = 80
            };
            btn.Click += OnTabClicked;
            return btn;
        }

        private void OnTabClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idx)
            {
                SelectedIndex = idx;
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                UpdateVisualState();
                SelectionChanged?.Invoke(this, _selectedIndex);
            }
        }

        private void UpdateVisualState()
        {
            var selBrush = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            var normalBrush = new SolidColorBrush(Colors.Transparent);

            _overallButton.Background = (_selectedIndex == 0) ? selBrush : normalBrush;
            _cropButton.Background = (_selectedIndex == 1) ? selBrush : normalBrush;
            _layersButton.Background = (_selectedIndex == 2) ? selBrush : normalBrush;
        }
    }
}
