using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;

namespace LuxFilter.Components;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ToolBox : Page
{
    private readonly RatingComponent _ratingComponent = new();
    public ToolBox()
    {
        InitializeComponent();

        MainGrid.Children.Add(_ratingComponent);
    }

    public void SetSelectedAsset(ref LuxAsset asset)
    {
        if (asset == null) return;
        _ratingComponent.SetSelectedAsset(asset);
        // Set the selected asset in the toolbox
        //SelectedAssetTextBlock.Text = asset?.Id ?? "No asset selected";
    }
}
