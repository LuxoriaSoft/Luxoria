using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using LuxEditor.Models;
using LuxEditor.EditorUI.Groups;
using System.Collections.Generic;
using SkiaSharp;

namespace LuxEditor.Controls
{
    public class LayersDetailsPanel : UserControl, INotifyPropertyChanged
    {
        private readonly List<MaskOperation> _operation;
        private Button _colorButton;
        private ColorPicker _flyoutPicker;
        private Slider _flyoutOpacity;
        private Slider _strengthSlider;
        private ContentControl _curveControlHost;
        private Flyout _colorFlyout;
        private Color _overlayColor = Color.FromArgb(255, 0, 0, 0);
        private double _overlayOpacity = 1.0;
        private double _strength = 100;

        public Color OverlayColor
        {
            get => _overlayColor;
            set => SetField(ref _overlayColor, value);
        }

        public double OverlayOpacity
        {
            get => _overlayOpacity;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                SetField(ref _overlayOpacity, value);
            }
        }

        public double Strength
        {
            get => _strength;
            set
            {
                if (value < 0) value = 0;
                if (value > 200) value = 200;
                SetField(ref _strength, value);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }


        public LayersDetailsPanel()
        {
            _operation = new List<MaskOperation>();

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
                OverlayColor = args.NewColor;
                OnPropertyChanged(nameof(OverlayColor));
                _colorButton.Background = new SolidColorBrush(OverlayColor);
                for (int i = 0; i < _operation.Count; i++)
                {
                    _operation[i].Tool.Color = new SKColor(OverlayColor.R, OverlayColor.G, OverlayColor.B, OverlayColor.A);
                }
            };
            _flyoutOpacity = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                StepFrequency = 0.01,
                Width = 100,
                Value = OverlayOpacity
            };
            _flyoutOpacity.ValueChanged += (_, args) =>
            {
                OverlayOpacity = args.NewValue;
                OnPropertyChanged(nameof(OverlayOpacity));
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
                Value = Strength,
                Width = 150
            };
            _strengthSlider.ValueChanged += (_, args) =>
            {
                Strength = args.NewValue;
                OnPropertyChanged(nameof(Strength));
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
            _colorButton.Background = new SolidColorBrush(OverlayColor);
            _flyoutPicker.Color = OverlayColor;
            _flyoutOpacity.Value = OverlayOpacity;
            _strengthSlider.Value = Strength;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
