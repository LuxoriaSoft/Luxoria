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
        public Dictionary<string, float> Filters { get; private set; }

        private Stack<Dictionary<string, float>> _history = new();
        private Stack<Dictionary<string, float>> _redo = new();

        /// <summary>
        /// Initializes a new instance of the EditableImage class with the specified image and metadata.
        /// </summary>
        public EditableImage(SKBitmap bitmap, ReadOnlyDictionary<string, string> metadata, string fileName)
        {
            FileName = fileName;
            OriginalBitmap = bitmap;
            Metadata = metadata;

            Filters = DefaultFilters();
            EditedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// Saves the current filter state into the history stack for undo support.
        /// </summary>
        public void SaveState()
        {
            _history.Push(new Dictionary<string, float>(Filters));
            _redo.Clear();
        }

        /// <summary>
        /// Reverts the filters to the last saved state if possible.
        /// </summary>
        public bool Undo()
        {
            if (_history.Count == 0) return false;
            _redo.Push(Filters);
            Filters = _history.Pop();
            return true;
        }

        /// <summary>
        /// Reapplies a previously undone filter state if available.
        /// </summary>
        public bool Redo()
        {
            if (_redo.Count == 0) return false;
            _history.Push(Filters);
            Filters = _redo.Pop();
            return true;
        }

        /// <summary>
        /// Creates a default dictionary of all supported filter keys with initial values.
        /// </summary>
        private Dictionary<string, float> DefaultFilters() => new()
        {
            { "Temperature", 6500 },
            { "Tint", 0 },
            { "Exposure", 0 },
            { "Contrast", 0 },
            { "Highlights", 0 },
            { "Shadows", 0 },
            { "Whites", 0 },
            { "Blacks", 0 },
            { "Texture", 0 },
            { "Clarity", 0 },
            { "Dehaze", 0 },
            { "Vibrance", 0 },
            { "Saturation", 0 }
        };
    }
}
