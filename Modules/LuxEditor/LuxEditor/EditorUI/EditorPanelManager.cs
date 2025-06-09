using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace LuxEditor.EditorUI
{
    /// <summary>
    /// Manages the dynamic editor UI: sliders, curves, etc.
    /// </summary>
    public class EditorPanelManager
    {
        private readonly StackPanel _rootPanel;
        private readonly Dictionary<string, IEditorControl> _controls = new();

        public EditorPanelManager(StackPanel rootPanel)
        {
            _rootPanel = rootPanel;
        }

        /// <summary>
        /// Adds a full expander group to the editor (e.g. "Basic", "Tone Curve").
        /// </summary>
        public void AddCategory(EditorGroupExpander expander)
            => _rootPanel.Children.Add(expander.GetElement());

        /// <summary>
        /// Registers any IEditorControl (slider, curve, etc.) for later lookup.
        /// </summary>
        public void RegisterControl(string key, IEditorControl control)
        {
            if (!_controls.ContainsKey(key))
                _controls[key] = control;
        }

        /// <summary>
        /// Retrieves a registered control by key, cast to the requested type.
        /// Returns null if not found or wrong type.
        /// </summary>
        public T? GetControl<T>(string key)
            where T : class, IEditorControl
        {
            if (_controls.TryGetValue(key, out var ctrl))
                return ctrl as T;
            return null;
        }

        /// <summary>
        /// Resets all sliders to their default values.
        /// </summary>
        public void ResetAll()
        {
            foreach (var ctrl in _controls.Values)
            {
                if (ctrl is EditorSlider slider)
                    slider.ResetToDefault();
            }
        }
    }
}
