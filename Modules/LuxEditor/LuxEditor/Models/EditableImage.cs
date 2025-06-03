using LuxEditor.Logic;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LuxEditor.Models
{
    public class EditableImage
    {
        public string FileName { get; }
        public SKBitmap OriginalBitmap { get; }
        public SKBitmap? ThumbnailBitmap { get; set; }
        public SKBitmap? MediumBitmap { get; set; }
        public SKBitmap? PreviewBitmap { get; set; }
        public SKBitmap EditedBitmap { get; set; }
        public SKBitmap EditedPreviewBitmap { get; set; }
        public ReadOnlyDictionary<string, string> Metadata { get; }

        public Dictionary<string, object> Settings { get; private set; }

        private readonly Stack<Dictionary<string, object>> _history = new();
        private readonly Stack<Dictionary<string, object>> _redo = new();

        public readonly LayerManager LayerManager = new();

        /// <summary>
        /// Creates a new EditableImage instance.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="metadata"></param>
        /// <param name="fileName"></param>
        public EditableImage(SKBitmap bitmap, ReadOnlyDictionary<string, string> metadata, string fileName)
        {
            FileName = fileName;
            OriginalBitmap = bitmap;
            Metadata = metadata;
            Settings = CreateDefaultSettings();
            EditedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            EditedPreviewBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// Saves the current state of the settings to the history stack.
        /// </summary>
        public void SaveState()
        {
            _history.Push(CloneSettings(Settings));
            _redo.Clear();
        }

        /// <summary>
        /// Restores the settings to the last saved state.
        /// </summary>
        /// <returns></returns>
        public bool Undo()
        {
            if (_history.Count == 0) return false;
            _redo.Push(Settings);
            Settings = _history.Pop();
            return true;
        }

        /// <summary>
        /// Restores the settings to the next state in the redo stack.
        /// </summary>
        /// <returns></returns>
        public bool Redo()
        {
            if (_redo.Count == 0) return false;
            _history.Push(Settings);
            Settings = _redo.Pop();
            return true;
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
    }
}
