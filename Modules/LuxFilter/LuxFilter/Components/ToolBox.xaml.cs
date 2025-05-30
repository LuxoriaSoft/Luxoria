using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LuxFilter.Components;

public sealed partial class ToolBox : Page
{
    private readonly RatingComponent _ratingComponent = new();
    private readonly FlagsComponent _flagsComponent = new();
    private readonly ScoreViewer _scoreViewer = new();

    public event Action<LuxAsset>? OnRatingChanged;
    public event Action<LuxAsset>? OnFlagUpdated;

    private ICollection<Action<LuxAsset>> _updateImages;

    public ToolBox()
    {
        InitializeComponent();
        RGrid.Children.Add(_ratingComponent);
        FGrid.Children.Add(_flagsComponent);
        SGrid.Children.Add(_scoreViewer);

        _updateImages = new Collection<Action<LuxAsset>>
        {
            e => _ratingComponent.SetSelectedAsset(e),
            e => _flagsComponent.SetSelectedAsset(e),
            e => _scoreViewer.SetSelectedAsset(e)
        };

        _ratingComponent.OnRatingChanged += SetSelectedAsset;
        _flagsComponent.OnFlagUpdated += SetSelectedAsset;

        _ratingComponent.OnRatingChanged += (asset) =>
        {
            OnRatingChanged?.Invoke(asset);
        };

        _flagsComponent.OnFlagUpdated += (asset) =>
        {
            OnFlagUpdated?.Invoke(asset);
        };
    }

    public void SetSelectedAsset(LuxAsset asset)
    {
        if (asset == null) return;
        foreach (var update in _updateImages)
        {
            update(asset);
        }
    }
}
