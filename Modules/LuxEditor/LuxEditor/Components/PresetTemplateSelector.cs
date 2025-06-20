using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LuxEditor.ViewModels;

namespace LuxEditor.Selectors;

public sealed class PresetTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CategoryTemplate { get; set; }
    public DataTemplate? PresetTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
        => item switch
        {
            PresetCategoryViewModel => CategoryTemplate!,
            PresetViewModel => PresetTemplate!,
            _ => base.SelectTemplateCore(item)
        };
}
