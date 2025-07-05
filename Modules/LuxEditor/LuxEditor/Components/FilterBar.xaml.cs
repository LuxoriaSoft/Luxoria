using LuxEditor.Models;
using Luxoria.Modules.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxEditor.Components;

public sealed partial class FilterBar : UserControl
{
    public record ActiveFilters(
        HashSet<FilterData.FlagType?> Flags,
        char? RatingOp,
        double RatingVal,
        string? ScoreAlgo,
        double ScoreMin);

    public event Action<ActiveFilters>? FiltersChanged;

    private readonly HashSet<FilterData.FlagType?> _flags = new();

    private double _ratingValue;
    private char? _ratingOp;

    private string? _scoreAlgo;
    private double _scoreMin = .5;

    public FilterBar()
    {
        InitializeComponent();
        Loaded += (s, e) => RefreshUi();
        OnFilterTypeToggled(null, null);
    }

    public void SetAlgorithms(IEnumerable<string> algos)
    {
        ScoreAlgo.ItemsSource = algos
            .Select(a => new ComboBoxItem { Content = a })
            .ToList();
    }

    private void OnFlagChanged(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            var flag = tb == FlagKeep ? FilterData.FlagType.Keep :
                       tb == FlagIgnore ? FilterData.FlagType.Ignore : (FilterData.FlagType?)null;

            if (tb.IsChecked == true)
                _flags.Add(flag);
            else
                _flags.Remove(flag);

            Push();
        }
    }

    private void OnRatingStarClicked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && int.TryParse(tb.Tag.ToString(), out int v))
        {
            _ratingValue = v;

            foreach (var child in RatingPanel.Children.OfType<ToggleButton>())
            {
                int star = int.Parse(child.Tag.ToString()!);
                child.Content = star <= _ratingValue ? "★" : "☆";
                child.IsChecked = star <= _ratingValue;
            }
            Push();
        }
    }

    private void OnRatingOpChanged(object? s, SelectionChangedEventArgs e) => Push();

    private void OnScoreAlgoChanged(object sender, SelectionChangedEventArgs e)
    {
        _scoreAlgo = (ScoreAlgo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (!string.IsNullOrEmpty(_scoreAlgo))
        {
            ScoreThreshold.Minimum = 0;
            ScoreThreshold.Maximum = 1;
            ScoreThreshold.StepFrequency = 0.01;
            ScoreThreshold.Value = 0.5;
        }
        Push();
    }
    private void OnScoreThresholdChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        _scoreMin = e.NewValue;
        Push();
    }

    private void OnClearClicked(object sender, RoutedEventArgs e)
    {
        TogFlags.IsChecked = TogRating.IsChecked = TogScore.IsChecked = false;
        OnFilterTypeToggled(null!, null!);

        FlagKeep.IsChecked = FlagNone.IsChecked = FlagIgnore.IsChecked = false;
        _flags.Clear();

        RatingOp.SelectedIndex = 0;
        _ratingValue = 0;
        foreach (var child in RatingPanel.Children.OfType<ToggleButton>())
        {
            child.IsChecked = false;
            child.Content = "☆";
        }

        ScoreAlgo.SelectedIndex = -1;
        _scoreAlgo = null;
        ScoreThreshold.Value = 0.5;

        Push();
    }


    private void RefreshUi()
    {
        RatingOp.SelectionChanged += OnRatingOpChanged;
    }

    private void Push()
    {
        _ratingOp = RatingOp.SelectedItem is ComboBoxItem cbi ? cbi.Content.ToString()![0] : null;

        FiltersChanged?.Invoke(new ActiveFilters(
            new HashSet<FilterData.FlagType?>(_flags),
            _ratingOp,
            _ratingValue,
            _scoreAlgo,
            _scoreMin));
    }

    private void OnFilterTypeToggled(object? sender, RoutedEventArgs e)
    {
        if (TogFlags.IsChecked == true)
            FlagsPanel.Visibility = Visibility.Visible;
        else
        {
            FlagsPanel.Visibility = Visibility.Collapsed;
            _flags.Clear();
            FlagKeep.IsChecked = FlagNone.IsChecked = FlagIgnore.IsChecked = false;
        }

        if (TogRating.IsChecked == true)
            RatingPanel.Visibility = Visibility.Visible;
        else
        {
            RatingPanel.Visibility = Visibility.Collapsed;
            _ratingValue = 0;
            _ratingOp = null;
            RatingOp.SelectedIndex = 0;
            foreach (var star in RatingPanel.Children.OfType<ToggleButton>())
            {
                star.IsChecked = false;
                star.Content = "☆";
            }
        }

        if (TogScore.IsChecked == true)
            ScorePanel.Visibility = Visibility.Visible;
        else
        {
            ScorePanel.Visibility = Visibility.Collapsed;
            _scoreAlgo = null;
            ScoreAlgo.SelectedIndex = -1;
            ScoreThreshold.Value = 0.5;
        }

        Push();
    }
}
