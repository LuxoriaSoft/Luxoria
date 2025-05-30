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
            RatingControl.Visibility = Visibility.Collapsed;
            NoSelectionText.Visibility = Visibility.Visible;
            return;
        }

        _selectedAsset = asset;

        // Show RatingControl and hide text
        RatingControl.Visibility = Visibility.Visible;
        NoSelectionText.Visibility = Visibility.Collapsed;

        // Set current rating from asset
        RatingControl.Value = 0; // _selectedAsset.Rating; // Assuming Rating is float/double
    }

    private void RatingControl_ValueChanged(RatingControl sender, object args)
    {
        if (_selectedAsset == null) return;

        //_selectedAsset.Rating = (float)sender.Value;

        OnRatingChanged?.Invoke(_selectedAsset);
    }
}
