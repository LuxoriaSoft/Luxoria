using LuxEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LuxEditor.Services
{
    /// <summary>
    /// Manages the list of editable images and handles selection state.
    /// </summary>
    public class ImageManager
    {
        private static readonly Lazy<ImageManager> _instance = new(() => new ImageManager());

        /// <summary>
        /// Gets the singleton instance of the ImageManager.
        /// </summary>
        public static ImageManager Instance => _instance.Value;

        public ObservableCollection<EditableImage> OpenedImages { get; } = new();
        public EditableImage? SelectedImage { get; private set; }

        public event Action<EditableImage>? OnSelectionChanged;

        private ImageManager() { }

        /// <summary>
        /// Loads a list of editable images into the manager.
        /// </summary>
        public void LoadImages(IEnumerable<EditableImage> images)
        {
            OpenedImages.Clear();

            foreach (var img in images)
            {
                OpenedImages.Add(img);
            }

            if (OpenedImages.Count > 0)
            {
                SelectImage(OpenedImages[0]);
            }
        }

        /// <summary>
        /// Selects the given image and notifies subscribers.
        /// </summary>
        public void SelectImage(EditableImage image)
        {
            SelectedImage = image;
            OnSelectionChanged?.Invoke(image);
        }

        /// <summary>
        /// Clears all images and resets the selection.
        /// </summary>
        public void Clear()
        {
            OpenedImages.Clear();
            SelectedImage = null;
        }
    }
}
