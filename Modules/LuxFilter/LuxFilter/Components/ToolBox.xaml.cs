using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LuxFilter.Components;

public sealed partial class ToolBox : Page
{
    private readonly RatingComponent _ratingComponent = new();
    private readonly FlagsComponent _flagsComponent = new();
    public event Action<LuxAsset>? OnRatingChanged;
    public event Action<LuxAsset>? OnFlagUpdated;

    private ICollection<Action<LuxAsset>> _updateImages;

    public ToolBox()
    {
        InitializeComponent();
        RGrid.Children.Add(_ratingComponent);

        FGrid.Children.Add(_flagsComponent);

        _updateImages = new Collection<Action<LuxAsset>>
        {
            e => _ratingComponent.SetSelectedAsset(e),
            e => _flagsComponent.SetSelectedAsset(e)
        };

        _ratingComponent.OnRatingChanged += (asset) =>
        {
            OnRatingChanged?.Invoke(asset);
        };

        _flagsComponent.OnFlagUpdated += (asset) =>
        {
            OnFlagUpdated?.Invoke(asset);
        };
    }

    public void SetSelectedAsset(ref LuxAsset asset)
    {
        if (asset == null) return;
        foreach (var update in _updateImages)
        {
            update(asset);
        }
    }
}
