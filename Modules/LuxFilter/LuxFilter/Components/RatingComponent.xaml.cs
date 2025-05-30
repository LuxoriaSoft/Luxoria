using Luxoria.Modules.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LuxFilter.Components;

public sealed partial class RatingComponent : UserControl
{
    private LuxAsset? _selectedAsset;

    public event Action<LuxAsset>? OnRatingChanged;

    public RatingComponent()
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

        // Show RatingControl and hide text
        HideNoSelectionMessage();

        // Set current rating from asset
        RatingControl.Value = _selectedAsset.FilterData.Rating;
    }

    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (_selectedAsset == null) return;

        _selectedAsset.FilterData.Rating = sender.Value;
        OnRatingChanged?.Invoke(_selectedAsset);
    }

    private void DisplayNoSelectionMessage()
    {
        RatingControl.Visibility = Visibility.Collapsed;
        NoSelectionPanel.Visibility = Visibility.Visible;
    }

    private void HideNoSelectionMessage()
    {
        RatingControl.Visibility = Visibility.Visible;
        NoSelectionPanel.Visibility = Visibility.Collapsed;
    }
}
