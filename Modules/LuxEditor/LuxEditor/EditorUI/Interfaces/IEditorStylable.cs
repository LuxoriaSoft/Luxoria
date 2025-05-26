using LuxEditor.EditorUI.Models;

namespace LuxEditor.EditorUI.Interfaces;

/// <summary>
/// Defines customizable styling options for editor controls.
/// </summary>
public interface IEditorStylable
{
    void ApplyStyle(EditorStyle style);
}
