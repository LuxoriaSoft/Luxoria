using LuxEditor.EditorUI.Controls;
using LuxEditor.Logic;
using LuxEditor.Utils;
using Luxoria.Modules.Models;
using Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Windows.UI;

namespace LuxEditor.Models
{
    public class EditableImage
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string FileName { get; }
        public SKBitmap OriginalBitmap { get; }
        public SKBitmap? ThumbnailBitmap { get; set; }
        public SKBitmap? MediumBitmap { get; set; }
        public SKBitmap? PreviewBitmap { get; set; }
        public SKBitmap EditedBitmap { get; set; }
        public SKBitmap EditedPreviewBitmap { get; set; }
        public ReadOnlyDictionary<string, string> Metadata { get; }

        public Dictionary<string, object> Settings { get; set; }
        public FilterData FilterData { get; private set; }
        public readonly LayerManager LayerManager;

        private readonly List<EditableImageSnapshot> _snapshots = new();
        private int _cursor = -1;
        private const int MaxSnapshots = 100;

        private DateTime _lastUndoRedoTime = DateTime.MinValue;

        public record EditableImageSnapshot
        {
            public required string FileName;
            public required SKBitmap EditedBitmap;
            public required Dictionary<string, object> Settings;
            public required FilterData FilterData;
            public required ReadOnlyDictionary<string, string> Metadata;
            public required LayerManager LayerManager;
        }

        private readonly LuxCfg _luxCfg;
        private readonly FileExtension _fileExtension;

        public EditableImage(LuxAsset asset)
        {
            Id = asset.Id;
            FileName = asset.MetaData.FileName;
            FilterData = asset.FilterData ?? new FilterData();
            OriginalBitmap = asset.Data.Bitmap;
            Metadata = asset.Data.EXIF;
            Settings = CreateDefaultSettings();
            EditedBitmap = OriginalBitmap.Copy();
            EditedPreviewBitmap = new SKBitmap(OriginalBitmap.Width, OriginalBitmap.Height);
            _luxCfg = asset.MetaData;
            _fileExtension = asset.MetaData.Extension;
            LayerManager = new(this);
            SaveState();
        }

        public LuxAsset ToLuxAsset() => new() { MetaData = _luxCfg, FilterData = FilterData, Data = new ImageData(EditedBitmap, _fileExtension, Metadata.ToDictionary()) };

        /// <summary>
        /// Saves the current state of the settings to the history stack.
        /// </summary>
        /// 
        public EditableImageSnapshot CaptureSnapshot()
        {
            //Debug.WriteLine("Capture Snapshot settings: " + PrintSettings(Settings));
            return new EditableImageSnapshot
            {
                FileName = FileName,
                EditedBitmap = EditedBitmap.Copy(),
                Metadata = Metadata,
                FilterData = FilterDataClone(FilterData),
                Settings = CloneSettings(Settings),
                LayerManager = LayerManager.Clone(),
            };
        }

        public void ClearHistory()
        {
            _snapshots.Clear();
            _cursor = -1;
            SaveState();
        }

        public void SaveState(bool isSensible = false)
        {
            if (isSensible && (DateTime.UtcNow - _lastUndoRedoTime) < TimeSpan.FromMilliseconds(500))
                return;

            var snap = CaptureSnapshot();

            if (_cursor >= 0 && Compare.AreSnapshotsEqual(_snapshots[_cursor], snap))
                return;

            if (_cursor < _snapshots.Count - 1)
                _snapshots.RemoveRange(_cursor + 1, _snapshots.Count - (_cursor + 1));

            if (_snapshots.Count == MaxSnapshots)
            {
                _snapshots.RemoveAt(0);
                _cursor--;
            }

            _snapshots.Add(snap);
            _cursor++;
            Debug.WriteLine($"Saved snapshot {_cursor + 1}/{_snapshots.Count}");
        }

        public bool Undo()
        {
            if (_cursor <= 0)
                return false;

            _cursor--;
            RestoreSnapshot(_snapshots[_cursor]);
            _lastUndoRedoTime = DateTime.UtcNow;
            Debug.WriteLine($"Undo -> {_cursor}/{_snapshots.Count}");
            return true;
        }

        public bool Redo()
        {
            if (_cursor >= _snapshots.Count - 1)
                return false;

            _cursor++;
            RestoreSnapshot(_snapshots[_cursor]);
            _lastUndoRedoTime = DateTime.UtcNow;
            Debug.WriteLine($"Redo -> {_cursor}/{_snapshots.Count}");
            return true;
        }

        private void RestoreSnapshot(EditableImageSnapshot s)
        {
            EditedBitmap.Dispose();
            EditedBitmap = s.EditedBitmap.Copy();
            FilterData = FilterDataClone(s.FilterData);
            Settings = CloneSettings(s.Settings);
            LayerManager.RestoreFrom(s.LayerManager);
        }

        /// <summary>
        /// Creates a default settings dictionary with initial values.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> CreateDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                ["Temperature"] = 6500f,
                ["Tint"] = 0f,
                ["Exposure"] = 0f,
                ["Contrast"] = 0f,
                ["Highlights"] = 0f,
                ["Shadows"] = 0f,
                ["Whites"] = 0f,
                ["Blacks"] = 0f,
                ["Texture"] = 0f,
                ["Dehaze"] = 0f,
                ["Vibrance"] = 0f,
                ["Saturation"] = 0f,
            };
        }

        /// <summary>
        /// Clones the settings dictionary to create a new instance.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static Dictionary<string, object> CloneSettings(Dictionary<string, object> src)
        {
            var copy = new Dictionary<string, object>(src.Count);
            foreach (var kv in src)
            {
                if (kv.Value is List<float> list)
                    copy[kv.Key] = new List<float>(list);
                else
                    copy[kv.Key] = kv.Value;
            }
            return copy;
        }

        public static FilterData FilterDataClone(FilterData original)
        {
            var clone = new FilterData();

            foreach (var kv in original.GetScores())
                clone.SetScore(kv.Key, kv.Value);

            clone.SetFlag(original.GetFlag());

            return clone;
        }

        private static string PrintSettings(Dictionary<string, object> settings)
        {
            string result = string.Empty;

            foreach (var elt in settings)
            {
                if (elt.Value is List<float> list)
                {
                    result += $"{elt.Key}: [{string.Join(", ", list)}]\n";
                }
                else if (elt.Value is List<Dictionary<string, int>> list2)
                {
                    result += $"{elt.Key}: [\n";
                    foreach (var dict in list2)
                    {
                        result += "  {\n";
                        foreach (var kv in dict)
                        {
                            result += $"    {kv.Key}: {kv.Value},\n";
                        }
                        result += "  },\n";
                    }
                }
                else if (elt.Value is Byte[] list3)
                {
                    result += $"{elt.Key}: [{string.Join(", ", list3.Select(b => b.ToString()))}]\n";
                }
                else if (elt.Value is SKBitmap bitmap)
                {
                    result += $"{elt.Key}: Bitmap ({bitmap.Width}x{bitmap.Height})\n";
                }
                else
                {
                    result += $"{elt.Key}: {elt.Value}\n";
                }
            }
            return result.TrimEnd('\n', ' ');
        }
    }
}
