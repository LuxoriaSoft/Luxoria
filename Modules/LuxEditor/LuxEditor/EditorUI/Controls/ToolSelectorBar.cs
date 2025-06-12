// Controls/ToolSelectorBar.cs
using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LuxEditor.Controls
{
    /// <summary>
    /// Simple tab-style selector with two options: Overall and Layers.
    /// Expose SelectedIndex (0 = Overall, 1 = Layers) and SelectionChanged event.
    /// </summary>
    public class ToolSelectorBar : StackPanel
    {
        private readonly Button _overallButton;
        private readonly Button _layersButton;
        private int _selectedIndex;

        /// <summary>
        /// Fired whenever SelectedIndex changes.
        /// </summary>
        public event EventHandler<int> SelectionChanged;

        public ToolSelectorBar()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 8;

            _overallButton = CreateTabButton("Edit", 0);
            _layersButton = CreateTabButton("Layers", 1);

            Children.Add(_overallButton);
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

        /// <summary>
        /// Gets or sets the currently selected tab (0 = Overall, 1 = Layers).
        /// </summary>
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
            // Simple visual feedback: background color
            var selBrush = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            var normalBrush = new SolidColorBrush(Colors.Transparent);

            _overallButton.Background = _selectedIndex == 0 ? selBrush : normalBrush;
            _layersButton.Background = _selectedIndex == 1 ? selBrush : normalBrush;
        }
    }
}
