using Windows.UI;

namespace LuxEditor.EditorUI.Models;

/// <summary>
/// Represents visual styling information for an editor control.
/// </summary>
public class EditorStyle
{
    public Color? GradientStart { get; set; }
    public Color? GradientEnd { get; set; }
    public bool ShowTicks { get; set; } = false;
    public double? TickFrequency { get; set; } = null;
}
