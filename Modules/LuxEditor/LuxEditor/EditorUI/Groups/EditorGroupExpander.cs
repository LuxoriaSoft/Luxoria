using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LuxEditor.EditorUI.Groups;

public class EditorGroupExpander : IEditorControl
{
    private readonly Expander _expander;
    private readonly StackPanel _container;

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

    public void AddCategory(EditorCategory category)
    {
        _container.Children.Add(category.GetElement());
    }

    public UIElement GetElement() => _expander;
}
