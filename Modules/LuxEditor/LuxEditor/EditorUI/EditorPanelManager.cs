using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace LuxEditor.EditorUI
{
    /// <summary>
    /// Manages the construction and rendering of the dynamic editor UI.
    /// </summary>
    public class EditorPanelManager
    {
        private readonly StackPanel _rootPanel;
        private readonly Dictionary<string, EditorSlider> _sliders = new();

        public EditorPanelManager(StackPanel rootPanel)
        {
            _rootPanel = rootPanel;
        }

        /// <summary>
        /// Adds a full expander group to the editor (like "Basic").
        /// </summary>
        public void AddCategory(EditorGroupExpander expander)
        {
            _rootPanel.Children.Add(expander.GetElement());
        }

        /// <summary>
        /// Registers a slider for lookup by key.
        /// </summary>
        public void RegisterSlider(string key, EditorSlider slider)
        {
            if (!_sliders.ContainsKey(key))
                _sliders[key] = slider;
        }

        /// <summary>
        /// Gets a slider by filter key.
        /// </summary>
        public EditorSlider? GetSlider(string key)
        {
            return _sliders.TryGetValue(key, out var s) ? s : null;
        }

        /// <summary>
        /// Resets all registered sliders to their default value.
        /// </summary>
        public void ResetAll()
        {
            foreach (var slider in _sliders.Values)
            {
                slider.ResetToDefault();
            }
        }
    }
}
