using Luxoria.Modules.Models;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace LuxFilter.Components;

public sealed partial class ScoreViewer : UserControl
{
    private LuxAsset? _selectedAsset;

    public ScoreViewer()
    {
        InitializeComponent();
    }

    public void SetSelectedAsset(LuxAsset asset)
    {
        if (asset == null)
        {
            _selectedAsset = null;
            DisplayNoSelectionMessage();
            return;
        }

        _selectedAsset = asset;

        DisplayScore(_selectedAsset.FilterData.GetScores());
        HideNoSelectionMessage();
    }

    private void DisplayNoSelectionMessage()
    {
        ScoresPanel.Children.Clear();
        ScoresScroll.Visibility = Visibility.Collapsed;
        NoSelectionPanel.Visibility = Visibility.Visible;
    }

    private void HideNoSelectionMessage()
    {
        ScoresScroll.Visibility = Visibility.Visible;
        NoSelectionPanel.Visibility = Visibility.Collapsed;
    }

    private void DisplayScore(IDictionary<string, double>? scores)
    {
        ScoresPanel.Children.Clear();

        if (scores == null || scores.Count == 0)
        {
            ScoresPanel.Children.Add(new TextBlock { Text = "No scores available." });
            return;
        }

        foreach (var pair in scores.OrderBy(i => i.Key))
        {
            ScoresPanel.Children.Add(CreateScoreRow(pair.Key, pair.Value.ToString("F2")));
        }
    }

    private UIElement CreateScoreRow(string name, string score)
    {
        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 0)
        };

        // Define columns Name / Score
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Auto) });

        var nameBlock = new TextBlock
        {
            Text = name,
            FontWeight = FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            MinWidth = 100
        };

        var scoreBlock = new TextBlock
        {
            Text = score,
            FontWeight = FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            MinWidth = 50
        };

        Grid.SetColumn(nameBlock, 0);
        Grid.SetColumn(scoreBlock, 1);

        grid.Children.Add(nameBlock);
        grid.Children.Add(scoreBlock);

        return grid;
    }
}
