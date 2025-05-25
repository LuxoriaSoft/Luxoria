using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class CurveSelectorBar : StackPanel
    {
        public event Action<int>? SelectionChanged;

        private readonly ToggleButton[] _buttons;

        public int SelectedIndex { get; private set; }

        /// <summary>
        /// Builds the five-button selector.
        /// </summary>
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

        /// <summary>
        /// Handles the checked event to ensure only one button is selected at a time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var b in _buttons)
            {
                if (b != sender) b.IsChecked = false;
            }
        }

        /// <summary>
        /// Handles the click event to update the selected index and fire the selection changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, RoutedEventArgs e)
        {
            SelectedIndex = (int)((ToggleButton)sender).Tag;
            SelectionChanged?.Invoke(SelectedIndex);
        }

        /// <summary>
        /// Renders the icons for each button in the selector bar.
        /// </summary>
        private void RenderIcons()
        {
            _buttons[0].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 200, 200, 200));
            _buttons[1].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 200, 200, 200));
            _buttons[2].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 232, 62, 62));
            _buttons[3].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 66, 220, 66));
            _buttons[4].Content = BuildCircleIcon(Windows.UI.Color.FromArgb(255, 66, 140, 255));
        }

        /// <summary>
        /// Builds a circle icon with the specified color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static UIElement BuildCircleIcon(Windows.UI.Color color) =>
            new Ellipse { Width = 12, Height = 12, Fill = new SolidColorBrush(color) };
    }
}
