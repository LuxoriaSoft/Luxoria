using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using LuxEditor.Models;
using LuxEditor.EditorUI.Groups;

namespace LuxEditor.Controls
{
    public class OperationDetailsPanel : UserControl, INotifyPropertyChanged
    {
        private readonly MaskOperation _operation;
        private ToggleSwitch _modeSwitch;
        private Button _colorButton;
        private ColorPicker _flyoutPicker;
        private Slider _flyoutOpacity;
        private Slider _strengthSlider;
        private ContentControl _curveControlHost;
        private Flyout _colorFlyout;

        public event PropertyChangedEventHandler? PropertyChanged;

        public OperationDetailsPanel(MaskOperation operation)
        {
            _operation = operation;
            BuildUI();
            UpdateUI();
        }

        private void BuildUI()
        {
            var root = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Padding = new Thickness(8)
            };

            var modeLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            modeLine.Children.Add(new TextBlock
            {
                Text = "Mode:",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center
            });
            _modeSwitch = new ToggleSwitch
            {
                OnContent = "Add",
                OffContent = "Sub",
                IsOn = _operation.Mode == StrokeMode.Add
            };
            _modeSwitch.Toggled += (_, __) =>
            {
                _operation.Mode = _modeSwitch.IsOn
                    ? StrokeMode.Add
                    : StrokeMode.Subtract;
                OnPropertyChanged(nameof(_operation.Mode));
            };
            modeLine.Children.Add(_modeSwitch);
            root.Children.Add(modeLine);

            var colorLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            colorLine.Children.Add(new TextBlock
            {
                Text = "Overlay color:",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center
            });
            _colorButton = new Button
            {
                Width = 24,
                Height = 24,
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
                BorderThickness = new Thickness(1)
            };
            _flyoutPicker = new ColorPicker { IsAlphaEnabled = false };
            _flyoutPicker.ColorChanged += (_, args) =>
            {
                _operation.OverlayColor = args.NewColor;
                OnPropertyChanged(nameof(_operation.OverlayColor));
                _colorButton.Background = new SolidColorBrush(_operation.OverlayColor);
            };
            _flyoutOpacity = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                StepFrequency = 0.01,
                Width = 100,
                Value = _operation.OverlayOpacity
            };
            _flyoutOpacity.ValueChanged += (_, args) =>
            {
                _operation.OverlayOpacity = args.NewValue;
                OnPropertyChanged(nameof(_operation.OverlayOpacity));
            };
            var flyContent = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8, Padding = new Thickness(8) };
            flyContent.Children.Add(_flyoutPicker);
            var opLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
            opLine.Children.Add(new TextBlock
            {
                Text = "Opacity",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center
            });
            opLine.Children.Add(_flyoutOpacity);
            flyContent.Children.Add(opLine);
            _colorFlyout = new Flyout { Content = flyContent };
            _colorButton.Flyout = _colorFlyout;
            colorLine.Children.Add(_colorButton);
            root.Children.Add(colorLine);

            var strengthLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            strengthLine.Children.Add(new TextBlock
            {
                Text = "Strength",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center
            });
            _strengthSlider = new Slider
            {
                Minimum = 0,
                Maximum = 200,
                Value = _operation.Strength,
                Width = 150
            };
            _strengthSlider.ValueChanged += (_, args) =>
            {
                _operation.Strength = args.NewValue;
                OnPropertyChanged(nameof(_operation.Strength));
            };
            strengthLine.Children.Add(_strengthSlider);
            root.Children.Add(strengthLine);

            _curveControlHost = new ContentControl();
            var toneCurve = new EditorToneCurveGroup();
            toneCurve.CurveChanged += (key, lut) =>
            {
                // TODO: lier lut à l’opération ou à l’image
            };
            _curveControlHost.Content = toneCurve;
            root.Children.Add(_curveControlHost);

            Content = root;
        }

        private void UpdateUI()
        {
            _colorButton.Background = new SolidColorBrush(_operation.OverlayColor);
            _flyoutPicker.Color = _operation.OverlayColor;
            _flyoutOpacity.Value = _operation.OverlayOpacity;
            _strengthSlider.Value = _operation.Strength;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
