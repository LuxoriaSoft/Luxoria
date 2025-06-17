using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using LuxEditor.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LuxEditor.Controls
{
    public class LayersDetailsPanel : UserControl, INotifyPropertyChanged
    {
        private Button _colorButton;
        private ColorPicker _flyoutPicker;
        private Slider _flyoutOpacity;
        private EditorSlider _strengthSlider;

        private StackPanel _filtersPanel;
        private ContentControl _toneCurveHost;
        private ScrollViewer _scrollViewer;

        private Layer? _currentLayer, _prevLayer;
        private Color _overlayColor = Color.FromArgb(255, 255, 0, 0);
        private double _overlayOpacity = 1;
        private double _strength = 100;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Color OverlayColor { get => _overlayColor; set => SetField(ref _overlayColor, value); }
        public double OverlayOpacity { get => _overlayOpacity; set => SetField(ref _overlayOpacity, Math.Clamp(value, 0, 1)); }
        public double Strength { get => _strength; set => SetField(ref _strength, Math.Clamp(value, 0, 200)); }

        public LayersDetailsPanel() => BuildUI();

        /// <summary>
        /// Sets the current layer for this details panel.
        /// </summary>
        /// <param name="layer"></param>
        public void SetLayer(Layer layer)
        {
            if (_prevLayer != null)
                _prevLayer.PropertyChanged -= OnLayerPropertyChanged;

            _currentLayer = layer;
            _currentLayer.PropertyChanged += OnLayerPropertyChanged;
            _prevLayer = _currentLayer;

            UpdateUI();
            BuildSliders();
        }

        /// <summary>
        /// Builds the user interface for the layers details panel.
        /// </summary>
        private void BuildUI()
        {
            var root = new Grid { Padding = new Thickness(8) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var fixedPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 10 };
            Grid.SetRow(fixedPanel, 0);

            var colorLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            colorLine.Children.Add(new TextBlock { Text = "Overlay :", Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center });

            _colorButton = new Button { Width = 24, Height = 24, BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.Gainsboro) };
            BuildColorFlyout();
            colorLine.Children.Add(_colorButton);
            fixedPanel.Children.Add(colorLine);

            _strengthSlider = new EditorSlider("Strength", 0, 200, 100, 0, 1);
            _strengthSlider.OnValueChanged = v =>
            {
                Strength = v;
                if (_currentLayer != null) _currentLayer.Strength = v;
            };
            fixedPanel.Children.Add(_strengthSlider.GetElement());
            root.Children.Add(fixedPanel);

            _filtersPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8, Padding = new Thickness(10) };
            _toneCurveHost = new ContentControl();

            var scrollContent = new StackPanel { Orientation = Orientation.Vertical, Spacing = 16 };
            scrollContent.Children.Add(_filtersPanel);
            scrollContent.Children.Add(_toneCurveHost);

            _scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = scrollContent };
            Grid.SetRow(_scrollViewer, 1);
            root.Children.Add(_scrollViewer);

            Content = root;

            var toneCurve = new EditorToneCurveGroup();
            toneCurve.CurveChanged += (k, lut) =>
            {
                if (_currentLayer == null) return;
                _currentLayer.Filters[k] = lut;
                _currentLayer.NotifyFiltersChanged();
            };
            _toneCurveHost.Content = toneCurve;
        }

        /// <summary>
        /// Builds the color picker flyout for the overlay color selection.
        /// </summary>
        private void BuildColorFlyout()
        {
            _flyoutPicker = new ColorPicker { IsAlphaEnabled = false };
            _flyoutOpacity = new Slider { Minimum = 0, Maximum = 1, StepFrequency = .01, Width = 100 };

            _flyoutPicker.ColorChanged += (_, a) =>
            {
                OverlayColor = a.NewColor;
                _colorButton.Background = new SolidColorBrush(OverlayColor);
                if (_currentLayer != null)
                    _currentLayer.OverlayColor = WithAlpha(OverlayColor, OverlayOpacity);
            };

            _flyoutOpacity.ValueChanged += (_, a) =>
            {
                OverlayOpacity = a.NewValue;
                if (_currentLayer != null)
                    _currentLayer.OverlayColor = WithAlpha(OverlayColor, OverlayOpacity);
            };

            var flyContent = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8, Padding = new Thickness(8) };
            flyContent.Children.Add(_flyoutPicker);

            var opLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            opLine.Children.Add(new TextBlock { Text = "Opacity", Foreground = new SolidColorBrush(Colors.White) });
            opLine.Children.Add(_flyoutOpacity);
            flyContent.Children.Add(opLine);

            var flyout = new Flyout { Content = flyContent };
            _colorButton.Flyout = flyout;
        }


        /// <summary>
        /// Builds the sliders for the layer filters and adds them to the filters panel.
        /// </summary>
        private void BuildSliders()
        {
            if (_currentLayer == null) return;
            _filtersPanel.Children.Clear();

            AddHeader("Layer Filters");

            AddSlider("Temperature", 2000, 50000, 6500, 0, 100);
            AddSlider("Tint", -150, 150, 0, 0, 1); AddSeparator();

            AddSlider("Exposure", -5, 5, 0, 2, 0.05f);
            AddSlider("Contrast", -1, 1, 0, 2, 0.05f); AddSeparator();

            AddSlider("Highlights", -100, 100, 0);
            AddSlider("Shadows", -100, 100, 0); AddSeparator();

            AddSlider("Whites", -100, 100, 0);
            AddSlider("Blacks", -100, 100, 0); AddSeparator();

            AddSlider("Texture", -100, 100, 0);
            AddSlider("Dehaze", -100, 100, 0); AddSeparator();

            AddSlider("Vibrance", -100, 100, 0);
            AddSlider("Saturation", -100, 100, 0);
        }

        /// <summary>
        /// Adds a header text block to the filters panel.
        /// </summary>
        /// <param name="txt"></param>
        private void AddHeader(string txt) =>
            _filtersPanel.Children.Add(new TextBlock { Text = txt, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Colors.White), Margin = new Thickness(0, 0, 0, 10) });

        /// <summary>
        /// Adds a separator line to the filters panel.
        /// </summary>
        private void AddSeparator() =>
            _filtersPanel.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), Margin = new Thickness(0, 5, 0, 5) });

        /// <summary>
        /// Adds a slider control for a specific filter to the filters panel.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="def"></param>
        /// <param name="dec"></param>
        /// <param name="step"></param>
        private void AddSlider(string key, float min, float max, float def, int dec = 0, float step = 1f)
        {
            var s = new EditorSlider(key, min, max, def, dec, step);
            if (_currentLayer != null && _currentLayer.Filters.TryGetValue(key, out var v))
                s.SetValue(Convert.ToSingle(v));

            s.OnValueChanged += v =>
            {
                if (_currentLayer == null) return;
                _currentLayer.Filters[key] = v;
                _currentLayer.NotifyFiltersChanged();
            };

            _filtersPanel.Children.Add(s.GetElement());
        }

        /// <summary>
        /// Handles property changes in the current layer and updates the UI accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_currentLayer == null) return;

            switch (e.PropertyName)
            {
                case nameof(Layer.OverlayColor):
                    OverlayColor = _currentLayer.OverlayColor;
                    OverlayOpacity = OverlayColor.A / 255.0;
                    _colorButton.Background = new SolidColorBrush(OverlayColor);
                    _flyoutPicker.Color = OverlayColor;
                    _flyoutOpacity.Value = OverlayOpacity;
                    break;

                case nameof(Layer.Strength):
                    Strength = _currentLayer.Strength;
                    _strengthSlider.SetValue((float)Strength);
                    break;

                case nameof(Layer.Filters):
                    BuildSliders();
                    break;
            }
        }

        /// <summary>
        /// Creates a new color with the specified alpha value applied to the given color.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        private static Color WithAlpha(Color c, double op) => Color.FromArgb((byte)(op * 255), c.R, c.G, c.B);

        /// <summary>
        /// Updates the UI elements based on the current layer's properties.
        /// </summary>
        private void UpdateUI()
        {
            if (_currentLayer == null) return;
            OverlayColor = _currentLayer.OverlayColor;
            OverlayOpacity = OverlayColor.A / 255.0;
            Strength = _currentLayer.Strength;

            _colorButton.Background = new SolidColorBrush(OverlayColor);
            _flyoutPicker.Color = OverlayColor;

            _flyoutOpacity.Value = OverlayOpacity;
            _strengthSlider.SetValue((float)Strength);
        }

        /// <summary>
        /// Sets a field value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string p = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
            return true;
        }
    }
}
