using ICSharpCode.SharpZipLib.Zip;
using LuxEditor.Services;
using Luxoria.Modules.Models.Events;
using Luxoria.Modules;
using Microsoft.UI.Xaml.Controls;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Luxoria.Modules.Interfaces;
using static LuxEditor.Models.EditableImage;
using LuxEditor.Components;
using Microsoft.UI.Xaml;

namespace LuxEditor.Logic;

/// <summary>
/// Singleton that handles .luxpreset files and zipped categories.
/// </summary>
public sealed class PresetManager
{
    public static PresetManager Instance { get; } = new();
    public event EventHandler<ReadOnlyCollection<PresetCategory>>? CategoriesLoaded;

    private IEventBus? _bus;

    private readonly List<PresetCategory> _categories = new();

    private readonly string _defaultsRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Luxoria", "Presets", "Luxoria defaults");

    private readonly string _userRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Luxoria", "Presets", "User presets");

    private PresetManager() { }


    public void ConfigureBus(IEventBus bus) => _bus = bus;

    /// <summary>
    /// Loads every preset found in both default and user roots, then raises <see cref="CategoriesLoaded"/>.
    /// </summary>
    public void LoadAll()
    {
        _categories.Clear();
        _categories.AddRange(LoadFolder(_defaultsRoot, readOnly: true));
        _categories.AddRange(LoadFolder(_userRoot, readOnly: false));
        CategoriesLoaded?.Invoke(this, _categories.AsReadOnly());
    }

    /// <summary>
    /// Opens the add-preset dialog (import or create-from-current).
    /// </summary>
    public void ShowAddPresetDialog()
    {
    }

    /// <summary>Creates a new preset file from current editor settings.</summary>
    public void CreatePreset(string category, string name, IDictionary<string, object> settings)
    {
        var folder = EnsureCategoryFolder(category, _userRoot);
        var file = GetUniqueFilePath(folder, name + ".luxpreset");

        var payload = new LuxPreset
        {
            Version = 1,
            Category = category,
            Name = name,
            Settings = settings
        };
        File.WriteAllText(file, JsonSerializer.Serialize(payload));
        ApplySnapshot();
        LoadAll();
    }

    public void ApplyPreset(Preset preset)
    {
        var json = File.ReadAllText(preset.FilePath);
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("Settings", out var settingsProp))
            return;

        var img = ImageManager.Instance.SelectedImage;
        if (img == null) return;

        var newSettings = new Dictionary<string, object>();

        foreach (var prop in settingsProp.EnumerateObject())
        {
            var key = prop.Name;
            var elem = prop.Value;
            object? valueToApply = elem.ValueKind switch
            {
                JsonValueKind.Number => (float)elem.GetDouble(),
                JsonValueKind.String => DecodeStringSetting(key, elem.GetString()!),
                JsonValueKind.Array => ParseArray(key, elem),
                _ => null
            };
            if (valueToApply != null)
                newSettings[key] = valueToApply;
        }

