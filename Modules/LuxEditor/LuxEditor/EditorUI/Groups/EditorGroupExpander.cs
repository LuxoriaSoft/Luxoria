using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LuxEditor.EditorUI.Groups;

public class EditorGroupExpander : IEditorControl
{
    private readonly Expander _expander;
    private readonly StackPanel _container;

    /// <summary>
    /// Creates a new expander group for the editor UI.
    /// </summary>
    /// <param name="title"></param>
    public EditorGroupExpander(string title)
    {
        _container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        _expander = new Expander
        {
            Header = title,
            IsExpanded = true,
            Content = _container,
            Padding = new Thickness(10),
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
    }

    /// <summary>
    /// Adds a category to the expander.
    /// </summary>
    /// <param name="category"></param>
    public void AddCategory(EditorCategory category)
    {
        _container.Children.Add(category.GetElement());
    }

    /// <summary>
    /// Adds a control to the expander.
    /// </summary>
    /// <param name="item"></param>
    public void AddControl(IEditorGroupItem item)
    {
        _container.Children.Add(item.GetElement());
    }

    /// <summary>
    /// Gets the UI element for this expander group.
    /// </summary>
    /// <returns></returns>
    public UIElement GetElement() => _expander;
}
