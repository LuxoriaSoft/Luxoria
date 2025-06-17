using LuxEditor.EditorUI.Interfaces;
using LuxEditor.EditorUI.Models;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;

namespace LuxEditor.EditorUI.Controls;

public class EditorSlider : IEditorGroupItem, IEditorStylable
{
    private readonly Slider _slider;
    private readonly TextBox _valueBox;
    private readonly StackPanel _container;
    private readonly int _decimalPlaces;

    public string Key { get; }
    public float DefaultValue { get; }

    public Action<float>? OnValueChanged;
    public Action RequestSaveState;

    private DispatcherTimer _debounceTimer;
    private float _lastValue;
    private bool _saveStateOnChange;

    /// <summary>
    /// Creates a new slider control for the editor UI.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="defaultValue"></param>
    /// <param name="decimalPlaces"></param>
    /// <param name="stepFrequency"></param>
    public EditorSlider(string key, float min, float max, float defaultValue, int decimalPlaces = 0, float stepFrequency = 1f, bool saveStateOnChange = true)
    {
        Key = key;
        DefaultValue = defaultValue;
        _decimalPlaces = decimalPlaces;

        _saveStateOnChange = saveStateOnChange;

        _slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            Value = defaultValue,
            StepFrequency = stepFrequency,
            TickFrequency = stepFrequency,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 5, 0),
            Tag = key        };

        _valueBox = new TextBox
        {
            Text = defaultValue.ToString($"F{_decimalPlaces}"),
            Width = 40,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            IsReadOnly = false
        };

        _slider.ValueChanged += SliderChanged;
        _valueBox.LostFocus += ValueBoxEdited;
        _valueBox.KeyDown += ValueBoxEnterKey;
        _slider.DoubleTapped += (s, e) => ResetToDefault();

        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _debounceTimer.Tick += DebounceElapsed;


        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        grid.Children.Add(_slider);
        Grid.SetColumn(_slider, 0);

        grid.Children.Add(_valueBox);
        Grid.SetColumn(_valueBox, 1);

        _container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                new TextBlock
                {
                    Text = key,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                    FontSize = 12,
                    Margin = new Thickness(0,0,0,2)
                },
                grid
            }
        };
    }

    private void DebounceElapsed(object? sender, object e)
    {
        _debounceTimer.Stop();

        if (!_saveStateOnChange)
        {
            RequestSaveState.Invoke();
        }
        else
        {
            ImageManager.Instance.SelectedImage?.SaveState(true);
        }
    }

    /// <summary>
    /// Handles the slider value change event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SliderChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        float newValue = (float)e.NewValue;
        _valueBox.Text = newValue.ToString($"F{_decimalPlaces}");
        _lastValue = newValue;
        OnValueChanged?.Invoke(newValue);

        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    /// <summary>
    /// Handles the value box edit event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ValueBoxEdited(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(_valueBox.Text, out float parsed))
        {
            float clamped = Math.Clamp(parsed, (float)_slider.Minimum, (float)_slider.Maximum);
            _slider.Value = clamped;
            _valueBox.Text = clamped.ToString($"F{_decimalPlaces}");
            OnValueChanged?.Invoke(clamped);
        }
        else
        {
            _valueBox.Text = _slider.Value.ToString($"F{_decimalPlaces}");
        }
    }

    /// <summary>
    /// Handles the Enter key event in the value box.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ValueBoxEnterKey(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            (_valueBox.Parent as FrameworkElement)?.Focus(FocusState.Programmatic);
        }
    }

    /// <summary>
    /// Resets the slider to its default value.
    /// </summary>
    public void ResetToDefault()
    {
        SetValue(DefaultValue);
    }

    /// <summary>
    /// Applies a style to the slider based on the provided EditorStyle.
    /// </summary>
    /// <param name="style"></param>
    public void ApplyStyle(EditorStyle style)
    {
        if (style.ShowTicks)
        {
            _slider.TickPlacement = TickPlacement.Outside;
            _slider.StepFrequency = style.TickFrequency ?? _slider.StepFrequency;
            _slider.TickFrequency = style.TickFrequency ?? _slider.TickFrequency;
        }

        if (style.GradientStart.HasValue && style.GradientEnd.HasValue)
        {
            _slider.Foreground = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0.5),
                EndPoint = new Windows.Foundation.Point(1, 0.5),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = style.GradientStart.Value, Offset = 0 },
                    new GradientStop { Color = style.GradientEnd.Value, Offset = 1 }
                }
            };
            _slider.Background = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0.5),
                EndPoint = new Windows.Foundation.Point(1, 0.5),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = style.GradientStart.Value, Offset = 0 },
                    new GradientStop { Color = style.GradientEnd.Value, Offset = 1 }
                }
            };
        }
    }

    /// <summary>
    /// Sets the value of the slider and updates the value box.
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(float value)
    {
        float clamped = Math.Clamp(value, (float)_slider.Minimum, (float)_slider.Maximum);
        _slider.Value = clamped;
        _valueBox.Text = clamped.ToString($"F{_decimalPlaces}");
    }

    /// <summary>
    /// Gets the current value of the slider.
    /// </summary>
    /// <returns></returns>
    public float GetValue() => (float)_slider.Value;

    /// <summary>
    /// Gets the UI element for this slider control.
    /// </summary>
    /// <returns></returns>
    public UIElement GetElement() => _container;
}
