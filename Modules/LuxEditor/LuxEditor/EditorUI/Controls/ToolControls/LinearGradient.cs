using Microsoft.UI.Xaml.Controls;
using LuxEditor.Models;

namespace LuxEditor.Controls.ToolControls
{
    public class LinearGradientToolControl : UserControl
    {
        public LinearGradientToolControl(MaskOperation operation)
        {
            Content = new TextBlock
            {
                Text = "Linear Gradient Tool (WIP)",
                Padding = new Microsoft.UI.Xaml.Thickness(8)
            };
        }
    }
}
