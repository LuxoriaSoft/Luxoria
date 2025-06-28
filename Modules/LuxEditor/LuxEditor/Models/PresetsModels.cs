using LuxEditor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class PresetCategory
{
    public string Name { get; }
    public string Path { get; }
    public bool IsReadOnly { get; }
    public ObservableCollection<Preset> Presets { get; } = new();

    public PresetCategory(string name, string folderPath, bool readOnly)
    {
        Name = name;
        Path = folderPath;
        IsReadOnly = readOnly;
    }
}

public sealed class Preset
{
    public string Name { get; }
    public string FilePath { get; }

    public Preset(string name, string filePath)
    {
        Name = name;
        FilePath = filePath;
    }
}
/// <summary>Persisted mask operation</summary>
public sealed class MaskOperationDto
{
    public ToolType ToolType { get; set; }
    public BooleanOperationMode Mode { get; set; }
    public string? MaskPngBase64 { get; set; }
}

/// <summary>Persisted layer (filters + operations)</summary>
public sealed class LayerDto
{
    public string Name { get; set; } = "";
    public bool Visible { get; set; }
    public bool Invert { get; set; }
    public double Strength { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
    public List<MaskOperationDto> Operations { get; set; } = new();
}

/// <summary>Root object written inside *.luxpreset*</summary>
public sealed class LuxPreset
{
    public int Version { get; set; }
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    public List<LayerDto>? Layers { get; set; }
}
