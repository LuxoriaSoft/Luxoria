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

public sealed class LuxPreset
{
    public int Version { get; set; }
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
}
