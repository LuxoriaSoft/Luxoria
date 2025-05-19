using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LuxEditor.Models
{
    public class EditableImage
    {
        public string FileName { get; }
        public SKBitmap OriginalBitmap { get; }
        public SKBitmap? MediumBitmap { get; set; }
        public SKBitmap? PreviewBitmap { get; set; }
        public SKBitmap EditedBitmap { get; set; }
        public ReadOnlyDictionary<string, string> Metadata { get; }

        public Dictionary<string, object> Settings { get; private set; }

        private readonly Stack<Dictionary<string, object>> _history = new();
        private readonly Stack<Dictionary<string, object>> _redo = new();

        public EditableImage(SKBitmap bitmap, ReadOnlyDictionary<string, string> metadata, string fileName)
        {
            FileName = fileName;
            OriginalBitmap = bitmap;
            Metadata = metadata;
            Settings = CreateDefaultSettings();
            EditedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
        }

        public void SaveState()
        {
            _history.Push(CloneSettings(Settings));
            _redo.Clear();
        }

        public bool Undo()
        {
            if (_history.Count == 0) return false;
            _redo.Push(Settings);
            Settings = _history.Pop();
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0) return false;
            _history.Push(Settings);
            Settings = _redo.Pop();
            return true;
        }

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
                ["Clarity"] = 0f,
                ["Dehaze"] = 0f,
                ["Vibrance"] = 0f,
                ["Saturation"] = 0f,
            };
        }

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
    }
}
