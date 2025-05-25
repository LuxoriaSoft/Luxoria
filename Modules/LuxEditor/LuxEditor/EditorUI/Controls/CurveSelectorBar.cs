using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace LuxEditor.EditorUI.Controls
{
    /// <summary>Horizontal bar that toggles between Parametric, Point, R, G, B modes.</summary>
    public sealed class CurveSelectorBar : StackPanel
    {
        public event Action<int>? SelectionChanged;

        private readonly ToggleButton[] _buttons;

        /// <summary>Builds the five-button selector.</summary>
        public CurveSelectorBar()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 8;

            _buttons = new ToggleButton[5];

            for (int i = 0; i < 5; i++)
            {
                var btn = new ToggleButton
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 60, 60, 60)),
                    BorderThickness = new Thickness(0),
                    Tag = i
                };
                btn.Checked += OnChecked;
                btn.Click += OnClicked;
                _buttons[i] = btn;
                Children.Add(btn);
            }

            RenderIcons();
            _buttons[0].IsChecked = true;
        }

        /// <summary>Currently selected index (0-4).</summary>
        public int SelectedIndex { get; private set; }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var b in _buttons)
            {
                if (b != sender) b.IsChecked = false;
            }
        }

        private void OnClicked(object sender, RoutedEventArgs e)
        {
            SelectedIndex = (int)((ToggleButton)sender).Tag;
            SelectionChanged?.Invoke(SelectedIndex);
        }

        private void RenderIcons()
        {
            _buttons[0].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 200, 200, 200));
            _buttons[1].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 200, 200, 200));
            _buttons[2].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 232, 62, 62));
            _buttons[3].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 66, 220, 66));
            _buttons[4].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 66, 140, 255));
        }

        private static UIElement BuildCircleIcon(Windows.UI.Color color) =>
            new Ellipse { Width = 12, Height = 12, Fill = new SolidColorBrush(color) };
    }
}
