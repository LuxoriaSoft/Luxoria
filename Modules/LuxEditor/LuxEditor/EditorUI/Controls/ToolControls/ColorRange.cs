using Microsoft.UI.Xaml.Controls;
using LuxEditor.Models;

namespace LuxEditor.Controls.ToolControls
{
    public class ColorRangeToolControl : UserControl
    {
        public ColorRangeToolControl(MaskOperation operation)
        {
            Content = new TextBlock
            {
                Text = "Color Range Tool (WIP)",
                Padding = new Microsoft.UI.Xaml.Thickness(8)
            };
        }
    }
}