        img.Settings = newSettings;
        img.SaveState();
        ImageManager.Instance.SelectImage(img);
    }

    private static object? ParseArray(string key, JsonElement arr)
    {
        if (key.StartsWith("ToneCurve_") &&
            arr.GetArrayLength() == 256 &&
            arr.EnumerateArray().All(x => x.ValueKind == JsonValueKind.Number))
        {
            var lut = new byte[256];
            int i = 0;
            foreach (var n in arr.EnumerateArray())
                lut[i++] = (byte)n.GetInt32();
            return lut;
        }

        if (arr.EnumerateArray().All(x => x.ValueKind == JsonValueKind.Number))
            return arr.EnumerateArray().Select(x => (float)x.GetDouble()).ToList();

        return arr.EnumerateArray().Select(x => x.EnumerateObject().ToDictionary(kv => kv.Name, kv => kv.Value.GetDouble())).ToList();
    }

    private object DecodeStringSetting(string key, string str)
    {
        if (key.StartsWith("ToneCurve_"))
            return Convert.FromBase64String(str);
        return str;
    }

    public void Import(string path)
    {
        if (Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            ImportZip(path);
        else
            ImportPresetFile(path);
        LoadAll();
    }

    /// <summary>Exports the given preset as a standalone .luxpreset.</summary>
    public async void ExportPreset(Preset preset)
    {
        var target = await PickSaveFileAsync(preset.Name, ".luxpreset");
        if (target == null) return;

        File.Copy(preset.FilePath, target, true);
        _bus?.Publish(new OpenCollectionEvent(
            Path.GetFileNameWithoutExtension(target),
            Path.GetDirectoryName(target)!));

        ApplySnapshot();
    }


    /// <summary>Zips an entire category so the user can share it.</summary>
    public async void ExportCategory(PresetCategory cat)
    {
        var target = await PickSaveFileAsync(cat.Name, ".zip");
        if (target == null) return;

        using var fs = File.Create(target);
        using var zip = new ZipOutputStream(fs);

        foreach (var file in Directory.EnumerateFiles(cat.Path, "*.luxpreset"))
        {
            var entry = new ZipEntry($"{cat.Name}/{Path.GetFileName(file)}") { DateTime = DateTime.Now };
            zip.PutNextEntry(entry);
            zip.Write(File.ReadAllBytes(file));
        }
        zip.Finish();

        _bus?.Publish(new OpenCollectionEvent(
            Path.GetFileNameWithoutExtension(target),
            Path.GetDirectoryName(target)!));

        ApplySnapshot();
    }


    /// <summary>Allows inline rename of a category folder.</summary>
    public void EditCategory(PresetCategory cat) { /* inline-rename logic */ }

    /// <summary>Shows the preset-edit dialog (name + category).</summary>
    public void EditPreset(Preset preset, PresetCategory parent) { /* dialog */ }

    /// <summary>Deletes a preset file and removes empty folders.</summary>
    public void DeletePreset(Preset preset, PresetCategory parent)
    {
        if (!parent.IsReadOnly) File.Delete(preset.FilePath);
        ApplySnapshot();
        LoadAll();
    }

    /// <summary>Deletes an entire category (folder).</summary>
    public void DeleteCategory(PresetCategory cat)
    {
        if (!cat.IsReadOnly) Directory.Delete(cat.Path, recursive: true);
        ApplySnapshot();
        LoadAll();
    }

    // ---------- helpers ----------

    private IEnumerable<PresetCategory> LoadFolder(string root, bool readOnly)
    {
        if (!Directory.Exists(root)) yield break;

        foreach (var dir in Directory.EnumerateDirectories(root))
        {
            var cat = new PresetCategory(
                System.IO.Path.GetFileName(dir),
                dir,
                readOnly);
            foreach (var file in Directory.EnumerateFiles(dir, "*.luxpreset"))
            {
                var p = JsonSerializer.Deserialize<LuxPreset>(File.ReadAllText(file));
                if (p != null) cat.Presets.Add(new Preset(p.Name, file));
            }
            if (cat.Presets.Count > 0) yield return cat;
        }
    }

    private void ImportPresetFile(string file)
    {
        var payload = JsonSerializer.Deserialize<LuxPreset>(File.ReadAllText(file));
        if (payload == null) return;

        var folder = EnsureCategoryFolder(payload.Category, _userRoot);
        var target = GetUniqueFilePath(folder, payload.Name + ".luxpreset");
        File.Copy(file, target);
    }

    private void ImportZip(string zipPath)
    {
        using var fs = File.OpenRead(zipPath);
        using var zip = new ZipInputStream(fs);

        ZipEntry? entry;
        while ((entry = zip.GetNextEntry()) != null)
        {
            if (!entry.IsFile || !entry.Name.EndsWith(".luxpreset", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = entry.Name.Split(new[] { '/', '\\' }, 2);
            var catFolder = EnsureCategoryFolder(parts[0], _userRoot);
            var filePath = GetUniqueFilePath(catFolder, Path.GetFileName(parts[1]));

            using var outFs = File.Create(filePath);
            zip.CopyTo(outFs);
        }
    }

    private static string EnsureCategoryFolder(string name, string root)
    {
        var path = Path.Combine(root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetUniqueFilePath(string folder, string fileName)
    {
        var path = Path.Combine(folder, fileName);
        var i = 1;
        while (File.Exists(path))
        {
            path = Path.Combine(folder,
                $"{Path.GetFileNameWithoutExtension(fileName)} ({i++}){Path.GetExtension(fileName)}");
        }
        return path;
    }

    private async Task<string?> PickSaveFileAsync(string suggested, string ext)
    {
        var win = new Luxoria.App.Views.PickSaveFileWindow(suggested, ext);
        win.Activate();
        return await win.PickAsync();
    }

    /// <summary>
    /// Create a preset from a full EditableImage snapshot.
    /// </summary>
    public void CreatePresetFromSnapshot(
        EditableImageSnapshot snap,
        string category,
        string name)
    {
        // Reuse your existing folder logic
        var folder = EnsureCategoryFolder(category, _userRoot);
        var file = GetUniqueFilePath(folder, name + ".luxpreset");

        var payload = new LuxPreset
        {
            Version = 1,
            Category = category,
            Name = name,
            Settings = snap.Settings,  // grab exactly what the snapshot captured
                                       // you could also serialize snap.FilterData or snap.LayerManager if desired
        };

        File.WriteAllText(file, JsonSerializer.Serialize(payload));
        ApplySnapshot();      // push an undo step :contentReference[oaicite:1]{index=1}
        LoadAll();
    }



    private static void ApplySnapshot()
    {
        ImageManager.Instance.SelectedImage.SaveState();
    }
}
