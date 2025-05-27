using Microsoft.UI.Xaml;

namespace LuxEditor.EditorUI.Interfaces;

/// <summary>
/// Represents a UI element that can be rendered inside the editor panel.
/// </summary>
public interface IEditorControl
{
    /// <summary>
    /// Returns the root UIElement to be added to the visual tree.
    /// </summary>
    UIElement GetElement();
}
