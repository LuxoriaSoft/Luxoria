using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxEditor.Logic;
using System.Collections.ObjectModel;

namespace LuxEditor.ViewModels;

/// <summary>
/// Root VM exposed by Infos.xaml behind the expander.
/// </summary>
public sealed partial class PresetsViewModel : ObservableObject
{
    public ObservableCollection<PresetCategoryViewModel> Categories { get; } = new();

    public PresetsViewModel()
    {
        PresetManager.Instance.CategoriesLoaded += (_, e) =>
        {
            Categories.Clear();
            foreach (var cat in e) Categories.Add(new PresetCategoryViewModel(cat));
        };
        PresetManager.Instance.LoadAll();
    }

    [RelayCommand]
    private void AddPreset() => PresetManager.Instance.ShowAddPresetDialog();
}

/// <summary>
/// One category (depth-0 node).
/// </summary>
public sealed partial class PresetCategoryViewModel : ObservableObject
{
    public ObservableCollection<PresetViewModel> Presets { get; } = new();
    public string Name => _model.Name;

    private readonly PresetCategory _model;

    public PresetCategoryViewModel(PresetCategory model)
    {
        _model = model;
        foreach (var p in model.Presets) Presets.Add(new PresetViewModel(p, model));
    }

    [RelayCommand] private void Edit() => PresetManager.Instance.EditCategory(_model);
    [RelayCommand] private void Delete() => PresetManager.Instance.DeleteCategory(_model);
    [RelayCommand] private void Export() => PresetManager.Instance.ExportCategory(_model);
}

/// <summary>
/// One preset (depth-1 node).
/// </summary>
public sealed partial class PresetViewModel : ObservableObject
{
    public string Name => _model.Name;

    private readonly Preset _model;
    private readonly PresetCategory _parent;

    public Preset Model => _model;
    public PresetViewModel(Preset model, PresetCategory parent)
    {
        _model = model;
        _parent = parent;
    }

    [RelayCommand] private void Edit() => PresetManager.Instance.EditPreset(_model, _parent);
    [RelayCommand] private void Delete() => PresetManager.Instance.DeletePreset(_model, _parent);
    [RelayCommand] private void Export() => PresetManager.Instance.ExportPreset(_model);
}
